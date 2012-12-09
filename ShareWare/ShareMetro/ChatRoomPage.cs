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
        public bool IsBlockMsg { get; set; }
        public string ChatRoomMsg { get; set; }

    }
}
