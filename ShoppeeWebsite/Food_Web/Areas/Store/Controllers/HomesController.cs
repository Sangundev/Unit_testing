using Food_Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;
using System.IO;
using GemBox.Document;
using Aspose.Words;
using System.Diagnostics;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace Food_Web.Areas.Store.Controllers
{
    public class HomesController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var totalProducts = CalculateTotalProductsForLoggedInUser();
            ViewBag.TotalStock = totalProducts;

            var TotalProductsSold = CalculateTotalnumForLoggedInStore();
            ViewBag.TotalProductsSold = TotalProductsSold;

            var TotalProductstodaySold = TotalProductsSoldToday();
            ViewBag.TotalProductstodaySold = TotalProductstodaySold;

            var TotalMoney = CalculateTotalMoneyForLoggedInStore();
            ViewBag.TotalMoney = TotalMoney;


            var products = db.Products.Where(p => p.Userid == userId);
            return View();
        }

        public int CalculateTotalnumForLoggedInStore()
        {
            var userId = User.Identity.GetUserId();
            double totalMoney = db.Order_detail.Where(o => o.Storeid == userId)
                .Select(o => o.num ?? 0)
                .DefaultIfEmpty()
                .Sum();
            int totalMoneyInt = (int)totalMoney;
            return totalMoneyInt;
        }

        public int TotalProductsSoldToday()
        {
            DateTime today = DateTime.Today;
            var userId = User.Identity.GetUserId();

            long? totalProducts = (from order in db.Orders
                                   join orderDetail in db.Order_detail on order.Od_id equals orderDetail.Od_id
                                   where orderDetail.Storeid == userId && order.Od_date != null && DbFunctions.TruncateTime(order.Od_date) == today.Date
                                   select (long?)orderDetail.num)
                       .Sum();

            int totalProductsInt = totalProducts.HasValue ? (int)totalProducts.Value : 0;

            return totalProductsInt;
        }


        public int CalculateTotalQuantitySoldToday()
        {
            var userId = User.Identity.GetUserId();
            DateTime today = DateTime.Today;

            double totalQuantity = db.Order_detail
                   .Where(o => o.Storeid == userId && o.Order.Od_date == today)
                   .Select(o => o.num ?? 0)
                   .DefaultIfEmpty()
                   .Sum();
            int totalQuantityInt = (int)totalQuantity;
            return totalQuantityInt;
        }

        public int CalculateTotalProductsForLoggedInUser()
        {
            var userId = User.Identity.GetUserId();
            int totalProducts = db.Products.Count(p => p.Userid == userId);

            if (totalProducts > 0)
            {
                return totalProducts;
            }
            return 0;
        }

        public int CalculateTotalMoneyForLoggedInStore()
        {
            var userId = User.Identity.GetUserId();
            var totalMoneyQuery = db.Order_detail
                .Where(o => o.Storeid == userId)
                .Select(o => o.tt_money);

            double? totalMoney = totalMoneyQuery.Any() ? totalMoneyQuery.Sum() : 0;
            int totalMoneyInt = Convert.ToInt32(totalMoney);
            return totalMoneyInt;
        }

        public async Task<ActionResult> Orderday()
        {
            var userId = User.Identity.GetUserId();
            var currentYear = DateTime.Now.Year;
            var firstDayOfCurrentYear = new DateTime(currentYear, 1, 1);
            var last12MonthsData = db.Order_detail
                .Where(o => o.Storeid == userId && EntityFunctions.TruncateTime(o.Order.Od_date) >= EntityFunctions.TruncateTime(firstDayOfCurrentYear))
                .GroupBy(o => EntityFunctions.TruncateTime(o.Order.Od_date))
                .Select(group => new
                {
                    Date = group.Key.Value,
                    TotalRevenue = group.Sum(o => o.tt_money) ?? 0.0,
                    OrderCount = group.Count()
                })
                .ToList();

            var monthData = new Dictionary<DateTime, RevenueData>();

            foreach (var month in Enumerable.Range(0, 12))
            {
                var targetDate = firstDayOfCurrentYear.AddMonths(month);
                var data = last12MonthsData.FirstOrDefault(d => d.Date.Year == targetDate.Year && d.Date.Month == targetDate.Month);

                if (data == null)
                {
                    monthData[targetDate] = new RevenueData
                    {
                        Date = targetDate,
                        TotalRevenue = 0.0M,
                        OrderCount = 0
                    };
                }
                else
                {
                    monthData[targetDate] = new RevenueData
                    {
                        Date = targetDate,
                        TotalRevenue = (decimal)data.TotalRevenue,
                        OrderCount = data.OrderCount
                    };
                }
            }

            var result = monthData.Values.OrderBy(data => data.Date).ToList();
            return View("OrderdayChart", result);
        }

        [HttpPost]
        [ValidateInput(false)]
        public EmptyResult ExportToWord(string GridHtml)
        {
            //Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Grid.doc");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-word";
            Response.Output.Write(GridHtml);
            Response.Flush();
            Response.End();
            return new EmptyResult();
        }
        public string RenderRazorViewToString(string viewName)
        {
            var viewEngineResult = ViewEngines.Engines.FindView(ControllerContext, viewName, null);
            if (viewEngineResult.View == null)
            {
                throw new InvalidOperationException("View not found: " + viewName);
            }

            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(ControllerContext, viewEngineResult.View, ViewData, TempData, writer);
                viewEngineResult.View.Render(viewContext, writer);
                return writer.ToString();
            }
        }

    }
}