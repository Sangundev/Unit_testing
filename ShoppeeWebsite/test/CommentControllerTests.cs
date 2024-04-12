using NUnit.Framework;
using Food_Web.Controllers;
using System.Web.Mvc;
using Food_Web.Models;
using System.Threading.Tasks;
using Moq;
using System.Security.Principal;
using System;
using NSubstitute;
using System.Web;
using System.Linq;

namespace Food_Web.Tests.Controllers
{
    [TestFixture]
    public class CommentControllerTests
    {
        [Test]
        public async Task Create_WhenUserNotAuthenticated_RedirectsToLogin()
        {
            // Arrange
            var controller = new StoresController();
            var userMock = new Mock<IPrincipal>();
            userMock.SetupGet(p => p.Identity.IsAuthenticated).Returns(false);
            var contextMock = new Mock<ControllerContext>();
            contextMock.SetupGet(ctx => ctx.HttpContext.User).Returns(userMock.Object);
            controller.ControllerContext = contextMock.Object;

            // Act
            var result = await controller.Create(null, null, null) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Login", result.RouteValues["action"]);
            Assert.AreEqual("Account", result.RouteValues["controller"]);
        }



    }
}
