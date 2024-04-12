using DevExpress.Utils.About;
using Microsoft.AspNet.Identity;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Food_Web.Models
{
    public class HomeController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();


        public ActionResult categoryindex()
        {
            return View(db.Categories.ToListAsync());
        }
        //public ActionResult Index()
        //{
        //    var products = db.Products.ToList();

        //    return View(products);
        //}
        public ActionResult Index()
        {
            // Lấy ngày hiện tại
            DateTime currentDate = DateTime.Now;

            // Lấy ngày trước 30 ngày
            DateTime dateBefore30Days = currentDate.AddDays(-30);

            // Lấy danh sách sản phẩm với điều kiện DateCreated không quá 30 ngày tính từ ngày hiện tại
            var products = db.Products
                .Where(p => p.DateCreated >= dateBefore30Days)
                .ToList();

            return View(products);
        }


        //public static string getIdProduct = "";
        //public ActionResult Detail(string id)
        //{
        //    int productId;
        //    if (!int.TryParse(id, out productId))
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    getIdProduct = id;
        //    Product product = db.Products.Find(productId);
        //    if (product == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // Continue with your code logic to display the product details
        //    ViewBag.Productid = id;
        //    return View(product);
        //}

        public ActionResult bestsell()
        {
            var products = db.Products.Where(p => p.DiscountPercent > 0).ToList();
            return View(products);
        }
        public ActionResult hot()
        {
            var products = db.Products.Where(p => p.is_hot == true).ToList();
            return View(products);
        }



        public ActionResult Arrivals(int? page)
        {
            int pageSize = 8; // Number of products to display per page
            int pageIndex = page.HasValue ? page.Value : 1; // Current page index, default to 1 if not specified

            var products = db.Products.OrderBy(p => p.Productid).ToList(); // Sort the products by Id before pagination

            // Create a paged list of products using the pageIndex and pageSize values
            IPagedList<Product> pagedProducts = products.ToPagedList(pageIndex, pageSize);

            return View(pagedProducts);
        }


        public ActionResult Hotdeal(int? page)
        {
            int pageSize = 8; // Number of products to display per page
            int pageIndex = page.HasValue ? page.Value : 1; // Current page index, default to 1 if not specified

            var products = db.Products.Where(p => p.is_hot == true).OrderBy(p => p.Productid).ToList(); // Sort the products by Id before pagination

            // Create a paged list of products using the pageIndex and pageSize values
            IPagedList<Product> pagedProducts = products.ToPagedList(pageIndex, pageSize);

            return View(pagedProducts);
        }

        public ActionResult flasdeal(int? page)
        {
            int pageSize = 8; // Number of products to display per page
            int pageIndex = page.HasValue ? page.Value : 1; // Current page index, default to 1 if not specified

            var products = db.Products.Where(p => p.DiscountPercent > 0).OrderBy(p => p.Productid).ToList(); // Sort the products by Id before pagination

            // Create a paged list of products using the pageIndex and pageSize values
            IPagedList<Product> pagedProducts = products.ToPagedList(pageIndex, pageSize);

            return View(pagedProducts);
        }

        public ActionResult Showhot()
        {
            var products = db.Products.Where(p => p.is_hot == true).ToList();
            return View(products);
        }
        public static int getIdProduct = 0;

        public int checkprice(Product findsp)
        {
            if (findsp != null)
            {
                if (findsp.Tinhtranggiamgia == true)
                {
                    return (int)findsp.GiaGiamTheoKhungGio;
                }
                else if (findsp.DiscountedPrice != null && findsp.price != null)
                {
                    return (int?)findsp.DiscountedPrice ?? findsp.price.Value;
                }
                else
                {
                    return findsp.price.Value;
                }
            }

            // Return a default value or handle the case where findsp is null
            return 0; // Default value (you can change it based on your requirements)
        }

        public ActionResult Detail(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            getIdProduct = id;

            using (var db = new FoodcontextDB())
            {
                Product product = db.Products.Find(id);

                if (product == null)
                {
                    return HttpNotFound();
                }
                var extrafood = db.extrafoods.Where(ef => ef.Productid == id).ToList();
                ViewBag.ExtraFoods = extrafood;
                ViewBag.Productid = id;
                var totalcomment = TotalComments(id);
                 ViewBag.TotalComments = totalcomment;
                var Totalstar = TotalStar(id) ;
                ViewBag.TotalStarRating = Totalstar; // Set it to 0 if null
               var price= checkprice(product);
                ViewBag.priceprodut = price;
                return View(product);
            }
        }
        public int TotalComments(int productId)
        {
            ViewBag.ProductId = productId;
            var context = new FoodcontextDB();
            if (productId != 0)
            {
                var count = context.comment_SP.Count(p => p.product_id == productId);
                return count;
            }
            else
            {
                ViewBag.TotalComments = 0; // Or set it to another appropriate value
                return 0;
            }
        }
        public int? TotalStar(int productId)
        {
            ViewBag.ProductId = productId;
            var context = new FoodcontextDB();

            if (productId != 0)
            {
                var count = context.comment_SP.Count(p => p.product_id == productId);

                if (count > 0)
                {
                    var totalStars = context.comment_SP.Where(p => p.product_id == productId).Sum(p => p.rating);
                    var averageRating = totalStars / count;
                    return averageRating;
                }
                else
                {
                    return 0; // Or set it to another appropriate value
                }
            }
            else
            {
                return 0;
            }
        }


        //public ActionResult Detail(int id)
        //{
        //    if (id <= 0)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    using (var db = new FoodcontextDB())
        //    {
        //        Product product = db.Products.Find(id);

        //        if (product == null)
        //        {
        //            return HttpNotFound();
        //        }

        //        // Load all the related extrafoods for the product
        //        var extraFoods = db.extrafoods.Where(ef => ef.Productid == id).ToList();

        //        ViewBag.Productid = id;

        //        // Pass both the product and the list of extrafoods to the view
        //        ViewBag.Product = product;
        //        ViewBag.ExtraFoods = extraFoods;

        //        return View();
        //    }
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(comment_SP comment_SP, HttpPostedFileBase image, HttpPostedFileBase clip)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                using (var db = new FoodcontextDB())
                {
                    int maxCommentId = db.comment_SP.Max(c => (int?)c.id) ?? 0;

                    comment_SP.id = maxCommentId + 1;

                    comment_SP.user_id = User.Identity.GetUserId();

                    //comment_SP.product_id = int.Parse(getIdProduct); 

                    comment_SP.product_id = getIdProduct;
                    if (image != null && image.ContentLength > 0)
                    {
                        string imagePath = SaveImage(image);
                        comment_SP.image = imagePath;
                    }
                    if (clip != null && clip.ContentLength > 0)
                    {
                        string clipPath = SaveClip(clip);

                        // Set the clip path in the comment
                        comment_SP.clip = clipPath;
                    }
                    db.comment_SP.Add(comment_SP);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Detail", new { id = getIdProduct });
            }
            return RedirectToAction("Detail", new { id = getIdProduct });
        }

        private string SaveClip(HttpPostedFileBase clip)
        {
            string uploadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "clips");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            string clipFileName = Guid.NewGuid().ToString() + Path.GetExtension(clip.FileName);
            string clipPath = Path.Combine(uploadDir, clipFileName);

            clip.SaveAs(clipPath);

            return "/Content/clips/" + clipFileName;  // Corrected the path for clips
        }

        private string SaveImage(HttpPostedFileBase image)
        {
            // Ensure the ~/Content/products directory exists, create it if necessary
            string uploadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "products");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            // Get a unique filename for the image
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            // Specify the path where you want to save the image
            string imagePath = Path.Combine(uploadDir, fileName);

            // Save the image to the specified path
            image.SaveAs(imagePath);

            // Return the relative image path
            return "/Content/products/" + fileName;
        }
        public ActionResult ShowCommentproduct()
        {
            int productId = getIdProduct;

            using (var db = new FoodcontextDB())
            {
                var comments = db.comment_SP.Where(c => c.product_id == productId).ToList();
                return View(comments);
            }
        }

    }
}