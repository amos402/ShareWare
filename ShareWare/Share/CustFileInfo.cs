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
       
        public FileInfo File { get; set; }
        public string Hash { get; set; }

        public override string ToString()
        {
            return File.FullName;
        }
    }
}
