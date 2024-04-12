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

namespace Food_Web.Areas.Admin.Controllers
{
    public class Order_detailssController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Admin/Order_detail
        public async Task<ActionResult> Index()
        {
            var order_detail = db.Order_detail.Include(o => o.Order).Include(o => o.Product);
            return View(await order_detail.ToListAsync());
        }

        // GET: Admin/Order_detail/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_detail order_detail = await db.Order_detail.FindAsync(id);
            if (order_detail == null)
            {
                return HttpNotFound();
            }
            return View(order_detail);
        }

        // GET: Admin/Order_detail/Create
        public ActionResult Create()
        {
            ViewBag.Od_id = new SelectList(db.Orders, "Od_id", "Od_name");
            ViewBag.Productid = new SelectList(db.Products, "Productid", "Productname");
            return View();
        }

        // POST: Admin/Order_detail/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Od_id,Productid,price,num,tt_money,Storeid")] Order_detail order_detail)
        {
            if (ModelState.IsValid)
            {
                db.Order_detail.Add(order_detail);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Od_id = new SelectList(db.Orders, "Od_id", "Od_name", order_detail.Od_id);
            ViewBag.Productid = new SelectList(db.Products, "Productid", "Productname", order_detail.Productid);
            return View(order_detail);
        }

        // GET: Admin/Order_detail/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_detail order_detail = await db.Order_detail.FindAsync(id);
            if (order_detail == null)
            {
                return HttpNotFound();
            }
            ViewBag.Od_id = new SelectList(db.Orders, "Od_id", "Od_name", order_detail.Od_id);
            ViewBag.Productid = new SelectList(db.Products, "Productid", "Productname", order_detail.Productid);
            return View(order_detail);
        }

        // POST: Admin/Order_detail/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Od_id,Productid,price,num,tt_money,Storeid")] Order_detail order_detail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order_detail).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Od_id = new SelectList(db.Orders, "Od_id", "Od_name", order_detail.Od_id);
            ViewBag.Productid = new SelectList(db.Products, "Productid", "Productname", order_detail.Productid);
            return View(order_detail);
        }

        // GET: Admin/Order_detail/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_detail order_detail = await db.Order_detail.FindAsync(id);
            if (order_detail == null)
            {
                return HttpNotFound();
            }
            return View(order_detail);
        }

        // POST: Admin/Order_detail/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Order_detail order_detail = await db.Order_detail.FindAsync(id);
            db.Order_detail.Remove(order_detail);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult GetOrderDetailss(int orderId)
        {
            try
            {
                var orderDetails = db.Order_detail
                    .Where(od => od.Od_id == orderId)
                    .ToList();

                // Prepare the data to be sent back to the client
                var result = new
                {
                    success = true,
                    orderDetails = orderDetails.Select(od => new
                    {
                        productName = od.Product.Productname,
                        Gia = od.Product.price,
                        soluong = od.num,
                        quantity = od.tt_money,
                        Status = od.Order.Od_status,
                        img = od.Product.image
                    })
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an error response to the client
                var result = new
                {
                    success = false,
                    message = "An error occurred while retrieving order details."
                };

                // You may also log the error for further investigation
                // Log.Error(ex, "An error occurred while retrieving order details.");

                return Json(result);
            }
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
