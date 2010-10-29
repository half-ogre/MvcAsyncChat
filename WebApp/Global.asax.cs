using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcAsyncChat.Svcs;
using MvcAsyncChat.Domain;

namespace MvcAsyncChat
{
    public class WebApp : HttpApplication
    {
        readonly IDependencyResolver dependencyResolver = new NinjectDependencyResolver();
        
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
            routes.MapRoute(RouteName.Leave, "leave", new { controller = "chat", action = "leave" });
            routes.MapRoute(RouteName.Say, "say", new { controller = "chat", action = "say" });
            routes.MapRoute(RouteName.Messages, "messages", new { controller = "chat", action = "messages" });
        }

        public static void StartTimers(ICallbackQueue callbackQueue, ITimerSvc timerSvc)
        {
            timerSvc.AddPeriodicTimer("CallExpiredCallbacks", 15 * 1000, () =>
            {
                var since = DateTime.UtcNow.AddSeconds(-1);

                foreach (var callback in callbackQueue.DequeueExpired(DateTime.UtcNow.AddSeconds(-30)))
                    callback(new string[] { }, since);
            });
        }

        public static void StopTimers(ITimerSvc timerSvc)
        {
            timerSvc.RemovePeriodicTimer("CallExpiredCallbacks");
        }

        protected void Application_Start()
        {
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            DependencyResolver.SetResolver(dependencyResolver);

            StartTimers(dependencyResolver.GetService<ICallbackQueue>(), dependencyResolver.GetService<ITimerSvc>());
        }

        public override void Dispose()
        {
            base.Dispose();
            StopTimers(dependencyResolver.GetService<ITimerSvc>());
        }
    }
}