using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Timers;

namespace ShareWare.ShareFile
{
    /// <summary>
    /// 包含共享文件信息与操作的类
    /// </summary>
    [Serializable]
    public class ShareFiles
    {
        Dictionary<string, string> _sharePath = new Dictionary<string, string>();

        public Dictionary<string, string> SharePath
        {
            get { return _sharePath; }
            set { _sharePath = value; }
        }

        Dictionary<KeyValuePair<string, string>, List<CustFileInfo>> _shareFileDict = new Dictionary<KeyValuePair<string, string>, List<CustFileInfo>>();

        public Dictionary<KeyValuePair<string, string>, List<CustFileInfo>> ShareFileDict
        {
            get { return _shareFileDict; }
            set { _shareFileDict = value; }
        }

        private List<FileInfoTransfer> _removeList = new List<FileInfoTransfer>();

        public List<FileInfoTransfer> RemoveList
        {
            get { return _removeList; }
            set { _removeList = value; }
        }

        private Dictionary<KeyValuePair<string, string>, List<CustFileInfo>> _tempList = new Dictionary<KeyValuePair<string, string>, List<CustFileInfo>>();

        private List<string> _systemShareNameList;

        public List<string> SystemShareNameList
        {
            get { return _systemShareNameList; }
        }

        [NonSerialized]
        PerformanceCounter _performCounter;

        [NonSerialized]
        private Thread _listThread;

        [NonSerialized]
        private AutoResetEvent _ret = new AutoResetEvent(true);

        //[NonSerialized]


        // [NonSerialized]
        public List<FileInfoTransfer> FileList
        {
            get { return GetTransferInfo(); }
            set { FileList = value; }
        }

        float _activePercent = 80;
        int _sleepTime = 1000;
        KeyValuePair<string, string> _curPath;

        private static int _partSize = 20;
        private static string _diskSerial = GetDiskSerialNumbers();

        [field: NonSerializedAttribute()]
        public event EventHandler<ShareFileEvent> HashingPath;

        [field: NonSerializedAttribute()]
        public event EventHandler<ShareFileEvent> OnePathComplete;

        [field: NonSerializedAttribute()]
        public event EventHandler<ShareFileEvent> Hashing;

        [field: NonSerializedAttribute()]
        public event EventHandler<ShareFileEvent> HashComplete;

        [field: NonSerializedAttribute()]
        public event EventHandler<ShareFileEvent> AllScanComplete;

        [field: NonSerializedAttribute()]
        public event EventHandler<ShareFileEvent> SharePathChanged;

        private DataTable _table;

        public DataTable Table
        {
            get { return _table; }
            set { _table = value; }
        }


