using Food_Web.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Food_Web.Models
{
    public class HeartController : Controller
    {
        // GET: Heart
        FoodcontextDB db;
        public HeartController()
        {
            db = new FoodcontextDB();
        }

        public ActionResult Index(int? page)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.Identity.GetUserId();
            var heartItems = db.Heartitems.Where(h => h.Userid == userId).ToList();

            int pageSize = 6; // Number of items to display per page
            int pageNumber = page ?? 1; // Current page number, default is 1

            IPagedList<Heartitem> pagedHeartItems = heartItems.ToPagedList(pageNumber, pageSize);

            return View(pagedHeartItems);
        }



        [HttpPost]
        public string AddToHeart(int id)
        {
            try
            {
                var IDUser = User.Identity.GetUserId();
                var findHeartitem = db.Heartitems.FirstOrDefault(p => p.Productid == id && p.Userid == IDUser);
                if (findHeartitem == null)
                {
                    Product findsp = db.Products.First(m => m.Productid == id);

                    var newHeartitem = new Heartitem();
                    newHeartitem.ID = db.Heartitems.Any() ? db.Heartitems.Max(d => d.ID) + 1 : 1;
                    newHeartitem.Userid = User.Identity.GetUserId();
                    newHeartitem.Productid = findsp.Productid;
                    newHeartitem.Image = findsp.image;
                    newHeartitem.Price = findsp.price.Value;
                    db.Heartitems.Add(newHeartitem);
                }
                else
                {
                    return "That bai!";
                }
                db.SaveChanges();
                return "thanh cong!";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [Authorize(Roles = "Admin")]
        public List<Heartitem> GetCartItemsFromSession()
        {
            var lstHeart = Session["Heart"] as List<Heartitem>;
            if (lstHeart == null)
            {
                lstHeart = new List<Heartitem>();
                Session["Heart"] = lstHeart;

            }
            return lstHeart;
        }
       
       
        public ActionResult HeartSummary()
        {
            int count = 0;
            var userId = User.Identity.GetUserId();


            var heartItems = db.Heartitems.Where(h => h.Userid == userId).ToList();
            if (heartItems != null)
                {
                    count = heartItems.Count;
                }

            return Content(count.ToString());
        }
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();

            using (var db = new FoodcontextDB())
            {
                var itemToRemove = db.Heartitems.FirstOrDefault(h => h.ID == id && h.Userid == userId);
                if (itemToRemove != null)
                {
                    db.Heartitems.Remove(itemToRemove);
                    db.SaveChanges();
                }
            }

            return Json(new { success = true });
        }


    }
}