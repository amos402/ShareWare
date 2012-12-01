using ShareMetro.ServiceReference;
using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareMetro
{
    public class CallBack : IShareServiceCallback
    {

        private ShareFiles _shFile;
        private MainPage _mainTab;
        public CallBack(MainPage mainTab)
        {
            //_shFile = shFile;
            _mainTab = mainTab;
        }

        public void DownloadPerformance(string szHash, string szIp, int nPort)
        {
            //  Console.WriteLine("send to {0} {1} {2}", szHash, szIp, nPort);
            var file = _shFile.FindFile(szHash);
            if (file != null)
            {
                //    Console.WriteLine(file.File.FullName);
            }
        }


        public void NewUser(int id, string name)
        {
            _mainTab.OnlineUser.Add(name);
        }

        public void RefreshUserList(List<string> userList)
        {
            // Console.WriteLine("Online users :");
            foreach (var item in userList)
            {
                _mainTab.OnlineUser.Add(item);
            }
        }

        public void UserLeave(string name)
        {
            //  Console.WriteLine("{0} Leave", name);
        }
    }
}
