using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using Xunit;
using MvcAsyncChat.Controllers;
using MvcAsyncChat.RequestModels;
using MvcAsyncChat.ResponseModels;
using MvcAsyncChat.Domain;

namespace MvcAsyncChat
{
    public class ChatControllerSpec
    {
        public class The_ShowEnterForm_method
        {
            [Fact]
            void will_show_the_enter_form()
            {
                var controller = CreateControllerWithMoqs();

                var viewResult = controller.ShowEnterForm() as ViewResult;

                Assert.Equal(string.Empty, viewResult.ViewName);
            }

            [Fact]
            void will_redirect_to_the_room_if_user_already_entered()
            {
                var moqIdentity = new Mock<IIdentity>();
                moqIdentity.Setup(x => x.IsAuthenticated).Returns(true);
                var controller = CreateControllerWithMoqs(moqIdentity: moqIdentity);

                var result = controller.ShowEnterForm() as RedirectToRouteResult;

                Assert.Equal(RouteName.Room, result.RouteName);
            }
        }

        public class The_EnterRoom_method
        {
            [Fact]
            void will_show_the_enter_form_with_errors_when_the_enter_attempt_state_is_invalid()
            {
                var controller = CreateControllerWithMoqs();
                var model = new EnterRequest() { Name = "aName" };
                controller.ModelState.AddModelError("aKey", "aMessage");

                var viewResult = controller.EnterRoom(model) as ViewResult;

                Assert.Empty(viewResult.ViewName);
                Assert.Same(model, viewResult.ViewData.Model);
            }

            [Fact]
            void will_authenticate_the_user_when_the_enter_attempt_is_valid()
            {
                var moqAuthSvc = new Mock<IAuthSvc>();
                var controller = CreateControllerWithMoqs(moqAuthSvc: moqAuthSvc);

                controller.EnterRoom(new EnterRequest() { Name = "theName" });

                moqAuthSvc.Verify(x => x.Authenticate("theName"));
            }

            [Fact]
            void will_redirect_to_the_room_after_a_successful_enter_attempt()
            {
                var moqAuthSvc = new Mock<IAuthSvc>();
                var controller = CreateControllerWithMoqs(moqAuthSvc: moqAuthSvc);

                var result = controller.EnterRoom(new EnterRequest() { Name = "theName" }) as RedirectToRouteResult;

                Assert.Equal(RouteName.Room, result.RouteName);
            }
        }

        public class The_ShowRoom_method
        {
            [Fact]
            void will_show_the_room()
            {
                var controller = CreateControllerWithMoqs();

                var result = controller.ShowRoom() as ViewResult;

                Assert.Equal(string.Empty, result.ViewName);
            }
        }

        public class The_LeaveRoom_method
        {
            [Fact]
            void will_unauthenticate_the_user()
            {
                var moqAuthSvc = new Mock<IAuthSvc>();
                var controller = CreateControllerWithMoqs(moqAuthSvc: moqAuthSvc);

                controller.LeaveRoom();

                moqAuthSvc.Verify(x => x.Unauthenticate());
            }

            [Fact]
            void will_redirect_to_the_enter_form()
            {
                var controller = CreateControllerWithMoqs();

                var result = controller.LeaveRoom() as RedirectToRouteResult;

                Assert.Equal(RouteName.Enter, result.RouteName);
            }
        }

        public class The_Say_method
        {
            [Fact]
            void will_return_an_error_message_if_say_request_is_invalid()
            {
                var controller = CreateControllerWithMoqs();
                controller.ModelState.AddModelError("theKey", "theError");

                var result = controller.Say(new SayRequest()) as JsonResult;

                Assert.Equal("The say request was invalid.", ((SayResponse)result.Data).error);
            }

            [Fact]
            void will_add_message_to_repo()
            {
                var moqMessageRepo = new Mock<IMessageRepo>();
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessageRepo);

                controller.Say(new SayRequest() { Text = "aMessage" });

                moqMessageRepo.Verify(x => x.Add("aMessage"));
            }

            [Fact]
            void will_send_a_response_with_no_error_when_say_request_is_valid()
            {
                var controller = CreateControllerWithMoqs();

                var result = controller.Say(new SayRequest() { Text = "aMessage" }) as JsonResult;

                Assert.Null(((SayResponse)result.Data).error);
            }
        }

        static ChatController CreateControllerWithMoqs(
            Mock<IIdentity> moqIdentity = null,
            Mock<IAuthSvc> moqAuthSvc = null,
            Mock<IMessageRepo> moqMessageRepo = null)
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
            moqMessageRepo = moqMessageRepo ?? new Mock<IMessageRepo>();

            var moqControllerContext = new Mock<ControllerContext>();
            moqControllerContext.Setup(x => x.HttpContext).Returns(moqHttpContext.Object);
            var controller = new ChatController(authSvc: moqAuthSvc.Object, messageRepo: moqMessageRepo.Object);

            controller.ControllerContext = moqControllerContext.Object;

            return controller;
        }
    }
}
