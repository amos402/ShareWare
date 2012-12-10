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

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {

        private void Inti()
        {
            time.Elapsed += new System.Timers.ElapsedEventHandler(Refresh);//����ʱ���ʱ��ִ���¼���
            time.AutoReset = true;//������ִ��һ�Σ�false������һֱִ��(true)��
            time.Enabled = true;//�Ƿ�ִ��System.Timers.Timer.Elapsed�¼���
            ImportInfo();
            ShowSharefile.CallShow += CallShowListView;
        }
        System.Timers.Timer time = new System.Timers.Timer(100);//ʵ����Timer�࣬���ü��ʱ��Ϊ10000���룻

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
                if (this.PropertyChanged != null)/////////////////////////////////////////////////////////
                    PropertyChanged(this, new PropertyChangedEventArgs("Directory"));
            }
        }


        public FileInfoDataList SeleteInfo { get; set; }
        public DownListInfo Down_list_Selete { get; set; }


        Thread t = null;
        string LoadIDName = "";
        ShowShareFileInfo ShowSharefile = new ShowShareFileInfo();
        private List<DownLoad> Down_d = new List<DownLoad>();

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
                    _client.RequestOpenShareFolder((string)p, LoadItems(p));
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
                return new ICom(CreatDowndLoad);
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
            _client.RequestOpenShareFolder((string)p, LoadItems(p));           
        }

        public int LoadItems(object p)
        {
            if (t != null)
            {
                if (t.IsAlive) t.Abort();
            }
            string D = "";
            string a = ((string)p);
            if (a != null && a is string)
            {
                D = directory + a + "\\";
                Directory = Directory + a + "\\";
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
            if (ShowSharefile.NewSock == null)
            {
                ShowSharefile.CreatSocket(D);
            }
            return ShowSharefile.Port;
        }

        private void CallShowListView(object sender, CallShowListView e)
        {
            ParseDirectoryThread(e.Directory);
        }

        private void ParseDirectoryThread(object p)//ʹ�ú�̨�̻߳�ȡ�ļ���Ϣ  
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
                FileItemInfo.Clear();
                t = new Thread(new ParameterizedThreadStart(ParseDirectoryRecursive));//�ں�̨�߳��е���ParseDirectoryRecursive����  
                t.IsBackground = true;  //ָ��Ϊ��̨�߳�  
                t.Priority = ThreadPriority.BelowNormal;//ָ���߳����ȼ���  
                t.Start(_directory); //Ϊ�̷߳��������ļ���·��  
            }
            catch (Exception)  //��������쳣  
            {   //�����Զ�����쳣��Ϣ����  
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

        private void CreatDowndLoad(object p)
        {
            Thread t1 = new Thread(new ParameterizedThreadStart(CreatDowndLoadThread));//�ں�̨�߳��е���ParseDirectoryRecursive����  
            t1.IsBackground = true;  //ָ��Ϊ��̨�߳�  
            t1.Priority = ThreadPriority.BelowNormal;//ָ���߳����ȼ���  
            t1.Start(SeleteInfo); //Ϊ�̷߳��������ļ���·��              
        }

        private void CreatDowndLoad_Talk(object sender, Control_Down e)
        {
            Thread t1 = new Thread(new ParameterizedThreadStart(CreatDowndLoadThread));//�ں�̨�߳��е���ParseDirectoryRecursive����  
            t1.IsBackground = true;  //ָ��Ϊ��̨�߳�  
            t1.Priority = ThreadPriority.BelowNormal;//ָ���߳����ȼ���  
            t1.Start(e.SeleteInfo); //Ϊ�̷߳��������ļ���·��              
        }

        private void CreatDowndLoadThread(object p)
        {
            FileInfoDataList SeleteInfo1 = (FileInfoDataList)p;
            if (SeleteInfo1 == null) return;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)delegate
            {
                DownLoad d = new DownLoad();
                LoadInfo l = new LoadInfo();
                //l.name = @"DriverGenius2012";
                //l.type = "�ļ���";
                l.name = SeleteInfo1.Name;
                if (SeleteInfo1.Size == null) l.size = 0;
                else l.size = (long)SeleteInfo1.Size;
                l.type = SeleteInfo1.Type;
                DownListInfo sm = d.CreatDownLoad(l);
                if (sm == null) return;
                sm.ID = Down_d.Count;
                sm.State = "����";
                sm.Hash = SeleteInfo.Hash;
                DownInfo.Add(sm);
                Down_d.Add(d);
                Task<int> task = _client.DownloadRequestAsync(SeleteInfo.Hash, d.Port);
                task.ContinueWith(T =>
                    {

                    });

                Serialization(@".\ImportDownLoadInfo", DownInfo);
            });
        }

        private void Stop_DowndLoad(object p)
        {
            if (Down_list_Selete == null) return;
            if (DownInfo[Down_list_Selete.ID].State == "��ͣ") return;
            DownInfo[Down_list_Selete.ID].State = "��ͣ";
            Down_d[Down_list_Selete.ID].Stop_DownLoad = true;
            Down_d[Down_list_Selete.ID].AllDone.Set();
            Down_d.RemoveAt(Down_list_Selete.ID);
            Serialization(@".\ImportDownLoadInfo", DownInfo);
        }

        private void Go_DowndLoad(object p)
        {
            if (Down_list_Selete == null) return;
            if (DownInfo[Down_list_Selete.ID].State == "����") return;
            DownLoad d = new DownLoad();
            string name = DownInfo[Down_list_Selete.ID].filename;
            Information inf = d.RSerialization(name);
            if (inf.filename == null)
            {
                MessageBox.Show("�ļ������ڣ�");
                DownInfo.RemoveAt(Down_list_Selete.ID);
                Serialization(@".\ImportDownLoadInfo", DownInfo);
                return;
            }
            d.ContinuousDownLoad(inf);
            DownInfo[Down_list_Selete.ID].ID = Down_d.Count;
            DownInfo[Down_list_Selete.ID].State = "����";
            Down_d.Add(d);
            Task<int> task = _client.DownloadRequestAsync(Down_list_Selete.Hash, d.Port);
            task.ContinueWith(T =>
            {

            });
            Serialization(@".\ImportDownLoadInfo", DownInfo);
        }

        private void Detelet_DownListViewInfo(object p)
        {
            if (Down_list_Selete == null) return;
            GarbageInfo.Add(Down_list_Selete);
            if (Down_list_Selete.State == "����")
            {
                Down_d[Down_list_Selete.ID].Stop_DownLoad = true;
                Down_d[Down_list_Selete.ID].AllDone.Set();
                Down_d.RemoveAt(Down_list_Selete.ID);
            }
            DownInfo.RemoveAt(Down_list_Selete.ID);
            Serialization(@".\GarbageInfo", GarbageInfo);
            Serialization(@".\ImportDownLoadInfo", DownInfo);
        }

        private void Detelet_HistoryListViewInfo(object p)
        {
            if (Down_list_Selete == null) return;
            HistoricalRecords.RemoveAt(Down_list_Selete.ID);
            Serialization(@".\HistoricalRecords", HistoricalRecords);
        }

        private void Open_File(object p)
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

        private void Detelet_File(object p)
        {
            if (Down_list_Selete == null) return;
            DeleteDownLoad(Down_list_Selete);
            GarbageInfo.RemoveAt(Down_list_Selete.ID);
            Serialization(@"./GarbageInfo", GarbageInfo);
        }

        private void DeleteDownLoad(DownListInfo d)
        {
            if (d.type == "�ļ���")
            {
                Delete_Directory(d.filename);
            }
            else
            {
                FileInfo f = new FileInfo(d.filename);
                f.Delete();
            }
            FileInfo f1 = new FileInfo(d.filename + ".dat");
            f1.Delete();
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

        private void Reduction_GarbageInfo(object p)
        {
            if (Down_list_Selete == null) return;
            GarbageInfo.RemoveAt(Down_list_Selete.ID);
            if (Down_list_Selete.State == "�����") HistoricalRecords.Add(Down_list_Selete);
            else
            {
                Down_list_Selete.State = "��ͣ";
                DownInfo.Add(Down_list_Selete);
            }
        }

        private void Talk(object p)
        {
            talkwin talk = new talkwin();
            talk.Owner = Application.Current.MainWindow;
            ((talkwinMVVMcs)talk.DataContext).Control_DownLoad += CreatDowndLoad_Talk;
            talk.ShowDialog();
           /// _client.RequestConversation(string.Empty, 0);
        }

        private void Refresh(object source, System.Timers.ElapsedEventArgs e)
        {
            if (downInfo.Count == 0) return;
            Thread t1 = new Thread(Update);//�ں�̨�߳��е���ParseDirectoryRecursive����  
            t1.IsBackground = true;  //ָ��Ϊ��̨�߳�  
            t1.Priority = ThreadPriority.BelowNormal;//ָ���߳����ȼ���  
            t1.Start(); //Ϊ�̷߳��������ļ���·��           
        }

        private void Update()
        {
            for (int i = 0; i < downInfo.Count; i++)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                    {
                        if (downInfo.Count == 0 || Down_d.Count == 0) return;
                        if (downInfo[i].Size == 0)
                        {
                            downInfo[i].Progress_Percentage = 0;
                            downInfo[i].Size = Down_d[downInfo[i].ID].Lls.t_size;
                        }
                        else downInfo[i].Progress_Percentage = (float)Down_d[downInfo[i].ID].Lls.DownLoadSize / (float)downInfo[i].Size * 100;
                        if (downInfo[i].Progress_Percentage >= 100)
                        {
                            CheckHash(i);
                        }
                    });
                }
                catch (Exception)
                {
                    ;
                }
            }
        }

        private void ImportInfo()
        {
            RSerialization(@"./ImportDownLoadInfo", DownInfo);
            RSerialization(@"./HistoricalRecords", HistoricalRecords);
            RSerialization(@"./GarbageInfo", GarbageInfo);
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
                fileStream = new FileStream(name + ".dat", FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                return;
            }
            BinaryFormatter b = new BinaryFormatter();
            ObservableCollection<DownListInfo> Pb = b.Deserialize(fileStream) as ObservableCollection<DownListInfo>;
            if (name == @"./ImportDownLoadInfo")
            {
                foreach (var item in Pb)
                {
                    item.State = "��ͣ";
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

        private void CheckHash(int ID)
        {
            if (Down_d[ID].CheckHash(Down_d[ID].Lls.filename))
            {
                Information info = new Information();
                Down_d[ID].Lls.PointInfo.Hash = true;
                DownInfo[ID].State = "����";
                Down_d[ID].ContinuousDownLoad(Down_d[ID].Lls.PointInfo);
                Task<int> task = _client.DownloadRequestAsync(Down_d[ID].T_Hash, Down_d[ID].Port);
                task.ContinueWith(T =>
                    {

                    });
            }
            else
            {
                HistoricalRecords.Add(downInfo[ID]);
                Down_d.RemoveAt(ID);
                downInfo.RemoveAt(ID);
                Serialization(@".\HistoricalRecords", HistoricalRecords);
            }
        }
    }
}