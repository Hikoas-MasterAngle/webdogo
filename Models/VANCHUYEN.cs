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
    
    public partial class VANCHUYEN
    {
        public int MAVANCHUYEN { get; set; }
        public System.DateTime NGAYBATDAUGUI { get; set; }
        public Nullable<System.DateTime> NGAYDUKIENDUOCGIAO { get; set; }
        public string PHUONGTHUCVANCHUYEN { get; set; }
        public Nullable<double> CHIPHIVANCHUYEN { get; set; }
        public string MOTA { get; set; }
        public string TRANGTHAIVANCHUYEN { get; set; }
        public Nullable<int> MANHANVIENVANCHUYEN { get; set; }
        public Nullable<int> MACHITIETHOADON { get; set; }
        public Nullable<int> MASANPHAMTHEOYEUCAU { get; set; }
        public Nullable<System.DateTime> NGAYGIAOTHUCTE { get; set; }
    
        public virtual CHITIETHOADON CHITIETHOADON { get; set; }
        public virtual NHANVIENVANCHUYEN NHANVIENVANCHUYEN { get; set; }
        public virtual SANPHAMTHEOYEUCAU SANPHAMTHEOYEUCAU { get; set; }
    }
}
