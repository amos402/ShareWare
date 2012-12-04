using Microsoft.Practices.Prism.Commands;
using ShareMetro.ServiceReference;
using ShareWare.ShareFile;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

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


            LoginAble = true;
            UserName = string.Empty;
            Password = string.Empty;

            OnlineUser = new ObservableCollection<string>();
            IsBusy_Main = false;

            LoginCmd = new DelegateCommand<object>(OnLogin, arg => true);
            SearchCmd = new DelegateCommand<object>(OnSearch, arg => true);
            UploadImageCmd = new DelegateCommand<object>(OnUploadImage, arg => true);
            DownLoadCmd = new DelegateCommand<object>(OnDownLoad, arg => true);

            _callBack = new CallBack(this);
            _client = new ShareServiceClient(new InstanceContext(_callBack));

            _client.InnerChannel.Closing += InnerChannel_Closing;
            ErrorOccur += OnErrorOccur;
            LoginSuccess += GetShareInfo;
            LoginSuccess += ClientTick;
            LoginSuccess += GetClientInfo;
            LoginFailed += OnLoginFailed;

            _defaultImage = _imagePath + "default.jpg";
            ImageSource = _defaultImage;
        }

        private string _defaultImage;

        private string _imageSource;
        public string ImageSource
        {
            get { return _imageSource; }
            set
            {
                if (File.Exists(value))
                {
                    _imageSource = value;
                }
                else
                {
                    _imageSource = _defaultImage;
                }

            }
        }

        private string _shareInfoPath = @"config\known.met";

        private ShareFiles _sh;

        public ShareFiles Sh
        {
            get { return _sh; }
            set { _sh = value; }
        }

        private int _id;

        public string _imagePath = @"C:\Users\Amos\Documents\Visual Studio 2012\Projects\ShareWare\ShareMetro\bin\Debug\image\";

        public int Index { get; set; }
        public bool HideMenu { get; set; }

        private static Mutex searchMut = new Mutex();
        private static int _tickTime = 60000;
        private static System.Timers.Timer _tickTimer;

        public event EventHandler<ModelEvent> LoginSuccess;
        public event EventHandler<ModelEvent> LoginFailed;
        public event EventHandler ServerTimeout;
        public event EventHandler<ModelEvent> ErrorOccur;

        private CallBack _callBack;
        private ShareServiceClient _client;

        void OnErrorOccur(object sender, ModelEvent e)
        {
            switch (e.Type)
            {
                case ModelEventType.Exception:
                    MessageBox.Show(e.ModelException.Message);
                    break;

                case ModelEventType.Meesage:
                    FailedMessage = e.FailedMessage;
                    break;

                default:
                    break;
            }
        }

        void OnLoginFailed(object sender, ModelEvent e)
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
            _sh.AddSharePath("RamDisk", @"R:\");
            //_sh.AddSharePath("damn", @"D:\TDDOWNLOAD");
            Thread t = _sh.ListFile();
            t.Join();
            _client.UploadShareInfo(_sh.FileList);
            _client.RemoveOldFile(_sh.RemoveList);
            // _sh.SetUploaded(_sh.FileList);

            SaveShareInfo();
        }

        private void GetClientInfo(object sender, ModelEvent e)
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash((Encoding.UTF8.GetBytes(UserName)));
            StringBuilder hash = new StringBuilder();
            foreach (var item in data)
            {
                hash.Append(item.ToString("x2"));
            }
            md5.Clear();
            ImageSource = _imagePath + hash.ToString() + ".jpeg";

        }

        private string GetImageSource()
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash((Encoding.UTF8.GetBytes(UserName)));
            StringBuilder hash = new StringBuilder();
            foreach (var item in data)
            {
                hash.Append(item.ToString("x2"));
            }
            md5.Clear();
            return _imagePath + hash.ToString() + ".jpeg";
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