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

        private MainWindowViewModel _vm;
        public CallBack(MainWindowViewModel vm)
        {
            //_shFile = shFile;
            _vm = vm;
         
        }

        public void DownloadPerformance(string szHash, string szIp, int nPort)
        {
            var file = _vm.Sh.FindFile(szHash);
            if (file != null)
            {
                //
            }
        }


        public void NewUser(int id, string name)
        {
            _vm.OnlineUser.Add(name);

        }

        public void RefreshUserList(List<string> userList)
        {
            // Console.WriteLine("Online users :");
            foreach (var item in userList)
            {
                _vm.OnlineUser.Add(item);
            }
        }

        public void UserLeave(string name)
        {
            _vm.OnlineUser.Remove(name);
            //  Console.WriteLine("{0} Leave", name);
        }
    }
}
