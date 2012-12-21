using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ShareWare.ShareFile
{
    [Serializable]
    public class FileInfoDataList
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public long? Size { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsFolder { get; set; }
        public bool IsOnline { get; set; }
        public string Type
        {
            get
            {
                if (IsFolder)
                {
                    return "文件夹";
                }
                else return Name.Substring(Name.LastIndexOf(".") + 1);
            }
        }

        public string  Online
        {
            get
            {
                if (IsOnline) return "在线资源";
                else return "离线资源";
            }
        }
        
        public int Source { get; set; }
        public ImageSource largeIcon { get; set; }
        public ImageSource smallIcon { get; set; }

        public FileInfoDataList()
        {
            Name = Hash = UserName = "";
        }
        public FileInfoDataList(string n, string t, long s, string hash, ImageSource l, ImageSource sm)
        {
            Name = n;
            Size = s;
            Hash = hash;
            largeIcon = l;
            smallIcon = sm;
            if (t == "文件夹") IsFolder = true;
            else IsFolder = false;
        }
        public FileInfoDataList(FileInfoData f)
        {
            Name = f.Name;
            Hash = f.Hash;
            Size = f.Size;
            UserId = f.UserId;
            UserName = f.UserName;
            IsFolder = f.IsFolder;
            Source = f.Source;
            IsOnline = f.IsOnline;
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
            IsOnline = f.IsOnline;
        }
    }
}
