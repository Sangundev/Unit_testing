using Food_Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Food_Web.Areas.Admin.Controllers
{
    public class SPtotController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();
        public ActionResult hot()
        {
           
            // Lấy danh sách sản phẩm thuộc người dùng đăng nhập và có thuộc tính is_hot bằng true
            var products = db.Products
                .Where(p => p.is_hot == true)
               ;

            return View(products.ToList());
        }
        public ActionResult Detailhot(int? id)
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