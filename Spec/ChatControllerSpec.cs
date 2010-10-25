using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using Xunit;
using MvcAsyncChat.Controllers;
using MvcAsyncChat.InputModels;

namespace MvcAsyncChat
{
    public class ChatControllerSpec
    {
        public class The_ShowEnterForm_method
        {
            [Fact]
            void will_show_the_enter_form()
            {
                var controller = CreateController();

                var viewResult = controller.ShowEnterForm() as ViewResult;

                Assert.Equal(string.Empty, viewResult.ViewName);
            }

            [Fact]
            void will_redirect_to_the_room_if_user_already_entered()
            {
                var moqIdentity = new Mock<IIdentity>();
                moqIdentity.Setup(x => x.IsAuthenticated).Returns(true);
                var controller = CreateController(moqIdentity: moqIdentity);

                var result = controller.ShowEnterForm() as RedirectToRouteResult;

                Assert.Equal(RouteName.Room, result.RouteName);
            }
        }

        public class The_EnterRoom_method
        {
            [Fact]
            void will_show_the_enter_form_with_errors_when_the_enter_attempt_state_is_invalid()
            {
                var controller = CreateController();
                var model = new EnterAttempt() { Name = "aName" };
                controller.ModelState.AddModelError("aKey", "aMessage");

                var viewResult = controller.EnterRoom(model) as ViewResult;

                Assert.Empty(viewResult.ViewName);
                Assert.Same(model, viewResult.ViewData.Model);
            }

            [Fact]
            void will_authenticate_the_user_when_the_enter_attempt_is_valid()
            {
                var moqAuthSvc = new Mock<IAuthSvc>();
                var controller = CreateController(moqAuthSvc: moqAuthSvc);

                controller.EnterRoom(new EnterAttempt() { Name = "theName" });

                moqAuthSvc.Verify(x => x.Authenticate("theName"));
            }

            [Fact]
            void will_redirect_to_the_room_after_a_successful_enter_attempt()
            {
                var moqAuthSvc = new Mock<IAuthSvc>();
                var controller = CreateController(moqAuthSvc: moqAuthSvc);

                var result = controller.EnterRoom(new EnterAttempt() { Name = "theName" }) as RedirectToRouteResult;

                Assert.Equal(RouteName.Room, result.RouteName);
            }
        }

        public class The_ShowRoom_method
        {
            [Fact]
            void will_show_the_room()
            {
                var controller = CreateController();

                var result = controller.ShowRoom() as ViewResult;

                Assert.Equal(string.Empty, result.ViewName);
            }
        }

        static ChatController CreateController(
            Mock<IIdentity> moqIdentity = null,
            Mock<IAuthSvc> moqAuthSvc = null)
        {
            if (moqIdentity == null)
            {
                moqIdentity = new Mock<IIdentity>();
                moqIdentity.Setup(x => x.IsAuthenticated).Returns(false);
            }

            var moqPrincipal = new Mock<IPrincipal>();
            moqPrincipal.Setup(x => x.Identity).Returns(moqIdentity.Object);
            var moqHttpContext = new Mock<HttpContextBase>();
            moqHttpContext.Setup(x => x.User).Returns(moqPrincipal.Object);
            
            moqAuthSvc = moqAuthSvc ?? new Mock<IAuthSvc>();

            var moqControllerContext = new Mock<ControllerContext>();
            moqControllerContext.Setup(x => x.HttpContext).Returns(moqHttpContext.Object);
            var controller = new ChatController(authSvc: moqAuthSvc.Object);

            controller.ControllerContext = moqControllerContext.Object;

            return controller;
        }
    }
}
