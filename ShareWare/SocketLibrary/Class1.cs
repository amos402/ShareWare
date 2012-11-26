using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace SocketLibrary
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
    struct info
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string name;
        public UInt32 size;
        public UInt32 number;
        public UInt16 type;//1，文件，2，目录 
        public info(string n, UInt32 s, int t,UInt32 num)
        {
            name = n;
            size = s;
            type = (UInt16)t;
            number = num;
        }
    }

    public class DownLoad
    {
        public class LoadInfo
        {
            public string name{ get; set; }
            public int size{ get; set; }
            public Socket s{ get; set; }
        }
        class SockInfo
        {
            public Socket s { get; set; }
            public info i;
            public FileStream file { get; set; }
            public DirectoryInfo di { get; set; }
            public int ID { get; set; }//记录第几个ll_S
            public int thread_num { get; set; }
            public int BufNum { get; set; }//记录第几份缓冲
            public int I_BufNum { get; set; }//记录第几份缓冲
            public byte[] buf { get; set; }
            public SockInfo(Socket s1, info i1, FileStream F, int id)
            {
                s = s1;
                i = i1;
                file = F;
                ID = id;
                BufNum = thread_num = 0;
                di = null;
            }
            public SockInfo(Socket s1, info i1, DirectoryInfo Di, int id)
            {
                s = s1;
                i = i1;
                file = null;
                ID = id;
                BufNum = thread_num = 0;
                di = Di;
            }
            public SockInfo(Socket s1, int id)
            {
                s = s1;
                i = new info();
                ID = id;
                BufNum = thread_num = 0;
                di = null;
            }
        }
        class ModuleFalt
        {
            public bool[] Complete { get; set; }
            public bool[] RW { get; set; }
            public UInt32[] sPos_n { get; set; }
            public ModuleFalt()
            {
                Complete = new bool[100];
                RW = new bool[100];
                sPos_n = new UInt32[100];
            }
            public ModuleFalt(bool[] mComplete, bool[] mRW, UInt32[] msPos_n)
            {
                Complete = new bool[100];
                RW = new bool[100];
                sPos_n = new UInt32[100];
                Array.Copy(mComplete, Complete, Complete.Length);
                Array.Copy(mRW, RW, RW.Length);
                Array.Copy(msPos_n, sPos_n, sPos_n.Length);
            }
        }
        class SocketModule
        {
            public int type { get; set; }//1，文件，2，目录 
            public int State { get; set; }//0，暂停，1，开始，2，下载中，3，已完成
            public int ID { get; set; }//记录第几个ll_S           
            private int com_Thread_num;
            public int Com_Thread_num
            {
                get { return Interlocked.Increment(ref com_Thread_num); }
                set { com_Thread_num = value; }
            }
            public ModuleFalt m { get; set; }
            public List<SockInfo> l_S { get; set; }
            public Information PointInfo { get; set; }
            public SocketModule(List<SockInfo> l, int id, int state, int t)
            {
                m = new ModuleFalt();
                l_S = new List<SockInfo>();
                PointInfo = new Information();
                l_S = l;
                ID = id;
                Com_Thread_num = 0;
                State = state;
                PointInfo.type = type = t;
                PointInfo.Di_file_num = 0;
                if (t == 1)
                {
                    PointInfo.name = l[0].file.Name;
                }
                else
                {
                    PointInfo.name = l[0].di.FullName;
                    PointInfo.total_file_number = l_S[0].i.number;
                }
            }
            public SocketModule(Information i,List<SockInfo> l,int id,int state)
            {
                m = new ModuleFalt(i.Complete,i.RW,i.sPos_n);
                l_S = new List<SockInfo>();
                PointInfo = new Information();
                l_S = l;
                ID = id;
                Com_Thread_num = 0;
                State = state;
                PointInfo.type = type = i.type;
                PointInfo.name = i.name;
                PointInfo.filename = i.filename;
                PointInfo.Di_file_num = i.Di_file_num;
                PointInfo.total_file_number = l_S[0].i.size = i.total_file_number;               
            }
        }
        [Serializable]
        public class Information
        {
            public bool[] Complete { get; set; }
            public bool[] RW { get; set; }
            public UInt32[] sPos_n { get; set; }
            public int type { get; set; }
            public string name { get; set; }
            public string filename { get; set; }
            public int Di_file_num { get; set; }
            public UInt32 total_file_number { get; set; }
            [NonSerialized]
            private FileStream file;
            public FileStream File
            {
                get { return new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.Write); }
                set { file = value; }
            }
            public Information()
            {
                Complete = new bool[100];
                RW = new bool[100];
                sPos_n = new UInt32[100];
            }
        }

        public DownLoad()
        {
            DriveInfoName = RootDirectory.Substring(0); 
        }

        #region 全局变量
        List<SocketModule> ll_S = new List<SocketModule>();
        List<SockInfo> l_S = new List<SockInfo>();
        int cache_num = 2 * 1024 * 1024;
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
        string DriveInfoName;
        #endregion

        public void CreatDownLoad(int listennum,LoadInfo d)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoit = new IPEndPoint(IPAddress.Any, 5000);
            sock.Bind(endpoit);
            sock.Listen(listennum);
            int m_s = 0;
            while (true)
            {
                if (m_s == 0)
                {
                    if (l_S != null) l_S.Clear();
                }
                d.s = sock;
                sock.BeginAccept(new AsyncCallback(AcceptCallback_Creat), d);
                Thread.Sleep(100);
                m_s++;
                if (m_s == 20)
                {
                    ll_S.Add(new SocketModule(l_S, ll_S.Count, 1, l_S[0].i.type));
                    CreatThreadPool_Download();
                    break;
                }
            }
        }

        public void ContinuousDownLoad(int listennum, Information infor)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoit = new IPEndPoint(IPAddress.Any, 5000);
            sock.Bind(endpoit);
            sock.Listen(listennum);
            int m_s = 0;
            while (true)
            {
                if (m_s == 0)
                {
                    if (l_S != null) l_S.Clear();
                }
                sock.BeginAccept(new AsyncCallback(AcceptCallback_Continuous), sock);
                Thread.Sleep(100);
                m_s++;
                if (m_s == 20)
                {
                    for (int i = 0; i < l_S.Count; i++)
                    {
                        l_S[i].i.type = (UInt16)infor.type;
                        l_S[i].i.size = (UInt32)infor.File.Length;
                        l_S[i].i.name = infor.name;
                        l_S[i].file = infor.File;
                    }
                    ll_S.Add(new SocketModule(infor, l_S, ll_S.Count, 1));
                    Serialization(ll_S[ll_S.Count - 1].PointInfo);
                    CreatThreadPool_Download();
                    break;
                }
            }
        }

        private void AcceptCallback_Continuous(IAsyncResult ar)
        {
            
            // Get the socket that handles the client request.
            Socket sock = (Socket)ar.AsyncState;
            Socket newSock = null;
            try
            {
                newSock = sock.EndAccept(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            byte[] bu = new byte[270];
            newSock.Receive(bu, SocketFlags.None);
            l_S.Add(new SockInfo(newSock, ll_S.Count));

        }

        private void AcceptCallback_Creat(IAsyncResult ar)
        {

            // Get the socket that handles the client request.
            LoadInfo d = (LoadInfo)ar.AsyncState;
            Socket sock = d.s;
            Socket newSock = null;
            try
            {
                newSock = sock.EndAccept(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            CreatProject(newSock,d);

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
                    r = newSock.Receive(bu, r, bu.Length,SocketFlags.None);
                    n += r;
                }
                i = (info)BytesToStruct(bu, i.GetType());
            } while (r < 0);

            DriveInfo D = new DriveInfo(DriveInfoName);
            if (D.TotalFreeSpace >= i.size)
            {
                if (i.type == 1)
                {
                    try
                    {
                        file = new FileStream((RootDirectory + d.name), FileMode.Open, FileAccess.Write, FileShare.Write);
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        file = new FileStream((RootDirectory + d.name), FileMode.Create, FileAccess.Write, FileShare.Write);
                        file.SetLength(d.size);
                    }

                    i.name = d.name;
                    i.size = (UInt32)d.size;

                    #region 限制文件大小小于module_l * 100这个数的不能开第2条线程以上
                    if (file.Length <= module_l * 100)
                    {
                        if (l_S.Count < 1)
                        {
                            l_S.Add(new SockInfo(newSock, i, file, ll_S.Count));
                            module_num = (int)(file.Length / module_l) + 1;
                        }
                        else
                        {
                            byte[] b = new byte[4];
                            b = BitConverter.GetBytes(-1);
                            newSock.Send(b);
                            newSock.Close();
                        }
                    }
                    else
                    {
                        l_S.Add(new SockInfo(newSock, i, file, ll_S.Count));
                        module_num = 100;
                    }
                    #endregion

                }
                else
                {
                    DirectoryInfo target = new DirectoryInfo(RootDirectory + i.name);
                    target.Create();
                    l_S.Add(new SockInfo(newSock, i, target, ll_S.Count));
                }
            }

            else ///报错
            {

            }

            #endregion
        }

        private void CreatThreadPool_Download()
        {
            for (int i = 0; i < ll_S.Count; i++)
            {
                if (ll_S[i].State > 1) break;
                if (ll_S[i].type == 1)
                {
                    WaitCallback wcb = new WaitCallback(SockListen);
                    int workerThreads, availabeThreads;
                    ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
                    if (workerThreads > 0)//可用线程数>0
                    {
                        ThreadPool.QueueUserWorkItem(wcb, ll_S[i]);
                    }
                    else
                    {
                        //to do 可以采取一种策略，让这个任务合理地分配给线程
                    }
                }
                else
                {
                    WaitCallback wcb = new WaitCallback(Download_Directory);
                    int workerThreads, availabeThreads;
                    ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
                    if (workerThreads > 0)//可用线程数>0
                    {
                        ThreadPool.QueueUserWorkItem(wcb, ll_S[i]);
                    }
                    else
                    {
                        //to do 可以采取一种策略，让这个任务合理地分配给线程
                    }
                }
            }
        }

        private void SockListen(object obj)
        {
            SocketModule s_i = (SocketModule)obj;
            int a = s_i.l_S.Count;
            int bufnum = cache_num / module_l + 1;//单位为（module_l（b）大小）
            int m_bufnum = bufnum / a + 1;
            for (int i = 0; i < a; i++)
            {
                s_i.l_S[i].thread_num = a;
                s_i.l_S[i].buf = new byte[m_bufnum * module_l];
                s_i.l_S[i].I_BufNum = m_bufnum;
                WaitCallback wcb = new WaitCallback(ThreadFun_Download_File);
                int workerThreads, availabeThreads;
                ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
                if (workerThreads > 0)//可用线程数>0
                {
                    ThreadPool.QueueUserWorkItem(wcb, s_i.l_S[i]);
                }
            }
        }

        private void ThreadFun_Download_File(object obj)
        {
            SockInfo s_i = (SockInfo)obj;
            Download_File(ref s_i);
            Console.WriteLine("asdfasdf");
            s_i.s.Close();
        }

        private void Download_File(ref SockInfo s )
        {
            SockInfo s_i = s;
            FileStream file = s_i.file;
            SocketModule SM = ll_S[s_i.ID];
            module_num = 100;


            if (s_i.i.type == 2 && file.Length <= module_l * 100)
            {
                module_num = (int)(file.Length / module_l) + 1;
            }

            #region 本线程的局部变量
            int module_m = (int)(file.Length / module_l / 100) + 1;
            if ((file.Length / module_l) < 99 * module_m && module_m != 1)
            {
                module_m = (int)(file.Length / module_l / 10) + 1;
                module_num = 10;
            }
            int module = module_l + module_l_Head;           
            int num1 = 0;//粘包造成的第二次起初数组位置
            int j = 0;//粘包造成的第二次复制起初数组位置
            int Sticky_num = 0;//记录粘包处理次数
            byte[] buf = new byte[module];
            byte[] bu = new byte[module];
            byte[] p_buf = new byte[module_l];
            byte[] p_Head = new byte[module_l_Head];
            byte[] b = new byte[8];
            byte[] b1 = new byte[4];
            byte[] b2 = new byte[4];
            int nRecv = 0;
            UInt32 sPos = 0;
            UInt32 sPos_n = 0;
            UInt32 num = 0;
            #endregion

            #region 开始定位
            for (int i = 0; i < module_num; i++)
            {
                if (!SM.m.RW[i] && !SM.m.Complete[i])
                {
                    SM.PointInfo.RW[i] = SM.m.RW[i] = true;
                    b1 = BitConverter.GetBytes(i);
                    b2 = BitConverter.GetBytes(SM.m.sPos_n[i]);
                    Array.Copy(b1, b, b1.Length);
                    Array.Copy(b2, 0, b, b1.Length, b2.Length);
                    try
                    {
                        s_i.s.Send(b);
                    }
                    catch (Exception)
                    {

                        ;
                    }
                    break;
                }
            } 
            #endregion

            do
            {
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
                    Sticky_num++;
                }
                #endregion

                if (num1 == bu.Length)
                {
                    Array.Copy(bu, 0, p_Head, 0, p_Head.Length);
                    Array.Copy(bu, p_Head.Length, p_buf, 0, p_buf.Length);

                    Package p = (Package)TestStruct(p_Head);
                    sPos_n = p.module_m;
                    sPos = p.num;
                    num = p.module_n;

                    #region 缓冲区算法
                    Array.Copy(p_buf, 0, s_i.buf, s_i.BufNum * module_l, p_buf.Length);
                    s_i.BufNum++;
                    if (s_i.I_BufNum == s_i.BufNum || sPos_n == num + 1)
                    {
                        file.Seek((sPos * module_m + num - s_i.BufNum + 1) * module_l, SeekOrigin.Begin);
                        file.Write(s_i.buf, 0, (s_i.BufNum - 1) * module_l + p.bufLen);
                        s_i.BufNum = 0;
                        SM.PointInfo.sPos_n[sPos] = num;
                        Serialization(SM.PointInfo);
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
                                Array.Copy(b1, b, b1.Length);
                                Array.Copy(b2, 0, b, b1.Length, b2.Length);
                                try
                                {
                                    s_i.s.Send(b);
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
                                FileInfo d = new FileInfo(SM.PointInfo.name + ".data");
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
#if DEBUG
                Console.WriteLine("4!!!{0}     {1}    {2}    {3}     {4}    {5}", sPos, nRecv, num, num1, j, Sticky_num);
#endif
                #endregion

                Thread.Sleep(1);
            } while (nRecv > 0);
            SM.PointInfo.RW[sPos] = SM.m.RW[sPos] = false;
            b = BitConverter.GetBytes(-1);
            s_i.s.Send(b);
            if (SM.Com_Thread_num == s_i.thread_num) file.Close();           
        }

        private void Download_Directory(object obj)
        {
            SocketModule s_i = (SocketModule)obj;
            SockInfo si = null;
            Socket s = s_i.l_S[0].s;

            s.Send(BitConverter.GetBytes(s_i.PointInfo.Di_file_num),SocketFlags.None);

            int r = 0;
            info i = new info();
            int s_num = 0;
            #region 主循环
            do
            {
                s_num++;

                byte[] bu = new byte[270];
                int n = 0;
                n = r = s.Receive(bu, SocketFlags.None);

                while (n != 270)
                {
                    r = s.Receive(bu, r, bu.Length, SocketFlags.None);
                    n += r;
                }

                i = (info)BytesToStruct(bu, i.GetType());

                if (i.type == 0) break;
                if (i.type == 1)
                {                    
                    s_i.PointInfo.filename = _rootDirectory + i.name;
                    int bufnum = cache_num / module_l + 1;//单位为（module_l（b）大小）
                    int m_bufnum = bufnum / s_i.l_S.Count + 1;
                    FileStream file = new FileStream(_rootDirectory + i.name, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                    file.SetLength(i.size);
                    si = new SockInfo(s_i.l_S[0].s, s_i.l_S[0].i, file, s_i.ID);
                    si.thread_num = s_i.l_S.Count;
                    si.buf = new byte[m_bufnum * module_l];
                    si.I_BufNum = m_bufnum;
                    Download_File(ref si);
                    s_i.m = new ModuleFalt();
                }
                else
                {
                    DirectoryInfo target = new DirectoryInfo(_rootDirectory + i.name);
                    target.Create();
                }
                s_i.PointInfo.Di_file_num++;
            } while (s_num < s_i.l_S[0].i.number); 
            #endregion
            FileInfo d = new FileInfo(s_i.PointInfo.name + ".data");
            d.Delete();
            Console.WriteLine("asdf");
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
            FileStream fileStream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryFormatter b = new BinaryFormatter();
            i = b.Deserialize(fileStream) as Information;
            fileStream.Close();
            return i;
        }

        public Information DownLoadInfo(int i)
        {
            if (i == 0) return null;
            Information info = new Information();
            info = ll_S[i - 1].PointInfo;
            return info;
        }

        #region 两种结构体转化byte[]
        private object BytesToStruct(byte[] bytes, Type type)
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

        private object TestStruct(byte[] rawdatas)
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
    }


    public class Upload
    {
        class Directory_info
        {
            public string name{ get; set; }
            public long size{ get; set; }
        }
        class SockInfo
        {
            public int State { get; set; }//1，上传中，2，已结束
            public int type { get; set; }//1，文件，2，目录 
            public Socket s { get; set; }
            public FileStream file { get; set; }
            public DirectoryInfo di { get; set; }
            public List<info> l { get; set; }
            public SockInfo(int state,int t,Socket S,FileStream f)
            {
                l = new List<info>();
                State = state;
                s = S;
                file = f;
                type = t;
                di = null;
            }
            public SockInfo(int state, int t, Socket S, DirectoryInfo Di)
            {
                l = new List<info>();
                State = state;
                s = S;
                type = t;
                di = Di;
                file = null;
            }
        }
        class Cache_info
        {
            public int bufLen { get; set; }
            public int sPos_n { get; set; }
            public int Chache_n { get; set; }
            public int Chache_m { get; set; }
        }
        class Directory_Message
        {
            public string name { get; set; }
            public int Owner_number { get; set; }//文件在list中的拥有者目录编号
            public bool file { get; set; }//true文件false目录
            public Directory_Message(string n,int o ,bool f)
            {
                name = n;
                Owner_number = o;
                file = f;
            }
        }

        #region 全局变量
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
        List<SockInfo> l_s = new List<SockInfo>();
        #endregion

        public void CreatUpload(string filename, string IP, int t)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint endpoit = new IPEndPoint(IPAddress.Parse(IP), 5000);
            sock.Connect(endpoit);

            DirectoryInfo di = new DirectoryInfo(filename);
            if (di.Attributes == FileAttributes.Directory)
	        {
                l_s.Add(new SockInfo(2, t, sock, di));
	        }
            else if (di.Attributes == FileAttributes.Normal || di.Attributes == FileAttributes.Archive)
            {
                FileStream file = new FileStream((filename), FileMode.Open, FileAccess.Read, FileShare.Read);
                l_s.Add(new SockInfo(1, t, sock, file));
            }
            CreatThreadPool_Upload();      
        }

        private void CreatThreadPool_Upload()
        {
            for (int i = 0; i < l_s.Count; i++)
            {
                if (l_s[i].State == 2)
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
            SockInfo s = (SockInfo)obj;
            FileStream file = s.file;
            Socket sock = s.s;
            Upload_File(file, sock, file.Name.Substring(file.Name.LastIndexOf(@"\") + 1));
            Console.WriteLine("asdfasdfasdf");
            sock.Close();
        }

        private void Upload_File(FileStream f, Socket s, string socketname)
        {
            FileStream file = f;
            Socket sock = s;
            module_num = 100;

            if (f.Name == @"D:\DriverGenius2012\vulfix_gui.dll")
            {
                
            }

            byte[] bu = new byte[270];

            bu = StructToBytes(new info(socketname, (UInt32)file.Length, 1 ,1));
            sock.Send(bu, bu.Length, SocketFlags.None);

            #region 变量
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
            byte[] b = new byte[8];
            byte[] p_buf = new byte[module_l];
            byte[] p_Head = new byte[module_l_Head];
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
                if (c.Chache_m * module_l >= c.bufLen )
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
                p_Head = StructToBytes(package);
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

#if DEBUG
                    Console.WriteLine("{0}    {1}           {2}           {3}", sPos, nSend, nLen, n);
#endif
                    sPos_n++;
                    nLen++;
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
                    sPos_n = BitConverter.ToInt32(b, 4);
                    if (sPos == -1) break;
                } 
                #endregion
            } while (sPos >= 0);

            file.Close();
        }

        private void ThreadFunUpload_Directory(object obj)
        {           
            SockInfo s = (SockInfo)obj;
            Directory_info d = new Directory_info();
            List<string> l = new List<string>();
            d.name = s.di.Name;
            GetAllDirList(s.di.FullName, l, d);
            byte[] bu = new byte[270];
            string Root_directory_name = s.di.FullName.Substring(s.di.FullName.LastIndexOf(@"\") + 1);

            bu = StructToBytes(new info(Root_directory_name, (UInt32)d.size, 2 ,(UInt32)l.Count));
            s.s.Send(bu, bu.Length, SocketFlags.None);

            s.s.Receive(bu , SocketFlags.None);
            int i = BitConverter.ToInt32(bu , 0);
            Thread.Sleep(10);
            for (;i<l.Count;i++)
            {
                string name = l[i];
                DirectoryInfo di = new DirectoryInfo(name);
                string n = name.Substring(name.IndexOf(Root_directory_name));
                if (di.Attributes == FileAttributes.Directory)
                {                    
                    bu = StructToBytes(new info(n, 0 , 2, 1));
                    s.s.Send(bu, bu.Length, SocketFlags.None);
                }
                else if (di.Attributes == FileAttributes.Normal || di.Attributes == FileAttributes.Archive)
                {
                    Upload_File(new FileStream((name), FileMode.Open, FileAccess.Read, FileShare.Read), s.s ,n);
                }
            }
            bu = StructToBytes(new info("asdf", 0, 0, 0));
            s.s.Send(bu, bu.Length, SocketFlags.None);
            Console.WriteLine("asdf");
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

        private byte[] StructToBytes(object obj)
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
