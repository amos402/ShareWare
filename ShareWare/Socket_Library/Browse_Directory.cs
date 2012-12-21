﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ShareWare;
using ShareWare.ShareFile;
using System.Reflection;

namespace Socket_Library
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Share
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string type;//扩展名
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string hash;
        public UInt32 size;
    }

    public class ShareInfo
    {
        public string name { get; set; }
        public string type { get; set; }
        public long size { get; set; }
        public ImageSource largeIcon { get; set; }
        public ImageSource smallIcon { get; set; }
        public ShareInfo(string n, string t, long s, ImageSource l, ImageSource sm)
        {
            name = n;
            type = t;
            size = s;
            largeIcon = l;
            smallIcon = sm;
        }
    }

    public class ShowShareFileInfo
    {
        public ShowShareFileInfo()
        {
            pu = new BitmapImage(new Uri(SPath + "FolderIcon.png", UriKind.Absolute));

            pu.Freeze();
        }
        string SPath = System.Environment.CurrentDirectory + @"\images\";
        string d;
        ImageSource pu;
        public event EventHandler<CallShowListView> CallShow;
        StructByte S_B = new StructByte();
        public int Port { get; set; }
        private Socket newSock = null;
        public Socket NewSock
        {
            get { return newSock; }
            set { newSock = value; }
        }

        public void CreatSocket(string D)
        {
            d = D;
            IPEndPoint endpoit = new IPEndPoint(IPAddress.Any, 0);//实际在项目中运行时要改为0
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(endpoit);
            sock.Listen(10);
            Port = ((IPEndPoint)sock.LocalEndPoint).Port;
            sock.BeginAccept(new AsyncCallback(AcceptCallback), sock);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            try
            {
                newSock = sock.EndAccept(ar);
                if (CallShow != null && d != null)
                {

                    foreach (EventHandler<CallShowListView> item in CallShow.GetInvocationList())
                    {
                        item.BeginInvoke(this, new CallShowListView() { Directory = d }, null, null);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SendDirectoryName(string name)
        {
            if (newSock == null)
            {
                return;
            }
            byte[] b = new byte[260];
            b = System.Text.Encoding.UTF8.GetBytes(name);
            try
            {
                newSock.Send(b, SocketFlags.None);
            }
            catch (Exception)
            {
                ;
            }
        }

        public FileInfoDataList ReceiveShare()
        {
            Share sh = new Share();
            if (newSock == null)
            {
                return null;
            }
            byte[] b = new byte[315];
            int r = newSock.Receive(b, SocketFlags.None);
            if (r == 0) return null;
            sh = (Share)S_B.BytesToStruct(b, new Share().GetType());
            FileInfoDataList si = new FileInfoDataList(sh.name, sh.type, sh.size, sh.hash, null, null);
            return si;
        }

        public FileInfoDataList GetInfo(FileInfoDataList si)
        {
            Icon largeIcon = null;
            Icon smallIcon = null;
            GetItem(si.Type, ref largeIcon, ref smallIcon);
            ImageSource lar = null; ImageSource sma = null;
            try
            {
                lar = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon
                    (largeIcon.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                sma = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon
                    (smallIcon.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception)
            {
                return null;
            }
            if (si.Type == "文件夹")
            {
                //StreamResourceInfo resourceInfo = Application.GetResourceStream(new Uri(@"images\icon.jpg", UriKind.Relative));
                //_image = new BitmapImage();
                //_image.BeginInit();
                //_image.StreamSource = resourceInfo.Stream;
                //_image.EndInit();

                lar = sma = pu;
                //Assembly asm = Assembly.GetExecutingAssembly();

                //string asd = this.GetType().Namespace;
                //Stream imageStream = asm.GetManifestResourceStream(this.GetType().Namespace + ".images.FolderIcon.png");

                //var sam1 = sma as BitmapImage;

                //lar = sma;
                //sam1 = new BitmapImage();
                //sam1.BeginInit();
                //sam1.StreamSource = imageStream;
                //sam1.EndInit();
                //lar = sma;

            }
            return new FileInfoDataList(si, lar, sma);
        }

        private void GetItem(string type, ref Icon largeIcon, ref Icon smallIcon)
        {
            GetExtsIconAndDescription("." + type, ref largeIcon, ref smallIcon);
        }

        private void GetExtsIconAndDescription(string ext, ref Icon largeIcon, ref Icon smallIcon)
        {
            GetDefaultIcon(ref largeIcon, ref smallIcon);   //得到缺省图标
            RegistryKey extsubkey = Registry.ClassesRoot.OpenSubKey(ext);   //从注册表中读取扩展名相应的子键
            if (extsubkey == null) return;

            string extdefaultvalue = extsubkey.GetValue(null) as string;     //取出扩展名对应的文件类型名称
            if (extdefaultvalue == null) return;

            if (extdefaultvalue.Equals("exefile", StringComparison.OrdinalIgnoreCase))  //扩展名类型是可执行文件
            {
                RegistryKey exefilesubkey = Registry.ClassesRoot.OpenSubKey(extdefaultvalue);  //从注册表中读取文件类型名称的相应子键
                System.IntPtr exefilePhiconLarge = new IntPtr();
                System.IntPtr exefilePhiconSmall = new IntPtr();
                NativeMethods.ExtractIconExW(Path.Combine(Environment.SystemDirectory, "shell32.dll"), 2, ref exefilePhiconLarge, ref exefilePhiconSmall, 1);
                if (exefilePhiconLarge.ToInt32() > 0) largeIcon = Icon.FromHandle(exefilePhiconLarge);
                if (exefilePhiconSmall.ToInt32() > 0) smallIcon = Icon.FromHandle(exefilePhiconSmall);
                return;
            }

            RegistryKey typesubkey = Registry.ClassesRoot.OpenSubKey(extdefaultvalue);  //从注册表中读取文件类型名称的相应子键
            if (typesubkey == null) return;

            RegistryKey defaulticonsubkey = typesubkey.OpenSubKey("DefaultIcon");  //取默认图标子键
            if (defaulticonsubkey == null) return;

            //得到图标来源字符串
            string defaulticon = defaulticonsubkey.GetValue(null) as string; //取出默认图标来源字符串
            if (defaulticon == null) return;
            string[] iconstringArray = defaulticon.Split(',');
            int nIconIndex = 0; //声明并初始化图标索引
            if (iconstringArray.Length > 1)
                if (!int.TryParse(iconstringArray[1], out nIconIndex))
                    nIconIndex = 0;     //int.TryParse转换失败，返回0

            //得到图标
            System.IntPtr phiconLarge = new IntPtr();
            System.IntPtr phiconSmall = new IntPtr();
            NativeMethods.ExtractIconExW(iconstringArray[0].Trim('"'), nIconIndex, ref phiconLarge, ref phiconSmall, 1);
            if (phiconLarge.ToInt32() > 0) largeIcon = Icon.FromHandle(phiconLarge);
            if (phiconSmall.ToInt32() > 0) smallIcon = Icon.FromHandle(phiconSmall);
        }

        private static void GetDefaultIcon(ref Icon largeIcon, ref Icon smallIcon)
        {
            largeIcon = smallIcon = null;
            System.IntPtr phiconLarge = new IntPtr();
            System.IntPtr phiconSmall = new IntPtr();
            NativeMethods.ExtractIconExW(Path.Combine(Environment.SystemDirectory, "shell32.dll"), 0, ref phiconLarge, ref phiconSmall, 1);
            if (phiconLarge.ToInt32() > 0) largeIcon = Icon.FromHandle(phiconLarge);
            if (phiconSmall.ToInt32() > 0) smallIcon = Icon.FromHandle(phiconSmall);
        }

    }

    public class NativeMethods
    {
        [System.Runtime.InteropServices.DllImportAttribute("shell32.dll", EntryPoint = "ExtractIconExW", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static extern uint ExtractIconExW([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpszFile, int nIconIndex, ref System.IntPtr phiconLarge, ref System.IntPtr phiconSmall, uint nIcons);

    }

    public class SendShareFileInfo/////////////////////////////////////要注意设置好
    {
        StructByte S_B = new StructByte();
        Socket sock = null;
        string Local_Address = "";
        private string local_ID = "";
        public string Local_ID
        {
            get { return local_ID; }
            set { local_ID = value; }
        }
        byte[] b1 = new byte[260];
        Thread t = null;
        string Name = "";

        private ShareFiles sh = ShareFiles.Deserialize(@".\config\known.met");
        public ShareFiles Sh
        {
            get { return sh; }
            set { sh = value; }
        }

        public event EventHandler<CallUpLoadInfo> CallUpLoad_D;

        public void CreatSocket(string IP, int port)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint endpoit = new IPEndPoint(IPAddress.Parse(IP), port);
            int m = 0;
            while (true)
            {
                try
                {
                    //if (m == 20)////以后要加上去做一个超时连接
                    //{
                    //    break;
                    //}
                    m++;
                    sock.Connect(endpoit);
                    break;
                }
                catch (Exception)
                {
                    ;
                }
            }

            WaitCallback wcb = new WaitCallback(ReceiveInfo);
            int workerThreads, availabeThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out availabeThreads);
            if (workerThreads > 0)//可用线程数>0
            {
                ThreadPool.QueueUserWorkItem(wcb, sock);
            }
            else
            {
                //to do 可以采取一种策略，让这个任务合理地分配给线程
            }
        }

        private void ReceiveInfo(object p)
        {
            Socket s = (Socket)p;
            try
            {
                s.BeginReceive(b1, 0, b1.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), s);
            }
            catch (Exception)
            {
                ;
            }
        }

        private void Send_ALL_Directory(Socket s, string name)
        {
            List<Share> l = new List<Share>();
            byte[] b = new byte[315];
            if (name.CompareTo(Local_ID + "\\") == 0)
            {
                GetInitializationDir(l);
            }
            else if (name.CompareTo(Local_ID) == 0)
            {
                GetInitializationDir(l);
            }
            //else if (name.Length == 36 && name.IndexOf(Local_ID) == -1)
            //{
            //    string filename = sh.FindFile(name).File.FullName;
            //    GetDirList(filename,l);
            //}
            else
            {
                string[] split = name.Split(new Char[] { '\\' });
                if (split[0] == @":")
                {
                    CallUpLoad(name, s);
                    return;
                }
                if (sh.SharePath.ContainsKey(split[1]))
                {
                    Local_Address = sh.SharePath[split[1]];
                    Local_Address = Local_Address + "\\" + name.Substring(local_ID.Length + 2 + split[1].Length);
                    GetDirList(Local_Address, l);
                }
                else
                {
                    b = S_B.StructToBytes(new Share());
                    try
                    {
                        s.Send(b, SocketFlags.None);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    Console.WriteLine("asdf!!");/////////////////测试
                }
            }
            foreach (var item in l)
            {
                b = S_B.StructToBytes(item);
                try
                {
                    s.Send(b, SocketFlags.None);
                }
                catch (Exception)
                {
                    return;
                }
                Console.WriteLine("asdf");/////////////////测试
            }
            b = S_B.StructToBytes(new Share());
            try
            {
                s.Send(b, SocketFlags.None);
            }
            catch (Exception)
            {
                return;
            }
            Console.WriteLine("asdf!!");/////////////////测试
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
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
                if (t != null && t.IsAlive)
                {
                    t.Abort();
                }
                Name = System.Text.Encoding.UTF8.GetString(b1, 0, i);
                t = new Thread(new ParameterizedThreadStart(p => Send_ALL_Directory(s, Name)));
                t.Start();
                try
                {
                    s.BeginReceive(b1, 0, b1.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), s);
                }
                catch (Exception)
                {
                    ;
                }
            }
        }

        private void GetDirList(string strBaseDir, List<Share> list)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(strBaseDir);
                DirectoryInfo[] diA = di.GetDirectories();
                FileInfo[] files = di.GetFiles();
                Share _sh = new Share();
                foreach (var item in files)
                {
                    _sh.name = Name + item.Name;
                    _sh.size = (uint)item.Length;
                    _sh.type = item.Extension;
                    _sh.hash = sh.GetFileHash(item.FullName);
                    list.Add(_sh);
                }
                foreach (var item in diA)
                {
                    _sh.name = Name + item.Name;
                    _sh.size = 0;
                    _sh.type = "文件夹";
                    _sh.hash = sh.GetFileHash(item.FullName);
                    list.Add(_sh);
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        private void GetInitializationDir(List<Share> list)
        {
            Share sa = new Share();
            foreach (var item in sh.SharePath.Keys)
            {
                sa.name = Name + item;
                sa.type = "文件夹";
                sa.size = 0;
                list.Add(sa);
            }
        }

        private void CallUpLoad(string message, Socket s)
        {
            string[] split = message.Split(new Char[] { '\\' });
            if (sh.SharePath.ContainsKey(split[2]))
            {
                Local_Address = sh.SharePath[split[2]];
                Local_Address = Local_Address + "\\" + message.Substring(local_ID.Length + 2 + split[1].Length);
            }
        }

    }

    public class CallUpLoadInfo : EventArgs
    {
        public string IP { get; set; }
        public string Filename { get; set; }
        public int Potr { get; set; }
    }

    public class CallShowListView : EventArgs
    {
        public string Directory { get; set; }
    }
}
