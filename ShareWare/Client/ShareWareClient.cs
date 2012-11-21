using ShareWare.ServiceReference;
using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.Client
{
    public class ShareWareClient
    {
        private IShareService _client;
        private int _userId;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public ShareWareClient(IShareService client)
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
            List<FileInfoTransfer> fileList = new List<FileInfoTransfer>();
            foreach (var item in sh.ShareFileDict)
            {
                foreach (var item1 in item.Value)
                {
                    fileList.Add(new FileInfoTransfer());
                }
            }

            _client.UploadShareInfoAsync(fileList, _userId);

        }

        public void ShearchFile(string fileName)
        {
            _client.SearchFile(fileName);
        }


    }
}
