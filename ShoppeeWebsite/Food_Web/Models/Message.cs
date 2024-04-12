namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Message")]
    public partial class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Content { get; set; }

        public TimeSpan? Time { get; set; }

        [StringLength(128)]
        public string Storeid { get; set; }

        [StringLength(128)]
        public string Userid { get; set; }

        public string Img { get; set; }
    }
}
