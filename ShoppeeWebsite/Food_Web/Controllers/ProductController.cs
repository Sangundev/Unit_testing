using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Food_Web.Models;
using PagedList;
using PagedList.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Hosting.Server;
using System.IO;
using Microsoft.AspNet.Identity;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using DevExpress.Utils.About;

namespace Food_Web.Models
{
    //[Authorize(Roles = "Member")]
    public class ProductController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();




        public ActionResult GetCategorys()
        {
            List<Category> listCategorys = db.Categories.OrderBy(p => p.Categoryid).ToList();
            return PartialView(listCategorys);
        }

        public ActionResult GetProductByCategory(int categoryid)
        {
            List<Product> listProduct = db.Products.Where(p => p.Categoryid == categoryid && p.status == true).ToList();
            return PartialView(listProduct);
        }

        //public ActionResult Index()
        //{
        //    var context = new FoodcontextDB();
        //    return View(context.Products.ToList());
        //}

        public ActionResult Index(int? page)
        {
            const int pageSize = 12;  // Change the page size as needed
            int pageNumber = page ?? 1;

            var context = new FoodcontextDB();
            var products = context.Products.OrderBy(p => p.Productid).ToPagedList(pageNumber, pageSize);

            return View(products);
        }

        public ActionResult Sphomnay(int? page)
        {
            const int pageSize = 12;  // Change the page size as needed
            int pageNumber = page ?? 1;

            var context = new FoodcontextDB();
            var products = context.Products.OrderBy(p => p.Productid).ToPagedList(pageNumber, pageSize);

            return View(products);
        }


        public ActionResult ChangeLanguage(string lang)
        {
            CultureInfo.CurrentCulture = new CultureInfo(lang, false);
            CultureInfo.CurrentUICulture = new CultureInfo(lang, false);
            var context = new FoodcontextDB();
            return View("Index", context.Products.ToList());
        }
        // GET: Admin/Products/Details/5
        public ActionResult Details(string id)
        {
            int productId;
            if (!int.TryParse(id, out productId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Continue with your code logic to display the product details

            return View(product);
        }
        public ActionResult Error() {
            return View();
            }
        //[HttpPost]
        //public ActionResult Search(String searchString)
        //{
        //    var context = new FoodcontextDB();
        //    var results = from p in context.Products
        //                  where (p.Productname.Contains(searchString)
        //                         || p.Category.Categoryname.Contains(searchString)
        //                         || p.price.ToString().Contains(searchString))
        //                         && p.status == true
        //                  select p;

        //    if (results.Any())
        //    {
        //        return PartialView(results.ToList());
        //    }
        //    return View("Error");
        //}
        [HttpPost]
        public ActionResult Search(string searchString)
        {
            var context = new FoodcontextDB();
            if (string.IsNullOrEmpty(searchString))
            {
                return View("Error");
            }

            var results = from p in context.Products
                          where (p.Productname.Contains(searchString)
                                 || p.Category.Categoryname.Contains(searchString)
                                 || p.price.ToString().Contains(searchString))
                                 && p.status == true
                          select p;

            if (results.Any())
            {
                return PartialView(results.ToList());
            }
            return View("NoResults");
        }


        //[HttpPost]
        //public ActionResult Search(String searchString)
        //{
        //    var context = new FoodcontextDB();
        //    var results = from p in context.Products
        //                  where (p.Productname.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0
        //                         || p.Category.Categoryname.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0
        //                         || p.price.ToString().IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
        //                         && p.status == true
        //                  select p;

        //    if (results.Any())
        //    {
        //        return PartialView(results.ToList());
        //    }
        //    return View("Error");
        //}


        //public ActionResult Filter(decimal priceFrom, decimal priceTo)
        //{
        //    var filteredProducts = db.Products.Where(p => p.price >= priceFrom && p.price <= priceTo).ToList();

        //    return View(filteredProducts);
        //}


        public ActionResult Menu()
        {
            var context = new FoodcontextDB();
            var products = context.Products.Where(p => p.status == true).ToList();
            return View(products);
        }


        public ActionResult Showgiamgia()
        {
            DateTime now = DateTime.Now;
            var products = db.Products
                .Where(p => p.Tinhtranggiamgia == true &&
                            p.DiscountStartTime <= now &&
                            (!p.DiscountEndTime.HasValue || p.DiscountEndTime >= now))
                .ToList();

            return View(products);
        }






    }
}
