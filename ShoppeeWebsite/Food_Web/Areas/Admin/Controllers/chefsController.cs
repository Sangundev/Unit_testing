using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Food_Web.Models;

namespace Food_Web.Areas.Admin.Controllers
{
    public class chefsController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Admin/chefs
        public ActionResult Index()
        {
            return View(db.chefs.ToList());
        }

        // GET: Admin/chefs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            chef chef = db.chefs.Find(id);
            if (chef == null)
            {
                return HttpNotFound();
            }
            return View(chef);
        }

        // GET: Admin/chefs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/chefs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "chef_id,image,chitiet,chef_name")] chef chef)
        {
            if (ModelState.IsValid)
            {
                db.chefs.Add(chef);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(chef);
        }

        // GET: Admin/chefs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            chef chef = db.chefs.Find(id);
            if (chef == null)
            {
                return HttpNotFound();
            }
            return View(chef);
        }

        // POST: Admin/chefs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "chef_id,image,chitiet,chef_name")] chef chef)
        {
            if (ModelState.IsValid)
            {
                db.Entry(chef).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(chef);
        }

        // GET: Admin/chefs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            chef chef = db.chefs.Find(id);
            if (chef == null)
            {
                return HttpNotFound();
            }
            return View(chef);
        }

        // POST: Admin/chefs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            chef chef = db.chefs.Find(id);
            db.chefs.Remove(chef);
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
