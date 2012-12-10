using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ShareWare.ShareFile;
using Socket_Library;

namespace ShareMetro
{
    class talkwinMVVMcs : INotifyPropertyChanged
    {

        public talkwinMVVMcs()
        {
            IDName = "ACE";
            talk.RecvMessage += ReveicMessage;
            talk.Control_DownLoadListView += DownLoadListView;
            talk.SendFile_R += ShowListView;
            Cancel_Buttom = Receiv_Buttom = Receiv_ListView = false;
        }

        public ICommand SendMessageCommand
        {
            get
            {
                return new ICom(p => SendMessage(Message_S));
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new ICom(p =>
                {
                    talk.CloseTalk();
                    Application.Current.MainWindow.Close();
                });
            }
        }

        public ICommand SendFileCommand
        {
            get
            {
                return new ICom(p =>
                {
                    System.Windows.Forms.FolderBrowserDialog fb = new System.Windows.Forms.FolderBrowserDialog();
                    fb.ShowDialog();
                    SendFileLsit.Add(fb.SelectedPath);                  
                    talk.SendFile(fb.SelectedPath);
                });
            }
        }

        public ICommand CancelFileCommand
        {
            get
            {
                return new ICom(p =>
                {
                    for (int i = 0; i < SendFileLsit.Count; i++)
                    {
                        if (SendFileLsit[i].CompareTo(SelectFileName) == 0) SendFileLsit.RemoveAt(i);
                    }
                });
            }
        }

        public ICommand ReceivFileCommand
        {
            get
            {
                return new ICom(p =>
                {
                    DirectoryInfo di = new DirectoryInfo(SelectFileName);
                    string type = "";
                    if (di.Attributes == FileAttributes.Directory)
                    {
                        type = "文件夹";
                    }
                    else type = di.Extension;
                    if (Control_DownLoad != null) Control_DownLoad.BeginInvoke(this, new Control_Down() 
                    {
                        SeleteInfo = new FileInfoDataList(SelectFileName, type, 0, null, null)
                    }, null, null);
                });
            }
        }


        public event EventHandler<Control_Down> Control_DownLoad;


        ObservableCollection<string> sendFileLsit = new ObservableCollection<string>();
        public ObservableCollection<string> SendFileLsit
        {
            get { return sendFileLsit; }
            set { sendFileLsit = value; }
        }
        public bool Cancel_Buttom { get; set; }
        public bool Receiv_Buttom { get; set; }
        public bool Receiv_ListView { get; set; }
        public string SelectFileName { get; set; }
        private string message_S;
        public string Message_S
        {
            get { return message_S; }
            set
            {
                message_S = value;
                OnPropertyChanged("Message_S");
            }
        }
        private string message_Show;
        public string Message_Show
        {
            get { return message_Show; }
            set
            {
                message_Show = value;
                OnPropertyChanged("Message_Show");
            }
        }



        Chat talk = new Chat();
        public string IDName { get; set; }

        private void SendMessage(object p)
        {
            if (talk.Sock == null)
            {
                talk.CreatSenderChat(IDName);
            }
            string s = p as string;
            talk.SendChat(s);
            Message_Show += (IDName + ":\n");
            Message_Show += (s + "\n");
            Message_S = "";
        }

        public void DownLoadListView(object sender, EventArgs e)
        {
            Cancel_Buttom = Receiv_Buttom = Receiv_ListView = true;
        }

        public void ReveicMessage(object sender, MessageEventArgs e)
        {
            Message_Show += (e.Name + ":\n");
            Message_Show += (e.Message + "\n");
        }

        public void ShowListView(object sender, MessageEventArgs e)
        {
            SendFileLsit.Add(e.Name);
        }

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
    public class Control_Down : EventArgs
    {
        public FileInfoDataList SeleteInfo;
    }
    public class Call_Talk : EventArgs
    {
        
    }
}
