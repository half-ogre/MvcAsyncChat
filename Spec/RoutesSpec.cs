using System;
using System.Web;
using System.Web.Routing;
using Moq;
using Xunit;

namespace MvcAsyncChat
{
    public class RoutesSpec
    {
        [Fact]
        void The_enter_route_will_go_to_the_chat_controller_and_enter_action()
        {
            var routes = GetRoutes();
            var httpContext = CreateHttpContext("~/enter");

            var routeData = routes.GetRouteData(httpContext);

            Assert.Equal("chat", routeData.Values["controller"]);
            Assert.Equal("enter", routeData.Values["action"]);
        }

        [Fact]
        void The_home_route_will_go_to_the_chat_controller_and_enter_action()
        {
            var routes = GetRoutes();
            var httpContext = CreateHttpContext("~/");

            var routeData = routes.GetRouteData(httpContext);

            Assert.Equal("chat", routeData.Values["controller"]);
            Assert.Equal("enter", routeData.Values["action"]);
        }

        [Fact]
        void The_room_route_will_go_to_the_chat_controller_and_room_action()
        {
            var routes = GetRoutes();
            var httpContext = CreateHttpContext("~/room");

            var routeData = routes.GetRouteData(httpContext);

            Assert.Equal("chat", routeData.Values["controller"]);
            Assert.Equal("room", routeData.Values["action"]);
        }

        [Fact]
        void The_leave_route_will_go_to_the_chat_controller_and_leave_action()
        {
            var routes = GetRoutes();
            var httpContext = CreateHttpContext("~/leave");

            var routeData = routes.GetRouteData(httpContext);

            Assert.Equal("chat", routeData.Values["controller"]);
            Assert.Equal("leave", routeData.Values["action"]);
        }

        [Fact]
        void The_say_route_will_go_to_the_chat_controller_and_say_action()
        {
            var routes = GetRoutes();
            var httpContext = CreateHttpContext("~/say");

            var routeData = routes.GetRouteData(httpContext);

            Assert.Equal("chat", routeData.Values["controller"]);
            Assert.Equal("say", routeData.Values["action"]);
        }
        
        static HttpContextBase CreateHttpContext(string appRelativeCurrentExecutionFilePath)
        {
            var moqHttpContext = new Mock<HttpContextBase>();
            var moqHttpRequest = new Mock<HttpRequestBase>();

            moqHttpRequest.Setup(x => x.AppRelativeCurrentExecutionFilePath)
                .Returns(appRelativeCurrentExecutionFilePath);
            moqHttpRequest.Setup(x => x.ApplicationPath)
                .Returns("/");
            moqHttpContext.Setup(x => x.Request)
                .Returns(moqHttpRequest.Object);

            return moqHttpContext.Object;
        }

        static RouteCollection GetRoutes()
        {
            var routes = new RouteCollection();

            WebApp.RegisterRoutes(routes);

            return routes;
        }
    }
}