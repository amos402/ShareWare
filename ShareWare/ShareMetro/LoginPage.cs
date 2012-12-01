using Microsoft.Practices.Prism.Commands;
using ShareMetro.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShareMetro
{
    public class LoginPage : INotifyPropertyChanged
    {

        public LoginPage()
        {
            Visable = false;
            UserName = string.Empty;
            Password = string.Empty;
        
        }

        public bool IsBusy { get; set; }
        public bool Visable { get; set; }
        public bool ButtonVisiable { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
