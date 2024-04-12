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
using Microsoft.AspNet.Identity;

namespace Food_Web.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Admin/Categories
        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        // GET: Admin/Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        public ActionResult Create(Category newcategory, HttpPostedFileBase Content)
        {

            var context = new FoodcontextDB();

            if (ModelState.IsValid)
            {

                newcategory = context.Categories.Add(newcategory);

                if (Content != null && Content.ContentLength > 0)
                {
                    var typeFile = Path.GetExtension(Content.FileName);
                    newcategory.image = newcategory.Categoryid + typeFile;
                    var filePath = Path.Combine(Server.MapPath("~/Content/products"), newcategory.image);
                    Content.SaveAs(filePath);

                }

                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("Create", newcategory);
        }

        // GET: Admin/Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Categoryid, Categoryname")] Category newcategory, HttpPostedFileBase Content)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var find = await db.Categories.FindAsync(newcategory.Categoryid);
                    if (find == null)
                    {
                        return HttpNotFound();
                    }

                    if (Content != null && Content.ContentLength > 0)
                    {
                        string fileName = "";
                        int index = Content.FileName.IndexOf('.');
                        fileName = "ImageProduct" + newcategory.Categoryid.ToString() + "." + Content.FileName.Substring(index + 1);
                        string path = Path.Combine(Server.MapPath("~/Content/products"), fileName);
                        Content.SaveAs(path);
                        newcategory.image = fileName;
                    }
                    else
                    {
                        newcategory.image = find.image;
                    }

                    db.Entry(find).CurrentValues.SetValues(newcategory);
                    await db.SaveChangesAsync();

                    ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname", newcategory.Categoryid);
                    return RedirectToAction("Index");
                }
                catch
                {
                    // Handle any exceptions that occur during the saving process
                   
                    return View(newcategory);
                }
            }
            else
            {
                
                return View(newcategory);
            }
        }


        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
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
