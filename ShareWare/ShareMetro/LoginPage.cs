using Microsoft.Practices.Prism.Commands;
using ShareMetro.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {

        public bool IsBusy_Login { get; set; }

        public ICommand LoginCmd { get; private set; }
        public ICommand RegisterCmd { get; private set; }

        public bool LoginAble { get; set; }
        private string _userName;

        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                if (_userName == string.Empty)
                {
                    return;
                }
                //_cts.Cancel();
                Task<BitmapImage> imageTask = new Task<BitmapImage>((T) => GetBitmapImageSource(), _cts);
                imageTask.Start();
                imageTask.ContinueWith(T =>
                    {
                        if (T.Result != null)
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          (ThreadStart)delegate
                                          {
                                              ImageSource = T.Result;
                                          });
                        }
                        else
                        {
                            ImageSource = _defaultImage;
                        }

                    });

                OnPropertyChanged("UserName");
            }
        }

        public string Password { get; set; }
        public string FailedMessage { get; set; }

        private void OnLogin(object obj)
        {
            IsBusy_Login = true;
            LoginAble = false;
            FailedMessage = string.Empty;
            try
            {
                Task<int> task = _client.LoginAsync(UserName, Password, GetFirstMac());

                task.ContinueWith(T =>
                {
                    AggregateException ex = T.Exception;
                    if (ex != null)
                    {
                        if (ex.InnerExceptions.Count > 0)
                        {
                            ErrorOccur(this, new ModelEvent(ModelEventType.Meesage) { FailedMessage = "连接服务器出错" });
                            IsBusy_Login = false;
                            LoginAble = true;
                            return;
                        }
                    }
                    if (T.Result < 0)
                    {
                        ErrorOccur(this, new ModelEvent(ModelEventType.Meesage) { FailedMessage = "用户名或密码错误" });
                        IsBusy_Login = false;
                        LoginAble = true;
                        return;
                    }
                    Index = 1;
                    _id = T.Result;
                    if (LoginSuccess != null)
                    {
                        ExecuteEventAsync(this, null, LoginSuccess, null);
                    }
                    IsBusy_Login = false;
                    LoginAble = true;

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
        private void OnRegister(object obj)
        {
            Register dlg = new Register();
            dlg.Owner = Application.Current.MainWindow;
            if (dlg.ShowDialog() == true)
            {
                
            }

        }
    }
}
