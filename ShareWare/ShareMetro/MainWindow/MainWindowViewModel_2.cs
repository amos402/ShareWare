using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using MahApps.Metro.Controls;
//using ShareMetro.Models;
using ShareWare;
using System.Threading;
using System.Windows.Input;
using System;
using System.Windows;
using System.Timers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Socket_Library;
using ShareWare.ShareFile;
using System.Threading.Tasks;
using System.Net;

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {

        private void Inti()
        {
            ImportInfo();
            ShowSharefile.CallShow += CallShowListView;
        }

        public bool Islist { get; set; }

        private string directory = @"";
        public string Directory
        {
            get
            {
                return directory;
            }
            set
            {
                directory = value;
                OnPropertyChanged("Directory");
                //if (this.PropertyChanged != null)/////////////////////////////////////////////////////////
                //    PropertyChanged(this, new PropertyChangedEventArgs("Directory"));
            }
        }
        private string t_upLoadSpeed = "上传：0 KB/S";
        public string t_UpLoadSpeed
        {
            get { return t_upLoadSpeed; }
            set { t_upLoadSpeed = value; OnPropertyChanged("t_UpLoadSpeed"); }
        }
        private string t_downLoadSpeed = "下载：0 KB/S";
        public string t_DownLoadSpeed
        {
            get { return t_downLoadSpeed; }
            set { t_downLoadSpeed = value; OnPropertyChanged("t_DownLoadSpeed"); }
        }



        public OnlineUserData SeleteManInfo { get; set; }
        public FileInfoDataList SeleteInfo { get; set; }
        public DownListInfo Down_list_Selete { get; set; }

        int limit_Uplaod_Speed = 10;
        public int Limit_Uplaod_Speed
        {
            get { return limit_Uplaod_Speed; }
            set { limit_Uplaod_Speed = value; }
        }
        int limit_DownLoad_Speed = 10;
        public int Limit_DownLoad_Speed
        {
            get { return limit_DownLoad_Speed; }
            set { limit_DownLoad_Speed = value; }
        }
        int d_m = 0; int u_m = 0;
        Thread t = null;
        string LoadIDName = "";
        ShowShareFileInfo showSharefile = new ShowShareFileInfo();
        public ShowShareFileInfo ShowSharefile
        {
            get { return showSharefile; }
            set { showSharefile = value; }
        }
        List<DownLoad> Down_d = new List<DownLoad>();
        List<Upload> upLoad = new List<Upload>();
        public List<Upload> UpLoad
        {
            get { return upLoad; }
            set { upLoad = value; }
        }
        private List<talkwin> talk_List = new List<talkwin>();
        public List<talkwin> Talk_List
        {
            get { return talk_List; }
            set { talk_List = value; }
        }


        private ObservableCollection<DownListInfo> downInfo = new ObservableCollection<DownListInfo>();
        public ObservableCollection<DownListInfo> DownInfo
        {
            get { return downInfo; }
            set { downInfo = value; }
        }
        private ObservableCollection<DownListInfo> historicalRecords = new ObservableCollection<DownListInfo>();
        public ObservableCollection<DownListInfo> HistoricalRecords
        {
            get { return historicalRecords; }
            set { historicalRecords = value; }
        }
        private ObservableCollection<DownListInfo> garbageInfo = new ObservableCollection<DownListInfo>();
        public ObservableCollection<DownListInfo> GarbageInfo
        {
            get { return garbageInfo; }
            set { garbageInfo = value; }
        }

        public ICommand GoCommand
        {
            get
            {
                return new ICom(p =>
                {
                    if (directory == "") return;
                    Islist = true;
                    string[] split = directory.Split(new Char[] { '\\' });
                    if (ShowSharefile.NewSock != null)
                    {
                        ShowSharefile.NewSock.Close();
                        ShowSharefile.NewSock = null;
                    } 
                    ShowSharefile.CreatSocket(directory);
                    _client.RequestOpenShareFolder(split[0], ShowSharefile.Port);
                });
            }
        }

        public ICommand UpCommand
        {
            get
            {
                return new ICom(p =>
                {
                    if (Directory != "")
                    {
                        Directory.Trim();
                        string[] split = Directory.Split(new Char[] { '\\' });
                        if (split.Length != 2)
                        {
                            Directory = Directory.Substring(0, Directory.Length - split[split.Length - 2].Length - split[split.Length - 3].Length - 2);
                            LoadItems(split[split.Length - 3]);
                        }
                    }
                });
            }
        }

        public ICommand DownLoadCommand
        {
            get
            {
                return new ICom(p => CreatDowndLoad(SeleteInfo.Name));
            }
        }

        public ICommand Stop_DownLoadCommand
        {
            get
            {
                return new ICom(Stop_DowndLoad);
            }
        }

        public ICommand Go_DownLoadCommand
        {
            get
            {
                return new ICom(Go_DowndLoad);
            }
        }

        public ICommand Detelet_DownListViewInfoCommand
        {
            get
            {
                return new ICom(Detelet_DownListViewInfo);
            }
        }

        public ICommand Detelet_HistoryListViewInfoCommand
        {
            get
            {
                return new ICom(Detelet_HistoryListViewInfo);
            }
        }

        public ICommand Open_FileCommand
        {
            get
            {
                return new ICom(Open_File);
            }
        }

        public ICommand Detelet_FileCommand
        {
            get
            {
                return new ICom(Detelet_File);
            }
        }

        public ICommand Reduction_GarbageInfoCommand
        {
            get
            {
                return new ICom(Reduction_GarbageInfo);
            }
        }

        public ICommand ChatTalkCommand
        {
            get
            {
                return new ICom(p => Talk(OnlineUser));
            }
        }


        public void CallLoad(object p)
        {
            if (p == null) return;
            LoadItems(p);
            //if (ShowSharefile.NewSock == null)
            //    _client.RequestOpenShareFolder((string)p, f);
        }

        public int LoadItems(object p)
        {
            if (t != null)
            {
                if (t.IsAlive) t.Abort();
            }
            string D = "";
            string a = ((string)p);
            //if (a.Length == 36 && Directory.IndexOf("\\") == -1)
            //{
            //}
            //else
            {
                if (a != null && a is string)
                {
                    D = directory + a + "\\";
                    Directory = Directory + a + "\\";
                    string[] split = Directory.Split(new Char[] { '\\' });
                    if ((split.Length == 2 || split.Length == 1) && ShowSharefile.NewSock != null)
                    {
                        ShowSharefile.NewSock.Close();
                        ShowSharefile.NewSock = null;
                    }
                }
                else
                {
                    if (Directory.IndexOf("\\") == -1 || Directory.LastIndexOf("\\") != Directory.Length - 1)
                    {
                        Directory = Directory + "\\";
                    }
                    if (LoadIDName != "")
                    {
                        string Lname = "";
                        if (Directory.IndexOf("\\") == -1) Lname = Directory;
                        else Lname = Directory.Substring(Directory.IndexOf("\\"));
                        if (LoadIDName.CompareTo(Lname) != 0)
                        {
                            ShowSharefile.NewSock.Close();
                            ShowSharefile.NewSock = null;
                        }
                    }
                    D = Directory;
                }
            }
            if (ShowSharefile.NewSock == null)
            {
                ShowSharefile.CreatSocket(D);
                _client.RequestOpenShareFolder((string)p, ShowSharefile.Port);
            }
            else ParseDirectoryThread(D);
            return ShowSharefile.Port;
        }

        private void CallShowListView(object sender, CallShowListView e)
        {
            ParseDirectoryThread(e.Directory);
        }

        private void ParseDirectoryThread(object p)//使用后台线程获取文件信息  
        {
            /////////////_client.RequestOpenShareFolder(string.Empty, 0);

            string _directory = "";
            if (p != null && p is string)
                _directory = (string)p;
            if (_directory == "")
            {
                return;
            }
            ShowSharefile.SendDirectoryName(_directory);
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                {
                    FileItemInfo.Clear();
                });
                t = new Thread(new ParameterizedThreadStart(ParseDirectoryRecursive));//在后台线程中调用ParseDirectoryRecursive方法  
                t.IsBackground = true;  //指定为后台线程  
                t.Priority = ThreadPriority.BelowNormal;//指定线程优先级别  
                t.Start(_directory); //为线程方法传入文件夹路径  
            }
            catch (Exception)  //如果产生异常  
            {   //调用自定义的异常信息窗口  
                throw;
                // ExceptionManagement.Manage("Catalog:ParseDirectoryThread", err);
            }
        }

        private void ParseDirectoryRecursive(object path)
        {
            try
            {
                do
                {
                    FileInfoDataList ss = ShowSharefile.ReceiveShare();
                    if (ss.Name == "") break;
                    int asdf = ss.Name.LastIndexOf(@"\");
                    string a = ss.Name.Substring(0, asdf + 1);
                    if (Directory != a) continue;
                    ss.Name = ss.Name.Substring(asdf + 1, ss.Name.Length - asdf - 1);
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                    {
                        FileInfoDataList si = ShowSharefile.GetInfo(ss);
                        if (si != null) FileItemInfo.Add(si);
                        Thread.Sleep(5);
                    });
                } while (true);
            }
            catch (Exception)
            {
                ;
            }

        }

        public void CreatDowndLoad(object p)
        {
            Thread t1 = new Thread(new ParameterizedThreadStart(CreatDowndLoadThread));//在后台线程中调用ParseDirectoryRecursive方法  
            t1.IsBackground = true;  //指定为后台线程  
            t1.Priority = ThreadPriority.BelowNormal;//指定线程优先级别  
            t1.Start(SeleteInfo); //为线程方法传入文件夹路径              
        }

        private void CreatDowndLoad_Talk(object sender, Control_Down e)
        {
            Thread t1 = new Thread(new ParameterizedThreadStart(CreatDowndLoadThread));//在后台线程中调用ParseDirectoryRecursive方法  
            t1.IsBackground = true;  //指定为后台线程  
            t1.Priority = ThreadPriority.BelowNormal;//指定线程优先级别  
            t1.Start(e.SeleteInfo); //为线程方法传入文件夹路径              
        }

        private void CreatDowndLoadThread(object p)
        {
            FileInfoDataList SeleteInfo1 = (FileInfoDataList)p;
            if (SeleteInfo1 == null) return;
            #region MyRegion
            //if (SeleteInfo1.Hash.Length < 36)
            //{
            //    MessageBox.Show("根目录不能下载");
            //    return;
            //} 
            #endregion

            if (SeleteInfo1.IsFolder == true)
            {
                MessageBox.Show("根目录不能下载");
                return;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)delegate
            {
                DownLoad d = new DownLoad();
                LoadInfo l = new LoadInfo();
                //l.name = @"DriverGenius2012";
                //l.type = "文件夹";
                d.RootDirectory = CurrentDownloadPath + @"\";
                l.name = SeleteInfo1.Name;
                if (SeleteInfo1.Size == null) l.size = 0;
                else l.size = (long)SeleteInfo1.Size;
                l.type = SeleteInfo1.Type;
                DownListInfo sm = d.CreatDownLoad(l);
                if (sm == null) return;
                sm.ID = d.ID = DownInfo.Count;
                sm.State = "下载";
                sm.Hash = SeleteInfo.Hash;
                DownInfo.Add(sm);
                Down_d.Add(d);
                for (int i = 0; i < Down_d.Count; i++)
                {
                    Down_d[i].Maxseepd = Limit_DownLoad_Speed;
                    Down_d[i].Sum = Down_d.Count;
                }
                d.Percentage += Refresh;
                //if (ShowSharefile == null)
                {
                    Task<int> task = _client.DownloadRequestAsync(SeleteInfo.Hash, d.Port);
                    task.ContinueWith(T =>
                    {

                    });
                }
                //else ShowSharefile.SendDirectoryName(@":\" + Directory + @"\" + l.name);
                Serialization(@"./config/ImportDownLoadInfo", DownInfo);
            });
        }

        public void Stop_DowndLoad(object p)
        {
            if (Down_list_Selete == null) return;
            if (DownInfo[Down_list_Selete.ID].State == "暂停") return;
            DownInfo[Down_list_Selete.ID].State = "暂停";
            Down_d[Down_list_Selete.ID].Stop_DownLoad = true;
            Down_d[Down_list_Selete.ID].AllDone.Set();
            Down_d.RemoveAt(Down_list_Selete.ID);
            Serialization(@"./config/ImportDownLoadInfo", DownInfo);
        }

        public void Go_DowndLoad(object p)
        {
            if (Down_list_Selete == null) return;
            if (DownInfo[Down_list_Selete.ID].State == "下载") return;
            DownLoad d = new DownLoad();
            string name = DownInfo[Down_list_Selete.ID].filename;
            Information inf = d.RSerialization(name);
            if (inf.filename == null)
            {
                MessageBox.Show("文件不存在！");
                int i = Down_list_Selete.ID;
                DownInfo.RemoveAt(Down_list_Selete.ID);
                for (; i < DownInfo.Count; i++)
                {
                    DownInfo[i].ID = i;
                }
                Serialization(@"./config/ImportDownLoadInfo", DownInfo);
                return;
            }
            d.ContinuousDownLoad(inf);
            DownInfo[Down_list_Selete.ID].ID = Down_d.Count;
            DownInfo[Down_list_Selete.ID].State = "下载";
            Down_d.Add(d);
            for (int i = 0; i < Down_d.Count; i++)
            {
                Down_d[i].Maxseepd = Limit_DownLoad_Speed;
                Down_d[i].Sum = Down_d.Count;
            }
            Task<int> task = _client.DownloadRequestAsync(Down_list_Selete.Hash, d.Port);
            task.ContinueWith(T =>
            {

            });
            Serialization(@"./config/ImportDownLoadInfo", DownInfo);
        }

        public void Detelet_DownListViewInfo(object p)
        {
            if (Down_list_Selete == null) return;
            GarbageInfo.Add(Down_list_Selete);
            if (Down_list_Selete.State == "下载")
            {
                Down_d[Down_list_Selete.ID].Stop_DownLoad = true;
                Down_d[Down_list_Selete.ID].AllDone.Set();
                Down_d.RemoveAt(Down_list_Selete.ID);
            }
            int i = Down_list_Selete.ID;
            DownInfo.RemoveAt(Down_list_Selete.ID);
            for (; i < DownInfo.Count; i++)
            {
                DownInfo[i].ID = i;
            }
            Serialization(@"./config/GarbageInfo", GarbageInfo);
            Serialization(@"./config/ImportDownLoadInfo", DownInfo);
        }

        public void Detelet_HistoryListViewInfo(object p)
        {
            if (Down_list_Selete == null) return;
            HistoricalRecords.RemoveAt(Down_list_Selete.ID);
            Serialization(@"./config/HistoricalRecords", HistoricalRecords);
        }

        public void Open_File(object p)
        {
            if (Down_list_Selete == null) return;
            FileInfo f = null;
            try
            {
                f = new FileInfo(Down_list_Selete.filename);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Data.ToString());
                return;
            }
            try
            {
                if (f.Attributes == FileAttributes.Directory) System.Diagnostics.Process.Start("explorer", Down_list_Selete.filename);
                else System.Diagnostics.Process.Start(Down_list_Selete.filename);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Data.ToString()); ;
            }

        }

        public void Detelet_File(object p)
        {
            if (Down_list_Selete == null) return;
            GarbageInfo.RemoveAt(Down_list_Selete.ID);
            DeleteDownLoad(Down_list_Selete);
            Serialization(@"./config/GarbageInfo", GarbageInfo);
        }

        private void DeleteDownLoad(DownListInfo d)
        {
            if (d == null) return;
            if (d.type == "文件夹")
            {
                if (System.IO.Directory.Exists(d.filename))
                {
                    Delete_Directory(d.filename);
                }
            }
            else
            {
                if (System.IO.File.Exists(d.filename))
                {
                    FileInfo f = new FileInfo(d.filename);
                    f.Delete();
                }
            }
            if (System.IO.File.Exists(d.filename + ".dat"))
            {
                FileInfo f1 = new FileInfo(d.filename + ".dat");
                f1.Delete();
            }
            for (int i = d.ID; i < GarbageInfo.Count; i++)
            {
                GarbageInfo[i].ID = i;
            }
        }

        private void Delete_Directory(string name)
        {
            DirectoryInfo di = new DirectoryInfo(name);
            DirectoryInfo[] diA = di.GetDirectories();
            FileInfo[] files = di.GetFiles();
            foreach (var item in files)
            {
                item.Delete();
            }
            foreach (var item in diA)
            {
                Delete_Directory(item.FullName);
                try
                {
                    item.Delete();
                }
                catch (Exception)
                {
                    ;
                }
            }
            di.Delete();
        }

        public void Reduction_GarbageInfo(object p)
        {
            if (Down_list_Selete == null) return;
            if (Down_list_Selete.State == "已完成") HistoricalRecords.Add(Down_list_Selete);
            else
            {
                Down_list_Selete.State = "暂停";
                DownInfo.Add(Down_list_Selete);
            }
            GarbageInfo.RemoveAt(Down_list_Selete.ID);
        }

        public void Talk(object p)
        {
            talkwin talk = new talkwin();
            talk.Owner = Application.Current.MainWindow;
            ((talkwinMVVMcs)talk.DataContext).IDName = UserName;
            if (SeleteManInfo.UserName != null) ((talkwinMVVMcs)talk.DataContext).OtherName = SeleteManInfo.UserName;
            ((talkwinMVVMcs)talk.DataContext).Call_Creat_Talk += CallTalk;
            ((talkwinMVVMcs)talk.DataContext).Control_DownLoad += CreatDowndLoad_Talk;
            talk.Show();
        }

        private void CallTalk(object p, Call_Talk e)
        {
            _client.RequestConversation(e.IDName, e.Port);
        }

        private void Refresh(object source, UpDataEvent e)
        {
            if (d_m == 10)
            {
                d_m = 0;
                float D_s = 0;
                downInfo[e.Id].Speed = GetSpeed(e.speed);
                foreach (var item in Down_d)
                {
                    D_s += item.Speed;
                }
                string sp = "下载：";
                sp += GetSpeed(D_s);
                t_DownLoadSpeed = sp;
            }
            downInfo[e.Id].Progress_Percentage = e.Percentage.ToString("F2") + "%";
            if (downInfo[e.Id].Progress_Percentage.CompareTo("100.00%") == 0) CheckHash(downInfo[e.Id].ID);
            d_m++;
        }

        public void Refresh_Speed(object source, EventArgs e)
        {
            if (u_m == 10)
            {
                u_m = 0;
                float U_s = 0;
                foreach (var item in UpLoad)
                {
                    U_s += item.Speed;
                }
                string sp = "上传：";
                sp += GetSpeed(U_s / 1024);
                t_UpLoadSpeed = sp;
            }
            u_m++;
        }

        public void Delete_UpLoadList(object source, DeleteUpLoadList e)
        {
            t_UpLoadSpeed = "上传：0 KB/S";
            UpLoad.RemoveAt(e.Id);
            for (int i = 0; i < UpLoad.Count; i++)
            {
                UpLoad[i].MaxSeepd = Limit_Uplaod_Speed;
                UpLoad[i].sum = UpLoad.Count;
            }
        }

        public void Delete_TalkList(object source, DeleteUpLoadList e)
        {
            Talk_List.RemoveAt(e.Id);
        }

        private string GetSpeed(float speed)
        {
            string Speed = "";
            if (speed > 1024)
            {
                Speed = ((int)(speed / 1024)).ToString() + @"M/S";
            }
            else if (speed > 0) Speed = ((int)speed).ToString() + @"KB/S";
            else if (speed < 0) Speed = ((int)(speed * 1024)).ToString() + @"B/S";
            return Speed;
        }

        private void ImportInfo()
        {
            RSerialization(@"./config/ImportDownLoadInfo", DownInfo);
            RSerialization(@"./config/HistoricalRecords", HistoricalRecords);
            RSerialization(@"./config/GarbageInfo", GarbageInfo);
        }

        private void Serialization(string name, ObservableCollection<DownListInfo> d)
        {
            FileStream fileStream = new FileStream(name + ".dat", FileMode.Create);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(fileStream, d);
            fileStream.Close();
        }

        public void RSerialization(string name, ObservableCollection<DownListInfo> d)
        {
            FileStream fileStream = null;
            try
            {
                if (!File.Exists(name + ".dat"))
                {
                    return;
                }

                fileStream = new FileStream(name + ".dat", FileMode.Open, FileAccess.Read, FileShare.Read);

                BinaryFormatter b = new BinaryFormatter();
                ObservableCollection<DownListInfo> Pb = b.Deserialize(fileStream) as ObservableCollection<DownListInfo>;
                if (name == @"./config/ImportDownLoadInfo")
                {
                    foreach (var item in Pb)
                    {
                        item.State = "暂停";
                        d.Add(item);
                    }
                }
                else
                {
                    foreach (var item in Pb)
                    {
                        d.Add(item);
                    }
                }
                fileStream.Close();
            }
            catch (Exception)
            {
                return;
            }
        }

        private void CheckHash(int ID)
        {
            if (Down_d[ID].CheckHash(Down_d[ID].Lls.filename))
            {
                Information info = new Information();
                Down_d[ID].Lls.PointInfo.Hash = true;
                DownInfo[ID].State = "下载";
                Down_d[ID].ContinuousDownLoad(Down_d[ID].Lls.PointInfo);
                Task<int> task = _client.DownloadRequestAsync(Down_d[ID].T_Hash, Down_d[ID].Port);
                task.ContinueWith(T =>
                {

                });
            }
            else
            {
                ThreadPool.QueueUserWorkItem(p =>
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                    {
                        downInfo[ID].State = "已完成";
                        HistoricalRecords.Add(downInfo[ID]);
                        Down_d.RemoveAt(ID);
                        downInfo.RemoveAt(ID);
                        Serialization(@"./config/ImportDownLoadInfo", DownInfo);
                        Serialization(@"./config/HistoricalRecords", HistoricalRecords);
                        if (Down_d.Count == 0)
                        {
                            string sp = "下载：0 KB/S";
                            t_DownLoadSpeed = sp;
                        }
                    });
                });
            }
        }

        public void Close_Socket()
        {
            for (int i = 0; i < DownInfo.Count; i++)
            {
                DownInfo[i].State = "暂停";
            }
            Serialization(@"./config/ImportDownLoadInfo", DownInfo);
            Serialization(@"./config/HistoricalRecords", HistoricalRecords);
            Serialization(@"./config/GarbageInfo", GarbageInfo);
            Down_d.Clear();
            Talk_List.Clear();
            UpLoad.Clear();
        }
    }
}