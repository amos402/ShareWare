using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare
{
    [Serializable]
    [DataContract]
    public class FileInfoData
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Hash { get; set; }

        [DataMember]
        public long Size { get; set; }

        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public bool IsFolder { get; set; }

        [DataMember]
        public int Source { get; set; }
    }
}
