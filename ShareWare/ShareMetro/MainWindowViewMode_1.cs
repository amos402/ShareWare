using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using Microsoft.Practices.Prism.Commands;
using ShareMetro.ServiceReference;
using ShareWare.ShareFile;
using System.ServiceModel;
using System.Windows.Input;
using System.Management;
using System.Threading.Tasks;
using ShareWare;
using System;
using System.Threading;
using System.Timers;
using System.Windows;
using Socket_Library;

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private Dispatcher _dispatcher;
        public MainWindowViewModel(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            LoginTab = new LoginPage() { ButtonVisiable = true };
            MainTab = new MainPage();
            LoginCmd = new DelegateCommand<object>(OnLogin, arg => true);
            SearchCmd = new DelegateCommand<object>(OnSearch, arg => true);
            _callBack = new CallBack(MainTab);
            _client = new ShareServiceClient(new InstanceContext(_callBack));
            _client.InnerChannel.Closing += InnerChannel_Closing;
            ErrorOccur += OnErrorOccur;
            LoginSuccess += GetShareInfo;
            LoginSuccess += ClientTick;
            LoginFailed += OnLoginFailed;

        }

        void OnErrorOccur(object sender, ModelEvent e)
        {
            switch (e.Type)
            {
                case ModelEventType.Exception:
                    MessageBox.Show(e.ModelException.Message);
                    break;

                case ModelEventType.Meesage:
                    _loginTab.FailedMessage = e.FailedMessage;
                    break;

                default:
                    break;
            }
        }



        void OnLoginFailed(object sender, ModelEvent e)
        {

        }


        private string _shareInfoPath = @"config\known.met";
        private static Mutex searchMut = new Mutex();
        private static int _tickTime = 60000;
        private static System.Timers.Timer _tickTimer;

        public event EventHandler<ModelEvent> LoginSuccess;
        public event EventHandler<ModelEvent> LoginFailed;
        public event EventHandler ServerTimeout;
        public event EventHandler<ModelEvent> ErrorOccur;

        private CallBack _callBack;
        private ShareServiceClient _client;
        internal ShareServiceClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        private int _id;

        public int Index { get; set; }
        public bool HideMenu { get; set; }

        private ShareFiles _sh;

        private ObservableCollection<FileInfoData> _fileList = new ObservableCollection<FileInfoData>();
        public ObservableCollection<FileInfoData> FileList
        {
            get { return _fileList; }
            set
            {
                _fileList = value;
                OnPropertyChanged("FileList");
            }
        }

        private ObservableCollection<FileInfoDataList> _fileItemInfo = new ObservableCollection<FileInfoDataList>();
        public ObservableCollection<FileInfoDataList> FileItemInfo
        {
            get { return _fileItemInfo; }
            set
            {
                _fileItemInfo = value;
                OnPropertyChanged("FileItemInfo");
            }
        }

        private bool _sysShareSwitch;

        public bool SysShareSwitch
        {
            get { return _sysShareSwitch; }
            set
            {
                _sysShareSwitch = value;
                if (_sh != null)
                {
                    if (_sysShareSwitch)
                    {

                        _sh.AddSystemSharePath();

                    }
                    else
                    {
                        foreach (var item in _sh.SystemShareNameList)
                        {
                            _sh.RemoveSharePath(item);
                        }

                    }
                }
            }
        }

        public Dictionary<string, string> SharePath
        {
            get
            {
                if (_sh != null)
                {
                    return _sh.SharePath;
                }
                return null;
            }
            set
            {
                _sh.SharePath = value;
                OnPropertyChanged("SharePath");
            }
        }

        #region Commands
        public ICommand LoginCmd { get; set; }
        public ICommand SearchCmd { get; set; }
        public ICommand AddSearchPathCmd { get; set; }

        #endregion

        private LoginPage _loginTab;
        public LoginPage LoginTab
        {
            get { return _loginTab; }
            set
            {
                _loginTab = value;
                OnPropertyChanged("LoginTab");
            }
        }

        private MainPage _mainTab;
        public MainPage MainTab
        {
            get { return _mainTab; }
            set
            {
                _mainTab = value;
                OnPropertyChanged("MainTab");
            }
        }


        private void OnLogin(object obj)
        {
            _loginTab.IsBusy = true;
            _loginTab.LoginAble = false;
            _loginTab.FailedMessage = string.Empty;
            try
            {
                Task<int> task = _client.LoginAsync(_loginTab.UserName, _loginTab.Password, GetFirstMac());

                task.ContinueWith(T =>
                {
                    AggregateException ex = T.Exception;
                    if (ex != null)
                    {
                        if (ex.InnerExceptions.Count > 0)
                        {
                            ErrorOccur(this, new ModelEvent(ModelEventType.Meesage) { FailedMessage = "连接服务器出错" });
                            _loginTab.IsBusy = false;
                            _loginTab.LoginAble = true;
                            return;
                        }
                    }
                    if (T.Result < 0)
                    {
                        ErrorOccur(this, new ModelEvent(ModelEventType.Meesage) { FailedMessage = "用户名或密码错误" });
                        _loginTab.IsBusy = false;
                        _loginTab.LoginAble = true;
                        return;
                    }

                    _id = T.Result;
                    HideMenu = true;
                    Index = 1;
                    if (LoginSuccess != null)
                    {
                        ExecuteEventAsync(this, null, LoginSuccess, null);
                    }
                    _loginTab.IsBusy = false;
                    _loginTab.LoginAble = true;

                });
            }
            catch (Exception e)
            {

                if (ErrorOccur != null)
                {
                    ErrorOccur(this, new ModelEvent(ModelEventType.Exception) { ModelException = e });
                }
            }

        }

        private void OnSearch(object obj)
        {
            _mainTab.IsBusy = true;
            try
            {
                string[] sp = MainTab.FileName.Split(' ');
                List<string> nameList = new List<string>();
                nameList.AddRange(sp);
                Task<List<FileInfoData>> task = _client.SearchFileAsync(nameList);
                FileList.Clear();
                task.ContinueWith(T =>
                    {
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            searchMut.WaitOne();
                            _mainTab.IsBusy = false;
                            if (T.Result != null)
                            {
                                foreach (var item in T.Result)
                                {
                                    FileInfoDataList f = new FileInfoDataList(item);
                                    try
                                    {
                                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                            (ThreadStart)delegate
                                        {
                                            if (item != null)
                                            {
                                                FileItemInfo.Add(f);
                                            }
                                            Thread.Sleep(30);
                                        });
                                    }
                                    catch (Exception)
                                    {

                                        //throw;
                                    }
                                }
                            }
                            searchMut.ReleaseMutex();
                        });
                    });
            }
            catch (Exception e)
            {
                if (ErrorOccur != null)
                {
                    ErrorOccur(this, new ModelEvent(ModelEventType.Exception) { ModelException = e });
                }
            }
        }

        private void OnAddSharePath(object obj)
        {

        }

        private void InnerChannel_Closing(object sender, EventArgs e)
        {
            Console.WriteLine(sender);
        }

        private void ExecuteEventAsync(object sender, ModelEvent e, EventHandler<ModelEvent> handler, AsyncCallback callBack)
        {
            foreach (EventHandler<ModelEvent> item in handler.GetInvocationList())
            {
                item.BeginInvoke(sender, e, callBack, null);
            }
        }


        private void ClientTick(object sender, EventArgs e)
        {
            _tickTimer = new System.Timers.Timer(_tickTime);
            _tickTimer.Interval = _tickTime;
            _tickTimer.Elapsed += OnClientTick;
            _tickTimer.Start();

        }
        private void OnClientTick(object source, ElapsedEventArgs e)
        {
            try
            {
                _client.TickTack();
                Console.WriteLine(_client.InnerChannel.State);
                Console.WriteLine(_client.InnerDuplexChannel.State);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void GetShareInfo(object sender, ModelEvent e)
        {
            _sh = ShareFiles.Deserialize(_shareInfoPath);
            _sh.AddSharePath("RamDisk", @"D:\eclipse");
            //_sh.AddSharePath("damn", @"D:\TDDOWNLOAD");
            Thread t = _sh.ListFile();
            t.Join();
            _client.UploadShareInfo(_sh.FileList);
            _client.RemoveOldFile(_sh.RemoveList);
            // _sh.SetUploaded(_sh.FileList);

            SaveShareInfo();
        }

        private void SaveShareInfo()
        {
            if (_sh != null)
            {
                _sh.Serialize(_shareInfoPath);
            }
        }

        public static string GetFirstMac()
        {
            string mac = null;
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                if (mo["IPEnabled"].ToString() == "True")
                {
                    mac = mo["MacAddress"].ToString();
                    break;
                }
            }
            return (mac);
        }


    }
}