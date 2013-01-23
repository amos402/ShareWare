using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        public bool IsBlockMsg { get; set; }
        public string ChatRoomMsg { get; set; }
        public string SendingMsg { get; set; }
        public ICommand SendMsgCmd { get; set; }

        private void OnSendMessage(object obj)
        {
            if (SendingMsg != string.Empty)
            {
                _client.SendChatRoomMessage(SendingMsg);
            }
           
            SendingMsg = string.Empty;
            OnPropertyChanged("SendingMsg");
        }
    }
}
