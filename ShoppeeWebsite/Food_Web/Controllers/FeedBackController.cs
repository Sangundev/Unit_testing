using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Food_Web.Models;
using PagedList;
using PagedList.Mvc;

namespace Food_Web.Controllers
{
    public class FeedBackController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: FeedBacks
        public ActionResult Index()
        {
        
            var feedbacks = db.FeedBacks.ToList();
            return View(feedbacks);
        }
        [Authorize]
        public ActionResult Create(string username)
        {
            var feedback = new FeedBack { fistname = username };
            return View(feedback);  
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Fb_id, Fistname, Lastname, Fb_email, Fb_phone_number, subject_name, Username")] FeedBack feedBack)
        {
            if (ModelState.IsValid)
            {
                db.FeedBacks.Add(feedBack);
                db.SaveChanges();
                return RedirectToAction("Index","Product");
            }

            return View(feedBack);
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
