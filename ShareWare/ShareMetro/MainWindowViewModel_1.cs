using Microsoft.Practices.Prism.Commands;
using ShareMetro.ServiceReference;
using ShareWare.ShareFile;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
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

        public MainWindowViewModel()
        {
            Inti();

            IsLoginInfoEnable = true;
            LoginAble = true;
            UserName = string.Empty;
            Password = string.Empty;
            PasswordWatermark = "Enter Password";
            LoginBtnContent = "登陆";

            AboutCmd = new DelegateCommand<object>(OnAbout, arg => true);
            SetBtnCmd = new DelegateCommand<object>(OnSetBtn, arg => true);

            OnlineUser = new ObservableCollection<OnlineUserData>();
            DownloadPathList = new ObservableCollection<string>();
            AddSharePathCmd = new DelegateCommand<object>(OnAddSharePath, arg => true);
            RemoveSharePathCmd = new DelegateCommand<object>(OnRemoveSharePath, arg => true);

            SendMsgCmd = new DelegateCommand<object>(OnSendMessage, arg => true);

            AcceptCmd = new DelegateCommand<object>(OnAcceptOpiton, arg => true);
            ConfirmCmd = new DelegateCommand<object>(OnConfirmOpiton, arg => true);
            CancelCmd = new DelegateCommand<object>(OnCancelOpiton, arg => true);

            IsBusy_Main = false;

            LoginCmd = new DelegateCommand<object>(OnLogin, arg => true);
            RegisterCmd = new DelegateCommand<object>(OnRegister, arg => true);
            SearchCmd = new DelegateCommand<object>(OnSearch, arg => true);
            UploadImageCmd = new DelegateCommand<object>(OnUploadImage, arg => true);
            DownLoadCmd = new DelegateCommand<object>(OnDownLoad, arg => true);
            HeadIConCmd = new DelegateCommand<object>(OnClickHeadIcon, arg => true);

            SetDownloadPathCmd = new DelegateCommand<object>(OnSetDownloadPath, arg => true);

            ErrorOccur += OnErrorOccur;
            LoginSuccess += GetShareInfo;
            LoginSuccess += ClientTick;
            LoginSuccess += GetUserInfo;
            LoginSuccess += AfterLogin;
            LogoutSuccess += OnAfterLogout;
            LoginFailed += OnLoginFailed;
            ServerTimeout += OnServerTimeout;

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

            _sharePath = new ObservableCollection<SharePathData>();

            if (ShareWareSettings.Default.DownloadPathList != null)
            {
                foreach (var item in ShareWareSettings.Default.DownloadPathList)
                {
                    DownloadPathList.Add(item);
                }
            }

            DownloadPathList.CollectionChanged += ((sender, e) =>
            {
                if (ShareWareSettings.Default.DownloadPathList == null)
                {
                    ShareWareSettings.Default.DownloadPathList = new System.Collections.Specialized.StringCollection();
                }
                else
                {
                    ShareWareSettings.Default.DownloadPathList.Clear();
                }

                foreach (var item in DownloadPathList)
                {
                    ShareWareSettings.Default.DownloadPathList.Add(item);
                    ShareWareSettings.Default.Save();
                }
            });

            if (ShareWareSettings.Default.CurrentDownloadPath == null || ShareWareSettings.Default.CurrentDownloadPath == string.Empty)
            {
                CurrentDownloadPath = System.Environment.CurrentDirectory + @"\Download";
            }
            else
            {
                CurrentDownloadPath = ShareWareSettings.Default.CurrentDownloadPath;
            }


            IsRememberPwd = ShareWareSettings.Default.RememberPwd;
            if (IsRememberPwd)
            {
                UserName = ShareWareSettings.Default.UserName;
                Password = ShareWareSettings.Default.Password;
                PasswordWatermark = string.Empty;
            }

            IsAutoLogin = ShareWareSettings.Default.AutoLogin;
            if (IsAutoLogin)
            {
                LoginCmd.Execute(null);

            }

        }

        private void OnSetBtn(object obj)
        {
            if (_id > 0)
            {
                Index = 4;
            }
        }

        public DateTime LastLoginTime { get; set; }

        public ICommand SetBtnCmd { get; set; }

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

        private int _preIndex;
        private int _index;

        public int Index
        {
            get { return _index; }
            set
            {
                _preIndex = _index;
                _index = value;
                OnPropertyChanged("Index");
                switch (Index)
                {
                    case 4:
                        Index_Option = 0;
                        IsConfirmBtnEnable_Option = true;
                        IsAcceptBtnEnable_Option = true;
                        IsCancelBtnEnable_Option = false;
                        break;
                    default:
                        break;
                }
            }
        }

        public bool IsMenuEnable { get; set; }

        private static int _tickTime = 60000;
        private static System.Timers.Timer _tickTimer;

        public event EventHandler<ModelEventArgs> LoginSuccess;
        public event EventHandler<ModelEventArgs> LoginFailed;
        public event EventHandler ServerTimeout;
        public event EventHandler<ModelEventArgs> ErrorOccur;

        public ICommand AboutCmd { get; set; }

        private CallBack _callBack;
        private ShareServiceClient _client;

        internal ShareServiceClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        void OnErrorOccur(object sender, ModelEventArgs e)
        {
            switch (e.Type)
            {
                case ModelEventType.Exception:
                    MessageBox.Show(e.ModelException.Message);
                    break;

                case ModelEventType.ConnectMeesage:
                    FailedMessage = e.FailedMessage;
                    break;
                case ModelEventType.ErrorMessage:
                    MessageBox.Show(e.ErrorMessage);
                    break;

                default:
                    break;
            }
        }

        void OnLoginFailed(object sender, ModelEventArgs e)
        {
            ErrorOccur(sender, e);
            IsBusy_Login = false;
            LoginAble = true;
        }

        void OnServerTimeout(object sender, EventArgs e)
        {
            OnErrorOccur(this, new ModelEventArgs(ModelEventType.ErrorMessage) { ErrorMessage = "与服务器断开连接,准备重新连接" });
            LoginCmd.Execute(null);
        }

        private void InnerChannel_Closing(object sender, EventArgs e)
        {
            Console.WriteLine(sender);
        }

        private void ExecuteEventAsync(object sender, ModelEventArgs e, EventHandler<ModelEventArgs> handler, AsyncCallback callBack)
        {
            foreach (EventHandler<ModelEventArgs> item in handler.GetInvocationList())
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
                //Console.WriteLine(_client.InnerChannel.State);
                //Console.WriteLine(_client.InnerDuplexChannel.State);
            }
            catch (Exception ex)
            {
                if (ServerTimeout != null)
                {
                    ServerTimeout(this, new EventArgs());
                }
                Console.WriteLine(ex);
            }
        }

        private void GetShareInfo(object sender, ModelEventArgs e)
        {
            _sh = ShareFiles.Deserialize(_shareInfoPath);
            _sh.SharePathChanged += ((sender1, eventArg) => OnPropertyChanged("SharePath"));
            _sh.HashingPath += ((sender1, arg) => AddHashingInfo(arg.Path));
            _sh.OnePathComplete += ((sender1, arg) => AddHashCompleteInfo(arg.Path));

            Thread t = _sh.ListFile();
            t.Join();
            try
            {
                MaintenanceShareInfo();
                _client.UploadShareInfo(_sh.FileList);
                _client.RemoveOldFile(_sh.RemoveList);
                _sh.SetUploaded(_sh.FileList);

            }
            catch (Exception)
            {

                //throw;
            }

            SaveShareInfo();
            Show_ShareFile(null, ShareLoact);
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
            string path = _imagePath + hash.ToString() + ".jpg";
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
                    //image.DecodePixelWidth = 200;
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

        private void OnAbout(object obj)
        {
            AboutDlg dlg = new AboutDlg();
            dlg.ShowDialog();
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

        public static string ComputeStringMd5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder hash = new StringBuilder();
            foreach (var item in data)
            {
                hash.Append(item.ToString("x2"));
            }
            return hash.ToString();
        }

        public static string ComputeFileMd5(string path)
        {
            try
            {
                MD5 md5 = MD5.Create();
                FileStream fs = File.OpenRead(path);
                byte[] data = md5.ComputeHash(fs);
                fs.Close();
                StringBuilder hash = new StringBuilder();
                foreach (var item in data)
                {
                    hash.Append(item.ToString("x2"));
                }
                return hash.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}