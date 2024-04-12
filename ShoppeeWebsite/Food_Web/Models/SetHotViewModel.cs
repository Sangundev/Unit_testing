using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;


namespace Food_Web.Models
{
    public class SetHotViewModel
    {
        public List<SelectListItem> Products { get; set; }
        public int SelectedProductId { get; set; }
    }
}