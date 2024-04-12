namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Order_detail
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Od_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Productid { get; set; }

        public double? price { get; set; }

        public long? num { get; set; }

        public double? tt_money { get; set; }

        [StringLength(128)]
        public string Storeid { get; set; }

        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }

        [StringLength(50)]
        public string VoucherCode { get; set; }

        public double? Totalinvoucher { get; set; }
    }
}
