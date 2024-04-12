using Food_Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Food_Web.Areas.Admin.Controllers
{
    public class HomessController : Controller
    {
        // GET: Admin/Home

        private FoodcontextDB db = new FoodcontextDB();


        public ActionResult Index()
        {
           
            var totalProducts = CalculateTotalProductsForLoggedInUser();
            ViewBag.TotalStock = totalProducts;

            //thme
            var TotalProductsSold = CalculateTotalnumForLoggedInStore();
            ViewBag.TotalProductsSold = TotalProductsSold;

            ////them 
            var TotalProductstodaySold = CalculateTotalQuantitySoldToday();
            ViewBag.TotalProductstodaySold = TotalProductstodaySold;



            var TotalMoney = CalculateTotalMoneyForLoggedInStore();
            ViewBag.TotalMoney = TotalMoney;

            return View(/*products.ToList()*/);
        }
        public int CalculateTotalnumForLoggedInStore()
        {
           

            double totalMoney = db.Order_detail
                .Select(o => o.num ?? 0)
                .DefaultIfEmpty()
                .Sum();

            int totalMoneyInt = (int)totalMoney;

            return totalMoneyInt;
        }



        public int CalculateTotalQuantitySoldToday()
        {
           
            DateTime today = DateTime.Today;

            double totalQuantity = db.Order_detail
                   .Where(o => o.Order.Od_date == today)
                   .Select(o => o.num ?? 0)
                   .DefaultIfEmpty()
                   .Sum();
            int totalMoneytodayInt = (int)totalQuantity;
            return totalMoneytodayInt;
        }


        public int CalculateTotalProductsForLoggedInUser()
        {
            
            int totalProducts = db.Products.Count();

            if (totalProducts > 0)
            {
                return totalProducts;
            }

            return 0;
        }
        public int CalculateTotalMoneyForLoggedInStore()
        {
          

            var totalMoneyQuery = db.Order_detail
                .Select(o => o.tt_money);

            double? totalMoney = totalMoneyQuery.Any() ? totalMoneyQuery.Sum() : 0;

            int totalMoneyInt = Convert.ToInt32(totalMoney);

            return totalMoneyInt;
        }
    }
}