using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using ShareMetro.RegisterServiceReference;
using ShareMetro.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ShareMetro
{
    public class RegisterVM : INotifyPropertyChanged
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
        public event EventHandler RequestClose;

        public RegisterVM()
        {
            RegisterCmd = new DelegateCommand<object>(OnRegister, arg => true);
            Reset = new DelegateCommand<object>(OnReset, arg => true);
            UploadImageCmd = new DelegateCommand<object>(OnUploadImage, arg => true);
            User = new RegisterServiceReference.UserInfo();
        }

        public ShareMetro.RegisterServiceReference.UserInfo User { get; set; }

        //public string UserName { get; set; }

        //public string NickName { get; set; }

        //public string Password { get; set; }

        //public bool IsMale { get; set; }

        //public string QQ { get; set; }

        //public string MicroBlog { get; set; }

        //public string Signature { get; set; }

        //public Bitmap Image { get; set; }

        public bool IsBusy { get; set; }

        public string ErrorInfo { get; set; }

        private string _password2;

        public string Password2
        {
            get { return _password2; }
            set
            {
                _password2 = value;
                if (_password2 != User.Password)
                {
                    ErrorInfo = "两次密码不相符";
                    CanRegister = false;

                }
                else if (User.UserName != string.Empty)
                {
                    CanRegister = true;
                }
                else
                {
                    CanRegister = false;
                }

                OnPropertyChanged("Password2");
            }
        }

        private BitmapImage _image;

        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }
        public bool CanRegister { get; set; }

        public ICommand RegisterCmd { get; set; }
        public ICommand Reset { get; set; }
        public ICommand UploadImageCmd { get; set; }

        private void OnRegister(object obj)
        {
            // Register dlg = Application.Current.Dispatcher as Register;
            var asd = Application.Current.Dispatcher;
            RegisterServiceClient client = new RegisterServiceClient();

            User.Image = new Bitmap(Image.StreamSource);
            Task<RegError> task = client.RegisterAsync(User);
            task.ContinueWith(T =>
                {
                    switch (T.Result)
                    {
                        case RegError.NoError:

                            break;
                        case RegError.UserExist:
                            ErrorInfo = "用户已存在";
                            break;
                        case RegError.Ohter:
                            ErrorInfo = "注册失败，发生未知错误";
                            break;
                        default:
                            break;
                    }
                    IsBusy = false;
                    client.Close();
                });

        }


        private void OnReset(object obj)
        {
            User.UserName = string.Empty;
            User.NickName = string.Empty;
            User.Password = string.Empty;
            User.QQ = string.Empty;
            User.MicroBlog = string.Empty;
            User.Signature = string.Empty;
            Password2 = string.Empty;
            Image = null;
        }

        private void OnUploadImage(object obj)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            bool? success = dlg.ShowDialog();
            if (success == true)
            {

                try
                {
                    FileStream fs = File.OpenRead(dlg.FileName);
                    byte[] buf = new byte[fs.Length];
                    fs.Read(buf, 0, buf.Length);
                    fs.Close();
                    MemoryStream ms = new MemoryStream(buf);
                    _image = new BitmapImage();
                    _image.BeginInit();
                    _image.StreamSource = ms;
                    _image.CacheOption = BitmapCacheOption.OnLoad;
                    _image.EndInit();
                    _image.Freeze();
                    OnPropertyChanged("Image");
                }
                catch (Exception)
                {
                    
                    //throw;
                }
            }
        }

        private void CloseWindow()
        {
            if (RequestClose != null)
            {
                RequestClose(this, null);
            }
        }

    }

}
