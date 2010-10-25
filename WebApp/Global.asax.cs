using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcAsyncChat
{
    public class WebApp : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(RouteName.Home, "", new { controller = "chat", action = "enter" });
            routes.MapRoute(RouteName.Enter, "enter", new { controller = "chat", action = "enter" });
            routes.MapRoute(RouteName.Room, "room", new { controller = "chat", action = "room" });
        }

        protected void Application_Start()
        {
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}