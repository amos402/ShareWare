using Microsoft.Practices.Prism.Commands;
using ShareMetro.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.ServiceModel;
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
        public bool IsLoginInfoEnable { get; set; }

        public ICommand LoginCmd { get; private set; }
        public ICommand RegisterCmd { get; private set; }

        private int _maintenanceDay = 3;

        public EventHandler<ModelEventArgs> LogoutSuccess;

        public string PasswordWatermark { get; set; }
        public string LoginBtnContent { get; private set; }
        private bool _hasLogin;

        public bool LoginAble { get; set; }
        private string _userName;

        private bool _isAutoLogin;
        public bool IsAutoLogin
        {
            get { return _isAutoLogin; }
            set
            {
                _isAutoLogin = value;
                OnPropertyChanged("IsAutoLogin");
                ShareWareSettings.Default.AutoLogin = IsAutoLogin;
                ShareWareSettings.Default.Save();
            }
        }


        private bool _isRememberPwd;
        public bool IsRememberPwd
        {
            get { return _isRememberPwd; }
            set
            {
                _isRememberPwd = value;
                OnPropertyChanged("IsRememberPwd");
                ShareWareSettings.Default.RememberPwd = IsRememberPwd;
                if (IsRememberPwd)
                {
                    if (UserName != string.Empty && Password != string.Empty)
                    {
                        ShareWareSettings.Default.UserName = UserName;
                        ShareWareSettings.Default.Password = Password;
                    }

                }
                else
                {
                    ShareWareSettings.Default.UserName = string.Empty;
                    ShareWareSettings.Default.Password = string.Empty;
                }
                ShareWareSettings.Default.Save();
            }
        }

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
                _cts.Cancel();
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
            string md5Password = ComputeStringMd5(Password);
            if (IsRememberPwd)
            {
                ShareWareSettings.Default.Password = Password;
                ShareWareSettings.Default.UserName = UserName;
                ShareWareSettings.Default.Save();
            }

            try
            {
                if (_hasLogin)
                {
                    _client.Logout();
                    _client.Close();

                    IsBusy_Login = false;
                    if (LogoutSuccess != null)
                    {
                        LogoutSuccess(this, null);
                    }
                }
                else
                {
                    Task task = new Task(() =>
                        {
                            _callBack = new CallBack(this);
                            _client = new ShareServiceClient(new InstanceContext(_callBack));
                            _client.InnerChannel.Closing += InnerChannel_Closing;

                            try
                            {
                                _id = _client.Login(UserName, md5Password, GetFirstMac());

                                if (_id < 0)
                                {
                                    if (LoginFailed != null)
                                    {
                                        LoginFailed(this, new ModelEventArgs(ModelEventType.ConnectMeesage) { FailedMessage = "用户名或密码错误" });
                                    }
                                    return;
                                }

                                if (LoginSuccess != null)
                                {
                                    //LoginSuccess(this, null);
                                    ExecuteEventAsync(this, null, LoginSuccess, null);
                                }

                                IsBusy_Login = false;
                                LoginAble = true;
                            }
                            catch (Exception)
                            {
                                ErrorOccur(this, new ModelEventArgs(ModelEventType.ConnectMeesage) { FailedMessage = "连接服务器出错" });
                                IsBusy_Login = false;
                                LoginAble = true;
                                return;
                            }

                        });
                    task.Start();
                }
            }
            catch (Exception e)
            {

                if (ErrorOccur != null)
                {
                    ErrorOccur(this, new ModelEventArgs(ModelEventType.Exception) { ModelException = e });
                }
            }

        }

        private void AfterLogin(object sender, ModelEventArgs e)
        {

            Index = 1;
            IsLoginInfoEnable = false;
            IsMenuEnable = true;
            _hasLogin = true;
            LoginBtnContent = "注销";
            RefreshHostUserImage(UserName, ImageHash_Option);
        }

        private void MaintenanceShareInfo()
        {
            if (ShareWareSettings.Default.LastLoginTime == null)
            {
                ShareWareSettings.Default.LastLoginTime = DateTime.Now;
            }
            LastLoginTime = ShareWareSettings.Default.LastLoginTime;
            TimeSpan timeSpan = DateTime.Now - LastLoginTime;
            ShareWareSettings.Default.LastLoginTime = DateTime.Now;
            ShareWareSettings.Default.Save();

            //if (timeSpan.Days >= _maintenanceDay)
            //{
                foreach (var item in _sh.ShareFileDict.Values)
                {
                    foreach (var item1 in item)
                    {
                        item1.Uploaded = false;
                    }
                }
                var task = _client.DownloadShareInfoAsync();
                task.ContinueWith(T =>
                    {
                        if (T.Result != null)
                        {
                            var result = from c in T.Result
                                         select c.Hash;
                            var exist = from c in _sh.ShareFileDict
                                        from b in c.Value
                                        select b.Hash;
                            var notExist = result.Except(exist);
                            // _client.RemoveNotExistShreFileList(notExist.ToList());

                            foreach (var item in notExist)
                            {
                                _client.RemoveNotExistShreFile(item);
                            }
                        }
                    });
            //}
        }

        private int _errorCount = 0;
        private void RefreshHostUserImage(string userName, string imageHash)
        {
            string hash = ComputeStringMd5(userName);
            string filePath = ImagePath + hash + ".jpg";

            if (File.Exists(filePath))
            {
                string fileHash = ComputeFileMd5(filePath);
                if (fileHash != imageHash)
                {
                    Task<Bitmap> task = Client.DownloadUserImageAsync(userName);
                    task.ContinueWith(T =>
                    {
                        try
                        {
                            if (T.Result != null)
                            {
                                T.Result.Save(filePath);
                                ImageSource = GetBitmapImageSource();
                            }
                        }
                        catch (Exception)
                        {
                            //throw;
                        }
                    });
                }
            }
            else
            {
                Task<Bitmap> task = Client.DownloadUserImageAsync(userName);
                task.ContinueWith(T =>
                {
                    try
                    {
                        T.Result.Save(filePath, ImageFormat.Jpeg);
                        ImageSource = GetBitmapImageSource();
                    }
                    catch (Exception)
                    {
                        if (_errorCount < 5)
                        {
                            FileInfo file = new FileInfo(filePath);
                            if (!System.IO.Directory.Exists(file.DirectoryName))
                            {
                                System.IO.Directory.CreateDirectory(file.DirectoryName);
                            }
                            RefreshHostUserImage(userName, imageHash);
                            _errorCount++;
                        }
                    }
                });
            }
        }

        private void OnAfterLogout(object sender, ModelEventArgs e)
        {
            _hasLogin = false;
            LoginBtnContent = "登陆";
            LoginAble = true;
            IsLoginInfoEnable = true;
            IsMenuEnable = false;
        }

        private void OnRegister(object obj)
        {
            Register dlg = new Register();
            dlg.Owner = Application.Current.MainWindow;
            dlg.ShowDialog();
        }
    }
}
