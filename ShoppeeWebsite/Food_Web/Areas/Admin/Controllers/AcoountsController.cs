using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Food_Web.Models;
using System.Threading.Tasks;
using static Food_Web.Models.ManageController;

namespace Food_Web.Areas.Admin.Controllers
{
    public class AcoountsController : Controller
    {
        // GET: Admin/Acoounts
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = userManager.FindById(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }
        public ActionResult Logoff()
        {
            // Perform logout logic

            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    var currentUser = await userManager.FindByNameAsync(User.Identity.Name);

                    if (currentUser != null)
                    {
                        currentUser.Email = user.Email;
                        currentUser.PhoneNumber = user.PhoneNumber;
                        currentUser.Fullname = user.Fullname;
                        currentUser.Adress = user.Adress;
                        currentUser.Opentime = user.Opentime;
                        currentUser.Closetime = user.Closetime;
                        // Gán các thuộc tính khác của người dùng từ form

                        var result = await userManager.UpdateAsync(currentUser);

                        if (result.Succeeded)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            // Xử lý khi cập nhật thất bại
                            // Có thể thêm các thông báo lỗi vào ModelState
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error);
                            }
                        }
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý khi có lỗi xảy ra trong quá trình lưu dữ liệu
                    ModelState.AddModelError("", "An error occurred while saving the user.");
                }
            }

            return View("Index", user);
        }
        // Get View using this Method

    }
}