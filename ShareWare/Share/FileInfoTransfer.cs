﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ShareFile
{
    [Serializable]
    public class FileInfoTransfer
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
