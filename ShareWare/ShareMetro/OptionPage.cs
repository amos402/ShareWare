using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {

        public ICommand AddSearchPathCmd { get; set; }

        public ICommand UploadImageCmd { get; set; }

        private bool _sysShareSwitch;
        public bool SysShareSwitch
        {
            get { return _sysShareSwitch; }
            set
            {
                _sysShareSwitch = value;
                if (_sh != null)
                {
                    if (_sysShareSwitch)
                    {

                        _sh.AddSystemSharePath();

                    }
                    else
                    {
                        foreach (var item in _sh.SystemShareNameList)
                        {
                            _sh.RemoveSharePath(item);
                        }

                    }
                }
            }
        }

        public Dictionary<string, string> SharePath
        {
            get
            {
                if (_sh != null)
                {
                    return _sh.SharePath;
                }
                return null;
            }
            set
            {
                _sh.SharePath = value;
                OnPropertyChanged("SharePath");
            }
        }


        private void OnUploadImage(object obj)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            bool? success = dlg.ShowDialog();
            if (success == true)
            {
                Bitmap image = new Bitmap(dlg.FileName);
                _client.UploadImage(image);
            }

        }

        private void OnAddSharePath(object obj)
        {

        }
    }
}
