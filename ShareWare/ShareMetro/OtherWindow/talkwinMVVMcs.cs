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
            Cancel_Buttom = Receiv_Buttom = Receiv_ListView = "Hidden";
        }

        public ICommand SendMessageCommand
        {
            get
            {
                return new ICom(p =>
                {
                    CreatTalk();
                    SendMessage(Message_S);
                });
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new ICom(p =>
                {
                    Close_Talk();
                    Application.Current.MainWindow.Close();
                });
            }
        }

        public ICommand SendDirectoryCommand
        {
            get
            {
                return new ICom(p =>
                {
                    CreatTalk();
                    System.Windows.Forms.FolderBrowserDialog fb = new System.Windows.Forms.FolderBrowserDialog();
                    fb.ShowDialog();
                    SendFileLsit.Add(new sendFileLsitClass(fb.SelectedPath, SendFileLsit.Count, "文件夹"));
                    talk.SendFile(fb.SelectedPath, 2);
                });
            }
        }

        public ICommand SendFileCommand
        {
            get
            {
                return new ICom(p =>
                {
                    CreatTalk();
                    System.Windows.Forms.OpenFileDialog of = new System.Windows.Forms.OpenFileDialog();
                    if (of.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        SendFileLsit.Add(new sendFileLsitClass(of.FileName, SendFileLsit.Count, of.FileName.Substring(of.FileName.LastIndexOf(".") + 1)));
                        talk.SendFile(of.FileName, 3);
                    }
                });
            }
        }

        public ICommand CancelFileCommand
        {
            get
            {
                return new ICom(p =>
                {

                    for (int i = SelectionChange.ID; i < SendFileLsit.Count; i++)
                    {
                        SendFileLsit[i].ID = i;
                    }
                    SendFileLsit.RemoveAt(SelectionChange.ID);
                    if (SendFileLsit.Count == 0) Cancel_Buttom = Receiv_Buttom = Receiv_ListView = "Hidden";
                });
            }
        }

        public ICommand ReceivFileCommand
        {
            get
            {
                return new ICom(p =>
                {
                    DirectoryInfo di = new DirectoryInfo(SelectionChange.name);
                    string type = "";
                    if (di.Attributes == FileAttributes.Directory)
                    {
                        type = "文件夹";
                    }
                    else type = di.Extension;
                    if (Control_DownLoad != null) Control_DownLoad.BeginInvoke(this, new Control_Down()
                    {
                        SeleteInfo = new FileInfoDataList(SelectionChange.name, type, 0, "", null, null)
                    }, null, null);
                });
            }
        }


        public event EventHandler<Control_Down> Control_DownLoad;
        public event EventHandler<Call_Talk> Call_Creat_Talk;
        public event EventHandler<DeleteUpLoadList> Detelet_Talk;

        public sendFileLsitClass SelectionChange { get; set; }

        ObservableCollection<sendFileLsitClass> sendFileLsit = new ObservableCollection<sendFileLsitClass>();
        public ObservableCollection<sendFileLsitClass> SendFileLsit
        {
            get { return sendFileLsit; }
            set { sendFileLsit = value; }
        }
        public string Cancel_Buttom { get; set; }
        public string Receiv_Buttom { get; set; }
        public string Receiv_ListView { get; set; }
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


        public int ID { get; set; }
        Chat _talk = new Chat();
        public Chat talk
        {
            get { return _talk; }
            set { _talk = value; }
        }
        public string IDName { get; set; }
        public string OtherName { get; set; }

        public void CreatTalk()
        {
            if (talk.Sock == null)
            {
                talk.CreatSenderChat(IDName);
                if (Call_Creat_Talk != null) Call_Creat_Talk.BeginInvoke(this, new Call_Talk() { Port = talk.Port, IDName = OtherName }, null, null);
            }
        }

        private void SendMessage(object p)
        {
            CreatTalk();
            string s = p as string;
            talk.SendChat(s);
            Message_Show += (IDName + ":\n");
            Message_Show += (s + "\n");
            Message_S = "";
        }

        public void DownLoadListView(object sender, EventArgs e)
        {
            Cancel_Buttom = Receiv_Buttom = Receiv_ListView = "Visible";
        }

        public void ReveicMessage(object sender, MessageEventArgs e)
        {
            Message_Show += (e.Name + ":\n");
            Message_Show += (e.Message + "\n");
        }

        public void ShowListView(object sender, MessageEventArgs e)
        {
            SendFileLsit.Add(new sendFileLsitClass(e.Name, SendFileLsit.Count, e.Message));
        }

        public void Close_Talk()
        {
            talk.CloseTalk();
            if (Detelet_Talk != null) Detelet_Talk.Invoke(this, new DeleteUpLoadList() { Id = ID });
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

    public class sendFileLsitClass
    {
        public string name { get; set; }
        public int ID { get; set; }
        public string Type { get; set; }
        public sendFileLsitClass(string n, int id, string type)
        {
            name = n;
            ID = id;
            Type = type;
        }
    }
    public class Control_Down : EventArgs
    {
        public FileInfoDataList SeleteInfo;
    }
    public class Call_Talk : EventArgs
    {
        public int Port { get; set; }
        public string IDName { get; set; }
    }
}
