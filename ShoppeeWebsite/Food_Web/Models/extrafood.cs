namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("extrafood")]
    public partial class extrafood
    {
        [Key]
        public int ext_id { get; set; }

        [StringLength(100)]
        public string image { get; set; }

        public int Productid { get; set; }

        public int? price { get; set; }

        public virtual Product Product { get; set; }
    }
}
