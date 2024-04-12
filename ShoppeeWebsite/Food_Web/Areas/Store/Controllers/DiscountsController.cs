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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.SignalR;
using System.Web.Configuration;
using DocumentFormat.OpenXml.Spreadsheet;
using PagedList;

namespace Food_Web.Areas.Store.Controllers
{
    public class DiscountsController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        public async Task<ActionResult> Index(int? page)
        {
            var storeId = User.Identity.GetUserId();
            DateTime now = DateTime.Now;

            var discounts = await db.Discounts
                .Where(p => p.StoreId == storeId)
                .ToListAsync();

            foreach (var discount in discounts)
            {
                if ((discount.StartDate <= now && now <= discount.EndDate) || discount.SoLuong > 0)
                {
                    discount.Status = true;
                }
                else if ((discount.StartDate <= now && now <= discount.EndDate) || discount.SoLuong <= 0)
                {
                    discount.Status = false;
                }
            }

            await db.SaveChangesAsync();

            // Set the page size for pagination
            int pageSize = 10; // Adjust the page size according to your needs
            int pageNumber = page ?? 1;

            // Paginate the discounts
            var pagedDiscounts = discounts.ToPagedList(pageNumber, pageSize);

            return View(pagedDiscounts);
        }


        // GET: Store/Discounts/Details/5
        public async Task<ActionResult> Details(int? id)
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

        // GET: Store/Discounts/Create
        public ActionResult Create()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "DiscountPercent,EndDate,SoLuong")] Discount discount)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        discount.Id = db.Discounts.Max(d => d.Id) + 1;
        //        // Find the store that belongs to the user
        //        discount.StoreId = User.Identity.GetUserId();

        //        // Set the start date to the current date and time
        //        discount.StartDate = DateTime.Now;


        //        discount.Code = Guid.NewGuid().ToString().Substring(0, 8);

        //        // Add the discount to the database
        //        db.Discounts.Add(discount);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(discount);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "DiscountPercent,EndDate,SoLuong")] Discount discount)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            if (ModelState.IsValid)
            {
                discount.Id = db.Discounts.Max(d => d.Id) + 1;
                discount.StoreId = User.Identity.GetUserId();
                discount.StartDate = DateTime.Now;
                discount.Code = Guid.NewGuid().ToString().Substring(0, 8);

                db.Discounts.Add(discount);
                await db.SaveChangesAsync();

                // Notify users about the successful creation of the discount
                var discountMessage = "New discount available!"; // Customize your message
                hubContext.Clients.All.SendDiscountNotification(discount.StoreId, discountMessage);

                return RedirectToAction("Index");
            }

            return View(discount);
        }


        // GET: Store/Discounts/Edit/5
        public async Task<ActionResult> Edit(int? id)
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

        // POST: Store/Discounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Code,StoreId,DiscountPercent,StartDate,SoLuong,EndDate")] Discount discount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(discount).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(discount);
        }

        [HttpPost]
        public JsonResult RemoveDiscout(int id)
        {
            var dc = db.Discounts.SingleOrDefault(x => x.Id == id);
            if (dc != null)
            {
                db.Discounts.Remove(dc);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}