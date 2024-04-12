using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Food_Web.Models;
using Microsoft.AspNet.Identity.Owin;
using Owin;

namespace Food_Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Admin/Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Admin/Products/Details/5
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

        // GET: Admin/Products/Create
        public ActionResult Create()
        {

            ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname");
            return View();
        }

     
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(Product newproduct, HttpPostedFileBase Content)
        {
           
            var context = new FoodcontextDB();

            if (ModelState.IsValid)
            {
               
                newproduct = context.Products.Add(newproduct);

                if (Content != null && Content.ContentLength > 0)
                {
                    var typeFile = Path.GetExtension(Content.FileName);
                    newproduct.image = newproduct.Productid + typeFile;
                    var filePath = Path.Combine(Server.MapPath("~/Content/products"), newproduct.image);
                    Content.SaveAs(filePath);

                }

                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("Create", newproduct);
        }

        
        public ActionResult Edit(int? id)
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
            ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname", product.Categoryid);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Productid,Productname,price,image,discription,Categoryid,sortdiscription")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname", product.Categoryid);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
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
