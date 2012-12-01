using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using ShareMetro.Models;
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

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        #region MyRegion

        //public ObservableCollection<PanoramaGroup> Groups { get; set; }
        //readonly PanoramaGroup _albums;
        //readonly PanoramaGroup _artists;


        //public bool Busy { get; set; }

        //public int SelectedIndex { get; set; }
        //public List<Album> Albums { get; set; }
        //public List<Artist> Artists { get; set; }

        public MainWindowViewModel(Dispatcher dispatcher)
        {

            //SampleData.Seed();
            //Albums = SampleData.Albums;
            //Artists = SampleData.Artists;
            //Busy = true;
            //_albums = new PanoramaGroup("trending tracks");
            //_artists = new PanoramaGroup("trending artists");
            //Groups = new ObservableCollection<PanoramaGroup> { _albums, _artists };

            //_artists.SetSource(SampleData.Artists.Take(25));
            //_albums.SetSource(SampleData.Albums.Take(25));
            //Busy = false;
            LoginTab = new LoginPage() { ButtonVisiable = true };
            MainTab = new MainPage();
            LoginCmd = new DelegateCommand<object>(OnLogin, arg => true);
            SearchCmd = new DelegateCommand<object>(OnSearch, arg => true);
            callBack = new CallBack(MainTab);
            _client = new ShareServiceClient(new InstanceContext(callBack));
            Login += GetShareInfo;
        }

        #endregion
        private string _shareInfoPath = @"config\known.met";

        public event EventHandler<ModelEvent> Login;

        private CallBack callBack;
        private ShareServiceClient _client;
        internal ShareServiceClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        private int _id;

        public int Index { get; set; }
        public bool HideMenu { get; set; }
        private List<FileInfoData> _fileList;

        private ShareFiles _sh;

        public List<FileInfoData> FileList
        {
            get { return _fileList; }
            set
            {
                _fileList = value;
                OnPropertyChanged("FileList");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ICommand LoginCmd { get; set; }
        public ICommand SearchCmd { get; set; }

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


        public void OnLogin(object obj)
        {
            _loginTab.IsBusy = true;

            Task<int> task = _client.LoginAsync(_loginTab.UserName, _loginTab.Password, GetFirstMac());
            task.ContinueWith(T =>
            {
                if (T.Result < 0)
                {
                    return;
                }
                _id = T.Result;
                HideMenu = true;
                Index = 1;

                if (Login != null)
                {
                    ExecuteEventAsync(this, new ModelEvent(), Login, null);
                }

            });
            LoginTab.ButtonVisiable = false;
        }

        public void OnSearch(object obj)
        {
            Task<List<FileInfoData>> task = _client.SearchFileAsync(MainTab.FileName);
            task.ContinueWith(T =>
                {
                    FileList = T.Result;
                });
        }


        private void ExecuteEventAsync(object sender, ModelEvent e, EventHandler<ModelEvent> handler, AsyncCallback callBack)
        {
            foreach (EventHandler<ModelEvent> item in handler.GetInvocationList())
            {
                item.BeginInvoke(sender, e, callBack, null);
            }
        }

       

        private void GetShareInfo(object sender, ModelEvent e)
        {
            _sh = ShareFiles.Deserialize(_shareInfoPath);
            //_sh.AddSharePath("RamDisk", @"R:\");
            Thread t = _sh.ListFile();
            t.Join();
            _client.UploadShareInfo(_sh.FileList);
            _sh.SetUploaded(_sh.FileList);

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