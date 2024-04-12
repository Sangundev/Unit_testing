namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Order
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Order()
        {
            Order_detail = new HashSet<Order_detail>();
        }

        [Key]
        public int Od_id { get; set; }

        [StringLength(50)]
        public string Od_name { get; set; }

        [StringLength(50)]
        public string Od_email { get; set; }

        public double? Od_phone_number { get; set; }

        [StringLength(100)]
        public string Od_address { get; set; }

        [StringLength(100)]
        public string Od_note { get; set; }

        public DateTime? Od_date { get; set; }

        public bool? Od_status { get; set; }

        public bool? VoidanOder { get; set; }
        public int? idthanhtoan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order_detail> Order_detail { get; set; }

        public virtual ThanhToan ThanhToan { get; set; }
    }
}
