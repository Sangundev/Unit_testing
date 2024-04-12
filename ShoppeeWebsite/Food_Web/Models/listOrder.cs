using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Food_Web.Models
{
    public class listOrder
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Pic { get; set; }
        public decimal gia { get; set; }
        public int Quantity { get; set; }

    }
}