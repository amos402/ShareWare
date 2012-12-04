using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ShareWare;

namespace Socket_Library
{
    #region DownLoad_Struct
    public class LoadInfo
    {
        public string name { get; set; }
        public long size { get; set; }
        public string type { get; set; }
        public Socket s { get; set; }
    }
    public class SockInfo
    {
        public int ID { get; set; }
        public Socket s { get; set; }
        public info i;
        public FileStream file { get; set; }
        public DirectoryInfo di { get; set; }
        public int BufNum { get; set; }//记录第几份缓冲
        public int I_BufNum { get; set; }//记录第几份缓冲
        public byte[] buf { get; set; }
        public SockInfo(Socket s1, info i1, FileStream F)
        {
            s = s1;
            i = i1;
            file = F;
            BufNum = 0;//thread_num = 0;
            di = null;
        }
        public SockInfo(Socket s1, info i1, DirectoryInfo Di)
        {
            s = s1;
            i = i1;
            file = null;
            BufNum = 0;// thread_num = 0;
            di = Di;
        }
        public SockInfo(Socket s1)
        {
            s = s1;
            i = new info();
            BufNum = 0;// thread_num = 0;
            di = null;
        }
    }
    public class ModuleFalt
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
    public class SocketModule
    {
        public string name { get; set; }
        public string filename { get; set; }
        public long t_size { get; set; }
        public long DownLoadSize { get; set; }//已经下载的大小
        float progress_Percentage;
        public float Progress_Percentage
        {
            get { return progress_Percentage; }
            set { progress_Percentage = value; }
        }
        public string type { get; set; }//1，文件，2，目录 
        public int ID { get; set; }//记录第几个ll_S           
        public ModuleFalt m { get; set; }
        public List<SockInfo> l_S { get; set; }
        public Information PointInfo { get; set; }
        public SocketModule(List<SockInfo> l, int t)
        {
            m = new ModuleFalt();
            l_S = new List<SockInfo>();
            PointInfo = new Information();
            l_S = l;
            PointInfo.type = t;
            PointInfo.Di_file_num = 0;
            t_size = PointInfo.t_Size = l[0].i.size;
            if (t == 1)
            {
                type = l[0].file.Name.Substring(l[0].file.Name.LastIndexOf("."));
                filename = PointInfo.name = l[0].file.Name;
                PointInfo.filename = l[0].file.Name;
            }
            else
            {
                type = "文件夹";
                filename = PointInfo.name = l[0].di.FullName;
                PointInfo.total_file_number = l_S[0].i.number;
            }
            name = filename.Substring(filename.LastIndexOf(@"\") + 1);
        }
        public SocketModule(Information i)
        {
            m = new ModuleFalt(i.Complete, i.RW, i.sPos_n);
            l_S = new List<SockInfo>();
            PointInfo = new Information();
            l_S = new List<SockInfo>();
            PointInfo.type = i.type;
            filename = PointInfo.name = i.name;
            name = filename.Substring(filename.LastIndexOf(@"\") + 1);
            PointInfo.filename = i.filename;
            PointInfo.Di_file_num = i.Di_file_num;
            PointInfo.total_file_number = i.total_file_number;
            DownLoadSize = PointInfo.DownLoadSize = i.DownLoadSize;
            t_size = PointInfo.t_Size = i.t_Size;
            if (i.type == 1)
            {
                type = i.filename.Substring(i.filename.LastIndexOf("."));
            }
            else
            {
                type = "文件夹";
            }
        }
        public SocketModule(LoadInfo d)
        {
            name = d.name;
            t_size = d.size;
            type = d.type;
            l_S = new List<SockInfo>();
        }
    }
    [Serializable]
    public class DownListInfo : INotifyPropertyChanged
    {
        private string state;
        public string State
        {
            get { return state; }
            set { state = value; NotifyPropertyChange("Size"); }
        }public int ID { get; set; }
        public string name { get; set; }
        public string filename { get; set; }
        private long size;
        public long Size
        {
            get { return size; }
            set
            {
                size = value;
                NotifyPropertyChange("Size");
            }
        }
        public string type { get; set; }
        float progress_Percentage;
        public float Progress_Percentage
        {
            get { return progress_Percentage; }
            set
            {
                progress_Percentage = value;
                NotifyPropertyChange("Progress_Percentage");
            }
        }
        public DownListInfo()
        {
            ;
        }
        public DownListInfo(LoadInfo d)
        {
            name = d.name;
            size = d.size;
            type = d.type;
            progress_Percentage = 0;
        }
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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
        public long DownLoadSize { get; set; }
        public long t_Size { get; set; }
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
    #endregion

    #region UpLoad_Struct
    class Directory_info
    {
        public string name { get; set; }
        public long size { get; set; }
    }
    class SockInfo_U
    {
        public int State { get; set; }//1，上传中，2，已结束
        public int type { get; set; }//1，文件，2，目录 
        public Socket s { get; set; }
        public FileStream file { get; set; }
        public DirectoryInfo di { get; set; }
        public List<info> l { get; set; }
        public SockInfo_U(int state, int t, Socket S, FileStream f)
        {
            l = new List<info>();
            State = state;
            s = S;
            file = f;
            type = t;
            di = null;
        }
        public SockInfo_U(int state, int t, Socket S, DirectoryInfo Di)
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
        public Directory_Message(string n, int o, bool f)
        {
            name = n;
            Owner_number = o;
            file = f;
        }
    }
    #endregion

}
