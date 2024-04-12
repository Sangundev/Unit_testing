using Food_Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Food_Web.Areas.Admin.Controllers
{
    public class VoucherController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();
        // GET: Admin/Voucher
        public async Task<ActionResult> Index()
        {
            // Get the discounts that belong to the logged in store
            var discounts = await db.Discounts.ToListAsync();

            return View(discounts);
        }
        public async Task<ActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discount discount = await db.Discounts.FindAsync(id);
            if (discount == null)
            {
                return HttpNotFound();
            }
            return View(discount);
        }

    }
}