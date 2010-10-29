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

        public static void StartTimers(ICallbackQueue callbackQueue, ITimerSvc timerSvc, IDateTimeSvc dateTimeSvc)
        {
            var period = Math.Floor(Const.IdleCallbackLimit / 2) * 1000;
            
            timerSvc.AddPeriodicTimer(Const.CallIdleCallbacksTimerId, period, () =>
            {
                var expiry = dateTimeSvc.GetCurrentDateTimeAsUtc().AddSeconds(Const.IdleCallbackLimit * -1);
                var newSince = dateTimeSvc.GetCurrentDateTimeAsUtc().AddSeconds(-1);

                foreach (var callback in callbackQueue.DequeueExpired(expiry))
                    callback(new string[] { }, newSince);
            });
        }

        public static void StopTimers(ITimerSvc timerSvc)
        {
            timerSvc.RemovePeriodicTimer(Const.CallIdleCallbacksTimerId);
        }

        protected void Application_Start()
        {
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            DependencyResolver.SetResolver(dependencyResolver);

            StartTimers(
                dependencyResolver.GetService<ICallbackQueue>(), 
                dependencyResolver.GetService<ITimerSvc>(),
                dependencyResolver.GetService<IDateTimeSvc>());
        }

        public override void Dispose()
        {
            base.Dispose();
            StopTimers(dependencyResolver.GetService<ITimerSvc>());
        }
    }
}