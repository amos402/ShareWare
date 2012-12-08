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
using System.Windows.Media.Imaging;
using System.Windows.Resources;
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

            OnlineUser = new ObservableCollection<OnlineUserData>();
            IsBusy_Main = false;

            LoginCmd = new DelegateCommand<object>(OnLogin, arg => true);
            RegisterCmd = new DelegateCommand<object>(OnRegister, arg => true);
            SearchCmd = new DelegateCommand<object>(OnSearch, arg => true);
            UploadImageCmd = new DelegateCommand<object>(OnUploadImage, arg => true);
            DownLoadCmd = new DelegateCommand<object>(OnDownLoad, arg => true);
            HeadIConCmd = new DelegateCommand<object>(OnClickHeadIcon, arg => true);

            _callBack = new CallBack(this);
            _client = new ShareServiceClient(new InstanceContext(_callBack));

            _client.InnerChannel.Closing += InnerChannel_Closing;
            ErrorOccur += OnErrorOccur;
            LoginSuccess += GetShareInfo;
            LoginSuccess += ClientTick;
            LoginSuccess += GetClientInfo;
            LoginFailed += OnLoginFailed;

            try
            {
                StreamResourceInfo resourceInfo = Application.GetResourceStream(new Uri(@"images\icon.jpg", UriKind.Relative));
                _defaultImage = new BitmapImage();
                _defaultImage.BeginInit();
                _defaultImage.StreamSource = resourceInfo.Stream;
                _defaultImage.EndInit();
                _defaultImage.Freeze();
                ImageSource = _defaultImage;
            }
            catch (Exception)
            {
                ImageSource = null;
            }
        }


        private string _imagePath = System.Environment.CurrentDirectory + @"\image\";

        public string ImagePath
        {
            get { return _imagePath; }
        }
        private BitmapImage _defaultImage;

        private BitmapImage _imageSource;
        public BitmapImage ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                OnPropertyChanged("ImageSource");
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

        internal ShareServiceClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

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
           // ImageSource = _imagePath + hash.ToString() + ".jpeg";

        }

        private BitmapImage GetBitmapImageSource()
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash((Encoding.UTF8.GetBytes(UserName)));
            StringBuilder hash = new StringBuilder();
            foreach (var item in data)
            {
                hash.Append(item.ToString("x2"));
            }
            md5.Clear();
            //BitmapImage bitMap = new BitmapImage();
            //bitMap.BeginInit();
            //bitMap.CacheOption = BitmapCacheOption.
            //return _imagePath + hash.ToString() + ".jpeg";
            string path = _imagePath + hash.ToString() + ".jpeg";
            if (File.Exists(path))
            {
                try
                {
                    var image = new BitmapImage();
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] buf = new byte[fs.Length];
                    fs.Read(buf, 0, (int)fs.Length);
                    fs.Close();
                    MemoryStream ms = new MemoryStream(buf);
                    
                    image.BeginInit();
                    //image.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                    image.StreamSource = ms;
                    image.DecodePixelWidth = 200;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
                catch (Exception)
                {

                    return null;
                }
            }

            return null;
            
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
         
            return _imagePath + hash.ToString() + ".jpg";
          
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