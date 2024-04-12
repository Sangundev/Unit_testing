namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    [Table("Blog")]
    public partial class Blog
    {
        public int Blogid { get; set; }

        [StringLength(100)]
        [AllowHtml]
        public string Bloglong { get; set; }

        [Column(TypeName = "date")]
        public DateTime Blogday { get; set; }

        [StringLength(100)]
        public string image { get; set; }

        [StringLength(100)]
        public string Blogshort { get; set; }
    }
}
