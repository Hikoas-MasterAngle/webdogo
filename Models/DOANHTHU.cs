//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WEBDOGO.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DOANHTHU
    {
        public int MADOANHTHU { get; set; }
        public Nullable<int> SOLUONG { get; set; }
        public Nullable<double> GIADABAN { get; set; }
        public Nullable<System.DateTime> NGAYTAO { get; set; }
        public Nullable<int> MACHITIETHOADON { get; set; }
        public Nullable<int> MASANPHAMTHEOYEUCAU { get; set; }
    
        public virtual CHITIETHOADON CHITIETHOADON { get; set; }
        public virtual SANPHAMTHEOYEUCAU SANPHAMTHEOYEUCAU { get; set; }
    }
}
