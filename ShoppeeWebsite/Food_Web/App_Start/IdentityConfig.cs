using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Food_Web.Models;
using System.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Diagnostics;
using System.Net;

namespace Food_Web
{
    public class EmailService : IIdentityMessageService
    {
        //public Task SendAsync(IdentityMessage message)
        //{
        // Plug in your email service here to send an email.
        //return Task.FromResult(0);
        //return Task.Factory.StartNew(() => sendMail(message));

        //}
        public async Task SendAsync(IdentityMessage message)
        {
            await configSendGridasync(message);
        }

        private async Task configSendGridasync(IdentityMessage message)
        {
            var myMessage = new SendGridMessage();
            myMessage.AddTo(message.Destination);
            myMessage.From = new EmailAddress(
                                "nguyenngocsang1682@gmail.com", "(f!VE.x6zim3Dws)");
            myMessage.Subject = message.Subject;
            myMessage.PlainTextContent = message.Body;
            myMessage.HtmlContent = message.Body;

            var apiKey = ConfigurationManager.AppSettings["sendGridApiKey"];

            // Create a SendGrid client with the API key.
            var client = new SendGridClient(apiKey);

            try
            {
                // Send the email using SendGrid client.
                var response = await client.SendEmailAsync(myMessage);

                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.Accepted)
                {
                    // Handle unsuccessful email sending.
                    Trace.TraceError($"Failed to send email. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Handle exception if any error occurs during email sending.
                Trace.TraceError($"Error sending email: {ex.Message}");
            }
        }

        //void sendMail(IdentityMessage message)
        //{
        //    #region formatter
        //    string text = string.Format("Please click on this link to {0}: {1}", message.Subject, message.Body);
        //    string html = "Please confirm your account by clicking this link: <a href=\"" + message.Body + "\">link</a><br/>";

        //    html += HttpUtility.HtmlEncode(@"Or click on the copy the following link on the browser:" + message.Body);
        //    #endregion

        //    MailMessage msg = new MailMessage();
        //    msg.From = new MailAddress(ConfigurationManager.AppSettings["Email"].ToString());
        //    msg.From = new MailAddress("nxxn736@gmail.com");
        //    msg.To.Add(new MailAddress(message.Destination));
        //    msg.Subject = message.Subject;
        //    msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
        //    msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

        //    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32(465));
        //    System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["Email"].ToString(), ConfigurationManager.AppSettings["Password"].ToString());
        //    //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("nxxn736@gmail.com", "nxxnnguyen736");
        //    smtpClient.Credentials = credentials;
        //    smtpClient.EnableSsl = true;
        //    smtpClient.Send(msg);
        //}



    }



    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
