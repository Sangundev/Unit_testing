using Food_Web.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace test
{
    [TestFixture]
    public class SearchControllerTests
    {
       
        [Test]
        public void Search_WithEmptySearchString_ReturnsErrorView()
        {
            // Arrange
            var controller = new ProductController();
            var searchString = "";

            // Act
            var result = controller.Search(searchString) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Error", result.ViewName);
        }
    }

}
