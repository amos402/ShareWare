using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ShareFile
{
    [Serializable]
    [DataContract]
    public class FileInfoTransfer
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Hash { get; set; }
        [DataMember]
        public long? Size { get; set; }
        [DataMember]
        public bool IsFolder { get; set; }


        public override string ToString()
        {
            return Name;
        }

    }
}
