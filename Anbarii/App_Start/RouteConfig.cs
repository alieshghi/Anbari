using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Anbarii
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Index_2",
                url: "Portal/Index_2/{id}",
                defaults: new { controller = "Portal", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Index_3",
                url: "Portal/Index_3/{id}",
                defaults: new { controller = "Portal", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
