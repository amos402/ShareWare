//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ShareWare
{
    using System;
    using System.Collections.Generic;
    
    public partial class FileOwner
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public string Path { get; set; }
        public bool IsFolder { get; set; }
    
        public virtual FileInfo FileInfo { get; set; }
        public virtual Users Users { get; set; }
    }
}