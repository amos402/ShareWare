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
using System.Windows.Input;

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {

        public bool IsBusy_Login { get; set; }

        public ICommand LoginCmd { get; set; }
        public ICommand RegisterCmd { get; set; }

        public bool LoginAble { get; set; }
        private string _userName;

        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                _cts.Cancel();
                Task<string> imageTask = new Task<string>((T) => GetImageSource(), _cts);
                imageTask.Start();
                imageTask.ContinueWith(T =>
                    {
                        ImageSource = T.Result;
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
    }
}
