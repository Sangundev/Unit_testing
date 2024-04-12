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
using System.IO;
using System.Data.Entity.Infrastructure; // For Entity Framework 6
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DevExpress.Skins;
using PagedList;
namespace Food_Web.Areas.Store.Controllers
{
    public class ProductssController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();


        public ActionResult Index(int? page, string searchString)
        {
            // Lấy thông tin người dùng đăng nhập
            var userId = User.Identity.GetUserId();

            var pageSize = 5; // Số sản phẩm hiển thị trên mỗi trang
            var pageNumber = (page ?? 1); // Trang hiện tại

            // Lấy danh sách sản phẩm thuộc người dùng đăng nhập và thực hiện tìm kiếm nếu có
            var products = db.Products
                .Where(p => p.Userid == userId && (string.IsNullOrEmpty(searchString) || p.Productname.Contains(searchString)))
                .Include(p => p.Category)
                .OrderBy(p => p.Productid) // Sắp xếp theo một trường nào đó (ví dụ: ProductId)
                .ToPagedList(pageNumber, pageSize);
            ViewBag.SearchString = searchString; // Truyền searchString để giữ giá trị nhập vào ô tìm kiếm
            return View(products);
        }
        // GET: Store/Productss/Details/5
        [HttpGet]
        public ActionResult Indextl(int? categoryId)
        {
            var userId = User.Identity.GetUserId();

            var products = db.Products
                .Where(p => p.Userid == userId && (categoryId == null || p.Categoryid == categoryId))
                .Include(p => p.Category)
                .ToList();

            var categories = db.Categories.ToList();
            ViewBag.CategoryId = new SelectList(categories, "Categoryid", "Categoryname", categoryId);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ProductListPartial", products);
            }
            Session["SelectedCategoryId"] = categoryId;
            return View(products);
        }
        public ActionResult Exporttl()
        {
            var userId = User.Identity.GetUserId();
            var selectedCategoryId = Session["SelectedCategoryId"] as int?;

            List<Product> products;

            if (selectedCategoryId.HasValue)
            {
                products = db.Products
                    .Where(p => p.Userid == userId && p.Categoryid == selectedCategoryId)
                    .ToList();
            }
            else
            {
                products = db.Products
                    .Where(p => p.Userid == userId)
                    .ToList();
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");

                // Set the header row
                worksheet.Cells[1, 1].Value = "Product ID";
                worksheet.Cells[1, 2].Value = "Product Name";
                worksheet.Cells[1, 3].Value = "Price";
                worksheet.Cells[1, 4].Value = "Description";
                worksheet.Cells[1, 5].Value = "Category";
                worksheet.Cells[1, 6].Value = "Sort Description";
                worksheet.Cells[1, 7].Value = "Status";

                // Populate data rows
                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    worksheet.Cells[i + 2, 1].Value = product.Productid;
                    worksheet.Cells[i + 2, 2].Value = product.Productname;
                    worksheet.Cells[i + 2, 3].Value = product.price;
                    worksheet.Cells[i + 2, 4].Value = product.discription;
                    worksheet.Cells[i + 2, 5].Value = product.Category?.Categoryname;
                    worksheet.Cells[i + 2, 6].Value = product.sortdiscription;
                    worksheet.Cells[i + 2, 7].Value = product.status ? "Active" : "Inactive";
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=Products.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }

            return View("success");
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var worksheet = package.Workbook.Worksheets.First();

                        int rowCount = worksheet.Dimension.Rows;

                        // Assuming the header row is present
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var newProduct = new Product
                            {
                                Userid = User.Identity.GetUserId(),
                                DateCreated = DateTime.Now,
                                Productname = worksheet.Cells[row, 2].Value?.ToString(),
                                price = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                                discription = worksheet.Cells[row, 4].Value?.ToString(),
                                sortdiscription = worksheet.Cells[row, 6].Value?.ToString(),
                                status = worksheet.Cells[row, 7].Value?.ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) ?? false,
                                Soluong = Convert.ToInt32(worksheet.Cells[row, 8].Value),
                                image = worksheet.Cells[row, 9].Value?.ToString()
                            };

                            // Get the category name from the Excel file
                            var categoryName = worksheet.Cells[row, 5].Value?.ToString();
                            if (!string.IsNullOrEmpty(categoryName))
                            {
                                var category = db.Categories.FirstOrDefault(c => c.Categoryname == categoryName);
                                if (category != null)
                                {
                                    newProduct.Categoryid = category.Categoryid;
                                }
                            }
                            var imageFile = worksheet.Cells[row, 9].Value?.ToString();

                            if (!string.IsNullOrEmpty(imageFile))
                            {
                                // Handle data URI prefix if present
                                var dataUriPrefix = "data:image/png;base64,";
                                if (imageFile.StartsWith(dataUriPrefix))
                                {
                                    imageFile = imageFile.Substring(dataUriPrefix.Length);
                                }

                                // Trim whitespaces
                                imageFile = imageFile.Trim();

                                try
                                {
                                    // Generate unique image name based on Productid
                                    var fileType = GetFileExtension(imageFile);
                                    var imageName = newProduct.Productid + "_img" + fileType;
                                    var filePath = Path.Combine(Server.MapPath("~/Content/products"), imageName);

                                    // Save the image file
                                    byte[] imageData = Convert.FromBase64String(imageFile);
                                    System.IO.File.WriteAllBytes(filePath, imageData);

                                    // Update the newProduct object with the image path
                                    newProduct.image = imageName;
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception or handle it appropriately
                                    Console.WriteLine($"Error converting Base64 to byte array: {ex.Message}");
                                }
                            }
                            // Add the new product to the database
                            db.Products.Add(newProduct);
                        }

                        // Save changes to the database
                        db.SaveChanges();

                        return Json(new { success = true, message = "Import successful" });
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error during import: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "No file selected");
            }

            return Json(new { success = false, message = "Invalid file or other validation errors" });
        }

        private string GetFileExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            int lastDotIndex = fileName.LastIndexOf('.');
            if (lastDotIndex >= 0 && lastDotIndex < fileName.Length - 1)
            {
                return fileName.Substring(lastDotIndex);
            }

            return null;
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Store/Productss/Create
        public ActionResult Create()
        {

            ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname");
            return View();
        }



        [HttpPost]
        public ActionResult Create(Product newproduct, HttpPostedFileBase MainImage, List<HttpPostedFileBase> ExtraImages)
        {
            var userId = User.Identity.GetUserId();
            var context = new FoodcontextDB();
            int imageCount = 0;

            if (ModelState.IsValid)
            {
                newproduct.Userid = userId;
                newproduct.status = true;
                newproduct.DateCreated = DateTime.Now;
                newproduct = context.Products.Add(newproduct);
                context.SaveChanges();

                if (MainImage != null && MainImage.ContentLength > 0)
                {
                    var typeFile = Path.GetExtension(MainImage.FileName);
                    newproduct.image = newproduct.Productid + typeFile;
                    var filePath = Path.Combine(Server.MapPath("~/Content/products"), newproduct.image);
                    MainImage.SaveAs(filePath);
                }

                foreach (var file in ExtraImages)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileType = Path.GetExtension(file.FileName);
                        var imageName = newproduct.Productid + "_img" + imageCount + fileType;
                        var filePath = Path.Combine(Server.MapPath("~/Content/products"), imageName);
                        file.SaveAs(filePath);

                        var extraFood = new extrafood
                        {
                            Productid = newproduct.Productid,
                            image = imageName
                            // Add other extra food properties if needed
                        };
                        context.extrafoods.Add(extraFood);
                        imageCount++;
                    }
                }

                context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname");
            return View("Create", newproduct);
        }






        // GET: Store/Productss/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname", product.Categoryid);

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Productid,Productname,price,discription,Categoryid,sortdiscription,Userid")] Product product, HttpPostedFileBase Picture, List<HttpPostedFileBase> ExtraImages)
        {
            int imageCount = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the original product information
                    var originalProduct = await db.Products.FindAsync(product.Productid);
                    if (originalProduct == null)
                    {
                        return HttpNotFound();
                    }

                    // Update the product information
                    originalProduct.Productname = product.Productname;
                    originalProduct.discription = product.discription;
                    originalProduct.Categoryid = product.Categoryid;
                    originalProduct.sortdiscription = product.sortdiscription;

                    // Check if the price has changed
                    if (originalProduct.price != product.price)
                    {
                        // Update the product price
                        originalProduct.price = product.price;

                        // Mark the product price property as modified
                        db.Entry(originalProduct).Property("price").IsModified = true;
                    }

                    // Update the image if a new image is provided
                    if (Picture != null && Picture.ContentLength > 0)
                    {
                        string fileName = "ImageProduct" + product.Productid + Path.GetExtension(Picture.FileName);
                        string filePath = Path.Combine(Server.MapPath("~/Content/products"), fileName);
                        Picture.SaveAs(filePath);
                        originalProduct.image = fileName;
                    }

                    // Save changes to the database
                    await db.SaveChangesAsync();


                    foreach (var file in ExtraImages)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileType = Path.GetExtension(file.FileName);
                            var imageName = product.Productid + "_img" + imageCount + fileType;
                            var filePath = Path.Combine(Server.MapPath("~/Content/products"), imageName);
                            file.SaveAs(filePath);

                            var extraFood = new extrafood
                            {
                                Productid = product.Productid,
                                image = imageName
                                // Add other extra food properties if needed
                            };
                            db.extrafoods.Add(extraFood);
                            imageCount++;
                        }
                    }


                    // Check if the product has been purchased
                    var orderDetails = await db.Order_detail.Where(o => o.Productid == originalProduct.Productid).ToListAsync();
                    if (!orderDetails.Any())
                    {
                        // Update the price in Order_detail table
                        foreach (var orderDetail in orderDetails)
                        {
                            orderDetail.price = originalProduct.price;
                        }

                        // Save changes to the Order_detail table
                        await db.SaveChangesAsync();
                    }

                    ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname", product.Categoryid);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname", product.Categoryid);
                    return View(product);
                }
            }
            else
            {
                ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname", product.Categoryid);
                return View(product);
            }
        }

        public ActionResult DeleteImage(int id, int? imageId)
        {
            // Kiểm tra xem sản phẩm có tồn tại không
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Tìm extrafood dựa trên id sản phẩm và imageId
            var extraFoodToDelete = db.extrafoods.SingleOrDefault(e => e.Productid == id && e.ext_id == imageId);

            if (extraFoodToDelete != null)
            {
                // Xóa extrafood khỏi cơ sở dữ liệu
                db.extrafoods.Remove(extraFoodToDelete);
                db.SaveChanges(); // Lưu thay đổi

                // Tiếp theo, bạn có thể xóa hình ảnh từ thư mục nếu được lưu trong cơ sở dữ liệu.

                // Xác định đường dẫn đến tệp hình ảnh cần xóa
                var imagePath = Path.Combine(Server.MapPath("~/Content/products"), imageId.ToString());

                if (System.IO.File.Exists(imagePath))
                {
                    // Xóa tệp hình ảnh
                    System.IO.File.Delete(imagePath);
                }

                // Chuyển hướng quay lại trang chỉnh sửa sản phẩm
                return RedirectToAction("Edit", new { id = id });
            }
            else
            {
                // Nếu extrafood không tồn tại
                return HttpNotFound();
            }

        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return PartialView("_DeleteConfirmation", product);
        }

        // POST: Store/Productss/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            var removeDetail = product.Order_detail.ToList();
            var removeCart = product.CartItems.ToList();
            db.Order_detail.RemoveRange(removeDetail);
            db.CartItems.RemoveRange(removeCart);
            db.Products.Remove(product);
            await db.SaveChangesAsync();

            // Return a JSON response indicating success
            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult RemoveProduct(int id)
        {
            try
            {
                var product = db.Products.SingleOrDefault(x => x.Productid == id);

                if (product != null)
                {
                    // Update the Userid to null so it's not associated with any user
                    product.Userid = null;
                    db.Entry(product).State = EntityState.Modified;

                    // Save changes
                    db.SaveChanges();

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Product not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while removing the product. Error: " + ex.Message });
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult UpdateStatus(int id, bool status)
        {
            // lấy sản phẩm từ database
            var product = db.Products.FirstOrDefault(p => p.Productid == id);
            if (product == null)
            {
                return Json(new { success = false });
            }
            product.status = status;
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult UpdateProductQuantity(int id, int newQuantity)
        {
            var product = db.Products.FirstOrDefault(p => p.Productid == id);
            if (product == null)
            {
                return Json(new { success = false });
            }

            // Cập nhật số lượng sản phẩm
            product.Soluong = newQuantity;

            // Cập nhật trạng thái sản phẩm
            product.status = newQuantity > 0;

            db.SaveChanges();
            return Json(new { success = true });
        }


        public ActionResult hot(int? page)
        {
            // Lấy thông tin người dùng đăng nhập
            var userId = User.Identity.GetUserId();

            // Lấy danh sách sản phẩm thuộc người dùng đăng nhập và có thuộc tính is_hot bằng true
            var productsQuery = db.Products
                .Where(p => p.Userid == userId && p.is_hot == true)
                .Include(p => p.Category);

            const int pageSize = 5; // Số sản phẩm hiển thị trên mỗi trang
            var pageNumber = page ?? 1; // Trang hiện tại

            // Sử dụng ToPagedList để chia dữ liệu thành các trang
            var pagedProducts = productsQuery.OrderBy(p => p.Productid)
                                              .ToPagedList(pageNumber, pageSize);

            return View(pagedProducts);
        }
        private List<SelectListItem> GetProductList(string userId)
        {
            var productList = db.Products
                .Where(p => p.Userid == userId)
                .Select(p => new SelectListItem
                {
                    Value = p.Productid.ToString(),
                    Text = p.Productname
                })
                .ToList();

            return productList;
        }

        public ActionResult SetHot()
        {
            // Lấy thông tin người dùng đăng nhập
            var userId = User.Identity.GetUserId();

            // Lấy danh sách sản phẩm thuộc người dùng đăng nhập
            var productList = GetProductList(userId);

            // Tạo đối tượng SetHotViewModel và gán danh sách sản phẩm vào
            var viewModel = new SetHotViewModel
            {
                Products = productList
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SetHot(SetHotViewModel model)
        {
            var product = db.Products.Find(model.SelectedProductId);
            if (product != null)
            {
                product.is_hot = true;
                db.SaveChanges();
            }

            // Lấy thông tin người dùng đăng nhập
            var userId = User.Identity.GetUserId();

            // Lấy danh sách sản phẩm thuộc người dùng đăng nhập
            var productList = GetProductList(userId);

            // Tạo đối tượng SetHotViewModel và gán danh sách sản phẩm vào
            var viewModel = new SetHotViewModel
            {
                Products = productList
            };

            return RedirectToAction("hot");
        }
        public async Task<ActionResult> DeleteHot(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = await db.Products.FindAsync(id);

            if (product == null)
            {
                return HttpNotFound();
            }


            product.is_hot = false;

            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("hot");
        }


        public async Task<ActionResult> Sale(int? page, string searchString)
        {
            var userId = User.Identity.GetUserId();

            var productsQuery = db.Products.Where(p => p.DiscountPercent > 0 && p.Userid == userId);

            // Thực hiện tìm kiếm nếu có chuỗi tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Productname.Contains(searchString));
            }

            var products = await productsQuery.ToListAsync();

            const int pageSize = 5; // Số sản phẩm hiển thị trên mỗi trang
            var pageNumber = page ?? 1;

            // Sử dụng ToPagedList để chia dữ liệu thành các trang
            var pagedProducts = products.ToPagedList(pageNumber, pageSize);

            // Truyền searchString để giữ giá trị nhập vào ô tìm kiếm
            ViewBag.SearchString = searchString;

            return View(pagedProducts);
        }


        public async Task<ActionResult> CreateDiscount()
        {
            var userId = User.Identity.GetUserId();
            var products = await db.Products.Where(p => p.Userid == userId).ToListAsync();
            ViewBag.Products = new SelectList(products, "Productid", "ProductName");
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateDiscount(int productId, int discountPercent)
        {
            var product = await db.Products.FindAsync(productId);

            if (product != null)
            {
                product.DiscountPercent = discountPercent;
                var discountedPrice = (int)(product.price * (100 - discountPercent) / 100);
                product.DiscountedPrice = discountedPrice;
                db.Entry(product).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Sale");
        }
        public async Task<ActionResult> DeleteSale(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = await db.Products.FindAsync(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Remove the sale
            product.DiscountPercent = 0;
            product.DiscountedPrice = product.price;

            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("Sale");
        }

        // GET: Sale/EditSale/{productId}
        // GET: Sale/EditSale/{productId}
        public ActionResult EditSale(int? id)
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
        public ActionResult EditSale([Bind(Include = "Categoryid,DiscountPercent")] Product product)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                product.Userid = userId; // Assign the logged-in user's ID to the UserId property

                db.Entry(product).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname", product.Categoryid);
            return View(product);
        }


        public int CalculateTotalnumForLoggedInStore()
        {
            var userId = User.Identity.GetUserId();

            double totalMoney = db.Order_detail
                .Where(o => o.Storeid == userId)
                .Select(o => o.num ?? 0)
                .DefaultIfEmpty()
                .Sum();

            int totalMoneyInt = (int)totalMoney;

            return totalMoneyInt;
        }





        public int CalculateTotalProductsForLoggedInUser()
        {
            var userId = User.Identity.GetUserId();

            int totalProducts = db.Products
                .Count(p => p.Userid == userId);

            if (totalProducts > 0)
            {
                return totalProducts;
            }

            return 0;
        }




        public int CalculateTotalMoneyForLoggedInStore()
        {
            var userId = User.Identity.GetUserId();

            var totalMoneyQuery = db.Order_detail
                .Where(o => o.Storeid == userId)
                .Select(o => o.tt_money);

            double? totalMoney = totalMoneyQuery.Any() ? totalMoneyQuery.Sum() : 0;

            int totalMoneyInt = Convert.ToInt32(totalMoney);

            return totalMoneyInt;
        }


        public ActionResult Export()
        {
            var products = db.Products.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");

                // Set the header row
                worksheet.Cells[1, 1].Value = "Product ID";
                worksheet.Cells[1, 2].Value = "Product Name";
                worksheet.Cells[1, 3].Value = "Price";
                worksheet.Cells[1, 4].Value = "Description";
                worksheet.Cells[1, 5].Value = "Category";
                worksheet.Cells[1, 6].Value = "Sort Description";
                worksheet.Cells[1, 7].Value = "Status";

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Populate data rows
                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    worksheet.Cells[i + 2, 1].Value = product.Productid;
                    worksheet.Cells[i + 2, 2].Value = product.Productname;
                    worksheet.Cells[i + 2, 3].Value = product.price;
                    worksheet.Cells[i + 2, 4].Value = product.discription;
                    worksheet.Cells[i + 2, 5].Value = product.Category?.Categoryname;
                    worksheet.Cells[i + 2, 6].Value = product.sortdiscription;
                    worksheet.Cells[i + 2, 7].Value = product.status ? "Active" : "Inactive";
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=Products.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }

            return View("success");
        }



        //public async Task<ActionResult> giamgiah()
        //{
        //    var userId = User.Identity.GetUserId();

        //    var products = await db.Products.Where(p => p.GiaGiamTheoKhungGio > 0 && p.Userid == userId).ToListAsync();

        //    return View(products);
        //}
        public async Task<ActionResult> giamgiah(int? page)
        {
            var userId = User.Identity.GetUserId();

            DateTime now = DateTime.Now;

            var products = await db.Products
                .Where(p => p.GiaGiamTheoKhungGio > 0 && p.Userid == userId)
                .ToListAsync();

            foreach (var product in products)
            {
                product.Tinhtranggiamgia = product.DiscountStartTime <= now && now <= product.DiscountEndTime;
            }

            await db.SaveChangesAsync();

            const int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại

            var pagedProducts = products.ToPagedList(pageNumber, pageSize);

            return View(pagedProducts);
        }

        public async Task<ActionResult> Creategiamgiah()
        {
            var userId = User.Identity.GetUserId();
            var products = await db.Products.Where(p => p.Userid == userId).ToListAsync();
            ViewBag.Product = new SelectList(products, "Productid", "ProductName");
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Creategiamgiah(int productId, int? phantramgiamgia, DateTime? discountStartTime, DateTime? discountEndTime)
        {
            var product = await db.Products.FindAsync(productId);

            if (product != null)
            {
                if (phantramgiamgia.HasValue)
                {
                    product.phantramgiamgia = phantramgiamgia.Value;
                    var discountedPrice = (int)(product.price * (100 - phantramgiamgia.Value) / 100);
                    product.GiaGiamTheoKhungGio = discountedPrice;
                }

                if (discountStartTime.HasValue)
                {
                    product.DiscountStartTime = discountStartTime.Value;
                }

                if (discountEndTime.HasValue)
                {
                    product.DiscountEndTime = discountEndTime.Value;
                }

                // Kiểm tra xem thời gian hiện tại có nằm trong khoảng giảm giá hay không
                DateTime now = DateTime.Now;
                if (now >= product.DiscountStartTime && now <= product.DiscountEndTime)
                {
                    product.Tinhtranggiamgia = true;
                }
                else
                {
                    product.Tinhtranggiamgia = false;
                }

                db.Entry(product).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("giamgiah");
        }


        public async Task<ActionResult> ClearDiscount(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = await db.Products.FindAsync(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Đặt các thuộc tính bằng null
            product.phantramgiamgia = null;
            product.GiaGiamTheoKhungGio = null;
            product.Tinhtranggiamgia = null;
            product.DiscountStartTime = null;
            product.DiscountEndTime = null;

            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("giamgiah");
        }
        public async Task<ActionResult> EditDiscount(int? id)
        {
            if (id == null)
            {
                // Trả về thông báo lỗi
                ViewBag.ErrorMessage = "Không tìm thấy sản phẩm để chỉnh sửa.";
                return View("Error"); // Tạo view "Error" để hiển thị thông báo lỗi.
            }

            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categoryid = new SelectList(db.Categories, "Categoryid", "Categoryname", product.Categoryid);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDiscount(int productId, int? phantramgiamgia, DateTime? discountStartTime, DateTime? discountEndTime)
        {
            var product = await db.Products.FindAsync(productId);

            if (product != null)
            {
                if (phantramgiamgia.HasValue)
                {
                    product.phantramgiamgia = phantramgiamgia.Value;
                    var discountedPrice = (int)(product.price * (100 - phantramgiamgia.Value) / 100);
                    product.GiaGiamTheoKhungGio = discountedPrice;
                }

                if (discountStartTime.HasValue)
                {
                    product.DiscountStartTime = discountStartTime.Value;
                }

                if (discountEndTime.HasValue)
                {
                    product.DiscountEndTime = discountEndTime.Value;
                }

                // Kiểm tra xem thời gian hiện tại có nằm trong khoảng giảm giá hay không
                DateTime now = DateTime.UtcNow;
                if (now >= product.DiscountStartTime && now <= product.DiscountEndTime)
                {
                    product.Tinhtranggiamgia = true;
                }
                else
                {
                    product.Tinhtranggiamgia = false;
                }

                db.Entry(product).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("giamgiah");
        }



    }
}