using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        public string ShareLog { get; set; }

        private static string _hashingStr = "正在Hash文件夹：";
        private void AddHashingInfo(string path)
        {
            ShareLog += (_hashingStr + path + '\n');
            //OnPropertyChanged("ShareLog");
        }

        private static string _hashCompleteStr = "完成Hash：";
        private void AddHashCompleteInfo(string path)
        {
            ShareLog += (_hashCompleteStr + path + '\n');
            //OnPropertyChanged("ShareLog");
        }
    }
}
