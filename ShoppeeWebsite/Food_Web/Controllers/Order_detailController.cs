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
using PagedList;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace Food_Web.Models
{
    public class Order_detailController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        // GET: Order_detail
        public ActionResult Index(bool? statusFilter, int? page)
        {
            // Check if the user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                var loggedInUserId = User.Identity.GetUserId();
                var orderDetails = db.Order_detail.Include(o => o.Order).Include(o => o.Product).Where(i => i.Order.Od_name == loggedInUserId);

                if (statusFilter.HasValue)
                {
                    bool status = statusFilter.Value;
                    orderDetails = orderDetails.Where(o => o.Order.Od_status == status);
                    
                }
                
                int pageSize = 7; // số sản phẩm hiển thị trên mỗi trang
                int pageIndex = page.HasValue ? page.Value : 1; // trang hiện tại, nếu không có thì mặc định là 1

                // Calculate the number of items to skip
                int itemsToSkip = (pageIndex - 1) * pageSize;

                // Sort the orderDetails based on a suitable property before applying Skip and Take
                
                orderDetails = orderDetails.OrderByDescending(o => o.Order.Od_date);
                // Get the desired page of items
                var pagedOrderDetails = orderDetails.Skip(itemsToSkip).Take(pageSize).ToList();

                // Convert the pagedOrderDetails to IPagedList
                IPagedList<Order_detail> pagedList = new StaticPagedList<Order_detail>(pagedOrderDetails, pageIndex, pageSize, orderDetails.Count());

                // Pass the filtered order details to the view
                return View(pagedList);
            }
            else
            {
                // User is not authenticated, you can handle this accordingly (e.g., redirect to login page)
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult CancelOrder(int orderId)
        {
            // Lấy đơn hàng từ database
            var order = db.Orders.FirstOrDefault(p => p.Od_id == orderId);

            // Kiểm tra xem đơn hàng có tồn tại không
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            // Kiểm tra xem có thể hủy đơn hàng không (chỉ được hủy trong ngày)
            DateTime today = DateTime.Today;
            if (order.Od_date?.Date != today)
            {
                return Json(new { success = false, message = "Order can only be canceled on the same day it was placed." });
            }

            // Thực hiện hủy đơn hàng
            order.VoidanOder = false;
            db.SaveChanges();

            return Json(new { success = true, message = "Order canceled successfully." });
        }

        [HttpPost]
        public ActionResult GetOrderDetail(int orderId)
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
                        total = od.tt_money,
                        quantity = od.Totalinvoucher,
                        Status = od.Order.Od_status,
                        img = od.Product.image,
                        odDate = od.Order.Od_date
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

        public async Task<ActionResult> tt(int? paymentStatusFilter)
        {
            // Check if the user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                var loggedInUserId = User.Identity.GetUserId();
                var orderDetails = db.Order_detail.Include(o => o.Order).Include(o => o.Product).Where(i => i.Order.Od_name == loggedInUserId);

                if (paymentStatusFilter.HasValue)
                {
                    int idThanhToan = paymentStatusFilter.Value;
                    orderDetails = orderDetails.Where(o => o.Order.idthanhtoan == idThanhToan);
                }

                // Pass the filtered order details to the view
                return View("Index", await orderDetails.ToListAsync());

            }
            else
            {
                // User is not authenticated, you can handle this accordingly (e.g., redirect to login page)
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult OrderItems(int orderId)
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
                        total = od.tt_money,
                        quantity = od.Totalinvoucher,
                        Status = od.Order.Od_status,
                        img = od.Product.image,
                        odDate = od.Order.Od_date
                    })
                };
                Session["OrderDetails"] = result;

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

                return Json(result);
            }
        }

        public List<listOrder> getListOrder()
        {
            var listOrders = Session["listOrder"] as List<listOrder>;
            if (listOrders == null)
            {
                listOrders = new List<listOrder>();
                Session["listOrder"] = listOrders;
            }
            return listOrders;
        }


        public int checkproduct(Product findsp)
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
    }   
}
