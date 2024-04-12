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
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Security.Cryptography;
using Microsoft.Owin.Security.Infrastructure;

namespace Food_Web.Areas.Store.Controllers
{
    public class MessagestoreController : Controller
    {
        private FoodcontextDB db = new FoodcontextDB();

        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                // Handle the case where user ID is missing
                // You might want to return an error view or redirect to login
                return RedirectToAction("Login", "Account");
            }

            // Fetch user messages for the given store ID
            var userMessages = await db.Messages
                .Where(m => m.Storeid == userId)
                .ToListAsync();

            return View(userMessages);
        }


        //public async Task<ActionResult> Indexchat(string userid)
        //{
        //    try
        //    {
        //        var storeid = User.Identity.GetUserId();
        //        Session["CurrentUserid"] = userid;
        //        var userMessages = await db.Messages
        //            .Where(m => m.Userid == userid && m.Storeid == storeid)
        //            .OrderBy(m => m.Time)
        //            .ToListAsync();

        //        return Json(new { success = true, listMessage = userMessages }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = "Error retrieving messages: " + ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public async Task<ActionResult> Indexchat(string userid)
        {
            try
            {
                var storeid = User.Identity.GetUserId();
                Session["CurrentUserid"] = userid;
                var userMessages = await db.Messages
                    .Where(m => (m.Userid == userid && m.Storeid == storeid))
                    .OrderBy(m => m.Time)
                    .ToListAsync();

                return Json(new { success = true, listMessage = userMessages }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Error retrieving messages: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult postMessage(string message)
        {
            string storeId = User.Identity.GetUserId();
            string Userid = Session["CurrentUserid"] as string;
            if (storeId != null)
            {
                // Tạo kết nối tới SignalR Hub  
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                //Gửi tin nhắn tới tất cả các máy khách

                //hubContext.Clients.All.addNewMessageToPage(Userid, message);
                hubContext.Clients.User(storeId).SendPrivateMessage(Userid, message);

                int max = db.Messages.Max(c => c.Id);
                Message mess = new Message
                {
                    Id = max + 1,
                    Content = message,
                    Userid = Userid,
                    Storeid = storeId
                };

                db.Messages.Add(mess);
                db.SaveChanges();
                // Tạo kết nối tới SignalR Hub
                //var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                //Gửi tin nhắn tới tất cả các máy khách
                //hubContext.Clients.All.SendPrivateMessage(Userid, message);
                hubContext.Clients.All.addNewMessageToPage(Userid, message);
                return Json(new { success = true });
            }
            return Json(new { success = false, Storeid = storeId });
        }





    }

}