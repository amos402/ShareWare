using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socket_Library
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SenderInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string Name;
        public int StrNum;
        public int State;//0，欲断开,1，欲连上,2,发送文件
    }
    public class Chat
    {
        public bool link { get; set; }
        //bool Sender = false;
        int state = 1;
        public int State
        {
            get { return state; }
            set { state = value; }
        }
        string Name;
        byte[] b = new byte[768];
        StructByte S_B = new StructByte();
        Socket sock = null;
        public Socket Sock
        {
            get { return sock; }
            set { sock = value; }
        }
        Socket newsock = null;
        public int Port { get; set; }

        public event EventHandler<MessageEventArgs> RecvMessage;
        public event EventHandler<MessageEventArgs> SendFile_R;
        public event EventHandler<EventArgs> Control_DownLoadListView;
        public void CreatSenderChat(string name)
        {
            Name = name;
            if (sock == null)
            {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endpoit = new IPEndPoint(IPAddress.Any, 6000);//////////实际运行port要设置为0
                // sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);  //SocketOptionName.ReuseAddress是关键  
                sock.Bind(endpoit);
                sock.Listen(10);
                link = true;
                Port = ((IPEndPoint)sock.LocalEndPoint).Port;
            }
            int workerThreads, availabeThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
            WaitCallback wcb = new WaitCallback(p =>
            {
                newsock = sock.Accept();
                newsock.BeginReceive(b, 0, b.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), newsock);
            }
                );
            if (workerThreads > 0)//可用线程数>0
            {
                ThreadPool.QueueUserWorkItem(wcb, null);
            }
            else
            {
                //to do 可以采取一种策略，让这个任务合理地分配给线程
            }
        }

        public void SendChat(string content)
        {
            byte[] b_s = new byte[768];
            byte[] b1 = new byte[268];
            SenderInfo s = new SenderInfo();
            byte[] b2 = System.Text.Encoding.UTF8.GetBytes(content);
            s.Name = Name;
            s.StrNum = b2.Length;
            s.State = State;
            b1 = S_B.StructToBytes(s);
            Array.Copy(b1, b_s, b1.Length);
            Array.Copy(b2, 0, b_s, b1.Length, b2.Length);
            Thread t = new Thread(p =>
                {
                    while (true)
                    {
                        try
                        {
                            newsock.Send(b_s, SocketFlags.None);
                            break;
                        }
                        catch (Exception)
                        {
                            ;
                        }
                    }
                }
            );
            t.Start();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            byte[] b1 = new byte[268];
            Socket s = (Socket)ar.AsyncState;
            int i = 0;
            try
            {
                i = s.EndReceive(ar);
            }
            catch (Exception)
            {
                ;
            }
            if (i > 0)
            {
                Array.Copy(b, b1, b1.Length);
                SenderInfo sri = (SenderInfo)S_B.BytesToStruct(b1, (new SenderInfo()).GetType());
                if (sri.State == 0)
                {
                    link = false;
                    s.Close();
                    s = null;
                    return;
                }
                byte[] b2 = new byte[sri.StrNum];
                Array.Copy(b, b1.Length, b2, 0, sri.StrNum);
                string send = System.Text.Encoding.UTF8.GetString(b2);
                DateTime now = DateTime.Now;
                string name = sri.Name + "  " + now;
                if (sri.State == 2) ReceiveFile(send);
                //Console.WriteLine(sri.Name + "  " + now);
                //Console.WriteLine(send);

                if (RecvMessage != null && sri.State == 1)
                {
                    foreach (EventHandler<MessageEventArgs> item in RecvMessage.GetInvocationList())
                    {
                        item.BeginInvoke(this, new MessageEventArgs() { Name = name, Message = send }, null, null);
                    }
                }

                try
                {
                    s.BeginReceive(b, 0, b.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), s);
                }
                catch (Exception)
                {
                    ;
                }
            }
        }

        public void CreatReceiverChat(string ip, int port, string name)
        {
            Name = name;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint endpoit = new IPEndPoint(IPAddress.Parse(ip), port);
            int workerThreads, availabeThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
            WaitCallback wcb = new WaitCallback(p =>
            {
                while (true)
                {
                    try
                    {
                        sock.Connect(endpoit);
                        break;
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
                newsock = sock;
                sock.BeginReceive(b, 0, b.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), sock);
            }
            );
            if (workerThreads > 0)//可用线程数>0
            {
                ThreadPool.QueueUserWorkItem(wcb, null);
            }
            else
            {
                //to do 可以采取一种策略，让这个任务合理地分配给线程
            }
        }

        public void CloseTalk()
        {
            State = 0;
            SendChat("");
            if (sock != null)
            {
                sock.Close();
                sock = null; 
            }
        }

        public void SendFile(string filename)
        {
            State = 2;
            SendChat(filename);
            State = 1;
            if (Control_DownLoadListView != null) Control_DownLoadListView.BeginInvoke(this, new EventArgs(), null, null);
        }

        public void ReceiveFile(string filename)
        {
            if (SendFile_R != null) SendFile_R.BeginInvoke(this, new MessageEventArgs() { Message = "", Name = filename }, null, null);
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string Name { get; set; }
    }
}
