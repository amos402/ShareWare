using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Data;

namespace ShareWare.ShareFile
{
    [Serializable]
    public class ShareFiles
    {
        private List<string> shFileList = new List<string>();
        private List<List<FileInfo>> _fileList = new List<List<FileInfo>>();
        Dictionary<string, string> _shareName = new Dictionary<string, string>();

        Dictionary<Dictionary<string, string>, List<CustFileInfo>> _shareFileDict = new Dictionary<Dictionary<string, string>, List<CustFileInfo>>();

        public Dictionary<Dictionary<string, string>, List<CustFileInfo>> ShareFileDict
        {
            get
            {
                return _shareFileDict;
            }
            set { _shareFileDict = value; }
        }

        Dictionary<string, List<FileInfo>> openWith = new Dictionary<string, List<FileInfo>>();

        private DataTable _table;

        public DataTable Table
        {
            get { return _table; }
            set { _table = value; }
        }

        public List<List<FileInfo>> FileList
        {
            get { return _fileList; }
            set { _fileList = value; }
        }

        public ShareFiles()
        {
            GetSharePath();
            ListFile();

        }

        private void GetSharePath()
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
                        if (name != "R")
                        {
                            continue;
                        }

                        DirectoryInfo source = new DirectoryInfo(path);
                        if (source.Exists)
                        {
                            _shareName.Add(name, path);

                            shFileList.Add(path);
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        bool GetAll(DirectoryInfo dir, ref List<CustFileInfo> FileList)//搜索文件夹中的文件
        {

            FileInfo[] allFile = dir.GetFiles();

            foreach (FileInfo fi in allFile)
            {
                string path = fi.FullName;
                string hash = "";
                try
                {
                    hash = HashHelper.ComputeSHA1(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // hash = "";
                }


                FileList.Add(new CustFileInfo() { File = fi, Hash = hash });
            }

            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo d in allDir)
            {
                GetAll(d, ref FileList);
            }
            return true;
        }

        private void ListFile()
        {
            foreach (KeyValuePair<string, string> item in _shareName)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(item.Value);
                List<CustFileInfo> Flst = new List<CustFileInfo>();
                try
                {
                    GetAll(dirInfo, ref Flst);

                    //_fileList.Add(Flst);
                    _shareFileDict.Add(_shareName, Flst);
                    //openWith.Add(item, Flst);


                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Something wrong {0}", e); //打印异常返回信息
                    //throw;
                }

            }

        }

        public List<CustFileInfo> SearchFile(string szFile)
        {
            //List<FileInfo> list = new List<FileInfo>();
            //foreach (List<FileInfo> firstList in _fileList)
            //{
            //    foreach (FileInfo fileInfo in firstList)
            //    {
            //        if (fileInfo.Name.Contains(szFile))
            //        {
            //            list.Add(fileInfo);
            //        }
            //    }
            //}
            List<CustFileInfo> list = new List<CustFileInfo>();
            foreach (var item in _shareFileDict)
            {
                var result = from c in item.Value where c.File.Name.Contains(szFile) select c;
                list.AddRange(result);

            }
            return list;
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
