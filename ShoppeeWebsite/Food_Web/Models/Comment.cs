namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Comment")]
    public partial class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int comment_id { get; set; }

        public string content { get; set; }

        [StringLength(128)]
        public string Store_id { get; set; }

        [StringLength(128)]
        public string user_id { get; set; }

        public int? Rating { get; set; }

        public DateTime created { get; set; }

        public string img { get; set; }

        public string clip { get; set; }
    }
}
