using Food_Web.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Food_Web
{
    public class ChatHub : Hub
    {
        private FoodcontextDB db = new FoodcontextDB();
        public void Send(string name, string message)
        {
            Clients.All.addNewMessageToPage(name, message);
        }
        public void LoadChatHistory(string otherUserId)
        {
            string currentUserId = Context.User.Identity.GetUserId();

            // Tìm tin nhắn giữa currentUserId và otherUserId
            var messages = db.Messages
                .Where(m => (m.Userid == currentUserId && m.Storeid == otherUserId) || (m.Userid == otherUserId && m.Storeid == currentUserId))
                .OrderBy(m => m.Time)
                .ToList();

            // Gửi danh sách tin nhắn đến người dùng hiện tại
            Clients.Caller.loadChatHistory(messages);
        }
        public void LoadChatHistoryForShop(string storeId)
        {
            string currentUserId = Context.User.Identity.GetUserId();

            // Tìm tin nhắn giữa currentUserId và storeId (UserID của shop)
            var messages = db.Messages
                .Where(m => (m.Userid == currentUserId && m.Storeid == storeId) || (m.Userid == storeId && m.Storeid == currentUserId))
                .OrderBy(m => m.Time)
                .ToList();

            // Gửi danh sách tin nhắn đến người dùng hiện tại
            Clients.Caller.loadChatHistory(messages);
        }
        public void SendDiscountNotification(string storeId, string discountMessage)
        {
            // Check if the current user is authorized to send discount notifications
            //if (Context.User.IsInRole("Member")) // Adjust this condition as needed
            //{
                // Broadcast the discount notification to all connected clients
                Clients.All.sendDiscountNotification(storeId, discountMessage);
            //}
        }

    }
}