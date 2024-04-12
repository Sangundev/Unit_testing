using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Food_Web.Models
{
    public class RevenueData
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}