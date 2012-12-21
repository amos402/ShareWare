using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ShareFile
{
    /// <summary>
    /// 用于与服务器通信的文件信息
    /// </summary>
    [Serializable]
    [DataContract]
    public class FileInfoTransfer
    {
        /// <summary>
        /// 文件或文件夹名
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// 文件Hash或文件夹GUID
        /// </summary>
        [DataMember]
        public string Hash { get; set; }

        /// <summary>
        /// 文件大小，文件夹无大小
        /// </summary>
        [DataMember]
        public long? Size { get; set; }

        /// <summary>
        /// 是否文件夹
        /// </summary>
        [DataMember]
        public bool IsFolder { get; set; }


        public override string ToString()
        {
            return Name;
        }

    }
}
