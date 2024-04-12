using Food_Web.Models;
using Microsoft.AspNet.Identity;
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
    public class SPgiamController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();
        public async Task<ActionResult> Sale()
        {
            var products = await db.Products.Where(p => p.DiscountPercent > 0).ToListAsync();
            return View(products);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

    }
}