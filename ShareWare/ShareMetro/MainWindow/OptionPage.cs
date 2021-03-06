﻿using Microsoft.Win32;
using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ShareMetro
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {

        public ICommand AddSharePathCmd { get; set; }
        public ICommand RemoveSharePathCmd { get; set; }

        public ICommand UploadImageCmd { get; set; }
        public ICommand SetDownloadPathCmd { get; set; }

        public ICommand AcceptCmd { get; set; }
        public ICommand CancelCmd { get; set; }
        public ICommand ConfirmCmd { get; set; }

        public ObservableCollection<string> DownloadPathList { get; set; }

        private string _currentDownloadPath;

        public string CurrentDownloadPath
        {
            get { return _currentDownloadPath; }
            set
            {
                _currentDownloadPath = value;
                OnPropertyChanged("CurrentDownloadPath");
                ShareWareSettings.Default.CurrentDownloadPath = _currentDownloadPath;
                ShareWareSettings.Default.Save();
            }
        }

        public string ErrorInfo_Option { get; set; }

        public string PasswordChangeSuccess { get; set; }
        public bool IsShowChangedResult_Option { get; set; }
        public string NickName_Option { get; set; }

        public string CurrPassword_Option { get; set; }
        public string Password_Option { get; set; }
        private string _password2_Option;

        public string Password2_Option
        {
            get { return _password2_Option; }
            set
            {
                _password2_Option = value;
                OnPropertyChanged("Password2");

                if (_password2_Option != Password_Option)
                {
                    ErrorInfo_Option = "两次密码不相符";
                    IsAcceptBtnEnable_Option = false;
                    IsConfirmBtnEnable_Option = false;

                }
                else if (NickName_Option != string.Empty)
                {
                    ErrorInfo_Option = string.Empty;
                    IsAcceptBtnEnable_Option = true;
                    IsConfirmBtnEnable_Option = true;
                }
                else
                {
                    ErrorInfo_Option = "两次密码不相符";
                    IsAcceptBtnEnable_Option = false;
                    IsConfirmBtnEnable_Option = false;
                }

                OnPropertyChanged("Password2");
            }
        }
        public bool IsMale_Option { get; set; }
        public string QQ_Option { get; set; }
        public string Microblog_Option { get; set; }
        public string Signature_Option { get; set; }
        public BitmapImage Image_Option { get; set; }
        public string ImageHash_Option { get; set; }

        private List<string> _systemSharePath = new List<string>();

        private int _index_Option;

        public int Index_Option
        {
            get { return _index_Option; }
            set
            {
                _index_Option = value;
                OnPropertyChanged("Index_Option");

                switch (_index_Option)
                {
                    case 0:
                        {


                        }
                        break;

                    case 1:
                        {
                            _sharePath.Clear();
                            foreach (var item in _sh.SharePath)
                            {
                                _sharePath.Add(new SharePathData() { ShareName = item.Key, Path = item.Value });
                            }
                        }
                        break;
                    default:
                        break;
                }


            }
        }

        public bool IsAcceptBtnEnable_Option { get; set; }
        public bool IsCancelBtnEnable_Option { get; set; }
        public bool IsConfirmBtnEnable_Option { get; set; }

        private bool _sysShareSwitch;
        public bool SysShareSwitch
        {
            get { return _sysShareSwitch; }
            set
            {
                _sysShareSwitch = value;
                if (_sh != null)
                {
                    if (_sysShareSwitch)
                    {
                        if (_sharePath != null)
                        {
                            AddSystemSharePath();
                        }
                        //_sh.AddSystemSharePath();

                    }
                    else
                    {
                        foreach (var item in _systemSharePath)
                        {
                            try
                            {
                                var result = _sharePath.Single(T => T.ShareName == item);
                                _sharePath.Remove(result);
                            }
                            catch (Exception)
                            {

                                // throw;
                            }
                        }

                    }
                }
            }
        }

        private List<SharePathData> _removePathList = new List<SharePathData>();

        private ObservableCollection<SharePathData> _sharePath;
        public ObservableCollection<SharePathData> SharePath
        {
            get
            {
                //if (_sh != null)
                //{
                //    _sharePath.Clear();
                //    foreach (var item in _sh.SharePath)
                //    {
                //        _sharePath.Add(new SharePathData() { ShareName = item.Key, Path = item.Value });
                //    }
                //    //_sharePath.Add(new SharePathData(){_sh.SharePath}
                //    return _sharePath;
                //}
                return _sharePath;
            }
            set
            {
                _sharePath = value;
                OnPropertyChanged("SharePath");
            }
        }

        private void GetUserInfo(object sender, ModelEventArgs e)
        {
            var task = _client.DownloadUserInfoAsync(_id);
            task.ContinueWith(AfterDownloadUserInfo);
        }

        private void AfterDownloadUserInfo(Task<ShareMetro.ServiceReference.UserInfo> T)
        {
            if (T.Result != null)
            {
                if (T.Result.NickName == null || T.Result.NickName == string.Empty)
                {
                    NickName_Option = T.Result.UserName;
                }
                else
                {
                    NickName_Option = T.Result.NickName;
                }

                QQ_Option = T.Result.QQ;
                Microblog_Option = T.Result.MicroBlog;
                Signature_Option = T.Result.Signature;
                IsMale_Option = T.Result.IsMale;
                ImageHash_Option = T.Result.ImageHash;

                Image_Option = ImageSource;

                PropertyChanged += OptionPropertyChanged;
            }
        }

        private void OptionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propName = e.PropertyName;
            bool hasChanged = propName == "NickName_Option" ||
                propName == "QQ_Option" ||
               propName == "Microblog" ||
               propName == "Signature_Option" ||
               propName == "Image_Option" ||
               propName == "IsMale_Option";
            if (hasChanged)
            {
                IsCancelBtnEnable_Option = true;
            }

        }

        private void ChangePersonalInfo()
        {
            var userInfo = new ShareMetro.ServiceReference.UserInfo()
            {
                NickName = NickName_Option,
                IsMale = IsMale_Option,
                QQ = QQ_Option,
                MicroBlog = Microblog_Option,
                Signature = Signature_Option
            };

            var task = _client.ChangeUserInfoAsync(userInfo);
            task.ContinueWith(T =>
                {

                });
        }

        private void AddSystemSharePath()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select  *  from  win32_share");
            _systemSharePath.Clear();

            foreach (ManagementObject share in searcher.Get())
            {
                try
                {
                    string name = share["Name"].ToString();
                    string path = share["Path"].ToString();

                    if ((share["Type"]).ToString() == "0")
                    {
                        DirectoryInfo source = new DirectoryInfo(path);
                        if (source.Exists)
                        {
                            _sharePath.Add(new SharePathData() { ShareName = name, Path = path });
                            _systemSharePath.Add(name);
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        private void ChangeSharePath()
        {
            // _sh.SharePath.Clear();
            Task task = new Task(() =>
                {
                    foreach (var item in _removePathList)
                    {
                        _sh.RemoveSharePath(item.ShareName);
                    }

                    foreach (var item in _sharePath)
                    {
                        if (!_sh.SharePath.ContainsKey(item.ShareName))
                        {
                            if (_sh.SharePath.ContainsValue(item.Path))
                            {
                                _sh.ChangeSharePath(item.ShareName, item);
                            }
                            else
                            {
                                _sh.AddSharePath(item.ShareName, item.Path);
                            }
                        }
                    }

                    //_sh.ChangeSharePath("asd", new SharePathData() { ShareName = "aaaaa", Path = @"R:\aaaaa" });
                    //_sh.RemoveSharePath(@"asd");
                    ////_sh.SharePath.Remove(@"asd");
                    //_sh.SharePath.Add(@"asd1", @"R:\");

                    _sh.ListFile().Join();

                    _client.UploadShareInfo(_sh.FileList);
                    _client.RemoveOldFile(_sh.RemoveList);
                    _sh.SetUploaded(_sh.FileList);
                    SaveShareInfo();
                });
            task.Start();

        }

        private void OnUploadImage(object obj)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            bool? success = dlg.ShowDialog();
            if (success == true)
            {
                Bitmap image = new Bitmap(dlg.FileName);
                string path = GetImageSource();
                File.Delete(path);
                image.Save(GetImageSource(), ImageFormat.Jpeg);
                _client.UploadImage(image);
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                (ThreadStart)delegate
                                {
                                    ImageSource = GetBitmapImageSource();
                                });
            }

        }

        private void OnSetDownloadPath(object obj)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            var success = dlg.ShowDialog();
            if (success == System.Windows.Forms.DialogResult.OK)
            {
                DownloadPathList.Add(dlg.SelectedPath);
                CurrentDownloadPath = dlg.SelectedPath;
            }
        }

        private void OnAddSharePath(object obj)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            var success = dlg.ShowDialog();
            if (success == System.Windows.Forms.DialogResult.OK)
            {
                string[] splitStr = dlg.SelectedPath.Split('\\');
                string shareName = splitStr[splitStr.Length - 1];
                SharePath.Add(new SharePathData() { Path = dlg.SelectedPath, ShareName = shareName });
            }
        }

        private void ChangeOption()
        {
            switch (Index_Option)
            {
                case 0:
                    ChangePersonalInfo();
                    break;

                case 1:
                    ChangeSharePath();
                    break;
                case 2:
                    {
                        ErrorInfo_Option = string.Empty;
                        var task = _client.ChangePasswordAsync(HashHelper.ComputeStringMd5(CurrPassword_Option),
                            HashHelper.ComputeStringMd5(Password_Option));

                        task.ContinueWith(T =>
                        {
                            IsShowChangedResult_Option = true;
                            if (T.Result == true)
                            {
                                PasswordChangeSuccess = "成功";
                            }
                            else
                            {
                                PasswordChangeSuccess = "失败";
                            }
                            Thread.Sleep(3000);
                            IsShowChangedResult_Option = false;
                            CurrPassword_Option = string.Empty;
                            Password_Option = string.Empty;
                            Password2_Option = string.Empty;
                        });
                    }
                    break;

                default:
                    break;
            }
        }

        private void OnRemoveSharePath(object obj)
        {
            SharePathData pathData = obj as SharePathData;
            if (pathData != null)
            {
                _sharePath.Remove(pathData);
                _removePathList.Add(pathData);
                _removePathList.Distinct();
            }
        }

        private void OnAcceptOpiton(object obj)
        {
            ChangeOption();
        }

        private void OnCancelOpiton(object obj)
        {

        }

        private bool CanCancelOption(object obj)
        {
            switch (Index_Option)
            {
                case 0:

                    break;
                default:
                    break;
            }
            return true;
        }

        private void OnConfirmOpiton(object obj)
        {
            ChangeOption();
            Index = _preIndex;
        }
    }
}
