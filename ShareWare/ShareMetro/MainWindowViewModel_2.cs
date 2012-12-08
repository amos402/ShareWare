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
            time.Elapsed += new System.Timers.ElapsedEventHandler(Refresh);//到达时间的时候执行事件；
            time.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            time.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        }
        System.Timers.Timer time = new System.Timers.Timer(100);//实例化Timer类，设置间隔时间为10000毫秒；

        private string directory = @"asd\";
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
                if (this.PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Directory"));
            }
        }


        public FileInfoDataList SeleteInfo { get; set; }
        public DownListInfo Down_list_Selete { get; set; }


        Thread t = null;
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
            get { return historicalRecords; }
            set { historicalRecords = value; }
        }

        public ICommand GoCommand
        {
            get
            {
                return new ICom(LoadItems);
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

        public ICommand Detelet_DowndLoadCommand
        {
            get
            {
                return new ICom(Detelet_DowndLoad);
            }
        }


        public void LoadItems(object p)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)delegate
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

                    D = Directory;
                }
                if (ShowSharefile.NewSock == null)
                {
                    ShowSharefile.CreatSocket();
                }
                Thread.Sleep(500);
                if (ShowSharefile.NewSock == null) return;

                ParseDirectoryThread(D);
            });
        }

        private void ParseDirectoryThread(object p)//使用后台线程获取文件信息  
        {
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
                t = new Thread(new ParameterizedThreadStart(ParseDirectoryRecursive));//在后台线程中调用ParseDirectoryRecursive方法  
                t.IsBackground = true;  //指定为后台线程  
                t.Priority = ThreadPriority.BelowNormal;//指定线程优先级别  
                t.Start(_directory); //为线程方法传入文件夹路径  
            }
            catch (Exception)  //如果产生异常  
            {   //调用自定义的异常信息窗口  
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
            Thread t1 = new Thread(CreatDowndLoadThread);//在后台线程中调用ParseDirectoryRecursive方法  
            t1.IsBackground = true;  //指定为后台线程  
            t1.Priority = ThreadPriority.BelowNormal;//指定线程优先级别  
            t1.Start(); //为线程方法传入文件夹路径              
        }

        private void CreatDowndLoadThread()
        {
            if (SeleteInfo == null) return;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)delegate
            {
                DownLoad d = new DownLoad();
                LoadInfo l = new LoadInfo();
                //l.name = @"DriverGenius2012";
                //l.type = "文件夹";
                l.name = SeleteInfo.Name;
              //  l.size = (long)SeleteInfo.Size;
                l.type = SeleteInfo.Type;
                DownListInfo sm = d.CreatDownLoad(l);
                if (sm == null) return;
                sm.ID = Down_d.Count;
                sm.State = "下载";
                DownInfo.Add(sm);
                Down_d.Add(d);
                Task<int> task =  _client.DownloadRequestAsync(SeleteInfo.Hash, d.Port);
                task.ContinueWith(T =>
                    {

                    });

                Serialization(@".\ImportDownLoadInfo",DownInfo);
            });
        }

        private void Stop_DowndLoad(object p)
        {
            if (Down_list_Selete == null) return;
            DownInfo[Down_list_Selete.ID].State = "暂停";
            Down_d[Down_list_Selete.ID].Stop_DownLoad = true;
            Down_d[Down_list_Selete.ID].AllDone.Set();
            Down_d.RemoveAt(Down_list_Selete.ID);
        }

        private void Go_DowndLoad(object p)
        {
            if (Down_list_Selete == null) return;
            DownLoad d = new DownLoad();
            string name = DownInfo[Down_list_Selete.ID].filename;
            d.ContinuousDownLoad(d.RSerialization(name));            
            DownInfo[Down_list_Selete.ID].ID = Down_d.Count;
            DownInfo[Down_list_Selete.ID].State = "下载";
            Down_d.Add(d);
        }

        private void Detelet_DowndLoad(object p)
        {
            if (Down_list_Selete == null) return;
            GarbageInfo.Add(Down_list_Selete);
            Down_d[Down_list_Selete.ID].Stop_DownLoad = true;
            Down_d[Down_list_Selete.ID].AllDone.Set();
           // Down_d[Down_list_Selete.ID].DeleteDownLoad();
            Down_d.RemoveAt(Down_list_Selete.ID);
            DownInfo.RemoveAt(Down_list_Selete.ID);
            Serialization(@"./GarbageInfo", GarbageInfo);
        }

        private void Refresh(object source, System.Timers.ElapsedEventArgs e)
        {
            if (downInfo.Count == 0) return;
            Thread t1 = new Thread(Update);//在后台线程中调用ParseDirectoryRecursive方法  
            t1.IsBackground = true;  //指定为后台线程  
            t1.Priority = ThreadPriority.BelowNormal;//指定线程优先级别  
            t1.Start(); //为线程方法传入文件夹路径           
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
                            HistoricalRecords.Add(downInfo[i]);
                            Down_d.RemoveAt(i);
                            downInfo.RemoveAt(i);
                           // Serialization(@".\HistoricalRecords", HistoricalRecords);
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
            RSerialization(@"./ImportDownLoadInfo",DownInfo);
            RSerialization(@"./HistoricalRecords", HistoricalRecords);
            RSerialization(@"./GarbageInfo", GarbageInfo);
        } 

        private void Serialization(string name ,ObservableCollection<DownListInfo> d)
        {
            FileStream fileStream = new FileStream(name + ".dat", FileMode.Create);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(fileStream, d);
            fileStream.Close();
        }

        public void  RSerialization(string name,ObservableCollection<DownListInfo> d)
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
            d = b.Deserialize(fileStream) as ObservableCollection<DownListInfo>;
            fileStream.Close();
        }
    }
}