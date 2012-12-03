using ConsoleApplication1.ServiceReference;
using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class ShareWareClient
    {
        private ShareServiceClient _client;
        private int _userId;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public ShareWareClient(ShareServiceClient client)
        {
            _client = client;
        }

        public bool Login(string userName, string passWord)
        {
           // _userId = _client.Login(userName, passWord);

            if (_userId != -1)
            {
                return true;
            }

            return false;
        }

        public void UploadFileInfo(List<FileInfoTransfer> fileList)
        {

           // _client.UploadShareInfo(fileList, _userId);

        }

        public void SearchFile(string fileName)
        {
           // _client.SearchFile(fileName);
        }


    }
}
