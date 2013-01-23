using ShareMetro.ServiceReference;
using ShareWare.ShareFile;
using Socket_Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace ShareMetro
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class CallBack : IShareServiceCallback
    {

        private MainWindowViewModel _vm;
        public CallBack(MainWindowViewModel vm)
        {
            _vm = vm;
        }

        public void DownloadPerformance(string szHash, string szIp, int nPort)
        {
            var file = _vm.Sh.FindFile(szHash);
            if (file != null && file.File.Exists)
            {
                Upload up = new Upload();
                if (up.Existence(file.File.FullName))
                {
                    up.ID = _vm.UpLoad.Count;
                    up.Dele_Updata += _vm.Delete_UpLoadList;
                    up.Updata += _vm.Refresh_Speed;
                    _vm.UpLoad.Add(up);
                    for (int i = 0; i < _vm.UpLoad.Count; i++)
                    {
                        _vm.UpLoad[i].MaxSeepd = _vm.Limit_Uplaod_Speed;
                        _vm.UpLoad[i].sum = _vm.UpLoad.Count;
                    }
                    up.CreatUpload(file.File.FullName, szIp, nPort, file.Hash, file.HashList);
                }
                else
                {
                    _vm.Client.RemoveNotExistShreFile(szHash);
                }
            }
            else
            {
                _vm.Client.RemoveNotExistShreFile(szHash);
            }
        }

        private static Mutex _mut = new Mutex();
        public void NewUser(OnlineUserInfo user)
        {
            _mut.WaitOne();
            RefreshImage(user.UserName, user.ImageHash);
            AddOnlineUserData(user);
            _mut.ReleaseMutex();
        }

        public void RefreshUserList(List<OnlineUserInfo> userList)
        {
            _mut.WaitOne();
            Application.Current.Dispatcher.Invoke(() => _vm.OnlineUser.Clear());
            foreach (var item in userList)
            {
                RefreshImage(item.UserName, item.ImageHash);
                AddOnlineUserData(item);
            }
            _mut.ReleaseMutex();
        }

        public void UserLeave(string name)
        {
            RemoveOnlineUser(name);
        }


        public void ConversationPerformance(string remoteIp, int remotePort)
        {
            talkwin talk = new talkwin();
            ((talkwinMVVMcs)talk.DataContext).Detelet_Talk += _vm.Delete_TalkList;
            ((talkwinMVVMcs)talk.DataContext).talk.CreatReceiverChat(remoteIp, remotePort, _vm.NickName_Option);
            _vm.Talk_List.Add(talk);
        }

        public void OpenShareFolderPerfromance(string nickName, string remoteIp, int remotePort)
        {       
            SendShareFileInfo ssf = new SendShareFileInfo();
            ssf.Local_ID = _vm.UserName;
            ssf.Sh = _vm.Sh;
            ssf.CreatSocket(remoteIp, remotePort);
        }



        private void AddOnlineUserData(OnlineUserInfo user)
        {
            OnlineUserData userData = new OnlineUserData();
            userData.UserName = user.UserName;
            if (user.NickName == null || user.NickName == string.Empty)
            {
                userData.NickName = user.UserName;
            }
            else
            {
                userData.NickName = user.NickName;
            }

            if (user.ImageHash != null)
            {
                try
                {
                    string path = _vm.ImagePath + HashHelper.ComputeStringMd5(user.UserName) + ".jpg";
                    if (File.Exists(path))
                    {
                        if (_vm.UserName == user.UserName)
                        {
                            userData.Image = new BitmapImage();
                            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                            byte[] buf = new byte[fs.Length];
                            fs.Read(buf, 0, (int)fs.Length);
                            fs.Close();
                            MemoryStream ms = new MemoryStream(buf);

                            userData.Image.BeginInit();
                            userData.Image.StreamSource = ms;
                            userData.Image.DecodePixelWidth = 200;
                            userData.Image.CacheOption = BitmapCacheOption.OnLoad;
                            userData.Image.EndInit();
                            userData.Image.Freeze();
                        }
                        else
                        {
                            userData.Image = new BitmapImage(new Uri(path));
                            userData.Image.Freeze();
                        }
                    }
                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                }
            }
            Application.Current.Dispatcher.Invoke(() => _vm.OnlineUser.Add(userData));
        }

        private void HaveConversation(string uerName)
        {
            Application.Current.Dispatcher.Invoke(() =>
                {
                    Task task = new Task(() =>
                        {
                            var user = _vm.OnlineUser.First(T => T.UserName == uerName);
                            if (user != null)
                            {
                                while (true)
                                {
                                    user.IsVisible = user.IsVisible ? false : true;
                                }
                            }
                        });
                });
        }

        private void RemoveOnlineUser(string userName)
        {
            try
            {
                OnlineUserData user = _vm.OnlineUser.Single(T => T.UserName == userName);
                Application.Current.Dispatcher.Invoke(() => _vm.OnlineUser.Remove(user));
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() => _vm.OnlineUser.Distinct());
                Console.WriteLine("无法移除用户");
            }
        }

        private static object o = new object();
        private void RefreshImage(string userName, string imageHash)
        {
            string hash = HashHelper.ComputeStringMd5(userName);
            string filePath = _vm.ImagePath + hash + ".jpg";

            if (File.Exists(filePath))
            {
                string fileHash = HashHelper.ComputeFileMd5(filePath);
                if (fileHash != imageHash)
                {
                    Task<Bitmap> task = _vm.Client.DownloadUserImageAsync(userName);
                    task.ContinueWith(T =>
                    {
                        try
                        {
                            if (T.Result != null)
                            {
                                T.Result.Save(filePath);
                            }
                        }
                        catch (Exception)
                        {
                            //throw;
                        }
                    });
                }
            }
            else
            {
                FileInfo file = new FileInfo(filePath);
                if (!System.IO.Directory.Exists(file.DirectoryName))
                {
                    System.IO.Directory.CreateDirectory(file.DirectoryName);
                }

                Task<Bitmap> task = _vm.Client.DownloadUserImageAsync(userName);
                task.ContinueWith(T =>
                {
                    if (T.Result != null)
                    {
                        try
                        {

                            T.Result.Save(filePath);
                        }
                        catch (Exception)
                        {

                            ;
                        }
                    }
                    else
                    {
                        StreamResourceInfo resourceInfo = Application.GetResourceStream(new Uri(@"images\icon.jpg", UriKind.Relative));
                        try
                        {
                            Bitmap image = new Bitmap(resourceInfo.Stream);
                            image.Save(filePath);
                        }
                        catch (Exception)
                        {
                            
                            //throw;
                        }
                    }

                });
            }
        }


        public void ReceiveChatRoomMessage(string msg, string userName, string nickName)
        {
            _vm.ChatRoomMsg += string.Format("{0}({1})   {2}\n  {3}\n\n",
                nickName, userName, DateTime.Now.ToString("T"), msg);
        }


    }
}
