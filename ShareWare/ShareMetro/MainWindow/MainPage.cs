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
        private DateTime _preSearchTime;
        private TimeSpan _elapsedTime = new TimeSpan(0, 0, 3);

        public ICommand SearchCmd { get; set; }
        public ICommand DownLoadCmd { get; set; }
        public ICommand HeadIConCmd { get; set; }

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

        private static CancellationTokenSource _ctsSearch;

        private void OnSearch(object obj)
        {
            Islist = false;
            if (_preSearchTime == null)
            {
                _preSearchTime = DateTime.Now;
            }
            else
            {
                var now = DateTime.Now;
                var spanTime = now - _preSearchTime;
                if (spanTime < _elapsedTime)
                {
                    MessageBox.Show(string.Format("每次搜索间隔不能超过{0}秒", _elapsedTime.Seconds.ToString()));
                    return;
                }
                else
                {
                    _preSearchTime = DateTime.Now;
                }
            }
            IsBusy_Main = true;
            try
            {
                List<string> nameList = FileName.Split(' ').ToList();
                nameList.RemoveAll(new Predicate<string>(T => T == string.Empty));

                Task<List<FileInfoData>> task = _client.SearchFileAsync(nameList, true);
                FileItemInfo.Clear();
                if (_ctsSearch != null)
                {
                    if (!_ctsSearch.Token.CanBeCanceled)
                    {
                        _ctsSearch.Cancel();
                    }
                }
                task.ContinueWith(T =>
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        IsBusy_Main = false;
                        if (T.Result != null)
                        {
                            _ctsSearch = new CancellationTokenSource();
                            Task listTask = new Task(() =>
                                 {
                                     foreach (var item in T.Result)
                                     {

                                         try
                                         {
                                             Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                 (ThreadStart)delegate
                                                 {
                                                     FileInfoDataList f = new FileInfoDataList(item);
                                                     f = ShowSharefile.GetInfo(f);
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
                                 }, _ctsSearch.Token);
                            listTask.Start();
                        }
                    });
                });
            }
            catch (Exception e)
            {
                if (ErrorOccur != null)
                {
                    ErrorOccur(this, new ModelEventArgs(ModelEventType.Exception) { ModelException = e });
                }
            }
        }

        private void OnDownLoad(object obj)
        {

        }

        private void OnClickHeadIcon(object obj)
        {
            Index = 4;
            Index_Option = 0;
        }

    }
}
