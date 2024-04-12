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

namespace Food_Web.Controllers
{
    public class CommentController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Comment
   
            public ActionResult Index()
            {
                var userId = User.Identity.GetUserId(); // Get the current user's ID
                var reviews = db.Comments.Where(r => r.Store_id == userId).ToList(); // Retrieve comments associated with the current user's ID
                return View(reviews); // Pass the list of comments to the view for rendering
            }


        // GET: Comment/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = await db.Comments.FindAsync(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comment/Create
        public ActionResult Create()
        {
            //Request.Params["userid"] = "POST";
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = Request.Form["Userid"];
            ViewBag.Product = userId;

            return View();
        }

        // POST: Comment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "content, Store_id,created_at,Rating")] Comment comment, string userId )
        {
            if (ModelState.IsValid)
            {
                comment.comment_id = db.Comments.Max(d => d.comment_id) + 1;
                comment.user_id = User.Identity.GetUserId();
                comment.Store_id = userId;
                
                db.Comments.Add(comment);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(comment);
        }

        // GET: Comment/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = await db.Comments.FindAsync(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "comment_id,content,Store_id,user_id,created_at,Rating")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(comment).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(comment);
        }

        // GET: Comment/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = await db.Comments.FindAsync(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Comment comment = await db.Comments.FindAsync(id);
            db.Comments.Remove(comment);
            await db.SaveChangesAsync();
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
