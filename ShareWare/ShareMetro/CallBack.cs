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
                up.CreatUpload(file.File.FullName, szIp, 1, nPort);

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


        public void ConversationPerformance(string remoteIp, int remotePort)
        {
            throw new NotImplementedException();
        }



        public void OpenShareFolderPerfromance(string remoteIp, int remotePort)
        {
            throw new NotImplementedException();
        }



        private void AddOnlineUserData(OnlineUserInfo user)
        {
            OnlineUserData userData = new OnlineUserData();
            userData.UserName = user.UserName;
            if (user.ImageHash != null)
            {
                string path = _vm.ImagePath + ComputeStringMd5(user.UserName) + ".jpg";
                File.Exists(path);
                userData.Image = new BitmapImage(new Uri(path));
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
            string hash = ComputeStringMd5(userName);
            string filePath = _vm.ImagePath + hash + ".jpg";

            if (File.Exists(filePath))
            {
                string fileHash = ComputeFileMd5(filePath);
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

        private static string ComputeStringMd5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder hash = new StringBuilder();
            foreach (var item in data)
            {
                hash.Append(item.ToString("x2"));
            }
            return hash.ToString();
        }

        private static string ComputeFileMd5(string path)
        {
            try
            {
                MD5 md5 = MD5.Create();
                FileStream fs = File.OpenRead(path);
                byte[] data = md5.ComputeHash(fs);
                fs.Close();
                StringBuilder hash = new StringBuilder();
                foreach (var item in data)
                {
                    hash.Append(item.ToString("x2"));
                }
                return hash.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