        public void AddSystemSharePath()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select  *  from  win32_share");
            _systemShareNameList = new List<string>();
            foreach (ManagementObject share in searcher.Get())
            {
                try
                {
                    string name = share["Name"].ToString();
                    string path = share["Path"].ToString();

                    if ((share["Type"]).ToString() == "0"/*name[name.Length - 1] != '$'*/)
                    {
                        DirectoryInfo source = new DirectoryInfo(path);
                        if (source.Exists)
                        {
                            _sharePath.Add(name, path);
                            if (SharePathChanged != null)
                            {
                                SharePathChanged(this, new ShareFileEvent() { FileName = name, Path = path });
                            }
                            _systemShareNameList.Add(name);

                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        public void AddSharePath(string name, string path)
        {
            try
            {
                _sharePath.Add(name, path);
                if (SharePathChanged != null)
                {
                    SharePathChanged(this, new ShareFileEvent() { FileName = name, Path = path });
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }

        }

        public bool RemoveSharePath(string shareName)
        {
            try
            {
                string path = _sharePath[shareName];
                _sharePath.Remove(shareName);

                if (SharePathChanged != null)
                {
                    SharePathChanged(this, new ShareFileEvent() { FileName = shareName, Path = path });
                }

                var pathKey = _shareFileDict.Keys.Single(T => T.Key == shareName);
                var value = _shareFileDict[pathKey];
                _shareFileDict.Remove(pathKey);
                // _sharePath.Remove(pathKey.Key);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool ChangeSharePath(string oldShareName, SharePathData newPathData)
        {
            try
            {
                string path = _sharePath[oldShareName];
                _sharePath.Remove(oldShareName);

                _sharePath.Add(newPathData.ShareName, newPathData.Path);

                if (SharePathChanged != null)
                {
                    SharePathChanged(this, new ShareFileEvent() { FileName = oldShareName, Path = path });
                }

                var pathKey = _shareFileDict.Keys.Single(T => T.Key == oldShareName);
                var value = _shareFileDict[pathKey];
                _shareFileDict.Remove(pathKey);

                var newPathKeyPair = _sharePath.Single(T =>
                    {
                        return T.Key == newPathData.ShareName;
                    });

                _shareFileDict.Add(newPathKeyPair, value);
                // _sharePath.Remove(pathKey.Key);

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public void SetHashOperationIdle(float percent, int sleepTime)
        {
            _activePercent = percent;
            _sleepTime = sleepTime;
        }

        private void CheckIdle()
        {
            // _performCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            float load;
            while ((load = _performCounter.NextValue()) > _activePercent)
            {
                //    Console.WriteLine("{0}   sleeping", load);
                Thread.Sleep(1000);
            }

            //Console.WriteLine("working");
        }


        bool GetAll(DirectoryInfo dir, ref List<CustFileInfo> fileList)
        {
            FileInfo[] allFile = dir.GetFiles();

            foreach (FileInfo fi in allFile)
            {
                string path = fi.FullName;
                bool bExist = false;

                if (_shareFileDict[_curPath] != null)
                {
                    bExist = _shareFileDict[_curPath].Exists(T => T.File.FullName == path);
                }

                if (!bExist)
                {
                    string hash = null;
                    List<string> hashList = null;
                    // CheckIdle(_activePercent, _sleepTime);

                    try
                    {
                        ShareFileEvent sfEvent = new ShareFileEvent();
                        sfEvent.FileName = fi.Name;
                        sfEvent.Path = fi.FullName;
                        if (Hashing != null)
                        {
                            Hashing(this, sfEvent);
                        }

                        hash = HashHelper.ComputeFileMd5(path, out hashList);

                        //hashList = ComputeSHA1ByParts(path, _partSize);
                        //SHA1 sha1 = SHA1.Create();
                        //StringBuilder hashBuild = new StringBuilder();

                        //foreach (var item in hashList)
                        //{
                        //    hashBuild.Append(item);
                        //}

                        //byte[] hashBuf = sha1.ComputeHash(Encoding.UTF8.GetBytes(hashBuild.ToString()));
                        //hashBuild.Clear();

                        //foreach (var item in hashBuf)
                        //{
                        //    hashBuild.Append(item.ToString("x2"));
                        //}

                        //hash = hashBuild.ToString();

                        if (HashComplete != null)
                        {
                            HashComplete(this, sfEvent);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (hash != null && hash != string.Empty)
                    {
                        fileList.Add(new CustFileInfo() { File = fi, Hash = hash, HashList = hashList, Size = fi.Length, IsFolder = false });
                    }
                }

            }

            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo item in allDir)
            {
                string path = item.FullName;
                if (_shareFileDict[_curPath] == null || !_shareFileDict[_curPath].Exists(T => T.File.FullName == path))
                {
                    string hash = HashHelper.ComputeStringMd5(_diskSerial + item.FullName);
                    fileList.Add(new CustFileInfo() { File = item, Hash = hash, IsFolder = true });
                }
                GetAll(item, ref fileList);
            }
            return true;
        }

        /// <summary>
        /// 遍历扫描共享文件信息
        /// </summary>
        /// <returns>扫描线程对象</returns>
        public Thread ListFile()
        {
            _listThread = new Thread(new ThreadStart(ListFileThread));
            _listThread.IsBackground = true;
            _listThread.Start();
            return _listThread;
        }

        private void RemoveNotExistFile()
        {
            foreach (var item in _shareFileDict)
            {
                if (item.Value != null)
                {
                    var list = item.Value.FindAll(T => !T.File.Exists);
                    foreach (var file in list)
                    {
                        _removeList.Add(new FileInfoTransfer()
                        {
                            Hash = file.Hash,
                            Name = file.File.Name,
                            IsFolder = file.IsFolder,
                            Size = file.Size
                        });
                    }
                    _tempList.Add(item.Key, list);
                }
            }

            foreach (var item in _tempList.Keys)
            {
                foreach (var info in _tempList[item])
                {
                    _shareFileDict[item].Remove(info);
                }
            }

            _tempList.Clear();
        }

        /// <summary>
        /// 执行文件扫描的线程
        /// </summary>
        private void ListFileThread()
        {
            if (_ret == null)
            {
                _ret = new AutoResetEvent(true);
            }
            _ret.WaitOne();

            using (_performCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total"))
            {
                RemoveNotExistFile();

                foreach (KeyValuePair<string, string> item in _sharePath)
                {
                    CheckIdle();
                    DirectoryInfo dirInfo = new DirectoryInfo(item.Value);
                    List<CustFileInfo> fileList = new List<CustFileInfo>();
                    try
                    {
                        if (!_shareFileDict.ContainsKey(item))
                        {
                            _shareFileDict.Add(item, null);
                        }

                        _curPath = item;

                        ShareFileEvent arg = new ShareFileEvent() { Path = item.Value };
                        if (HashingPath != null)
                        {
                            HashingPath(this, arg);
                        }

                        if (GetAll(dirInfo, ref fileList))
                        {
                            if (_shareFileDict.ContainsKey(item))
                            {
                                if (_shareFileDict[item] == null)
                                {
                                    _shareFileDict[item] = new List<CustFileInfo>();
                                }
                                _shareFileDict[item].AddRange(fileList);
                            }
                            else
                            {
                                _shareFileDict.Add(item, fileList);
                            }
                        }

                        if (OnePathComplete != null)
                        {
                            OnePathComplete(this, arg);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);

                    }
                }

                if (AllScanComplete != null)
                {
                    AllScanComplete(this, null);
                }

                // t.Dispose();
                _performCounter.Close();
            }

            _ret.Set();
        }

        public List<CustFileInfo> SearchFile(string fileName)
        {
            List<CustFileInfo> list = new List<CustFileInfo>();
            foreach (var item in _shareFileDict)
            {
                var result = from c in item.Value
                             where c.File.Name.Contains(fileName)
                             select c;
                list.AddRange(result);
            }
            return list;
        }

        public CustFileInfo FindFile(string hash)
        {
            CustFileInfo file = null;
            foreach (var item in _shareFileDict.Values)
            {
                file = item.Find(T =>
                {
                    if (T.Hash == hash)
                    {
                        return true;
                    }
                    return false;
                });

                if (file != null)
                {
                    break;
                }
            }

            return file;
        }

        protected class HashNameCompare : IEqualityComparer<ShareFile.FileInfoTransfer>
        {
            public bool Equals(ShareFile.FileInfoTransfer x, ShareFile.FileInfoTransfer y)
            {
                return x.Hash == y.Hash && x.Name == y.Name;
            }

            public int GetHashCode(ShareFile.FileInfoTransfer obj)
            {
                return obj == null ? 0 : obj.Name.GetHashCode();
            }
        }

        public List<FileInfoTransfer> GetTransferInfo()
        {
            List<FileInfoTransfer> fileList = new List<FileInfoTransfer>();

            foreach (var item in _shareFileDict.Values)
            {
                foreach (var fileInfo in item)
                {
                    if (!fileInfo.Uploaded)
                    {
                        fileList.Add(new FileInfoTransfer()
                        {
                            Name = fileInfo.File.Name,
                            Hash = fileInfo.Hash,
                            IsFolder = fileInfo.IsFolder,
                            Size = fileInfo.Size

                        });
                    }
                }
            }

            return fileList.Distinct(new HashNameCompare()).ToList();
        }

        public void SetUploaded(List<FileInfoTransfer> list)
        {
            foreach (var item in list)
            {
                foreach (var value in _shareFileDict.Values)
                {
                    foreach (var fileInfo in value)
                    {
                        if (fileInfo.Hash == item.Hash)
                        {
                            fileInfo.Uploaded = true;
                        }
                    }
                }
            }

        }

        public void Serialize(string path)
        {
            FileInfo file = new FileInfo(path);

            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName);
            }
            try
            {
                FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
                BinaryFormatter bFormat = new BinaryFormatter();
                bFormat.Serialize(stream, this);
                stream.Close();
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
        }

        public static ShareFiles Deserialize(string path)
        {
            try
            {
                FileStream stream = new FileStream(path, FileMode.Open);
                BinaryFormatter bFormat = new BinaryFormatter();
                ShareFiles sh = (ShareFiles)bFormat.Deserialize(stream);
                stream.Close();
                return sh;
            }
            catch (Exception e)
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception)
                {
                    // throw;
                }
                Console.WriteLine(e);
            }

            return new ShareFiles();

        }


        public static List<string> ComputeSHA1ByParts(string fileName, int partSize)
        {
            List<string> hashList = null;

            if (System.IO.File.Exists(fileName))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                {
                    System.Security.Cryptography.SHA1 calculator = System.Security.Cryptography.SHA1.Create();

                    long nSize = fs.Length;

                    hashList = new List<string>();
                    int bufSize = partSize * 1024 * 1024;
                    byte[] buffer = new byte[bufSize];
                    byte[] hashBuf;
                    int nLen = 0;
                    int nPos = 0;
                    int nCount = 0;
                    StringBuilder strBuild = new StringBuilder();

                    do
                    {
                        nLen = fs.Read(buffer, 0, bufSize);
                        nPos += nLen;

                        hashBuf = calculator.ComputeHash(buffer);
                        strBuild.Clear();
                        foreach (var item in hashBuf)
                        {
                            strBuild.Append(item.ToString("x2"));
                        }
                        hashList.Add(strBuild.ToString());
                        nCount++;

                    } while (nPos != nSize);

                    calculator.Clear();
                    //hashSHA1 = stringBuilder.ToString();
                }
            }

            return hashList;
        }

        public string GetFileHash(string fullName)
        {
            var file = (from b in
                            (from c in _shareFileDict.Values
                             select c)
                        select b.Find(T => T.File.FullName == fullName))
                   .Single();

            return file.Hash;
        }

        protected static string GetDiskSerialNumbers()
        {
            ManagementObjectSearcher s = new ManagementObjectSearcher("select * from Win32_DiskDrive");
            ManagementObjectCollection collection = s.Get();
            StringBuilder str = new StringBuilder();
            foreach (var item in collection)
            {
                str.Append(item["SerialNumber"].ToString());
            }
            return str.ToString();
        }

    }
}
