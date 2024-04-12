using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Food_Web.Models;

namespace Food_Web.Controllers
{
    public class DiscountssController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Discounts
        public async Task<ActionResult> Index()
        {
            var discounts = await db.Discounts.Where(d => d.Status == true && d.SoLuong > 0).ToListAsync();
            return View(discounts);
        }


        private List<Discount> GetNewDiscounts()
        {
            // Define the date threshold (e.g., 7 days ago)
            DateTime thresholdDate = DateTime.Now.AddDays(- 7);

            // Get the new discounts based on the threshold date
            var newDiscounts = db.Discounts.Where(d => d.StartDate >= thresholdDate).ToList();
            return newDiscounts;
        }

        //[HttpGet]
        public ActionResult NewDiscounts()
        {
            // Get the new discounts
            List<Discount> newDiscounts = GetNewDiscounts();

            return View(newDiscounts);
        }


    }
}
