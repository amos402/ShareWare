using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ShareFile
{
    [Serializable]
    public class CustFileInfo
    {

        public FileSystemInfo File { get; set; }
        public string Hash { get; set; }
        public List<string> HashList { get; set; }
        public long? Size { get; set; }
        public bool IsFolder { get; set; }
        public bool Uploaded { get; set; }

        public CustFileInfo()
        {
            Uploaded = false;
        }

        public override string ToString()
        {
            return File.FullName;
        }
    }
}
