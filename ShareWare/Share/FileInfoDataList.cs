using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ShareWare.ShareFile
{
    public class FileInfoDataList
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsFolder { get; set; }
        public string Type
        {
            get
            {
                if (IsFolder)
                {
                    return "文件夹";
                }
                else return Name.Substring(Name.LastIndexOf("."));
            }
        }
        public int Source { get; set; }
        public ImageSource largeIcon { get; set; }
        public ImageSource smallIcon { get; set; }

        public FileInfoDataList(FileInfoData f)
        {
            Name = f.Name;
            Hash = f.Hash;
            Size = f.Size;
            UserId = f.UserId;
            UserName = f.UserName;
            IsFolder = f.IsFolder;
            Source = f.Source;
        }

        public FileInfoDataList(FileInfoDataList f, ImageSource lar, ImageSource sma)
        {
            Name = f.Name;
            Hash = f.Hash;
            Size = f.Size;
            UserId = f.UserId;
            UserName = f.UserName;
            IsFolder = f.IsFolder;
            Source = f.Source;
            largeIcon = lar;
            smallIcon = sma;
        }
    }
}
