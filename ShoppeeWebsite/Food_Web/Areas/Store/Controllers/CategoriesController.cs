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
using System.IO;
using PagedList;

namespace Food_Web.Areas.Store.Controllers
{
    public class CategoriesController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Store/Categories
        public ActionResult Index(int? page, string searchString)
        {
            var categories = db.Categories.AsQueryable();

            // Thực hiện tìm kiếm nếu có chuỗi tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.Categoryname.Contains(searchString));
            }

            const int pageSize = 5;
            var pageNumber = page ?? 1;

            // Thêm OrderBy vào đây để sắp xếp dữ liệu
            categories = categories.OrderBy(c => c.Categoryname);

            var pagedCategories = categories.ToPagedList(pageNumber, pageSize);

            ViewBag.SearchString = searchString; // Truyền searchString để giữ giá trị nhập vào ô tìm kiếm
            return View(pagedCategories);
        }

        // GET: Store/Categories/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Store/Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(Category category, HttpPostedFileBase Content)
        {

            var context = new FoodcontextDB();

            if (ModelState.IsValid)
            {

                category = context.Categories.Add(category);

                if (Content != null && Content.ContentLength > 0)
                {
                    var typeFile = Path.GetExtension(Content.FileName);
                    category.image = category.Categoryid + typeFile;
                    var filePath = Path.Combine(Server.MapPath("~/Content/products"), category.image);
                    Content.SaveAs(filePath);

                }

                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("Create", category);
        }
        // GET: Store/Categories/Edit/5
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

        // POST: Store/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Categoryid,Categoryname,image")] Category category, HttpPostedFileBase Content)
        {
            if (ModelState.IsValid)
            {
                if (Content != null && Content.ContentLength > 0)
                {
                    var typeFile = Path.GetExtension(Content.FileName);
                    category.image = category.Categoryid + typeFile;
                    var filePath = Path.Combine(Server.MapPath("~/Content/products"), category.image);
                    Content.SaveAs(filePath);
                }

                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }


    }
}