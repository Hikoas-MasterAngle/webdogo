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
    
    public partial class NHANVIENSANXUAT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NHANVIENSANXUAT()
        {
            this.TIENDOSANXUATs = new HashSet<TIENDOSANXUAT>();
        }
    
        public int MANHANVIENSANXUAT { get; set; }
        public string TENNHANVIENSANXUAT { get; set; }
        public int TUOI { get; set; }
        public string DIACHI { get; set; }
        public long SDT { get; set; }
        public long SOCCCD { get; set; }
        public string TENDANGNHAP { get; set; }
        public string MATKHAU { get; set; }
        public Nullable<System.DateTime> NGAYVAOLAM { get; set; }
        public string TRANGTHAI { get; set; }
        public Nullable<System.DateTime> NGAYNGHILAM { get; set; }
        public string VAITRO { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TIENDOSANXUAT> TIENDOSANXUATs { get; set; }
    }
}
