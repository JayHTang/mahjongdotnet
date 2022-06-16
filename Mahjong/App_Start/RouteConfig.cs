using System.Web.Mvc;
using System.Web.Routing;

namespace Mahjong
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Account",
                url: "Account/{action}",
                defaults: new { controller = "Account" }
            );

            routes.MapRoute(
                name: "Admin",
                url: "Admin/{action}",
                defaults: new { controller = "Admin", action = "Index" }
            );

            routes.MapRoute(
                name: "Manage",
                url: "Manage/{action}",
                defaults: new { controller = "Manage" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{action}/{id}",
                defaults: new { controller = "Mahjong", action = "Stats", id = UrlParameter.Optional }
            );
        }
    }
}
