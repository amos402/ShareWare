using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;                 //Process必须命名空间
using System.ComponentModel;              //Process必须命名空间
using System.Security.Permissions;        //RegistryKey必须命名空间
using Microsoft.Win32;                    //RegistryKey必须命名空间
using System.Windows.Forms;               //显示窗口必须命名空间
using System.Windows.Forms.Design;        //创建文件浏览框必须命名空间
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Software_Design
{
    public class CloseComputer  //关机类
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int flg, int rea);

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int EWX_LOGOFF = 0x00000000;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_FORCE = 0x00000004;
        internal const int EWX_POWEROFF = 0x00000008;
        internal const int EWX_FORCEIFHUNG = 0x00000010;

        public static void DoExitWin(int flg)               //关机函数
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            //ok = ExitWindowsEx(flg, 0);
            ExitWindowsEx(EWX_SHUTDOWN, 0);
        }
    }
    public class AutoRunClass   //开机自动运行
    {
        public static void AutoRun()     //设置开机自动运行
        {
            try
            {
                string MyFileName = Process.GetCurrentProcess().MainModule.FileName;
                if (!System.IO.File.Exists(MyFileName))
                    return;
                String MyName = MyFileName.Substring(MyFileName.LastIndexOf("\\") + 1);
                RegistryKey MyReg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (MyReg == null)
                    MyReg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                MyReg.SetValue(MyName, MyFileName);
                MyReg.Close();
            }
            catch
            {
            }
        }

        public static void StopAuto()        //取消开机自动运行 
        {
            try
            {
                string MyFileName = Process.GetCurrentProcess().MainModule.FileName;
                if (!System.IO.File.Exists(MyFileName))
                    return;
                String MyName = MyFileName.Substring(MyFileName.LastIndexOf("\\") + 1);
                RegistryKey loca_chek = Registry.LocalMachine;
                RegistryKey runs = loca_chek.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                string[] runName = runs.GetValueNames();
                foreach (string strName in runName)
                {
                    if (strName.ToUpper() == MyName.ToUpper())
                        runs.DeleteValue(MyName, false);
                }
            }
            catch
            {
            }
        }

        public static bool State()            //判断是否开机运行
        {
            string MyFileName = Process.GetCurrentProcess().MainModule.FileName;
            /*if (!System.IO.File.Exists(MyFileName))
                return;*/
            String MyName = MyFileName.Substring(MyFileName.LastIndexOf("\\") + 1);
            RegistryKey loca_chek = Registry.LocalMachine;
            RegistryKey runs = loca_chek.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string[] runName = runs.GetValueNames();
            foreach(string strName in runName)
            {
                if (strName.ToUpper() == MyName.ToUpper())
                    return true;
            }
            return false;
        }
    }
    public class FolderDialog : FolderNameEditor      //文件浏览框类
    {
        FolderNameEditor.FolderBrowser fDialog = new System.Windows.Forms.Design.FolderNameEditor.FolderBrowser();
        public FolderDialog()
        {
        }
        public DialogResult DisplayDialog()
        {
            return DisplayDialog("请选择一个文件夹");
        }

        public DialogResult DisplayDialog(string description)
        {
            fDialog.Description = description;
            return fDialog.ShowDialog();
        }
        public string Path
        {
            get
            {
                return fDialog.DirectoryPath;
            }
        }
        ~FolderDialog()
        {
            fDialog.Dispose();
        }
    }
    public class SaveInstll     //保存设置
    {
        private static string BoolToString(bool B)  //bool转换为string
        {
            if (B)
                return "true";
            else
                return "false";
        }

        private static bool StringToBool(string S)  //string转换为bool
        {
            if (S == "true")
                return true;
            else
                return false;
        }

        public static void SetName(string Name)
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey SetUp = key.CreateSubKey("software\\SetUp");
            SetUp.SetValue(Name, Name);
            SetUp.Close();
        }

        public static void SaveMessage(string Name,string PWO, bool check)
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey SetUp = key.CreateSubKey("software\\SetUp\\"+Name);
            SetUp.SetValue("PWO", PWO);
            SetUp.SetValue("check", BoolToString(check));
            SetUp.Close();
        }

        public static void ListName(string Name)
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey SetUp = key.CreateSubKey("software\\SetUp");
            SetUp.SetValue("ListName", Name);
            SetUp.Close();
        }

        public static string ReturnName()
        {
            string T;
            try
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey GetUp = key.OpenSubKey("software\\SetUp", true);
                T = GetUp.GetValue("ListName").ToString();
                GetUp.Close();
                return T;
            }
            catch
            {
                NewReg();
                return "";
            }
        }

        public static string ReturnPWO(string Name)
        {
            string T;
            RegistryKey key = Registry.LocalMachine;
            RegistryKey GetUp = key.OpenSubKey("software\\SetUp\\"+Name, true);
            T = GetUp.GetValue("PWO").ToString();
            GetUp.Close();
            return T;
        }

        public static bool ReturnCheck(string Name)
        {
            string T;
            RegistryKey key = Registry.LocalMachine;
            RegistryKey GetUp = key.OpenSubKey("software\\SetUp\\" + Name, true);
            T = GetUp.GetValue("check").ToString();
            GetUp.Close();
            return StringToBool(T);
        }

        public static List<string> ReturnAllName()
        {
            List<string> AllName = new List<string>();
            RegistryKey key = Registry.LocalMachine;
            try
            {
                RegistryKey GetUp = key.OpenSubKey("software\\SetUp", true);
                foreach (string T in GetUp.GetValueNames())
                    AllName.Add(T);
                AllName.Remove("ListName");
                return AllName;
            }
            catch
            {
                NewReg();
                return null;
            }
        }

        public static void DelAll()
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey Del = key.OpenSubKey("software", true);
            Del.DeleteSubKeyTree("SetUp", false);
            Del.Close();
            NewReg();
        }

        public static void NewReg()
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey SetNew = key.CreateSubKey("software\\SetUp");
            SetNew.Close();
        }
    }
    public class OpenDialog     //打开图片
    {
        OpenFileDialog OD = new OpenFileDialog();
        Bitmap bit = null;
        string PictureDirectory;
        public OpenDialog()    
        {
            OD.Filter = "所有图像文件(*.ico,*.jpg,*.gif,*.Bmp,*.png,*.png,*.psd,*.jif)|*.ico;*.jpg;*.gif;*.Bmp;*.png;*.png;*.psd;*.jif |所有文件(*.*)|*.*";
            OD.FilterIndex = 1;
        }

        public string OpenPicture()
        {
            if (OD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    bit = new Bitmap(OD.FileName);
                }
                catch
                { }
                PictureDirectory = OD.FileName;
                return PictureDirectory;
            }
            else
                return "";
        }

        public Bitmap Show()
        {
            return bit;
        }

        ~OpenDialog()
        {
            OD.Dispose();
        }
    }  
    public class SavePicture    //保存图片
    {
        
        Bitmap bit = null;
        public SavePicture(Bitmap B)
        {
            bit = B;
        }

        public void SetPicture(Bitmap B)
        {
            bit = B;
        }

        public Bitmap GetPicture()
        {
            return bit;
        }

        public void Save(string UseName)
        {
            int y = DateTime.Now.Year;
            int m = DateTime.Now.Month;
            int d = DateTime.Now.Day;
            int hh = DateTime.Now.Hour;
            int mm = DateTime.Now.Minute;
            int ss = DateTime.Now.Second;
            string FName = y.ToString() + m.ToString() + d.ToString() + hh.ToString() + mm.ToString() + ss.ToString();
            string MyFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(@MyFileName + @"\imgs"))
            {
                try
                {
                    Directory.CreateDirectory(@MyFileName + @"\imgs");
                }
                catch
                { }
            }
            MyFileName = @MyFileName + @"\imgs\"+UseName;
            if (!Directory.Exists(@MyFileName))
            {
                try
                {
                    Directory.CreateDirectory(@MyFileName);
                }
                catch
                { }
            }
            string Path = MyFileName + "\\" + FName + ".jpg";
            bit.Save(Path, System.Drawing.Imaging.ImageFormat.Jpeg);
            RegistryKey key = Registry.LocalMachine;
            RegistryKey SetUp = key.CreateSubKey("software\\SetUp\\" + UseName);
            SetUp.SetValue("Path", Path);
            SetUp.Close();
        }
    }
    public class ShowPicture    //显示图片
    {
        public static string Path(string UseName)
        {
            try
            {
                string T;
                RegistryKey key = Registry.LocalMachine;
                RegistryKey GetUp = key.OpenSubKey("software\\SetUp\\" + UseName, true);
                T = GetUp.GetValue("Path").ToString();
                GetUp.Close();
                return T;
            }
            catch
            {
                return "";
            }
        }
        public static Bitmap Show(string Path)
        {
            try
            {
                return new Bitmap(Path);
            }
            catch
            {
                string MyFileName = Process.GetCurrentProcess().MainModule.FileName;
                Path = System.IO.Path.GetDirectoryName(MyFileName) + "\\default.jpg";
                return new Bitmap(Path);
            }
        }
    }
    public class ModPicture     //修改图片
    {
        public static Bitmap CutForSquare(Bitmap B, int size, int quality)   //把图片裁剪成正方形并缩放
        {
            Bitmap transfer;

            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            Image initImage = B;

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= size && initImage.Height <= size)
            {
                return B;
            }
            else
            {
                //原始图片的宽、高
                int initWidth = initImage.Width;
                int initHeight = initImage.Height;

                //非正方型先裁剪为正方型
                if (initWidth != initHeight)
                {
                    //截图对象
                    Image pickedImage = null;
                    Graphics pickedG = null;

                    //宽大于高的横图
                    if (initWidth > initHeight)
                    {
                        //对象实例化
                        pickedImage = new Bitmap(initHeight, initHeight);
                        pickedG = Graphics.FromImage(pickedImage);
                        //设置质量
                        pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = SmoothingMode.HighQuality;
                        //定位
                        Rectangle fromR = new Rectangle((initWidth - initHeight) / 2, 0, initHeight, initHeight);
                        Rectangle toR = new Rectangle(0, 0, initHeight, initHeight);
                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);
                        //重置宽
                        initWidth = initHeight;
                    }
                    //高大于宽的竖图
                    else
                    {
                        //对象实例化
                        pickedImage = new Bitmap(initWidth, initWidth);
                        pickedG = Graphics.FromImage(pickedImage);
                        //设置质量
                        pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = SmoothingMode.HighQuality;
                        //定位
                        Rectangle fromR = new Rectangle(0, (initHeight - initWidth) / 2, initWidth, initWidth);
                        Rectangle toR = new Rectangle(0, 0, initWidth, initWidth);
                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);
                        //重置高
                        initHeight = initWidth;
                    }

                    //将截图对象赋给原图
                    initImage = (Image)pickedImage.Clone();
                    //释放截图资源
                    pickedG.Dispose();
                    pickedImage.Dispose();
                }

                //缩略图对象
                System.Drawing.Image resultImage = new Bitmap(size, size);
                System.Drawing.Graphics resultG = Graphics.FromImage(resultImage);
                //设置质量
                resultG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                resultG.SmoothingMode = SmoothingMode.HighQuality;
                //用指定背景色清空画布
                resultG.Clear(Color.White);
                //绘制缩略图
                resultG.DrawImage(initImage, new Rectangle(0, 0, size, size), new Rectangle(0, 0, initWidth, initHeight), GraphicsUnit.Pixel);

                //关键质量控制
                //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo i in icis)
                {
                    if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                    {
                        ici = i;
                    }
                }
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                transfer = new Bitmap((Bitmap)resultImage);

                //释放关键质量控制所用资源
                ep.Dispose();

                //释放缩略图资源
                resultG.Dispose();
                resultImage.Dispose();

                //释放原始图片资源
                initImage.Dispose();
                return transfer;
            }
        }
        public static Bitmap Zoom(Bitmap B, int size, int quality)           //不进行裁剪直接进行缩放
        {
            Bitmap transfer;

            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            Image initImage = B;

            //原始图片的宽、高
            int initWidth = initImage.Width;
            int initHeight = initImage.Height;

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= size && initImage.Height <= size)
            {
                return B;
            }
            else
            {              
                //缩略图对象
                System.Drawing.Image resultImage = new Bitmap(size, size);
                System.Drawing.Graphics resultG = Graphics.FromImage(resultImage);
                //设置质量
                resultG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                resultG.SmoothingMode = SmoothingMode.HighQuality;
                //用指定背景色清空画布
                resultG.Clear(Color.White);
                //绘制缩略图
                resultG.DrawImage(initImage, new Rectangle(0, 0, size, size), new Rectangle(0, 0, initWidth, initHeight), GraphicsUnit.Pixel);

                //关键质量控制
                //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo i in icis)
                {
                    if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                    {
                        ici = i;
                    }
                }
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                transfer = new Bitmap((Bitmap)resultImage);

                //释放关键质量控制所用资源
                ep.Dispose();

                //释放缩略图资源
                resultG.Dispose();
                resultImage.Dispose();

                //释放原始图片资源
                initImage.Dispose();
                return transfer;
            }
        }
    }
    public class History        //历史记录
    {
        private List<string> history;   //浏览记录

        public void HistoryAdd(string NowRecord)  //添加记录
        {
            if (history != null)
            {
                foreach (string check in history)
                {
                    if (check == NowRecord)
                    {
                        history.Remove(NowRecord);
                        history.Add(NowRecord);
                        return;
                    }
                }
            }
            history.Add(NowRecord);
        }

        public void HistotyClear()             //清空记录
        {
            history.Clear();
        }

        public List<string> ReturnHistiry()    //返回记录
        {
            List<string> Flip = new List<string>(history);
            Flip.Reverse();
            return Flip;
        }

        public string HistoryHead()            //返回表头
        {
            if (history.Count!=0)
                return history[history.Count-1];
            else
                return null;
        }

        public void SaveHistory()             //序列化记录
        {
            if (history != null)
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("History.bin", FileMode.Create, FileAccess.Write, FileShare.None);
                foreach (string check in history)
                    formatter.Serialize(stream, check);
                stream.Close();
            }
        }

        public History()                        //初始化记录
        {
            history = new List<string>();
            IFormatter formatter = new BinaryFormatter();
            try
            {
                Stream stream = new FileStream("History.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                while (stream.Position != stream.Length)
                {
                    history.Add((string)formatter.Deserialize(stream));
                }
                stream.Close();
            }
            catch
            {
            }
        }

        ~History()
        {
            this.SaveHistory();
        }
    }
    public class class6         //添加右键菜单项目
    {
        public static void Start()
        {
            string MyFileName = Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo MyProcess = new ProcessStartInfo(Path.GetDirectoryName(MyFileName) + "\\reg.bat");
            MyProcess.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(MyProcess);
        }

        public static void Close()
        {
            string MyFileName = Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo MyProcess = new ProcessStartInfo(Path.GetDirectoryName(MyFileName) + "\\unreg.bat");
            MyProcess.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(MyProcess);
        }

        public static string test()
        {
            string T;
            RegistryKey key = Registry.LocalMachine;
            RegistryKey GetUp = key.OpenSubKey("software\\Buffer", true);
            T = GetUp.GetValue("Path").ToString();
            GetUp.Close();
            return T;
        }
    }
    //#region 菜单点击事件
    //public class Program
    //{
    //    //需要监控的字段
    //    private string myValue;

    //    //属性设置，此处调用了事件触发函数
    //    public string MyValue
    //    {
    //        get { return myValue; }
    //        set
    //        {
    //            //如果变量改变则调用事件触发函数
    //            if (value != myValue)
    //            {
    //                WhenMyValueChange();
    //            }
    //            myValue = value;
    //        }
    //    }

    //    //定义的委托
    //    public delegate void MyValueChanged(object sender, EventArgs e);
    //    //与委托相关联的事件
    //    public event MyValueChanged OnMyValueChanged;

    //    //构造函数初始化初值并绑定一个事件处理函数
    //    public Program()
    //    {
    //        myValue = "";
    //        OnMyValueChanged += new MyValueChanged(afterMyValueChanged);

    //    }

    //    //事件处理函数，在这里添加变量改变之后的操作
    //    private void afterMyValueChanged(object sender, EventArgs e)
    //    {
    //        //do something
    //    }

    //    //事件触发函数
    //    private void WhenMyValueChange()
    //    {
    //        if (OnMyValueChanged != null)
    //        {
    //            OnMyValueChanged(this, null);
    //        }
    //    }
    //}

    ////public delegate void MyCompute(object sender,MyEventArgs e);

    ////public class Employee
    ////{
    ////    public event MyCompute OnMyCompute;

    ////    public virtual void FireEvent(MyEventArgs e)
    ////    {
    ////        if (OnMyCompute != null)
    ////        {
    ////            OnMyCompute(this, e);
    ////        }
    ////    }
    ////}

    ////public class MyEventArgs : EventArgs
    ////{
    ////    public readonly string _MyValue;
    ////    public MyEventArgs(string MyValue)
    ////    {
    ////        this._MyValue = MyValue;
    ////    }
    ////}

    ////public class Resource
    ////{
    ////    public void MyHandler(object sender, MyEventArgs e)
    ////    {
    ////        MessageBox.Show(e._MyValue);
    ////    }
    ////}

    ////public class Text1
    ////{
    ////    public static void text()
    ////    {
    ////        string T;
    ////        Employee ep = new Employee();
    ////        Resource rs = new Resource();
    ////        ep.OnMyCompute += new MyCompute(rs.MyHandler);
    ////        RegistryKey key = Registry.LocalMachine;
    ////        RegistryKey GetUp = key.OpenSubKey("software\\Buffer", true);
    ////        T = GetUp.GetValue("path").ToString();
    ////        GetUp.Close();
    ////        if (T == Clipboard.GetText())
    ////        {
    ////            MyEventArgs e = new MyEventArgs(T);
    ////            ep.FireEvent(e);
    ////        }
    ////        //MyEventArgs e = new MyEventArgs(T);
    ////        //ep.FireEvent(e);
    ////    }
    ////}
    //#endregion
}
