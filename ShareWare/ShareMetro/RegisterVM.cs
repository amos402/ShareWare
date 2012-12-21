using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using ShareMetro.RegisterServiceReference;
using ShareMetro.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace ShareMetro
{
    public class RegisterVM : INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
        public event EventHandler RequestClose;

        public RegisterVM()
        {
            IsShowHint = false;
            IsShowInfo = true;
            RegisterCmd = new DelegateCommand<object>(OnRegister, arg => true);
            ResetCmd = new DelegateCommand<object>(OnReset, arg => true);
            CloseCmd = new DelegateCommand<object>(OnClose, arg => true);
            UploadImageCmd = new DelegateCommand<object>(OnUploadImage, arg => true);
            User = new RegisterServiceReference.UserInfo();

            StreamResourceInfo resourceInfo = Application.GetResourceStream(new Uri(@"images\icon.jpg", UriKind.Relative));
            _image = new BitmapImage();
            _image.BeginInit();
            _image.StreamSource = resourceInfo.Stream;
            _image.EndInit();
            _defaultImage = _image;
        }

        public ShareMetro.RegisterServiceReference.UserInfo User { get; set; }

        private BitmapImage _defaultImage;
        public bool IsBusy { get; set; }
        public bool IsShowInfo { get; set; }
        public bool IsShowHint { get; set; }

        public string ErrorInfo { get; set; }
        public bool IsShowErrorInfo { get; set; }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _password2;
        public string Password2
        {
            get { return _password2; }
            set
            {
                _password2 = value;
                if (_password2 != _password)
                {
                    ErrorInfo = "两次密码不相符";
                    CanRegister = false;

                }
                else if (User.UserName != string.Empty)
                {
                    CanRegister = true;
                }
                else
                {
                    CanRegister = false;
                }

                OnPropertyChanged("Password2");
            }
        }

        private BitmapImage _image;

        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }
        public bool CanRegister { get; set; }

        public ICommand RegisterCmd { get; set; }
        public ICommand ResetCmd { get; set; }
        public ICommand UploadImageCmd { get; set; }
        public ICommand CloseCmd { get; set; }

        private void OnRegister(object obj)
        {
            IsBusy = true;
            IsShowInfo = false;
            var asd = Application.Current.Dispatcher;
            RegisterServiceClient client = new RegisterServiceClient();

            User.Image = CutForSquare(new Bitmap(Image.StreamSource), 256, 100);
            User.Password = MainWindowViewModel.ComputeStringMd5(Password);
            Task<RegError> task = client.RegisterAsync(User);
            task.ContinueWith(T =>
                {
                    switch (T.Result)
                    {
                        case RegError.NoError:
                            IsShowHint = true;
                            IsShowInfo = false;
                            IsBusy = false;
                            Thread.Sleep(3000);
                            Application.Current.Dispatcher.Invoke(new Action(() => OnClose(null)));
                            break;
                        case RegError.UserExist:
                            ErrorInfo = "该用户已存在";
                            RegisterFailed();
                            break;
                        case RegError.Ohter:
                            ErrorInfo = "注册失败，发生未知错误";
                            RegisterFailed();
                            break;
                        default:
                            break;
                    }
                    IsBusy = false;
                    client.Close();
                });

        }

        private void RegisterFailed()
        {
            IsBusy = false;
            IsShowErrorInfo = true;
            Thread.Sleep(2000);
            IsShowInfo = true;
            IsShowErrorInfo = false;
            ResetCmd.Execute(null);
            ResetCmd.Execute(null);
        }

        private void OnReset(object obj)
        {
            User.UserName = string.Empty;
            User.NickName = string.Empty;
            User.Password = string.Empty;
            User.QQ = string.Empty;
            User.MicroBlog = string.Empty;
            User.Signature = string.Empty;
            Password2 = string.Empty;
            Image = _defaultImage;
            CanRegister = false;
        }

        private void OnUploadImage(object obj)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            bool? success = dlg.ShowDialog();
            if (success == true)
            {

                try
                {
                    FileStream fs = File.OpenRead(dlg.FileName);
                    byte[] buf = new byte[fs.Length];
                    fs.Read(buf, 0, buf.Length);
                    fs.Close();
                    MemoryStream ms = new MemoryStream(buf);
                    _image = new BitmapImage();
                    _image.BeginInit();
                    _image.StreamSource = ms;
                    _image.CacheOption = BitmapCacheOption.OnLoad;
                    _image.EndInit();
                    _image.Freeze();
                    OnPropertyChanged("Image");
                }
                catch (Exception)
                {

                    //throw;
                }
            }
        }

        private void OnClose(object obj)
        {
            if (RequestClose != null)
            {
                RequestClose(this, null);
            }
        }

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

}
