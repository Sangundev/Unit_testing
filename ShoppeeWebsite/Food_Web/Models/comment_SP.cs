namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class comment_SP
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }

        public string content { get; set; }

        public int? product_id { get; set; }

        [StringLength(128)]
        public string user_id { get; set; }

        public int? rating { get; set; }

        public string clip { get; set; }

        public string image { get; set; }
    }
}
