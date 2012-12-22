using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare
{
    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string NickName { get; set; }

        [DataMember]
        public bool IsMale { get; set; }

        [DataMember]
        public string QQ { get; set; }

        [DataMember]
        public string MicroBlog { get; set; }

        [DataMember]
        public string Signature { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public Bitmap Image { get; set; }

        [DataMember]
        public string ImageHash { get; set; }
    }
}
