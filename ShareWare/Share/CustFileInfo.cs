using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ShareFile
{
    /// <summary>
    /// 本地文件信息
    /// </summary>
    [Serializable]
    public class CustFileInfo
    {
        /// <summary>
        /// 文件信息
        /// </summary>
        public FileSystemInfo File { get; set; }

        /// <summary>
        /// 文件Hash
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// 文件分块Hash列表
        /// </summary>
        public List<string> HashList { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// 是否文件夹
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// 是否已上传
        /// </summary>
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
