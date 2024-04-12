using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Food_Web.Models;
using Microsoft.AspNet.Identity;

namespace Food_Web.Areas.Admin.Controllers
{
    public class BlogsController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Admin/Blogs
        public ActionResult Index()
        {
            return View(db.Blogs.ToList());
        }

        // GET: Admin/Blogs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // GET: Admin/Blogs/Create
        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Blogid, Blogday, image, Blogshort, Bloglong")] Blog blog, HttpPostedFileBase Content)
        {
            if (ModelState.IsValid)
            {
                blog.Blogday = DateTime.Now;

                if (Content != null && Content.ContentLength > 0)
                {
                    var typeFile = Path.GetExtension(Content.FileName);
                    blog.image = blog.Blogid + typeFile;
                    var filePath = Path.Combine(Server.MapPath("~/Content/images/blog"), blog.image);
                    Content.SaveAs(filePath);
                }

                db.Blogs.Add(blog);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View("Create", blog);
        }



        // GET: Admin/Blogs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // POST: Admin/Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Blogid,Bloglong,Blogday,image,Blogshort")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                db.Entry(blog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blog);
        }

        // GET: Admin/Blogs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // POST: Admin/Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Blog blog = db.Blogs.Find(id);
            db.Blogs.Remove(blog);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        [HttpPost]
        public JsonResult RemoveBlog(int id)
        {
            var cm = db.Blogs.SingleOrDefault(x => x.Blogid == id);
            if (cm != null)
            {
                db.Blogs.Remove(cm);
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
