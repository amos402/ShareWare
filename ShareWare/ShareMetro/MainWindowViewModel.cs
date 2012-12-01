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
            callBack = new CallBack();
            _client = new ShareServiceClient(new InstanceContext(callBack));
        }

        #endregion

        private CallBack callBack;
        private ShareServiceClient _client;
        public ShareServiceClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        private int _id;

        public int Index { get; set; }
        public bool HideMenu { get; set; }
        private List<FileInfoData> _fileList;

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

            var task = _client.LoginAsync(_loginTab.UserName, _loginTab.Password, GetFirstMac());
            task.ContinueWith(T =>
            {
                if (T.Result < 0)
                {
                    return;
                }
                _id = T.Result;
                HideMenu = true;
                Index = 1;
            });
            LoginTab.ButtonVisiable = false;
        }

        public void OnSearch(object obj)
        {
            var task = _client.SearchFileAsync(MainTab.FileName);
            task.ContinueWith(T =>
                {
                    FileList = T.Result;
                });
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