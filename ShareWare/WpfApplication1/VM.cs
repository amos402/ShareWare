using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class VM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public VM()
        {
            // _filePath.AddSystemSharePath();
        }

        private ShareFiles _filePath = new ShareFiles();

        public ShareFiles FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        //private ObservableCollection<KeyValuePair<string, string>> _sharePath;

        //public ObservableCollection<KeyValuePair<string, string>> SharePath
        //{
        //    get { return new ObservableCollection<KeyValuePair<string, string>>(_filePath.ShareName.ToList()); }
        //    set
        //    {
        //        _sharePath = value;

        //        if (PropertyChanged != null)
        //        {
        //            PropertyChanged(this, new PropertyChangedEventArgs("SharePath"));
        //        }
        //    }
        //}


        private Dictionary<string, string> _sharePath;

        public Dictionary<string, string> SharePath
        {
            get { return FilePath.SharePath; }
            set
            {
                FilePath.SharePath = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SharePath"));
                }
            }
        }


    }
}
