namespace Food_Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FeedBack")]
    public partial class FeedBack
    {
        [Key]
        public int Fb_id { get; set; }

        [StringLength(100)]
        public string fistname { get; set; }

        [MaxLength(100)]
        public byte[] lastname { get; set; }

        [StringLength(100)]
        public string Fb_email { get; set; }

        public double? Fb_phone_number { get; set; }

        [StringLength(100)]
        public string subject_name { get; set; }
    }
}
