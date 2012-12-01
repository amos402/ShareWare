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
        public CallBack()
        {
            //_shFile = shFile;
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
            //Console.WriteLine("New user : {0}  {1}", id, name);
        }

        public void RefreshUserList(List<string> userList)
        {
            // Console.WriteLine("Online users :");
            foreach (var item in userList)
            {
                //     Console.WriteLine(item);
            }
        }

        public void UserLeave(string name)
        {
            //  Console.WriteLine("{0} Leave", name);
        }
    }
}
