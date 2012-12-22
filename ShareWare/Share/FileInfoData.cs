using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ShareFile
{
    /// <summary>
    /// 服务器中存储的单个文件信息
    /// </summary>
    [Serializable]
    [DataContract]
    public class FileInfoData
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
        /// 资源拥有者ID
        /// </summary>
        [DataMember]
        public int UserId { get; set; }


        /// <summary>
        /// 资源拥有者用户名
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// 是否文件夹
        /// </summary>
        [DataMember]
        public bool IsFolder { get; set; }

        /// <summary>
        /// 资源数量
        /// </summary>
        [DataMember]
        public int Source { get; set; }

        [DataMember]
        public bool IsOnline { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
