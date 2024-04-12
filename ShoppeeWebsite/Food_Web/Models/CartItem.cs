namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CartItem")]
    public partial class CartItem
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string ProductName { get; set; }

        public decimal Price { get; set; }

        [Required]
        [StringLength(100)]
        public string Image { get; set; }

        public int? Quantity { get; set; }

        public decimal? Money { get; set; }

        public int? Productid { get; set; }

        [Required]
        [StringLength(128)]
        public string IdUser { get; set; }

        public virtual Product Product { get; set; }
    }
}
