using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Food_Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Product", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
            name: "StoreProducts",
            url: "Store/Products/{storeId}",
            defaults: new { controller = "Store", action = "Products", storeId = UrlParameter.Optional }
        );

            routes.MapRoute(
                name: "DeleteConfirmed",
                url: "Store/Discounts/DeleteConfirmed/{id}",
                defaults: new { controller = "Discounts", action = "DeleteConfirmed" }
            );

        }
    }
}
