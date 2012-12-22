using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ShareFile
{
    public class ShareFileEvent : EventArgs
    {
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}
