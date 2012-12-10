using ShareMetro.ServiceReference;
using ShareWare.ShareFile;
using Socket_Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ShareMetro
{
    public class CallBack : IShareServiceCallback
    {

        private MainWindowViewModel _vm;
        public CallBack(MainWindowViewModel vm)
        {
            //_shFile = shFile;
            _vm = vm;

        }

        public void DownloadPerformance(string szHash, string szIp, int nPort)
        {
            var file = _vm.Sh.FindFile(szHash);
            if (file != null && file.File.Exists)
            {
                Upload up = new Upload();
                up.CreatUpload(file.File.FullName, szIp, nPort, file.Hash, file.HashList);

            }
        }


        public void NewUser(OnlineUserInfo user)
        {
            RefreshImage(user.UserName, user.ImageHash);
            AddOnlineUserData(user);
        }

        public void RefreshUserList(List<OnlineUserInfo> userList)
        {
            foreach (var item in userList)
            {
                RefreshImage(item.UserName, item.ImageHash);
                AddOnlineUserData(item);
            }
        }

        public void UserLeave(string name)
        {
            RemoveOnlineUser(name);
        }


        public void ConversationPerformance(string remoteIp, int remotePort)////////////
        {
            throw new NotImplementedException();
        }



        public void OpenShareFolderPerfromance(string remoteIp, int remotePort)////////
        {
            throw new NotImplementedException();
        }



        private void AddOnlineUserData(OnlineUserInfo user)
        {
            OnlineUserData userData = new OnlineUserData();
            userData.UserName = user.UserName;
            userData.NickName = user.NickName;
            if (user.ImageHash != null)
            {
                try
                {
                    string path = _vm.ImagePath + MainWindowViewModel.ComputeStringMd5(user.UserName) + ".jpg";
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
                            //image.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                            userData.Image.StreamSource = ms;
                            userData.Image.DecodePixelWidth = 200;
                            userData.Image.CacheOption = BitmapCacheOption.OnLoad;
                            userData.Image.EndInit();
                            userData.Image.Freeze(); 
                        }
                        else
                        {
                            userData.Image = new BitmapImage(new Uri(path));
                        }
                    }
                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                }
            }
            _vm.OnlineUser.Add(userData);
        }

        private void RemoveOnlineUser(string userName)
        {
            try
            {
                OnlineUserData user = _vm.OnlineUser.Single(T => T.UserName == userName);
                _vm.OnlineUser.Remove(user);
            }
            catch (Exception)
            {
                _vm.OnlineUser.Distinct();
                Console.WriteLine("无法移除用户");
            }
        }

        private void RefreshImage(string userName, string imageHash)
        {
            string hash = MainWindowViewModel.ComputeStringMd5(userName);
            string filePath = _vm.ImagePath + hash + ".jpg";

            if (File.Exists(filePath))
            {
                string fileHash = MainWindowViewModel.ComputeFileMd5(filePath);
                if (fileHash != imageHash)
                {
                    Task<Bitmap> task = _vm.Client.DownloadUserImageAsync(userName);
                    task.ContinueWith(T =>
                    {
                        T.Result.Save(filePath);
                    });
                }
            }
        }




        public void ReceiveChatRoomMessage(string msg, string userName, string nickName)
        {
            _vm.ChatRoomMsg += string.Format("{0}({1})说：{2}", userName, nickName, msg);
        }


        public void NewChatRoomMessage()
        {
            throw new NotImplementedException();
        }
    }
}
