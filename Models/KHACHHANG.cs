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
    
    public partial class KHACHHANG
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KHACHHANG()
        {
            this.GIOHANGs = new HashSet<GIOHANG>();
            this.HOADONs = new HashSet<HOADON>();
            this.LIENHEs = new HashSet<LIENHE>();
            this.SANPHAMTHEOYEUCAUs = new HashSet<SANPHAMTHEOYEUCAU>();
        }
    
        public int MAKHACHHANG { get; set; }
        public string TENDANGNHAP { get; set; }
        public string MATKHAU { get; set; }
        public string HOVATEN { get; set; }
        public Nullable<int> TUOI { get; set; }
        public Nullable<long> SDT { get; set; }
        public string EMAIL { get; set; }
        public string CAPDO { get; set; }
        public string TINH { get; set; }
        public string DIACHICHITIET { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GIOHANG> GIOHANGs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HOADON> HOADONs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LIENHE> LIENHEs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SANPHAMTHEOYEUCAU> SANPHAMTHEOYEUCAUs { get; set; }
    }
}