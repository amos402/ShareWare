using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareMetro
{
    public class MainPage : INotifyPropertyChanged
    {

        public string FileName { get; set; }
        public bool IsBusy { get; set; }

        public ObservableCollection<string> OnlineUser { get; set; }


        public MainPage()
        {
            OnlineUser = new ObservableCollection<string>();
            IsBusy = false;
        }

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
