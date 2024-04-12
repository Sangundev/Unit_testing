using System.Web.Mvc;

namespace Food_Web.Areas.store
{
    public class storeAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "store";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "store_default",
                "store/{controller}/{action}/{id}",
                new { controller="Productss" ,action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}