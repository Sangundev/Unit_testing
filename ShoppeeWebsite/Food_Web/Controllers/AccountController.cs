using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Food_Web.Models;
using Microsoft.Ajax.Utilities;
using System.Net.Mail;
using System.Net;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;

namespace Food_Web.Models
{
    [Authorize]
    public class AccountController : Controller
    {

    

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl,FormCollection fc)
        {
            //var response = Request["g-recaptcha-response"];
            //string secretKey = "6LfM7_8oAAAAAFgE91nVCyTSpL-yN-9bUPt_q0fF";

            // Check if either username or password is not provided
            if (string.IsNullOrEmpty(model.EmailOrUsername) || string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("", "Please enter both username/email and password.");
                return View("Error");
            }

            if (!ModelState.IsValid)
            {
                return View("Error");
            }

            var user = await UserManager.FindByNameAsync(model.EmailOrUsername)
                        ?? await UserManager.FindByEmailAsync(model.EmailOrUsername);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View("Error");
            }

            string storedPasswordHash = user.PasswordHash;
            SignInStatus result;

            if (model.Password == storedPasswordHash)
            {
                return RedirectToAction("Index", "Product");
            }
            else
            {
                result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);
            }

            switch (result)
            {
                case SignInStatus.Success:
                    var roles = await UserManager.GetRolesAsync(user.Id);

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Products", new { area = "Admin" });
                    }
                    else if (roles.Contains("User"))
                    {
                        if (!user.IsApproved)
                        {
                            ViewBag.Message = "Tài khoản đang đợi Admin Duyệt hoặc bị block.";
                            return View("Message");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Productss", new { area = "Store" });
                        }
                    }
                    else if (roles.Contains("Member"))
                    {
                        return RedirectToAction("Index", "Product");
                    }
                    else
                    {
                        return RedirectToLocal(returnUrl);
                    }

                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }


        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            {
        //                var user = await UserManager.FindByEmailAsync(model.Email);
        //                var roles = await UserManager.GetRolesAsync(user.Id);

        //                if (roles.Contains("Admin"))
        //                {
        //                    return RedirectToAction("Index", "Products", new { area = "Admin" });
        //                }
        //                else if (roles.Contains("User"))
        //                {

        //                    //return RedirectToAction("Index", "Productss", new { area = "Store" });
        //                    if (!user.IsApproved)
        //                    {
        //                        ViewBag.Message = "Tài khoản Đang đợi Admin Duyệt / hoặc bị blog.";
        //                        return View("Message");
        //                    }
        //                    else
        //                    {
        //                        return RedirectToAction("Index", "Productss", new { area = "Store" });
        //                    }
        //                }
        //                else if (roles.Contains("Member"))
        //                {
        //                    return RedirectToAction("Index", "Product");
        //                }
        //                else
        //                {
        //                    return RedirectToLocal(returnUrl);
        //                }


        //            }
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Invalid login attempt.");
        //            return View(model);
        //    }
        //}


        //
        // GET: /Account/VerifyCode
        // GET: /Account/Login

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

 

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, IsApproved = false };
        //        //var user = new ApplicationUser { UserName = model.UserName, Email = model.Email};

        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await UserManager.AddToRoleAsync(user.Id, model.Role);

        //            // Get the list of roles for the current user
        //            var roles = await UserManager.GetRolesAsync(user.Id);

        //            // Redirect to the corresponding page based on the user role
        //            if (roles.Contains("Member"))
        //            {

        //                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
        //                // Send an email with this link
        //                 //string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //                 //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //                 //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
        //                return RedirectToAction("Index", "Product");
        //            }
        //            else if (roles.Contains("User"))
        //            {
        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                ViewBag.Message = "Your account has been created and is awaiting approval from the admin.";

        //                // Redirect to the home page
        //                return RedirectToAction("Index", "Product");
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, IsApproved = false, PhoneNumber = model.PhoneNumber ,Adress = model.Adress,Closetime=model.Closetime,Opentime=model.Opentime,Fullname = model.Fullname};
        //        //var user = new ApplicationUser { UserName = model.UserName, Email = model.Email};

        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await UserManager.AddToRoleAsync(user.Id, model.Role);

        //            //// Gửi email đăng ký thành công
        //            //SendRegistrationEmail(model.Email);

        //            // Get the list of roles for the current user
        //            var roles = await UserManager.GetRolesAsync(user.Id);

        //            // Redirect to the corresponding page based on the user role
        //            if (roles.Contains("Member"))
        //            {

        //                return RedirectToAction("Index", "Product");
        //            }
        //            else if (roles.Contains("User"))
        //            {
        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                // Display message for user account approval
        //                ViewBag.Message = "Tài khoản Đang đợi Admin Duyệt";
        //                return View("Message");

        //                // Redirect to the home page
        //                //return RedirectToAction("Index", "Product");
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser
        //        {
        //            UserName = model.UserName,
        //            Email = model.Email,
        //            IsApproved = false,
        //            PhoneNumber = model.PhoneNumber,
        //            Adress = model.Adress,
        //            Fullname = model.Fullname
        //        };

        //        string confirmationCode = Guid.NewGuid().ToString().Substring(0, 8);

        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await UserManager.AddToRoleAsync(user.Id, model.Role);

        //            if (model.Role == "Member")
        //            {
        //                //For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
        //                //Send an email with this link
        //                //string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //                //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //                //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
        //                string subject = "Xác nhận tài khoản";
        //                string body = "Mã xác nhận của bạn là: " + confirmationCode;
        //                string toAddress = model.Email;

        //                sendgmail(subject, body, toAddress);
        //                return RedirectToAction("Index", "Product");
        //            }
        //            else if (model.Role == "User")
        //            {
        //                user.Closetime = model.Closetime;
        //                user.Opentime = model.Opentime;
        //                await UserManager.UpdateAsync(user);

        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                ViewBag.Message = "Tài khoản đang đợi Admin duyệt";
        //                return View("Message");
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay the form
        //    return View(model);
        //}
        public void sendgmail(string subject, string body, string toAddress)
        {
            try
            {
                var fromAddress = "sannguyen261102@gmail.com"; // Replace with your email address
                var fromPassword = "xkjfdjzcsswokxle"; // Replace with your email password

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromAddress);
                    mail.To.Add(toAddress);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com"))
                    {
                        smtp.Port = 587;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                        smtp.EnableSsl = true;

                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log the error message for debugging
            }
        }
        //private bool CheckConfirmationCode(string confirmationCode)
        //{
        //    if (Session["ConfirmationCode"] != null)
        //    {
        //        string storedConfirmationCode = Session["ConfirmationCode"].ToString();
        //        return confirmationCode == storedConfirmationCode;
        //    }

        //    return false;
        //}

        private bool CheckConfirmationCode(string confirmationCode, HttpSessionStateBase session)
        {
            if (session["ConfirmationCode"] != null)
            {
                string storedConfirmationCode = session["ConfirmationCode"].ToString();
                return confirmationCode == storedConfirmationCode;
            }

            return false;
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult SendConfirmationCode(string email)
        {
            string confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 8);

            // Send the confirmation code via email
            string subject = "Xác nhận tài khoản";
            string body = "Mã xác nhận của bạn là: " + confirmationCode;

            try
            {
                var fromAddress = "sannguyen261102@gmail.com"; // Replace with your email address
                using (MailMessage mail = new MailMessage(fromAddress, email))
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com"))
                {
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(fromAddress, "xkjfdjzcsswokxle"); // Replace with your email password

                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    smtp.Send(mail);

                    // Store the confirmation code in a session variable
                    Session["ConfirmationCode"] = confirmationCode;

                    return Json(new { success = true, message = "Confirmation code sent to your email." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error sending confirmation code." });
            }
        }
        // dung dc


        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model, string confirmationCode)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        //// Tạo và lưu trữ mã xác nhận tạm thời
        //        //SendConfirmationCode(model.Email);

        //        // Kiểm tra mã xác nhận nhập từ biểu mẫu
        //        if (!CheckConfirmationCode(confirmationCode))
        //        {
        //            ModelState.AddModelError("ConfirmationCode", "Mã xác nhận không chính xác.");
        //            return View(model);
        //        }

        //        var user = new ApplicationUser
        //        {
        //            UserName = model.UserName,
        //            Email = model.Email,
        //            IsApproved = false,
        //            PhoneNumber = model.PhoneNumber,
        //            Adress = model.Adress,
        //            Fullname = model.Fullname
        //        };

        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await UserManager.AddToRoleAsync(user.Id, model.Role);

        //            if (model.Role == "Member")
        //            {
        //                return RedirectToAction("Index", "Product");
        //            }
        //            else if (model.Role == "User")
        //            {
        //                user.Closetime = model.Closetime;
        //                user.Opentime = model.Opentime;
        //                await UserManager.UpdateAsync(user);

        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                ViewBag.Message = "Tài khoản đang đợi Admin duyệt";
        //                return View("Message");
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay the form
        //    return View(model);
        //}
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string confirmationCode, HttpSessionStateBase session)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xác nhận mã từ biểu mẫu
                if (!CheckConfirmationCode(confirmationCode, session))
                {
                    ModelState.AddModelError("confirmationCode", "Mã xác nhận không chính xác.");
                }

                // Kiểm tra nếu Role không được chọn
                if (string.IsNullOrEmpty(model.Role))
                {
                    ModelState.AddModelError("Role", "Vui lòng chọn vai trò.");
                    return View("Register", model); // Trả về View "Register" với model và ModelState hiện tại
                }

                // Kiểm tra và thêm lỗi nếu các trường bắt buộc bị trống
                if (string.IsNullOrEmpty(model.UserName))
                {
                    ModelState.AddModelError("UserName", "Vui lòng nhập tên người dùng.");
                }

                if (string.IsNullOrEmpty(model.Email))
                {
                    ModelState.AddModelError("Email", "Vui lòng nhập địa chỉ email.");
                }

                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");
                }

                // Nếu không có lỗi, tiến hành tạo người dùng mới
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        IsApproved = false,
                        PhoneNumber = model.PhoneNumber,
                        Adress = model.Adress,
                        Fullname = model.Fullname
                    };

                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await UserManager.AddToRoleAsync(user.Id, model.Role);

                        if (model.Role == "Member")
                        {
                            return RedirectToAction("Index", "Product");
                        }
                        else if (model.Role == "User")
                        {
                            user.Closetime = model.Closetime;
                            user.Opentime = model.Opentime;
                            await UserManager.UpdateAsync(user);

                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            ViewBag.Message = "Tài khoản đang đợi Admin duyệt";
                            return View("Message");
                        }
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // Nếu có lỗi, hiển thị lại form đăng ký với thông báo lỗi
            return View("Register", model);
        }



        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Tạo và lưu trữ mã xác nhận tạm thời
        //        string IsEmailConfirmed = Guid.NewGuid().ToString().Substring(0, 8);
        //        TempData["ConfirmationCode"] = IsEmailConfirmed;

        //        // Gửi mã xác nhận qua email
        //        string subject = "Xác nhận tài khoản";
        //        string body = "Mã xác nhận của bạn là: " + IsEmailConfirmed;
        //        string toAddress = model.Email;
        //        sendgmail(subject, body, toAddress);

        //        // Kiểm tra mã xác nhận nhập từ biểu mẫu
        //        if (!CheckConfirmationCode(IsEmailConfirmed))
        //        {
        //            ModelState.AddModelError("ConfirmationCode", "Mã xác nhận không chính xác.");
        //            return View(model);
        //        }

        //        var user = new ApplicationUser
        //        {
        //            UserName = model.UserName,
        //            Email = model.Email,
        //            IsApproved = false,
        //            PhoneNumber = model.PhoneNumber,
        //            Adress = model.Adress,
        //            Fullname = model.Fullname
        //        };

        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await UserManager.AddToRoleAsync(user.Id, model.Role);

        //            if (model.Role == "Member")
        //            {
        //                return RedirectToAction("Index", "Product");
        //            }
        //            else if (model.Role == "User")
        //            {
        //                user.Closetime = model.Closetime;
        //                user.Opentime = model.Opentime;
        //                await UserManager.UpdateAsync(user);

        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                ViewBag.Message = "Tài khoản đang đợi Admin duyệt";
        //                return View("Message");
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay the form
        //    return View(model);
        //}


        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Product");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Product");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Product");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}