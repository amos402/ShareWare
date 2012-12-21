using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;


namespace Socket_Library
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Package
    {
        public UInt32 num;
        public UInt32 module_m;
        public UInt32 module_n;
        public UInt16 bufLen;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = module_l)]
        //public byte[] buf;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct info
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string name;
        public UInt32 size;
        public UInt32 number;
        public UInt16 type;//1，文件，2，目录 
        public info(string n, UInt32 s, int t, UInt32 num)
        {
            name = n;
            size = s;
            type = (UInt16)t;
            number = num;
        }
    }

    public class UpDataEvent : EventArgs
    {
        public float Percentage { get; set; }
        public float speed { get; set; }
        public int Id { get; set; }
    }


    public class DownLoad
    {
        public DownLoad()
        {
            DriveInfoName = RootDirectory.Substring(0);
        }

        public event EventHandler<UpDataEvent> Percentage;

        #region 全局变量
        public float Speed { get; set; }
        public int ID { get; set; }
        public string T_Hash { get; set; }
        int partSize = 20;
        public int PartSize
        {
            get { return partSize; }
            set { partSize = value; }
        }
        List<int> HashNum = new List<int>();
        List<string> Hashlist = new List<string>();
        DownListInfo dli = null;
        SocketModule lls = null;
        public SocketModule Lls
        {
            get { return lls; }
            set { lls = value; }
        }
        int cache_num = 1 * 1024 * 1024;
        public int Cache_num
        {
            get { return cache_num; }
            set { cache_num = value; }
        }
        int module_l = 1024 * 20;
        public int Module_l
        {
            get { return module_l; }
            set { module_l = value; }
        }
        int module_l_Head = 14;
        int module_num = 100;//默认标准块是100的，但是如果文件大小少于module_l * 100就按照有多少module_l来算
        string _rootDirectory = @"D:\TDDOWNLOAD\";
        public string RootDirectory
        {
            get { return _rootDirectory; }
            set { _rootDirectory = value; }
        }
        public int Port { get; set; }
        string DriveInfoName;
        private bool stop_DownLoad = false;
        public bool Stop_DownLoad
        {
            get { return stop_DownLoad; }
            set { stop_DownLoad = value; }
        }
        private int channel = 5;//上传通道数
        public int Channel
        {
            get { return channel; }
            set { channel = value; }
        }
        private ManualResetEvent allDone = new ManualResetEvent(false);
        public ManualResetEvent AllDone
        {
            get { return allDone; }
            set { allDone = value; }
        }

        int _maxseepd = 1;
        public int Maxseepd
        {
            get { return _maxseepd; }
            set { _maxseepd = value * 1024 * 1024; }
        }
        private int sum;
        public int Sum
        {
            get { return sum; }
            set { sum = value; }
        }

        #endregion

        public DownListInfo CreatDownLoad(LoadInfo d)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoit = new IPEndPoint(IPAddress.Any, 0);//////////实际运行port要设置为0
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);  //SocketOptionName.ReuseAddress是关键            
            sock.Bind(endpoit);
            sock.Listen(Channel);

            Port = ((IPEndPoint)sock.LocalEndPoint).Port;
            lls = new SocketModule(d);
            lls.m = new ModuleFalt();
            lls.PointInfo = new Information();
            dli = new DownListInfo(d);
            d.s = sock;
            int workerThreads, availabeThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
            WaitCallback wcb = new WaitCallback(CreatBeginAccept);
            if (workerThreads > 0)//可用线程数>0
            {
                ThreadPool.QueueUserWorkItem(wcb, d);
            }
            dli.filename = RootDirectory + lls.name;
            dli.Progress_Percentage = lls.Progress_Percentage.ToString() + "%";
            return dli;
        }

        private void CreatBeginAccept(object p)
        {
            LoadInfo d = (LoadInfo)p;
            while (true)
            {
                AllDone.Reset();
                if (Stop_DownLoad) break;
                d.s.BeginAccept(new AsyncCallback(AcceptCallback_Creat), d);
                AllDone.WaitOne();
            }
            d.s.Close();
        }

        public DownListInfo ContinuousDownLoad(Information infor)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoit = new IPEndPoint(IPAddress.Any, 0);
            sock.Bind(endpoit);
            sock.Listen(Channel);
            Port = ((IPEndPoint)sock.LocalEndPoint).Port;
            int workerThreads, availabeThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
            WaitCallback wcb = new WaitCallback(p =>
            {
                while (true)
                {
                    AllDone.Reset();
                    if (Stop_DownLoad) break;
                    sock.BeginAccept(new AsyncCallback(AcceptCallback_Continuous), sock);
                    AllDone.WaitOne();
                }
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
            dli = new DownListInfo();
            lls = new SocketModule(infor);
            lls.PointInfo.Hash = infor.Hash;
            dli.name = lls.name;
            dli.type = lls.type;
            dli.Size = lls.t_size;
            dli.filename = lls.filename;
            dli.Progress_Percentage = lls.Progress_Percentage.ToString() + "%";
            return dli;
        }

        private void AcceptCallback_Continuous(IAsyncResult ar)
        {
            AllDone.Set();
            Socket sock = (Socket)ar.AsyncState;
            Socket newSock = null;
            try
            {
                newSock = sock.EndAccept(ar);
            }
            catch (Exception)
            {
                //Console.WriteLine(e.Message);
                return;
            }
            byte[] bu = new byte[270];
            newSock.Receive(bu, SocketFlags.None);
            if (lls.l_S.Count >= Channel)
            {
                newSock.Close();
                return;
            }
            SockInfo si = new SockInfo(newSock);
            si.i.type = (UInt16)lls.PointInfo.type;
            si.i.size = (UInt32)lls.PointInfo.File.Length;
            si.i.name = lls.PointInfo.name;
            si.i.number = lls.PointInfo.total_file_number;
            si.file = lls.PointInfo.File;
            si.ID = lls.l_S.Count;
            lls.l_S.Add(si);
            Serialization(lls.PointInfo);
            CreatThreadPool_Download(lls);

        }

        private void AcceptCallback_Creat(IAsyncResult ar)
        {
            AllDone.Set();
            // Get the socket that handles the client request.
            LoadInfo d = (LoadInfo)ar.AsyncState;
            Socket sock = d.s;
            Socket newSock = null;
            try
            {
                newSock = sock.EndAccept(ar);
            }
            catch (Exception)
            {
                //Console.WriteLine(e.Message);
                return;
            }

            CreatProject(newSock, d);

        }

        private void CreatProject(Socket newSock, LoadInfo d)
        {
            int r;
            info i = new info();
            FileStream file = null;
            #region 获取信息后，打开文件或者创建文件，目录
            do
            {
                byte[] bu = new byte[270];
                int n = 0;
                n = r = newSock.Receive(bu, SocketFlags.None);

                while (n != 270)
                {
                    r = newSock.Receive(bu, r, bu.Length, SocketFlags.None);
                    n += r;
                }
                StructByte s = new StructByte();
                i = (info)s.BytesToStruct(bu, i.GetType());
            } while (r < 0);

            if (lls.l_S.Count >= Channel)
            {
                newSock.Close();
                return;
            }
            DriveInfo D = new DriveInfo(DriveInfoName);
            if (D.TotalFreeSpace >= i.size)
            {
                if (i.type == 1)
                {
                    try
                    {
                        file = new FileStream((RootDirectory + d.name), FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        file = new FileStream((RootDirectory + d.name), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                        file.SetLength(d.size);
                    }

                    i.name = d.name;
                    i.size = (UInt32)d.size;

                    #region 限制文件大小小于module_l * 100这个数的不能开第2条线程以上
                    if (file.Length <= module_l * 100)
                    {
                        if (lls.l_S.Count < 1)
                        {
                            lls.l_S.Add(new SockInfo(newSock, i, file, lls.l_S.Count));
                            module_num = (int)(file.Length / module_l) + 1;
                        }
                        else
                        {
                            byte[] b = new byte[4];
                            b = BitConverter.GetBytes(-1);
                            newSock.Send(b);
                            newSock.Close();
                            return;
                        }
                    }
                    else
                    {
                        lls.l_S.Add(new SockInfo(newSock, i, file, lls.l_S.Count));
                        module_num = 100;
                    }
                    #endregion

                }
                else
                {
                    DirectoryInfo target = new DirectoryInfo(RootDirectory + d.name);
                    target.Create();
                    lls.l_S.Add(new SockInfo(newSock, i, target, lls.l_S.Count));
                }
            }
            else ///报错
            {
                newSock.Close();
                return;
            }
            #endregion

            #region 创建数据
            if (lls.l_S.Count == 1)
            {
                lls.PointInfo.type = lls.l_S[0].i.type;
                lls.PointInfo.Di_file_num = lls.PointInfo.Di_file_num;
                lls.t_size = lls.PointInfo.t_Size = lls.l_S[0].i.size;
                if (lls.l_S[0].i.type == 1)
                {
                    lls.type = lls.l_S[0].file.Name.Substring(lls.l_S[0].file.Name.LastIndexOf("."));
                    lls.filename = lls.PointInfo.name = lls.l_S[0].file.Name;
                    lls.PointInfo.filename = lls.l_S[0].file.Name;
                }
                else
                {
                    lls.type = "文件夹";
                    lls.filename = lls.PointInfo.name = lls.l_S[0].di.FullName;
                    lls.PointInfo.total_file_number = lls.l_S[0].i.number;
                }
                lls.name = lls.filename.Substring(lls.filename.LastIndexOf(@"\") + 1);
            }
            #endregion

            CreatThreadPool_Download(lls);
        }

        private void CreatThreadPool_Download(SocketModule ss)
        {
            int workerThreads, availabeThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
            if (ss.l_S[0].i.type == 1)
            {
                WaitCallback wcb = new WaitCallback(SockListen);
                if (workerThreads > 0)//可用线程数>0
                {
                    ThreadPool.QueueUserWorkItem(wcb, ss);
                }
                else
                {
                    //to do 可以采取一种策略，让这个任务合理地分配给线程
                }
            }
            else
            {
                WaitCallback wcb = new WaitCallback(Download_Directory);
                if (workerThreads > 0)//可用线程数>0
                {
                    ThreadPool.QueueUserWorkItem(wcb, ss);
                }
                else
                {
                    //to do 可以采取一种策略，让这个任务合理地分配给线程
                }
            }
        }

        private void SockListen(object obj)
        {
            SocketModule s_i = (SocketModule)obj;
            int a = s_i.l_S.Count;
            if (a == 0) return;
            for (int i = 0; i < a; i++)
            {
                if (s_i.l_S[i].Stare) continue;
                int asdf = s_i.l_S[i].ID;
                int bufnum = cache_num / module_l + 1;//单位为（module_l（b）大小）           
                s_i.l_S[asdf].buf = new byte[bufnum * module_l];
                s_i.l_S[asdf].I_BufNum = bufnum;
                WaitCallback wcb = new WaitCallback(p => ThreadFun_Download_File(s_i.l_S[asdf], s_i));
                int workerThreads, availabeThreads;
                ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
                if (workerThreads > 0)//可用线程数>0
                {
                    ThreadPool.QueueUserWorkItem(wcb);
                }
            }
        }

        private void ThreadFun_Download_File(SockInfo s, SocketModule sm)
        {
            SockInfo s_i = s;
            SocketModule SM = sm;
            int r = 0;
            if (Download_File(ref s_i, ref SM))
            {
                byte[] b2 = new byte[40];
                r = s.s.Receive(b2, SocketFlags.None);
                T_Hash = System.Text.Encoding.UTF8.GetString(b2);

                do
                {
                    r = s.s.Receive(b2, SocketFlags.None);
                    if (r != 0) Hashlist.Add(System.Text.Encoding.UTF8.GetString(b2));
                } while (r > 0);
                s_i.s.Close();
            }
            Console.WriteLine("asd");
        }

        private bool Download_File(ref SockInfo s, ref SocketModule sm)
        {
            SockInfo s_i = s;
            FileStream file = s_i.file;
            SocketModule SM = sm;

            if (s_i.i.type == 2) module_num = 100;
            if (s_i.i.type == 2 && file.Length <= module_l * 100)
            {
                module_num = (int)(file.Length / module_l) + 1;
            }

            #region 本线程的局部变量
            Stopwatch time = new Stopwatch();
            StructByte S_B = new StructByte();
            int module_m = (int)(file.Length / module_l / 100) + 1;
            if ((file.Length / module_l) < 99 * module_m && module_m != 1)
            {
                module_m = (int)(file.Length / module_l / 10) + 1;
                module_num = 10;
            }
            int module = module_l + module_l_Head;
            int num1 = 0;//粘包造成的第二次起初数组位置
            int j = 0;//粘包造成的第二次复制起初数组位置
            byte[] buf = new byte[module];
            byte[] bu = new byte[module];
            byte[] p_buf = new byte[module_l];
            byte[] p_Head = new byte[module_l_Head];
            byte[] b = new byte[12];
            byte[] b1 = new byte[4];
            byte[] b2 = new byte[4];
            byte[] b3 = new byte[4];
            int nRecv = 0;
            UInt32 sPos = 0;
            UInt32 sPos_n = 0;
            UInt32 num = 0;
            long nLn = 0;
            long sLn = 0;

            int onetime;
            double frequency;
            bool Min;
            int CurrentSeepd = Maxseepd;
            #endregion

            #region 控制上传端发需要的片段
            if (lls.PointInfo.Hash)
            {
                if (file.Length < 20 * 1024 * 1024)
                {
                    lls.PointInfo.sPos_n[0] = sm.m.sPos_n[0] = (UInt32)0;
                    SM.m.Complete = new bool[100];
                    lls.PointInfo.Complete = new bool[100];
                }
                else
                {
                    int num_p = partSize * 1024 / 20;
                    lls.PointInfo.sPos_n = new uint[100];
                    foreach (var item in HashNum)
                    {
                        int pos = item * num_p;
                        int id = pos / module_m;
                        lls.PointInfo.sPos_n[id] = sm.m.sPos_n[id] = (UInt32)(pos - id * module_m);

                        int pos1 = (item + 1) * num_p;
                        int id1 = pos1 / module_m;
                        if (id1 * module_m > file.Length)
                        {
                            lls.PointInfo.sPos_n[id1] = sm.m.sPos_n[id1] = 0;
                            for (int i = id; i < 100; i++)
                            {
                                SM.m.Complete[i] = lls.PointInfo.Complete[i] = false;
                            }
                        }
                        else
                        {
                            lls.PointInfo.sPos_n[id1] = sm.m.sPos_n[id1] = (UInt32)(pos1 - id1 * module_m);
                            for (int i = id; i <= id1; i++)
                            {
                                SM.m.Complete[i] = lls.PointInfo.Complete[i] = false;
                            }
                        }
                    }
                }
                HashNum.Clear();
            }
            #endregion

            #region 开始定位
            int _i = 0;
            for (; _i < module_num; _i++)
            {
                if (!SM.m.RW[_i] && !SM.m.Complete[_i] || SM.m.sPos_n[_i] != 0)
                {
                    SM.PointInfo.RW[_i] = SM.m.RW[_i] = true;
                    b1 = BitConverter.GetBytes(_i);
                    b2 = BitConverter.GetBytes(SM.m.sPos_n[_i]);
                    b3 = BitConverter.GetBytes(SM.m.sPos_n_l[_i]);
                    Array.Copy(b1, b, b1.Length);
                    Array.Copy(b2, 0, b, b1.Length, b2.Length);
                    Array.Copy(b3, 0, b, b2.Length, b3.Length);
                    try
                    {
                        s_i.s.Send(b);
                    }
                    catch (Exception)
                    {
                        lls.l_S.RemoveAt(s_i.ID);
                        for (int i = 0; i < lls.l_S.Count; i++)
                        {
                            lls.l_S[i].ID = i;
                        }
                        return false;
                    }
                    break;
                }
            }
            if (_i == module_num)
            {
                b1 = BitConverter.GetBytes(-1);
                b2 = BitConverter.GetBytes(0);
                Array.Copy(b1, b, b1.Length);
                Array.Copy(b2, 0, b, b1.Length, b2.Length);
                try
                {
                    s_i.s.Send(b);
                }
                catch (Exception)
                {
                    lls.l_S.RemoveAt(s_i.ID);
                    for (int i = 0; i < lls.l_S.Count; i++)
                    {
                        lls.l_S[i].ID = i;
                    }
                    return false;
                }
                if (s_i.i.type == 1)
                {
                    FileInfo d = new FileInfo(SM.PointInfo.name + ".dat");
                    d.Delete();
                }
                nRecv = -1;
            }
            #endregion
            time.Start();
            do
            {
                if (nRecv == -1) break;
                #region 线程主工作循环
                try
                {
                    nRecv = s_i.s.Receive(buf, SocketFlags.None);
                }
                catch (Exception)
                {
                    break;
                }
                if (nRecv == 0) break;

                #region 粘包处理算法
                if (module >= num1 + nRecv)
                {
                    Array.Copy(buf, 0, bu, num1, nRecv);
                    num1 += nRecv;
                }
                else
                {
                    Array.Copy(buf, 0, bu, num1, module - num1);
                    j = module - num1;
                    num1 = bu.Length;
                    //Sticky_num++;
                }
                #endregion

                if (num1 == bu.Length)
                {
                    CurrentSeepd = (int)(Maxseepd / sum);
                    frequency = (int)(CurrentSeepd / buf.Length);
                    Min = false;
                    if (frequency == 0)
                    {
                        frequency = (double)(buf.Length / CurrentSeepd);
                        Min = true;
                    }

                    onetime = (int)(1000 / frequency);
                    if (Min)
                        onetime = (int)(1000 * frequency);
                    if (onetime > 0)
                        Thread.Sleep(onetime);

                    Array.Copy(bu, 0, p_Head, 0, p_Head.Length);
                    Array.Copy(bu, p_Head.Length, p_buf, 0, p_buf.Length);

                    Package p = (Package)S_B.TestStruct(p_Head);
                    sPos_n = p.module_m;
                    sPos = p.num;
                    num = p.module_n;
                    nLn += p.bufLen;
                    sLn += p.bufLen;

                    #region 缓冲区算法
                    Array.Copy(p_buf, 0, s_i.buf, s_i.BufNum * module_l, p_buf.Length);
                    s_i.BufNum++;
                    if (s_i.I_BufNum == s_i.BufNum || sPos_n == num + 1)
                    {
                        // if (sPos == 1 && !lls.PointInfo.Hash) s_i.buf = new byte[s_i.buf.Length - 1];////测试文件出错的
                        file.Seek((sPos * module_m + num - s_i.BufNum + 1) * module_l, SeekOrigin.Begin);
                        file.Write(s_i.buf, 0, (s_i.BufNum - 1) * module_l + p.bufLen);
                        SM.PointInfo.sPos_n[sPos] = num;
                        SM.DownLoadSize = SM.PointInfo.DownLoadSize += nLn;
                        //if (time.ElapsedMilliseconds > 1000)
                        {
                            Speed = 0;
                            time.Stop();
                            s_i.Speed = (int)(sLn / ((int)time.ElapsedMilliseconds / 1.024));
                            if (s_i.i.type == 2)
                            {
                                Speed = s_i.Speed;
                            }
                            else
                            {
                                foreach (var item in Lls.l_S)
                                {
                                    Speed += item.Speed;
                                }
                            }
                            if (Percentage != null) Percentage.BeginInvoke(this, new UpDataEvent() { Percentage = (float)SM.DownLoadSize / (float)SM.t_size * 100, Id = ID, speed = Speed }, null, null);
                            time.Restart();
                            sLn = 0;
                        }
                        nLn = 0;
                        s_i.BufNum = 0;
                        Serialization(SM.PointInfo);
                        if (Stop_DownLoad)
                        {
                            break;
                        }
                    }
                    #endregion

                    #region 呼叫让上传段发送某个块
                    if (sPos_n == num + 1)
                    {
                        int i;
                        for (i = 0; i < module_num; i++)
                        {
                            if (i == sPos)
                            {
                                SM.PointInfo.sPos_n[i] = 0;
                                SM.PointInfo.RW[i] = SM.m.RW[sPos] = false;
                                SM.PointInfo.Complete[i] = SM.m.Complete[sPos] = true;
                            }
                            if (!SM.m.RW[i] && !SM.m.Complete[i])
                            {
                                SM.PointInfo.RW[i] = SM.m.RW[i] = true;
                                b1 = BitConverter.GetBytes(i);
                                b2 = BitConverter.GetBytes(SM.m.sPos_n[i]);
                                b3 = BitConverter.GetBytes(SM.m.sPos_n_l[i]);
                                Array.Copy(b1, b, b1.Length);
                                Array.Copy(b2, 0, b, b1.Length, b2.Length);
                                Array.Copy(b3, 0, b, b2.Length, b3.Length);
                                try
                                {
                                    s_i.s.Send(b, SocketFlags.None);
                                }
                                catch (Exception)
                                {
                                    ;
                                }
                                break;
                            }
                        }
                        if (i == module_num)
                        {
                            nRecv = 0;
                            if (s_i.i.type == 1)
                            {
                                FileInfo d = new FileInfo(SM.PointInfo.name + ".dat");
                                d.Delete();
                            }
                            break;
                        }
                    }
                    #endregion

                    num1 = 0;
                    if (j != 0)
                    {
                        Array.Copy(buf, j, bu, num1, nRecv - j);
                        num1 += nRecv - j;
                        j = 0;
                    }

                }
                //#if DEBUG
                //                Console.WriteLine("4!!!{0}     {1}    {2}    {3}     {4}    {5}", sPos, nRecv, num, num1, j, Sticky_num);
                //#endif
                #endregion

            } while (nRecv > 0);
            SM.PointInfo.RW[sPos] = SM.m.RW[sPos] = false;
            if (sPos != 99) SM.PointInfo.RW[sPos + 1] = SM.m.RW[sPos + 1] = false;
            b = BitConverter.GetBytes(-1);
            try
            {
                s_i.s.Send(b, SocketFlags.None);
            }
            catch (Exception)
            {
                if (lls.l_S.Count != 0)
                {
                    lls.l_S.RemoveAt(s_i.ID);
                    for (int i = 0; i < lls.l_S.Count; i++)
                    {
                        lls.l_S[i].ID = i;
                    }
                    return false;
                }
            }
            file.Close();
            return true;
        }

        private void Download_Directory(object obj)
        {
            SocketModule s_i = (SocketModule)obj;
            SockInfo si = null;
            Socket s = s_i.l_S[0].s;

            try
            {
                s.Send(BitConverter.GetBytes(s_i.PointInfo.Di_file_num), SocketFlags.None);
            }
            catch (Exception)
            {
                s_i.l_S.RemoveAt(0);
                return;
            }

            int r = 0;
            info i = new info();
            int s_num = 0;
            bool break_off = true;
            #region 主循环
            do
            {
                s_num++;

                byte[] bu = new byte[270];
                int n = 0;
                try
                {
                    n = r = s.Receive(bu, SocketFlags.None);
                    if (n == 0) return;
                    while (n != 270)
                    {
                        r = s.Receive(bu, r, bu.Length, SocketFlags.None);
                        n += r;
                    }
                }
                catch (Exception)
                {
                    s_i.l_S.RemoveAt(0);
                    return;
                }
                StructByte S_B = new StructByte();
                i = (info)S_B.BytesToStruct(bu, i.GetType());

                if (i.type == 0) break;
                if (i.type == 1)
                {
                    s_i.PointInfo.filename = _rootDirectory + i.name;
                    int bufnum = cache_num / module_l;//单位为（module_l（b）大小）
                    FileStream file = new FileStream(_rootDirectory + i.name, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                    file.SetLength(i.size);
                    si = new SockInfo(s_i.l_S[0].s, s_i.l_S[0].i, file, 0);
                    si.buf = new byte[bufnum * module_l];
                    si.I_BufNum = bufnum;
                    break_off = Download_File(ref si, ref s_i);
                    if (!Stop_DownLoad && break_off)
                    {
                        s_i.m = new ModuleFalt();
                        s_i.PointInfo.Complete = new bool[100];
                        s_i.PointInfo.RW = new bool[100];
                        s_i.PointInfo.sPos_n = new uint[100];
                        Serialization(s_i.PointInfo);
                    }
                }
                else
                {
                    DirectoryInfo target = new DirectoryInfo(_rootDirectory + i.name);
                    target.Create();
                }
                s_i.PointInfo.Di_file_num++;
                if (s_i.l_S.Count == 0)
                {
                    s_i.PointInfo.Di_file_num--;
                    break;
                }
            } while (s_num < s_i.l_S[0].i.number && !Stop_DownLoad && break_off);
            #endregion
            if (!Stop_DownLoad && break_off)
            {
                FileInfo d = new FileInfo(s_i.PointInfo.name + ".dat");
                d.Delete();
            }
            if (break_off) s.Close();

            //   Console.WriteLine("asdf");////////////////////////////////////////
        }

        private void Serialization(Information i)
        {
            FileStream fileStream = new FileStream(i.name + ".dat", FileMode.Create);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(fileStream, i);
            fileStream.Close();
        }

        public Information RSerialization(string name)
        {
            Information i = new Information();
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(name + ".dat", FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                return i;
            }
            BinaryFormatter b = new BinaryFormatter();
            i = b.Deserialize(fileStream) as Information;
            fileStream.Close();
            return i;
        }

        public bool CheckHash(string fileName)
        {
            List<string> hashList = null;

            if (System.IO.File.Exists(fileName))
            {
                ///文件“D:\TDDOWNLOAD\QQ2012Beta1.exe”正由另一进程使用，因此该进程无法访问此文件
                #region 分块比较hash
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    //计算文件的SHA1值
                    System.Security.Cryptography.SHA1 calculator = System.Security.Cryptography.SHA1.Create();

                    long nSize = fs.Length;

                    hashList = new List<string>();
                    int bufSize = partSize * 1024 * 1024;
                    byte[] buffer = new byte[bufSize];
                    byte[] hashBuf;
                    int nLen = 0;
                    int nPos = 0;
                    int num = 0;
                    StringBuilder strBuild = new StringBuilder();

                    do
                    {
                        nLen = fs.Read(buffer, 0, bufSize);
                        nPos += nLen;

                        hashBuf = calculator.ComputeHash(buffer);
                        strBuild.Clear();
                        foreach (var item in hashBuf)
                        {
                            strBuild.Append(item.ToString("x2"));
                        }
                        if (strBuild.ToString().CompareTo(Hashlist[num]) != 0) HashNum.Add(num);
                        num++;

                    } while (nPos != nSize);

                    calculator.Clear();

                }//关闭文件流           
                #endregion
            }

            if (HashNum.Count != 0)
            {
                Lls.PointInfo.DownLoadSize = Lls.DownLoadSize -= HashNum.Count * partSize * 1024 * 1024;
                lls.PointInfo.Hash = true;
                Serialization(Lls.PointInfo);
                return true;
            }
            Lls.PointInfo.DownLoadSize = Lls.DownLoadSize = Lls.t_size;
            return false;
        }
    }

    public class Upload
    {
        public event EventHandler<EventArgs> Updata;
        public event EventHandler<DeleteUpLoadList> Dele_Updata;
        #region 全局变量
        public int ID { get; set; }
        public float Speed { get; set; }
        string T_Hash;
        List<string> Hashlist = new List<string>();
        StructByte S_B = new StructByte();
        int module_l = 1024 * 20;
        public int Module_l
        {
            get { return module_l; }
            set { module_l = value; }
        }
        int module_l_Head = 14;
        int module_num = 100;
        int cache_num = 2 * 1024 * 1024;
        public int Cache_num
        {
            get { return cache_num; }
            set { cache_num = value; }
        }
        List<SockInfo_U> l_s = new List<SockInfo_U>();

        int maxSeepd = 10;
        public int MaxSeepd
        {
            get { return maxSeepd; }
            set { maxSeepd = value * 1024 * 1024; }
        }
        public int sum { get; set; }
        #endregion

        public bool Existence(string filename)
        {
            bool f = false;
            if (System.IO.Directory.Exists(filename) || System.IO.File.Exists(filename)) f = true;
            return f;
        }

        public void CreatUpload(string filename, string IP, int port, string t_Hash, List<string> hash)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint endpoit = new IPEndPoint(IPAddress.Parse(IP), port);
            int m_list_num = 0;
            do
            {
                try
                {
                    if (m_list_num == 10)
                    {
                        break;
                    }
                    sock.Connect(endpoit);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(5000);
                }
            } while (true);

            DirectoryInfo di = new DirectoryInfo(filename);
            if (di.Attributes == FileAttributes.Directory)
            {
                l_s.Add(new SockInfo_U(2, sock, di));
            }
            else
            {
                FileStream file = new FileStream((filename), FileMode.Open, FileAccess.Read, FileShare.Read);
                l_s.Add(new SockInfo_U(1, sock, file));
            }
            Hashlist = hash;
            T_Hash = t_Hash;
            CreatThreadPool_Upload();
        }

        private void CreatThreadPool_Upload()
        {
            for (int i = 0; i < l_s.Count; i++)
            {
                if (l_s[i].type == 2)
                {
                    #region 启动下载目录
                    WaitCallback wcb = new WaitCallback(ThreadFunUpload_Directory);
                    int workerThreads, availabeThreads;
                    ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
                    if (workerThreads > 0)//可用线程数>0
                    {
                        ThreadPool.QueueUserWorkItem(wcb, l_s[i]);
                    }
                    else
                    {
                        //to do 可以采取一种策略，让这个任务合理地分配给线程
                    }
                    #endregion
                }
                else
                {
                    #region 启动下载单个文件
                    WaitCallback wcb = new WaitCallback(ThreadFun_Upload_File);
                    int workerThreads, availabeThreads;
                    ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
                    if (workerThreads > 0)//可用线程数>0
                    {
                        ThreadPool.QueueUserWorkItem(wcb, l_s[i]);
                    }
                    else
                    {
                        //to do 可以采取一种策略，让这个任务合理地分配给线程
                    }
                    #endregion
                }
            }
        }

        private void ThreadFun_Upload_File(object obj)
        {
            SockInfo_U s = (SockInfo_U)obj;
            FileStream file = s.file;
            Socket sock = s.s;
            Upload_File(file, sock, file.Name.Substring(file.Name.LastIndexOf(@"\") + 1));

            byte[] b2 = System.Text.Encoding.Default.GetBytes(T_Hash);
            try
            {
                sock.Send(b2, SocketFlags.None);

                for (int i = 0; i < Hashlist.Count; i++)
                {
                    b2 = System.Text.Encoding.Default.GetBytes(Hashlist[i]);
                    try
                    {
                        sock.Send(b2, SocketFlags.None);
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
                // Console.WriteLine("asdfasdfasdf");
                sock.Close();
            }
            catch (Exception)
            {
                ;
            }
            if (Dele_Updata != null) Dele_Updata.BeginInvoke(this, new DeleteUpLoadList() { Id = ID }, null, null);
        }

        private void Upload_File(FileStream f, Socket s, string socketname)
        {
            FileStream file = f;
            Socket sock = s;
            module_num = 100;

            byte[] bu = new byte[270];
            bu = S_B.StructToBytes(new info(socketname, (UInt32)file.Length, 1, 1));
            try
            {
                sock.Send(bu, bu.Length, SocketFlags.None);
            }
            catch (Exception)
            {
                ;
            }

            #region 变量
            Stopwatch time = new Stopwatch();
            int module_n_m = (int)(file.Length / module_l / 100) + 1;
            if ((file.Length / module_l) < 99 * module_n_m && module_n_m != 1)
            {
                module_n_m = (int)(file.Length / module_l / 10) + 1;
                module_num = 10;
            }
            int module = module_l + module_l_Head;
            int l_module = (int)(file.Length / module_l - (module_num - 1) * module_n_m + 1);
            if (file.Length % module_l == 0)
            {
                l_module = (int)(file.Length / module_l - (module_num - 1) * module_n_m);
            }
            int sPos = 0;
            int sPos_n = 0;
            int nLen = 0;
            int n = 0;
            byte[] buf = new byte[module];
            byte[] b = new byte[12];
            byte[] p_buf = new byte[module_l];
            byte[] p_Head = new byte[module_l_Head];

            double frequency;
            bool Min;
            int onetime;
            int CurrentSeepd = MaxSeepd;
            #endregion


            #region 缓冲设置变量
            int bufnum = cache_num / module_l + 1;//单位为（module_l（b）大小）
            bufnum = 20;
            byte[] buf1 = new byte[bufnum * module_l];
            Cache_info c = new Cache_info();
            #endregion

            Package package;
            package.module_m = (UInt32)module_n_m;
            package.bufLen = 0;
            do
            {
                CurrentSeepd = (int)(MaxSeepd / sum);
                frequency = (int)(CurrentSeepd / buf.Length);
                Min = false;
                if (frequency == 0)
                {
                    frequency = (double)(buf.Length / CurrentSeepd);
                    Min = true;
                }
                Speed = CurrentSeepd;

                #region 主循环
                if (sPos_n == 0)
                {
                    try
                    {
                        sock.Receive(b, SocketFlags.None);
                    }
                    catch (Exception)
                    {

                        break;
                    }
                    sPos = BitConverter.ToInt32(b, 0);
                    sPos_n = BitConverter.ToInt32(b, 4);
                    if (sPos == -1) break;
                    int bit = BitConverter.ToInt32(b, 8);
                    if (bit != 0) module_n_m = BitConverter.ToInt32(b, 8);
                    c.sPos_n = module_n_m - sPos_n;
                    c.Chache_n = sPos_n;
                    c.bufLen = c.Chache_m = 0;
                }
                if (sPos == module_num - 1)
                {
                    package.module_m = (UInt32)l_module;
                    c.sPos_n = l_module;
                }

                #region 上传缓冲区算法
                if (c.Chache_m * module_l >= c.bufLen)
                {
                    if (c.sPos_n >= bufnum)
                    {
                        file.Seek((sPos * module_n_m + c.Chache_n) * module_l, SeekOrigin.Begin);
                        c.bufLen = file.Read(buf1, 0, buf1.Length);
                        c.sPos_n -= bufnum;
                        c.Chache_n += bufnum;
                        c.Chache_m = 0;
                    }
                    else
                    {
                        if (c.sPos_n != 0)
                        {
                            file.Seek((sPos * module_n_m + c.Chache_n) * module_l, SeekOrigin.Begin);
                            c.bufLen = file.Read(buf1, 0, c.sPos_n * module_l);
                            c.Chache_n = c.sPos_n = 0;
                            c.Chache_m = 0;
                        }
                    }
                }

                if (c.Chache_m * module_l < c.bufLen)
                {
                    int io_num = module_l;
                    if (c.bufLen / module_l == c.Chache_m)
                    {
                        io_num = c.bufLen - c.Chache_m * module_l;
                    }
                    Array.Copy(buf1, c.Chache_m * module_l, p_buf, 0, io_num);
                    package.bufLen = (UInt16)io_num;
                    c.Chache_m++;
                }
                else
                {
                    package.bufLen = 0;
                }
                if (c.bufLen == 0)
                {
                    package.bufLen = (UInt16)c.bufLen;
                }
                #endregion

                package.num = (UInt32)sPos;
                package.module_n = (UInt32)sPos_n;
                n += package.bufLen;
                nLen += n;
                p_Head = S_B.StructToBytes(package);
                Array.Copy(p_Head, 0, buf, 0, p_Head.Length);
                Array.Copy(p_buf, 0, buf, p_Head.Length, p_buf.Length);
                if (package.bufLen >= 0)
                {
                    int nSend = 0;
                    try
                    {
                        nSend = sock.Send(buf, buf.Length, SocketFlags.None);
                    }
                    catch (Exception)
                    {

                        break;
                    }
                    if (Updata != null) Updata.BeginInvoke(this, new EventArgs(), null, null);
                    //#if DEBUG
                    //                    Console.WriteLine("{0}    {1}           {2}           {3}", sPos, nSend, nLen, n);
                    //#endif
                    sPos_n++;
                    if (sPos_n == module_n_m)
                    {
                        sPos++;
                        sPos_n = 0;
                    }
                }
                if (sPos == module_num - 1 && sPos_n == l_module)
                {
                    try
                    {
                        sock.Receive(b, SocketFlags.None);
                    }
                    catch (Exception)
                    {

                        break;
                    }
                    sPos = BitConverter.ToInt32(b, 0);
                    if (sPos == -1) break;
                }
                //if (nLen == file.Length) break;
                #endregion

                onetime = (int)(1000 / frequency);
                if (Min) onetime = (int)(1000 * frequency);
                if (onetime > 0) Thread.Sleep(onetime);
            } while (sPos >= 0);
            Console.WriteLine("adsf");
            file.Close();
        }

        private void ThreadFunUpload_Directory(object obj)
        {
            StructByte S_B = new StructByte();
            SockInfo_U s = (SockInfo_U)obj;
            Directory_info d = new Directory_info();
            List<string> l = new List<string>();
            d.name = s.di.Name;
            GetAllDirList(s.di.FullName, l, d);
            byte[] bu = new byte[270];
            string Root_directory_name = s.di.FullName.Substring(s.di.FullName.LastIndexOf(@"\") + 1);

            bu = S_B.StructToBytes(new info(Root_directory_name, (UInt32)d.size, 2, (UInt32)l.Count));
            s.s.Send(bu, bu.Length, SocketFlags.None);
            Thread.Sleep(1000);
            s.s.Receive(bu, SocketFlags.None);
            int i = BitConverter.ToInt32(bu, 0);
            Thread.Sleep(10);
            try
            {
                for (; i < l.Count; i++)
                {
                    string name = l[i];
                    DirectoryInfo di = new DirectoryInfo(name);
                    string n = name.Substring(name.IndexOf(Root_directory_name));
                    if (di.Attributes == FileAttributes.Directory)
                    {

                        bu = S_B.StructToBytes(new info(n, 0, 2, 1));
                        s.s.Send(bu, bu.Length, SocketFlags.None);
                    }
                    else if (di.Attributes == FileAttributes.Normal || di.Attributes == FileAttributes.Archive)
                    {
                        Upload_File(new FileStream((name), FileMode.Open, FileAccess.Read, FileShare.Read), s.s, n);
                    }
                }
                bu = S_B.StructToBytes(new info("asdf", 0, 0, 0));
                s.s.Send(bu, bu.Length, SocketFlags.None);
                s.s.Close();
            }
            catch (Exception)
            {
                ;
            }
            if (Dele_Updata != null) Dele_Updata.BeginInvoke(this, new DeleteUpLoadList() { Id = ID }, null, null);
            //  Console.WriteLine("asdf");
        }

        private List<string> GetAllDirList(string strBaseDir, List<string> list, Directory_info d)
        {
            DirectoryInfo di = new DirectoryInfo(strBaseDir);
            DirectoryInfo[] diA = di.GetDirectories();
            FileInfo[] files = di.GetFiles();
            foreach (var item in files)
            {
                d.size += item.Length;
                list.Add(item.FullName);
            }
            foreach (var item in diA)
            {
                list.Add(item.FullName);
                //diA[i].FullName是某个子目录的绝对地址，把它记录在ArrayList中
                GetAllDirList(item.FullName, list, d);
                //注意：递归了。逻辑思维正常的人应该能反应过来
            }
            return list;
        }

    }

    public class DeleteUpLoadList : EventArgs
    {
        public int Id { get; set; }
    }

    public class StructByte
    {
        #region 两种结构体转化byte[]
        public object BytesToStruct(byte[] bytes, Type type)
        {
            //得到结构的大小
            int size = Marshal.SizeOf(type);
            // Log(size.ToString(), 1);
            //byte数组长度小于结构的大小 不能转换则返回 null
            if (size > bytes.Length)
            {
                //返回空
                return null;
            }
            //分配结构大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回结构
            return obj;
        }

        public object TestStruct(byte[] rawdatas)
        {

            Type anytype = typeof(Package);

            int rawsize = Marshal.SizeOf(anytype);

            if (rawsize > rawdatas.Length) return null;

            IntPtr buffer = Marshal.AllocHGlobal(rawsize);

            Marshal.Copy(rawdatas, 0, buffer, rawsize);

            object retobj = Marshal.PtrToStructure(buffer, anytype);

            Marshal.FreeHGlobal(buffer);

            return retobj;

        }
        #endregion
        public byte[] StructToBytes(object obj)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(obj);
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(obj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }
    }




}
