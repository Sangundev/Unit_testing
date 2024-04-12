using NUnit.Framework;
using Food_Web.Controllers;
using Food_Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;

namespace Food_Web.Tests
{
    [TestFixture]
    public class AccountControllerTests
    {
        [Test]
        public async Task Login_WithMissingCredentials_ReturnsErrorView()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(MockBehavior.Strict, new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var model = new LoginViewModel { EmailOrUsername = "", Password = "", RememberMe = false };

            // Act
            var result = await accountController.Login(model, returnUrl: null, fc: null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(accountController.ModelState.IsValid);
            Assert.IsTrue(accountController.ModelState.ContainsKey(string.Empty));
            Assert.AreEqual("Error", result.ViewName);
        }
        [Test]
        public async Task Login_WithInvalidModelState_ReturnsErrorView()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(MockBehavior.Strict, new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var model = new LoginViewModel { EmailOrUsername = "username", Password = "password", RememberMe = false };

            // Simulate invalid ModelState
            accountController.ModelState.AddModelError("Key", "Some error message");

            // Act
            var result = await accountController.Login(model, returnUrl: null, fc: null) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsFalse(accountController.ModelState.IsValid, "Model state should be invalid");
            Assert.IsTrue(accountController.ModelState.ContainsKey("Key"), "Model state should contain an error with the specified key");
            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Login_WithNonexistentUser_ReturnsErrorView()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(MockBehavior.Strict, new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var model = new LoginViewModel { EmailOrUsername = "nonexistentuser", Password = "password", RememberMe = false };

            // Simulate null user returned from UserManager
            userManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);
            userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await accountController.Login(model, returnUrl: null, fc: null) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsFalse(accountController.ModelState.IsValid, "Model state should be invalid");
            Assert.IsTrue(accountController.ModelState.ContainsKey(string.Empty), "Model state should contain an error with an empty key");
            Assert.AreEqual("Error", result.ViewName, "Returned view name should be 'Error'");
            Assert.AreEqual("Invalid login attempt.", accountController.ModelState[string.Empty].Errors[0].ErrorMessage, "Model state error message should match");
        }

        [Test]
        public async Task Login_WithSuccessfulAuthentication_Admin_ReturnsAdminIndex()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(MockBehavior.Strict, new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var user = new ApplicationUser { UserName = "adminuser", Email = "adminuser@example.com" }; // Tạo một người dùng Admin
            var model = new LoginViewModel { EmailOrUsername = "adminuser", Password = "adminpassword", RememberMe = false }; // Thiết lập model với thông tin đăng nhập

            // Simulate user found by UserManager
            userManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Simulate successful sign-in
            signInManagerMock.Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                             .ReturnsAsync(SignInStatus.Success);

            // Simulate Admin role
            userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<string>())).ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await accountController.Login(model, returnUrl: null, fc: null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("Products", result.RouteValues["controller"], "Controller name should be 'Products'");
            Assert.AreEqual("Admin", result.RouteValues["area"], "Area name should be 'Admin'");
            Assert.AreEqual("Index", result.RouteValues["action"], "Action name should be 'Index'");
        }
        [Test]
        public async Task Login_WithLockedOutAccount_ReturnsLockoutView()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(MockBehavior.Strict, new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var user = new ApplicationUser { UserName = "lockedoutuser", Email = "lockedoutuser@example.com" }; // Tạo một người dùng bị khóa
            var model = new LoginViewModel { EmailOrUsername = "lockedoutuser", Password = "lockedoutpassword", RememberMe = false }; // Thiết lập model với thông tin đăng nhập

            // Simulate user found by UserManager
            userManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Simulate locked out status
            signInManagerMock.Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                             .ReturnsAsync(SignInStatus.LockedOut);

            // Act
            var result = await accountController.Login(model, returnUrl: null, fc: null) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("Lockout", result.ViewName, "Returned view name should be 'Lockout'");
        }
        [Test]
        public async Task Login_WithRequiresVerification_ReturnsSendCodeAction()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(MockBehavior.Strict, new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var user = new ApplicationUser { UserName = "userwithverification", Email = "userwithverification@example.com" }; // Tạo một người dùng yêu cầu xác minh
            var model = new LoginViewModel { EmailOrUsername = "userwithverification", Password = "verificationpassword", RememberMe = false }; // Thiết lập model với thông tin đăng nhập

            // Simulate user found by UserManager
            userManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Simulate requires verification status
            signInManagerMock.Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                             .ReturnsAsync(SignInStatus.RequiresVerification);

            // Act
            var result = await accountController.Login(model, returnUrl: null, fc: null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("SendCode", result.RouteValues["action"], "Action name should be 'SendCode'");
        }
        [Test]
        public async Task Login_WithApprovedUser_ReturnsStoreIndex()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(MockBehavior.Strict, new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var user = new ApplicationUser { UserName = "approveduser", Email = "approveduser@example.com", IsApproved = true }; // Tạo một người dùng đã được phê duyệt
            var model = new LoginViewModel { EmailOrUsername = "approveduser", Password = "approvedpassword", RememberMe = false }; // Thiết lập model với thông tin đăng nhập

            // Simulate user found by UserManager
            userManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Simulate successful sign-in
            signInManagerMock.Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                             .ReturnsAsync(SignInStatus.Success);

            // Simulate User role
            userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<string>())).ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await accountController.Login(model, returnUrl: null, fc: null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("Productss", result.RouteValues["controller"], "Controller name should be 'Productss'");
            Assert.AreEqual("Store", result.RouteValues["area"], "Area name should be 'Store'");
            Assert.AreEqual("Index", result.RouteValues["action"], "Action name should be 'Index'");
        }
        [Test]
        public async Task Login_WithUnapprovedUser_ReturnsMessageView()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(MockBehavior.Strict, new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var user = new ApplicationUser { UserName = "unapproveduser", Email = "unapproveduser@example.com", IsApproved = false }; // Tạo một người dùng chưa được phê duyệt
            var model = new LoginViewModel { EmailOrUsername = "unapproveduser", Password = "unapprovedpassword", RememberMe = false }; // Thiết lập model với thông tin đăng nhập

            // Simulate user found by UserManager
            userManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Simulate successful sign-in
            signInManagerMock.Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                             .ReturnsAsync(SignInStatus.Success);

            // Simulate User role
            userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<string>())).ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await accountController.Login(model, returnUrl: null, fc: null) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("Message", result.ViewName, "Returned view name should be 'Message'");
            Assert.AreEqual("Tài khoản đang đợi Admin Duyệt hoặc bị block.", accountController.ViewBag.Message, "ViewBag message should match");
        }


    }
}
