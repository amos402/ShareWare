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

namespace ShareWare.ShareFile
{
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

        [NonSerialized]
        PerformanceCounter _performCounter;

        //[NonSerialized]


        static AutoResetEvent event1 = new AutoResetEvent(true);

        // [NonSerialized]
        public List<FileInfoTransfer> FileList
        {
            get { return GetTransferInfo(); }
            set { FileList = value; }
        }

        float _activePercent = 80;
        int _sleepTime = 1000;
        KeyValuePair<string, string> _curPath;

        static int _partSize = 16;

        public event EventHandler<ShareFileEvent> OnePathComplete;
        public event EventHandler<ShareFileEvent> Hashing;
        public event EventHandler<ShareFileEvent> HashComplete;

        private DataTable _table;

        public DataTable Table
        {
            get { return _table; }
            set { _table = value; }
        }


        public void AddSystemSharePath()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select  *  from  win32_share");
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
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }

        }

        public void SetHashOperationIdle(float percent, int sleepTime)
        {
            _activePercent = percent;
            _sleepTime = sleepTime;
        }

        private void CheckIdle(object state)
        {
            //_performCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            Console.WriteLine("checking");
            float load;
            while ((load = _performCounter.NextValue()) > _activePercent)
            {
                Console.WriteLine("{0}   sleeping", load);
                Thread.Sleep(_sleepTime);
            }
            event1.Set();
            // Console.WriteLine("working");
        }

        bool GetAll(DirectoryInfo dir, ref List<CustFileInfo> fileList)//搜索文件夹中的文件
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

                        //hash = HashHelper.ComputeSHA1(path);
                        hashList = HashHelper.ComputeSHA1ByParts(path, _partSize);
                        SHA1 sha1 = SHA1.Create();
                        StringBuilder hashBuild = new StringBuilder();

                        foreach (var item in hashList)
                        {
                            hashBuild.Append(item);
                        }

                        byte[] hashBuf = sha1.ComputeHash(Encoding.UTF8.GetBytes(hashBuild.ToString()));
                        hashBuild.Clear();

                        foreach (var item in hashBuf)
                        {
                            hashBuild.Append(item.ToString("x2"));
                        }

                        hash = hashBuild.ToString();

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
                //else if (!fi.Exists)
                //{
                //    CustFileInfo fileInfo = _shareFileDict[_curPath].Find(T => T.File.FullName == path);
                //    if (fileInfo != null)
                //    {
                //        _removeList.Add(new FileInfoTransfer()
                //        {
                //            Hash = fileInfo.Hash,
                //            Name = fileInfo.File.Name,
                //            IsFolder = fileInfo.IsFolder,
                //            Size = fileInfo.Size
                //        });
                //    }
                //}

            }

            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo item in allDir)
            {
                string path = item.FullName;
                if (_shareFileDict[_curPath] == null || !_shareFileDict[_curPath].Exists(T => T.File.FullName == path))
                {
                    fileList.Add(new CustFileInfo() { File = item, Hash = Guid.NewGuid().ToString(), IsFolder = true });
                }
                GetAll(item, ref fileList);
            }
            return true;
        }

        public Thread ListFile()
        {
            Thread listThread = new Thread(new ThreadStart(ListFileThread));
            listThread.IsBackground = true;
            listThread.Start();
            return listThread;
        }

        private void RemoveNotExistFile()
        {
            //_tempList = new Dictionary<KeyValuePair<string, string>, List<CustFileInfo>>();
            foreach (var item in _shareFileDict)
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

            foreach (var item in _tempList.Keys)
            {
                foreach (var info in _tempList[item])
                {
                    _shareFileDict[item].Remove(info);
                }
            }

            _tempList.Clear();
        }

        private void ListFileThread()
        {
            using (_performCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total"))
            {
                Timer t = new Timer(CheckIdle, null, 0, 1000);

                RemoveNotExistFile();

                foreach (KeyValuePair<string, string> item in _sharePath)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(item.Value);
                    List<CustFileInfo> fileList = new List<CustFileInfo>();
                    try
                    {
                        if (!_shareFileDict.ContainsKey(item))
                        {
                            _shareFileDict.Add(item, null);
                        }

                        _curPath = item;
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
                            OnePathComplete(this, new ShareFileEvent() { Path = item.Value });
                        }

                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Something wrong {0}", e); //打印异常返回信息

                    }
                }

                t.Dispose();
                _performCounter.Close();

            }
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


            FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
            BinaryFormatter bFormat = new BinaryFormatter();
            bFormat.Serialize(stream, this);
            stream.Close();
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

                Console.WriteLine(e);
            }

            return new ShareFiles();

        }

        //private void CreateDataTable()
        //{
        //    DataTable fileInfo = new DataTable();
        //    fileInfo.Columns.AddRange(new DataColumn[]{
        //        new DataColumn("Hash", typeof(string)) {Unique = true},  
        //        new DataColumn("Name", typeof(string)),  
        //        new DataColumn("Size", typeof(long))}
        //        );


        //    DataTable fileOwner = new DataTable();
        //    fileOwner.Columns.AddRange(new DataColumn[]{
        //        new DataColumn("ID", typeof(int)) { AutoIncrement = true},
        //        new DataColumn("UserID", typeof(int)),
        //        new DataColumn("Name", typeof(string)),
        //        new DataColumn("Hash", typeof(string)) {AllowDBNull = true},
        //        new DataColumn("Path", typeof(string)) {AllowDBNull = true},
        //        new DataColumn("IsFolder", typeof(bool))
        //    });

        //    var fileList = GetTransferInfo();
        //    foreach (var item in fileList)
        //    {
        //        string guid = null;
        //        try
        //        {
        //            DataRow r1 = fileInfo.NewRow();
        //            if (item.Hash == null && item.IsFolder == true)
        //            {
        //                guid = Guid.NewGuid().ToString();
        //            }
        //            else
        //            {
        //                guid = item.Hash;
        //            }
        //            r1[0] = guid;
        //            r1[1] = item.Size;
        //            fileInfo.Rows.Add(r1);
        //        }
        //        catch (Exception)
        //        {

        //            //throw;
        //        }


        //        try
        //        {
        //            DataRow r2 = fileOwner.NewRow();
        //            r2[1] = userId;
        //            r2[2] = item.Name;
        //            r2[3] = guid;
        //            r2[4] = item.Path;
        //            r2[5] = item.IsFolder;
        //            fileOwner.Rows.Add(r2);
        //        }
        //        catch (Exception)
        //        {

        //            // throw;
        //        }

        //    }
        //#endregion

        //}


    }
}
