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

        [NonSerialized]
        PerformanceCounter _performCounter;

       // [NonSerialized]
        public List<FileInfoTransfer> FileList
        {
            get { return GetTransferInfo(); }
            set { FileList = value; }
        }

        float _activePercent = 80;
        int _sleepTime = 1000;

        int _partSize = 16;

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
            _sharePath.Add(name, path);
        }

        public void SetHashOperationIdle(float percent, int sleepTime)
        {
            _activePercent = percent;
            _sleepTime = sleepTime;
        }

        private void CheckIdle(float percent, int sleepTime)
        {
            //_performCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            float load;
            while ((load = _performCounter.NextValue()) > percent)
            {
                Console.WriteLine("{0}   sleeping", load);
                Thread.Sleep(sleepTime);
            }
            Console.WriteLine("working");
        }

        bool GetAll(DirectoryInfo dir, ref List<CustFileInfo> FileList)//搜索文件夹中的文件
        {

            FileInfo[] allFile = dir.GetFiles();

            foreach (FileInfo fi in allFile)
            {
                string path = fi.FullName;
                string hash = "";
                List<string> hashList = null;
                CheckIdle(_activePercent, _sleepTime);

                try
                {
                    ShareFileEvent sfEvent = new ShareFileEvent();
                    sfEvent.FileName = fi.Name;
                    sfEvent.Path = fi.FullName;
                    if (Hashing != null)
                    {
                        Hashing(this, sfEvent);
                    }


                    hash = HashHelper.ComputeSHA1(path);
                    hashList = HashHelper.ComputeSHA1ByParts(path, _partSize);
                    if (HashComplete != null)
                    {
                        HashComplete(this, sfEvent);
                    }

                    // hash = Guid.NewGuid().ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }


                FileList.Add(new CustFileInfo() { File = fi, Hash = hash, HashList = hashList, Size = fi.Length, IsFolder = false });
            }

            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo item in allDir)
            {
                FileList.Add(new CustFileInfo() { File = item, IsFolder = true });
                GetAll(item, ref FileList);
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

        private void ListFileThread()
        {
            using (_performCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total"))
            {
                foreach (KeyValuePair<string, string> item in _sharePath)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(item.Value);
                    List<CustFileInfo> fileList = new List<CustFileInfo>();
                    try
                    {

                        if (GetAll(dirInfo, ref fileList))
                        {
                            if (_shareFileDict.ContainsKey(item))
                            {
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

                _performCounter.Close();

            }
        }

        public List<CustFileInfo> SearchFile(string szFile)
        {
            List<CustFileInfo> list = new List<CustFileInfo>();
            foreach (var item in _shareFileDict)
            {
                var result = from c in item.Value where c.File.Name.Contains(szFile) select c;
                list.AddRange(result);

            }
            return list;
        }

        protected class HashCompare : IEqualityComparer<ShareFile.FileInfoTransfer>
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
                    fileList.Add(new FileInfoTransfer()
                    {
                        Name = fileInfo.File.Name,
                        Hash = fileInfo.Hash,
                        Path = fileInfo.IsFolder ? fileInfo.File.FullName : null,
                        IsFolder = fileInfo.IsFolder,
                        Size = fileInfo.Size

                    });
                }
            }

            return fileList.Distinct(new HashCompare()).ToList();
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
            FileStream stream = new FileStream(path, FileMode.Open);
            BinaryFormatter bFormat = new BinaryFormatter();
            ShareFiles sh = (ShareFiles)bFormat.Deserialize(stream);
            stream.Close();
            return sh;

        }
        #region MyRegion
        private void CreateDataTable()
        {
            _table = new DataTable("File");
            // Declare variables for DataColumn and DataRow objects.
            DataColumn column;

            // Create new DataColumn, set DataType, 
            // ColumnName and add to DataTable.    
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Hash";
            column.ReadOnly = false;
            column.Unique = true;
            // Add the Column to the DataColumnCollection.
            _table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Name";
            column.AutoIncrement = false;
            column.ReadOnly = false;
            column.Unique = false;
            // Add the column to the table.
            _table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int64");
            column.ColumnName = "Size";
            column.AutoIncrement = false;
            column.ReadOnly = false;
            column.Unique = false;
            // Add the column to the table.
            _table.Columns.Add(column);
        }

        private void AddDataRow(DataRow row)
        {
            DataRow newRow;
            newRow = _table.NewRow();
            newRow = row;
            _table.Rows.Add(newRow);

        }
        #endregion

    }
}
