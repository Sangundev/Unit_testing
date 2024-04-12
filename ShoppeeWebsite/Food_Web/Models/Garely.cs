namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Garely")]
    public partial class Garely
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Grl_id { get; set; }

        [StringLength(100)]
        public string image { get; set; }

        public int Productid { get; set; }

        public virtual Product Product { get; set; }
    }
}
