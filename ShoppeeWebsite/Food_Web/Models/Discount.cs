namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Discount")]
    public partial class Discount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        [StringLength(128)]
        public string StoreId { get; set; }

        public int? SoLuong { get; set; }

        public double DiscountPercent { get; set; }

        public bool Status { get; set; }

        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }
    }
}
