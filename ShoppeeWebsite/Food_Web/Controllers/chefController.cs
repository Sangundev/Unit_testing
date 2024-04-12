using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Food_Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;


namespace Food_Web.Controllers
{
    public class chefController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: chef
        public ActionResult Index()
        {
            return View(db.chefs.ToList());
        }


        // GET: chef/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: chef/Create
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
