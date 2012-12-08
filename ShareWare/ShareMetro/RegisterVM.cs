using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using ShareMetro.RegisterServiceReference;
using ShareMetro.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

        public ICommand RegisterCmd { get; set; }
        public ICommand Reset { get; set; }
        public ICommand UploadImageCmd { get; set; }

        private void OnRegister(object obj)
        {
            // Register dlg = Application.Current.Dispatcher as Register;
            var asd = Application.Current.Dispatcher;
            RegisterServiceClient client = new RegisterServiceClient();
            {
                client.Register(User);
                client.Close();
            }
        }

        private void OnReset(object obj)
        {
            User.UserName = string.Empty;
            User.NickName = string.Empty;
            User.Password = string.Empty;
            User.QQ = string.Empty;
            User.MicroBlog = string.Empty;
            User.Signature = string.Empty;
        }

        private void OnUploadImage(object obj)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            bool? success = dlg.ShowDialog();
            if (success == true)
            {
                User.Image = new Bitmap(dlg.FileName);
            }
        }

        public ShareMetro.RegisterServiceReference.UserInfo GetUserInfo()
        {
            var userInfo = new ShareMetro.RegisterServiceReference.UserInfo();
            //userInfo.UserName = UserName;
            //userInfo.NickName = NickName;
            //userInfo.IsMale = IsMale;
            //userInfo.Password = Password;
            //userInfo.QQ = QQ;
            //userInfo.Signature = Signature;
            return userInfo;
        }
    }

}
