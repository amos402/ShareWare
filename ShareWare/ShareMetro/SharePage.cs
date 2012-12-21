using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ShareWare.ShareFile;

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        public string ShareLog { get; set; }
        public FileInfoDataList Selection_ShareFile { get; set; }

        private ObservableCollection<FileInfoDataList> shareLoact = new ObservableCollection<FileInfoDataList>();
        public ObservableCollection<FileInfoDataList> ShareLoact
        {
            get { return shareLoact; }
            set { shareLoact = value; }
        }

        public ICommand Up_ShareCommand
        {
            get
            {
                return new ICom(p =>
                {
                    if (local_Address == "") return;
                    Show_ShareFile("\\", ShareLoact);
                });
            }
        }

        public ICommand Open_ShareFileCommand
        {
            get
            {
                return new ICom(p =>
                {
                    Open_shareFile(Selection_ShareFile);
                });
            }
        }

        public ICommand Detele_ShareFileCommand
        {
            get
            {
                return new ICom(p =>
                {
                    Delete_ShareFile(Selection_ShareFile);
                });
            }
        }

        public ICommand Ret_ShareFileCommand
        {
            get
            {
                return new ICom(p =>
                {
                    Show_ShareFile(Local_Address, ShareLoact);
                });
            }
        }

        List<string> Root_Name = new List<string>();
        Thread t_ShareFile = null;
        string local_Address = "";
        public string Local_Address
        {
            get { return local_Address; }
            set { local_Address = value; }
        }

        public void Open_shareFile(FileInfoDataList d)
        {
            if (Selection_ShareFile == null) return;
            if (_sh.SharePath.ContainsKey(Selection_ShareFile.Name))
            {
                Local_Address = _sh.SharePath[Selection_ShareFile.Name];
            }
            else
            {
                Local_Address += (@"\" + Selection_ShareFile.Name);
            }
            FileInfo f = null;
            try
            {
                f = new FileInfo(Local_Address);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Data.ToString());
                return;
            }
            try
            {
                if (f.Attributes == FileAttributes.Directory) System.Diagnostics.Process.Start("explorer", Local_Address);
                else System.Diagnostics.Process.Start(Local_Address);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Data.ToString());
            }

        }

        public void Delete_ShareFile(FileInfoDataList d)
        {
            if (Selection_ShareFile == null) return;
            if (_sh.SharePath.ContainsKey(Selection_ShareFile.Name))
            {
                Local_Address = _sh.SharePath[Selection_ShareFile.Name];
            }
            else
            {
                Local_Address += (@"\" + Selection_ShareFile.Name);
            }
            if (d.Type == "文件夹")
            {
                Delete_Directory(Local_Address);
            }
            else
            {
                FileInfo f = new FileInfo(Local_Address);
                f.Delete();
            }
        }

        public void Show_ShareFile(string strBaseDir, ObservableCollection<FileInfoDataList> list)
        {
            if (t_ShareFile != null)
            {
                if (t_ShareFile.IsAlive) t_ShareFile.Abort();
            }
            t_ShareFile = new Thread(new ParameterizedThreadStart(p =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate { list.Clear(); });
                if (strBaseDir == null || strBaseDir == "")
                {
                    Show_Root(list, true);
                }
                else if (strBaseDir == "\\")
                {
                    string[] split = Local_Address.Split(new Char[] { '\\' });
                    foreach (var item in Root_Name)
                    {
                        if (split[split.Length - 1] == item)
                        {
                            Local_Address = "";
                            Show_Root(list, false);
                            return;
                        }
                    }
                    int nl = Local_Address.LastIndexOf("\\");
                    Local_Address = Local_Address.Substring(0, nl);
                    GetDirList(Local_Address, list);
                }
                else
                {
                    if (_sh.SharePath.ContainsKey(strBaseDir))
                    {
                        Local_Address = _sh.SharePath[strBaseDir];
                    }
                    else
                    {
                        Local_Address += (@"\" + strBaseDir);
                    }
                    GetDirList(Local_Address, list);
                }
            }));
            t_ShareFile.Start();
        }

        private void Show_Root(ObservableCollection<FileInfoDataList> list, bool first_execution)
        {
            FileInfoDataList sh = new FileInfoDataList();
            foreach (var item in _sh.SharePath.Keys)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                {
                    sh.Name = item;
                    sh.IsFolder = true;
                    sh.Size = null;
                    sh = ShowSharefile.GetInfo(sh);
                    if (first_execution) Root_Name.Add(item);
                    list.Add(sh);
                });
            }
        }

        private void GetDirList(string strBaseDir, ObservableCollection<FileInfoDataList> list)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(strBaseDir);
                DirectoryInfo[] diA = di.GetDirectories();
                FileInfo[] files = di.GetFiles();
                FileInfoDataList sh = new FileInfoDataList();
                foreach (var item in files)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                    {
                        sh.Name = item.Name;
                        sh.IsFolder = false;
                        sh.Size = null;
                        sh = ShowSharefile.GetInfo(sh);
                        list.Add(sh);
                    });
                }
                foreach (var item in diA)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                    {
                        sh.Name = item.Name;
                        sh.IsFolder = true;
                        sh.Size = null;
                        sh = ShowSharefile.GetInfo(sh);
                        list.Add(sh);
                    });
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        private static string _hashingStr = "正在扫描文件夹：";
        private void AddHashingInfo(string path)
        {
            ShareLog += (_hashingStr + path + '\n');
            //OnPropertyChanged("ShareLog");
        }

        private static string _hashCompleteStr = "扫描完毕：";
        private void AddHashCompleteInfo(string path)
        {
            ShareLog += (_hashCompleteStr + path + '\n');
            //OnPropertyChanged("ShareLog");
        }
    }
}
