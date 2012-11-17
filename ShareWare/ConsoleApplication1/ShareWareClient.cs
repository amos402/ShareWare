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

        public ShareWareClient(ref ShareServiceClient client)
        {
            _client = client;
        }

        public bool Login(string userName, string passWord)
        {
            _userId = _client.Login(userName, passWord);

            if (_userId != -1)
            {
                return true;
            }

            return false;
        }

        public void UploadFileInfo()
        {
            ShareFiles sh = new ShareFiles();
            List<ShareWare.ShareFile.FileInfoTransfer> fileList = new List<ShareWare.ShareFile.FileInfoTransfer>();
            foreach (var item in sh.ShareFileDict)
            {
                foreach (var item1 in item.Value)
                {
                    fileList.Add(new ShareWare.ShareFile.FileInfoTransfer() { Name = item1.File.Name, Hash = item1.Hash, Size = item1.File.Length });
                }
            }

            _client.UploadShareInfo(fileList, _userId);

        }

        public void SearchFile(string fileName)
        {
            _client.SearchFile(fileName);
        }


    }
}
