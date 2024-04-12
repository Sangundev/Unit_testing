using Food_Web.Models;
using Food_Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using NSubstitute;
using System;

namespace test
{
    [TestFixture]
    public class RegisterControllerTests
    {
        [Test]
        public async Task Register_WithValidModelStateAndIncorrectConfirmationCode()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var model = new RegisterViewModel();

            // Tạo một đối tượng giả mạo của HttpSessionStateBase
            var sessionMock = new Mock<HttpSessionStateBase>();
            sessionMock.Setup(s => s["ConfirmationCode"]).Returns("correctcode");

            // Act
            var result = await accountController.Register(model, "incorrectcode", sessionMock.Object) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(accountController.ModelState.ContainsKey("confirmationCode"), "ModelState should contain key 'confirmationCode'");
            Assert.AreEqual("Mã xác nhận không chính xác.", accountController.ModelState["confirmationCode"].Errors[0].ErrorMessage, "Error message should match");
            Assert.AreEqual("Register", result.ViewName, "Returned view name should be 'Register'");
        }
        [Test]
        public async Task Register_WithEmptyRole_ReturnsRegisterViewWithErrorMessage()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var model = new RegisterViewModel { Role = "" }; // Role is empty

            // Tạo một đối tượng giả mạo của HttpSessionStateBase
            var sessionMock = new Mock<HttpSessionStateBase>();
            sessionMock.Setup(s => s["ConfirmationCode"]).Returns("correctcode");

            // Act
            var result = await accountController.Register(model, confirmationCode: null, session: sessionMock.Object) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(accountController.ModelState.ContainsKey("Role"), "ModelState should contain key 'Role'");
            Assert.AreEqual("Vui lòng chọn vai trò.", accountController.ModelState["Role"].Errors[0].ErrorMessage, "Error message should match");
            Assert.AreEqual("Register", result.ViewName, "Returned view name should be 'Register'");
        }

        [Test]
        public async Task Register_WithEmptyUserName_ReturnsViewResultWithModelError()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<ApplicationUserManager>(userStoreMock.Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var controller = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var model = new RegisterViewModel
            {
                UserName = "", // Empty UserName
                Email = "test@example.com",
                Password = "password",
                Role = "User"
                // Populate other required fields if necessary
            };
            var httpSessionMock = new Mock<HttpSessionStateBase>();

            // Act
            var result = await controller.Register(model, null, httpSessionMock.Object) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.IsTrue(controller.ModelState.ContainsKey("UserName"));
            Assert.AreEqual("Vui lòng nhập tên người dùng.", controller.ModelState["UserName"].Errors[0].ErrorMessage);
        }

        [Test]
        public async Task Register_WithEmptyEmail_ReturnsViewResultWithModelError()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var model = new RegisterViewModel
            {
                UserName = "testuser",
                Email = "", // Empty Email
                Password = "password",
                Role = "User"
                // Populate other required fields if necessary
            };
            var httpSessionMock = new Mock<HttpSessionStateBase>();

            // Act
            var result = await accountController.Register(model, confirmationCode: null, session: httpSessionMock.Object) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(accountController.ModelState.ContainsKey("Email"), "ModelState should contain key 'Email'");
            Assert.AreEqual("Vui lòng nhập địa chỉ email.", accountController.ModelState["Email"].Errors[0].ErrorMessage, "Error message should match");
            Assert.AreEqual("Register", result.ViewName, "Returned view name should be 'Register'");
        }

        [Test]
        public async Task Register_WithEmptyPassword_ReturnsViewResultWithModelError()
        {
            // Arrange
            var userManagerMock = new Mock<ApplicationUserManager>(new Mock<IUserStore<ApplicationUser>>().Object);
            var signInManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, new Mock<IAuthenticationManager>().Object);
            var accountController = new AccountController(userManagerMock.Object, signInManagerMock.Object);
            var model = new RegisterViewModel
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "", // Empty Password
                Role = "User"
                // Populate other required fields if necessary
            };
            var httpSessionMock = new Mock<HttpSessionStateBase>();

            // Act
            var result = await accountController.Register(model, confirmationCode: null, session: httpSessionMock.Object) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(accountController.ModelState.ContainsKey("Password"), "ModelState should contain key 'Password'");
            Assert.AreEqual("Vui lòng nhập mật khẩu.", accountController.ModelState["Password"].Errors[0].ErrorMessage, "Error message should match");
            Assert.AreEqual("Register", result.ViewName, "Returned view name should be 'Register'");
        }


    }
}
