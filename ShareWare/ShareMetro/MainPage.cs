using ShareWare;
using ShareWare.ShareFile;
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

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {

        public string FileName { get; set; }
        public bool IsBusy_Main { get; set; }



        public ICommand SearchCmd { get; set; }
        public ICommand DownLoadCmd { get; set; }
        public ICommand HeadIConCmd { get; set; }

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

        public ObservableCollection<OnlineUserData> OnlineUser { get; private set; }

        private void OnSearch(object obj)
        {
            IsBusy_Main = true;
            try
            {
                string[] sp = FileName.Split(' ');
                List<string> nameList = new List<string>();
                nameList.AddRange(sp);
                Task<List<FileInfoData>> task = _client.SearchFileAsync(nameList);
                FileItemInfo.Clear();
                task.ContinueWith(T =>
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        searchMut.WaitOne();
                        IsBusy_Main = false;
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

        private void OnDownLoad(object obj)
        {

        }

        private void OnClickHeadIcon(object obj)
        {
            Index = 4;
            OptinIndex = 0;
        }

    }
}
