namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Heartitem")]
    public partial class Heartitem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [StringLength(100)]
        public string ProductName { get; set; }

        public decimal? Price { get; set; }

        [StringLength(100)]
        public string Image { get; set; }

        public int? Productid { get; set; }

        [StringLength(128)]
        public string Userid { get; set; }

        public virtual Product Product { get; set; }
    }
}
