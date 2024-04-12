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
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Security;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Aspose.Words;
using System.Diagnostics;
using DocumentFormat.OpenXml.Drawing.Charts;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Microsoft.Security.Application;
using Microsoft.Extensions.Logging;
using PagedList;
using PagedList.Mvc;

namespace Food_Web.Areas.Store.Controllers
{
    public class Order_detailsController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        //GET: Store/Order_details
        public ActionResult Index(int? page)
        {
            // Get the currently logged-in user
            string userId = User.Identity.GetUserId();

            // Retrieve order details for the logged-in user
            var orderDetail = db.Order_detail
                .Include(o => o.Order)
                .Include(o => o.Product)
                .Where(o => o.Storeid == userId)
                .OrderBy(o => o.Od_id); // Replace YourProperty with the property you want to sort by

            const int pageSize = 10; // Set your desired page size
            int pageNumber = page ?? 1;

            // Apply pagination using ToPagedList method (not asynchronous)
            var pagedOrderDetails = orderDetail.ToPagedList(pageNumber, pageSize);

            return View(pagedOrderDetails);
        }


        // GET: Store/Order_details/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order_detail order_detail = await db.Order_detail.FindAsync(id);
            if (order_detail == null)
            {
                return HttpNotFound();
            }
            return View(order_detail);
        }


        [HttpPost]
        public ActionResult UpStatus(int id, bool status)
        {
            try
            {
                var orderDetail = db.Order_detail.FirstOrDefault(o => o.Order.Od_id == id);
                if (orderDetail == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                orderDetail.Order.Od_status = !status;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult Orderday(DateTime? od_day, int? page)
        {
            var userId = User.Identity.GetUserId();
            var order_details = db.Order_detail.Include(o => o.Order).Include(o => o.Product);

            if (od_day.HasValue)
            {
                order_details = order_details
                    .Where(o => DbFunctions.TruncateTime(o.Order.Od_date) == DbFunctions.TruncateTime(od_day.Value) && o.Storeid == userId)
                    .OrderBy(o => o.Order.Od_date);
                Session["selcetodday"] = od_day;
            }
            else
            {
                order_details = order_details
                    .Where(o => o.Storeid == userId)
                    .OrderBy(o => o.Order.Od_date);
            }

            const int pageSize = 10;
            int pageNumber = page ?? 1;

            var pagedOrderDetails = order_details
                .ToPagedList(pageNumber, pageSize); // Use ToPagedList for pagination

            return View(pagedOrderDetails);
        }


        public ActionResult Exportod_day()
        {
            var userId = User.Identity.GetUserId();
            var selectedOdDay = Session["selcetodday"] as DateTime?; // Get the selected od_day from Session

            List<Order_detail> orderDetails;

            if (selectedOdDay.HasValue)
            {
                orderDetails = db.Order_detail
                    .Where(o => o.Storeid == userId && DbFunctions.TruncateTime(o.Order.Od_date) == DbFunctions.TruncateTime(selectedOdDay))
                    .ToList();
            }
            else
            {
                orderDetails = db.Order_detail
                    .Where(o => o.Storeid == userId)
                    .ToList();
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Order Details");

                // Set the header row
                worksheet.Cells[1, 1].Value = "Order ID";
                worksheet.Cells[1, 2].Value = "Product ID";
                worksheet.Cells[1, 3].Value = "Total Money";
                worksheet.Cells[1, 4].Value = "Quantity";
                worksheet.Cells[1, 5].Value = "Price";
                worksheet.Cells[1, 6].Value = "Store ID";
                worksheet.Cells[1, 6].Value = "User ID";

                // Populate data rows
                for (int i = 0; i < orderDetails.Count; i++)
                {
                    var orderDetail = orderDetails[i];
                    worksheet.Cells[i + 2, 1].Value = orderDetail.Od_id;
                    worksheet.Cells[i + 2, 2].Value = orderDetail.Productid;
                    worksheet.Cells[i + 2, 3].Value = orderDetail.tt_money;
                    worksheet.Cells[i + 2, 4].Value = orderDetail.num;
                    worksheet.Cells[i + 2, 5].Value = orderDetail.price;
                    worksheet.Cells[i + 2, 6].Value = orderDetail.Storeid;
                    worksheet.Cells[i + 2, 7].Value = orderDetail.Order.Od_name;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=OrderDetails.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }

            return View("success");
        }
        [HttpPost]
        public ActionResult GetOrderDetails(int orderId)
        {
            try
            {
                var orderDetails = db.Order_detail
                    .Where(od => od.Od_id == orderId)
                    .ToList();

                // Prepare the data to be sent back to the client
                var result = new
                {
                    success = true,
                    orderDetails = orderDetails.Select(od => new
                    {
                        productName = od.Product.Productname,
                        Gia = od.price,
                        soluong = od.num,
                        quantity = od.Totalinvoucher,
                        total = od.tt_money,
                        Status = od.Order.Od_status,
                        img = od.Product.image
                    })
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an error response to the client
                var result = new
                {
                    success = false,
                    message = "An error occurred while retrieving order details."
                };

                // You may also log the error for further investigation
                // Log.Error(ex, "An error occurred while retrieving order details.");

                return Json(result);
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
        public ActionResult Exportexcel()
        {
            // Get the currently logged-in user's ID
            string userId = User.Identity.GetUserId();

            var orderDetails = db.Order_detail
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.Storeid == userId)
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("OrderDetails");

                // Set the header row
                worksheet.Cells[1, 1].Value = "Order ID";
                worksheet.Cells[1, 2].Value = "Product Name";
                worksheet.Cells[1, 3].Value = "Price";
                worksheet.Cells[1, 4].Value = "Quantity";
                worksheet.Cells[1, 5].Value = "Total Money";
                //worksheet.Cells[1, 6].Value = "Status";
                worksheet.Cells[1, 7].Value = "Image";

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Populate data rows
                for (int i = 0; i < orderDetails.Count; i++)
                {
                    var orderDetail = orderDetails[i];
                    worksheet.Cells[i + 2, 1].Value = orderDetail.Order.Od_id;
                    worksheet.Cells[i + 2, 2].Value = orderDetail.Product.Productname;
                    worksheet.Cells[i + 2, 3].Value = orderDetail.price;
                    worksheet.Cells[i + 2, 4].Value = orderDetail.num;
                    worksheet.Cells[i + 2, 5].Value = orderDetail.tt_money;
                    //worksheet.Cells[i + 2, 6].Value = orderDetail.Order.Od_status ? "Active" : "Inactive";
                    worksheet.Cells[i + 2, 7].Value = orderDetail.Product.image;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=OrderDetails.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }

            return new EmptyResult();
        }

        [HttpPost]
        [ValidateInput(false)]
        public EmptyResult ExportToWord(string GridHtml)
        {
            //Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Grid.doc");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-word";
            Response.Output.Write(GridHtml);
            Response.Flush();
            Response.End();
            return new EmptyResult();
        }
        //[HttpPost]
        //[ValidateInput(false)] // Disable request validation for this action
        //public FileResult ExportToPdf(string GridHtml)
        //{
        //    // Log the sanitized HTML content for debugging
        //    System.Diagnostics.Debug.WriteLine("Sanitized HTML content: " + Sanitizer.GetSafeHtml(GridHtml));

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        string sanitizedHtml = Sanitizer.GetSafeHtml(GridHtml);
        //        iTextSharp.text.Document document = new iTextSharp.text.Document();
        //        PdfWriter writer = PdfWriter.GetInstance(document, ms);

        //        document.Open();
        //        StringReader sr = new StringReader(sanitizedHtml); // Use the sanitized HTML
        //        try
        //        {
        //            XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, sr);
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log or handle the parsing exception
        //            System.Diagnostics.Debug.WriteLine("HTML Parsing Error: " + ex.Message);
        //        }

        //        document.Close();

        //        byte[] pdfBytes = ms.ToArray();

        //        // Return the PDF file using FileResult
        //        return File(pdfBytes, "application/pdf", "exported-pdf.pdf");
        //    }
        //}

        [HttpPost]
        [ValidateInput(false)] // Disable request validation for this action
        public FileResult ExportToPdf(string GridHtml)
        {
            System.Diagnostics.Debug.WriteLine("Received HTML content: " + GridHtml);
            using (MemoryStream ms = new MemoryStream())
            {
                iTextSharp.text.Document document = new iTextSharp.text.Document();
                PdfWriter writer = PdfWriter.GetInstance(document, ms);

                document.Open();
                try
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, new StringReader(GridHtml));
                }
                catch (Exception ex)
                {
                    // Log or handle the parsing exception
                    System.Diagnostics.Debug.WriteLine("HTML Parsing Error: " + ex.Message);
                }

                document.Close();

                byte[] pdfBytes = ms.ToArray();

                // Return the PDF file using FileResult
                return File(pdfBytes, "application/pdf", "exported-pdf.pdf");
            }
        }


    }
}