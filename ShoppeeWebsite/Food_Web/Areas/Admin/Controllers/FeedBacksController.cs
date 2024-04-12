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
    public class FeedBacksController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Admin/FeedBacks
        public ActionResult Index()
        {
            return View(db.FeedBacks.ToList());
        }

        // GET: Admin/FeedBacks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FeedBack feedBack = db.FeedBacks.Find(id);
            if (feedBack == null)
            {
                return HttpNotFound();
            }
            return View(feedBack);
        }

        // GET: Admin/FeedBacks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/FeedBacks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Fb_id,fistname,lastname,Fb_email,Fb_phone_number,subject_name")] FeedBack feedBack)
        {
            if (ModelState.IsValid)
            {
                db.FeedBacks.Add(feedBack);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(feedBack);
        }

        // GET: Admin/FeedBacks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FeedBack feedBack = db.FeedBacks.Find(id);
            if (feedBack == null)
            {
                return HttpNotFound();
            }
            return View(feedBack);
        }

        // POST: Admin/FeedBacks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Fb_id,fistname,lastname,Fb_email,Fb_phone_number,subject_name")] FeedBack feedBack)
        {
            if (ModelState.IsValid)
            {
                db.Entry(feedBack).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(feedBack);
        }

        // GET: Admin/FeedBacks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FeedBack feedBack = db.FeedBacks.Find(id);
            if (feedBack == null)
            {
                return HttpNotFound();
            }
            return View(feedBack);
        }

        // POST: Admin/FeedBacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FeedBack feedBack = db.FeedBacks.Find(id);
            db.FeedBacks.Remove(feedBack);
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
