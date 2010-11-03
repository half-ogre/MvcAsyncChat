using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using Xunit;
using MvcAsyncChat.Controllers;
using MvcAsyncChat.Domain;
using MvcAsyncChat.RequestModels;
using MvcAsyncChat.ResponseModels;
using MvcAsyncChat.Svcs;

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
            void will_redirect_to_the_room_if_user_is_authenticated()
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
            void will_show_the_enter_form_with_errors_when_the_request_is_invalid()
            {
                var controller = CreateControllerWithMoqs();
                var model = new EnterRequest() { Name = "aName" };
                controller.ModelState.AddModelError("aKey", "aMessage");

                var viewResult = controller.EnterRoom(model) as ViewResult;

                Assert.Empty(viewResult.ViewName);
                Assert.Same(model, viewResult.ViewData.Model);
            }

            [Fact]
            void will_authenticate_the_user_when_the_request_is_valid()
            {
                var moqAuthSvc = new Mock<IAuthSvc>();
                var controller = CreateControllerWithMoqs(moqAuthSvc: moqAuthSvc);

                controller.EnterRoom(new EnterRequest() { Name = "theName" });

                moqAuthSvc.Verify(x => x.Authenticate("theName"));
            }

            [Fact]
            void will_add_the_participant_to_the_chat_room()
            {
                var moqChatRoom = new Mock<IChatRoom>();
                var controller = CreateControllerWithMoqs(moqChatRoom: moqChatRoom);

                var result = controller.EnterRoom(new EnterRequest() { Name = "theName" }) as RedirectToRouteResult;

                moqChatRoom.Verify(x => x.AddParticipant("theName"));
            }

            [Fact]
            void will_redirect_to_the_room_after_adding_the_participant()
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
            void will_remove_the_participant_from_the_chat_room()
            {
                var moqIdentity = new Mock<IIdentity>();
                moqIdentity.Setup(x => x.Name).Returns("theName");
                var moqChatRoom = new Mock<IChatRoom>();
                var controller = CreateControllerWithMoqs(moqIdentity: moqIdentity, moqChatRoom: moqChatRoom);

                controller.LeaveRoom();

                moqChatRoom.Verify(x => x.RemoveParticipant("theName"));
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
            void will_return_an_error_message_if_request_is_invalid()
            {
                var controller = CreateControllerWithMoqs();
                controller.ModelState.AddModelError("theKey", "theError");

                var result = controller.Say(new SayRequest()) as JsonResult;

                Assert.Equal("The say request was invalid.", ((SayResponse)result.Data).error);
            }

            [Fact]
            void will_add_the_message_to_the_chat_room()
            {
                var moqChatRoom = new Mock<IChatRoom>();
                var controller = CreateControllerWithMoqs(moqChatRoom: moqChatRoom);

                controller.Say(new SayRequest() { Text = "aMessage" });

                moqChatRoom.Verify(x => x.AddMessage("aMessage"));
            }

            [Fact]
            void will_send_a_success_response()
            {
                var controller = CreateControllerWithMoqs();

                var result = controller.Say(new SayRequest() { Text = "aMessage" }) as JsonResult;

                Assert.Null(((SayResponse)result.Data).error);
            }
        }

        public class The_GetMessagesAsync_method
        {
            [Fact]
            void will_send_an_error_message_if_the_request_is_invalid()
            {
                var controller = CreateControllerWithMoqs();
                controller.ModelState.AddModelError("theKey", "theError");

                controller.GetMessagesAsync(new GetMessagesRequest());

                Assert.Equal(0, controller.AsyncManager.OutstandingOperations.Count);
                Assert.Equal("The messages request was invalid.", controller.AsyncManager.Parameters["error"]);
            }

            [Fact]
            void will_use_the_specified_since_timestamp()
            {
                var since = DateTime.UtcNow;
                var moqChatRoom = new Mock<IChatRoom>();
                var controller = CreateControllerWithMoqs(moqChatRoom: moqChatRoom);

                controller.GetMessagesAsync(new GetMessagesRequest() { since = since.ToString("o") });

                moqChatRoom.Verify(x => x.GetMessages(since, It.IsAny<Action<IEnumerable<string>, DateTime>>()));
            }

            [Fact]
            void will_use_the_current_time_as_a_new_since_timestamp_when_one_is_not_specified()
            {
                var now = DateTime.UtcNow;
                var moqDateTimeSvc = new Mock<IDateTimeSvc>();
                moqDateTimeSvc.Setup(x => x.GetCurrentDateTimeAsUtc()).Returns(now);
                var moqChatRoom = new Mock<IChatRoom>();
                var controller = CreateControllerWithMoqs(moqDateTimeSvc: moqDateTimeSvc, moqChatRoom: moqChatRoom);

                controller.GetMessagesAsync(new GetMessagesRequest());

                moqChatRoom.Verify(x => x.GetMessages(now, It.IsAny<Action<IEnumerable<string>, DateTime>>()));
            }

            [Fact]
            void will_request_new_messages_from_the_chat_room()
            {
                var moqChatRoom = new Mock<IChatRoom>();
                var controller = CreateControllerWithMoqs(moqChatRoom: moqChatRoom);

                controller.GetMessagesAsync(new GetMessagesRequest());

                Assert.Equal(1, controller.AsyncManager.OutstandingOperations.Count);
                moqChatRoom.Verify(x => x.GetMessages(It.IsAny<DateTime>(), It.IsAny<Action<IEnumerable<string>, DateTime>>()));
            } 
        }

        public class The_GetMessagesCompleted_method
        {
            [Fact]
            void will_send_an_error_if_one_has_occurred()
            {
                var controller = CreateControllerWithMoqs();

                var result = controller.GetMessagesCompleted("theError", null, null) as JsonResult;

                Assert.Equal("theError", ((GetMessagesResponse)result.Data).error);
            }

            [Fact]
            void will_send_the_new_since_timestamp()
            {
                var since = DateTime.UtcNow;
                var controller = CreateControllerWithMoqs();

                var result = controller.GetMessagesCompleted(null, since, null) as JsonResult;

                Assert.Equal(since.ToString("o"), ((GetMessagesResponse)result.Data).since);
            }

            [Fact]
            void will_send_the_new_messages()
            {
                var controller = CreateControllerWithMoqs();

                var result = controller.GetMessagesCompleted(null, DateTime.UtcNow, new [] { "theFirstMessage", "theSecondMessage" }) as JsonResult;

                Assert.Equal("theFirstMessage", ((GetMessagesResponse)result.Data).messages.First());
                Assert.Equal("theSecondMessage", ((GetMessagesResponse)result.Data).messages.Skip(1).First());
            }
        }

        public class The_get_messages_callback
        {
            [Fact]
            void will_send_new_messages_when_the_callback_is_called()
            {
                Action<IEnumerable<string>, DateTime> callback = null;
                var moqChatRoom = new Mock<IChatRoom>();
                moqChatRoom.Setup(x => x.GetMessages(It.IsAny<DateTime>(), It.IsAny<Action<IEnumerable<string>, DateTime>>()))
                    .Callback<DateTime, Action<IEnumerable<string>, DateTime>>((x, y) => { callback = y; });
                var controller = CreateControllerWithMoqs(moqChatRoom: moqChatRoom);
                controller.GetMessagesAsync(new GetMessagesRequest());

                callback(new[] { "theFirstMessage" }, DateTime.UtcNow);

                Assert.Equal(0, controller.AsyncManager.OutstandingOperations.Count);
                Assert.Equal("theFirstMessage", ((IEnumerable<string>)controller.AsyncManager.Parameters["messages"]).First());
            }

            [Fact]
            void will_send_the_new_since_timestamp_when_the_callback_is_called()
            {
                var now = DateTime.UtcNow;
                Action<IEnumerable<string>, DateTime> callback = null;
                var moqChatRoom = new Mock<IChatRoom>();
                moqChatRoom.Setup(x => x.GetMessages(It.IsAny<DateTime>(), It.IsAny<Action<IEnumerable<string>, DateTime>>()))
                    .Callback<DateTime, Action<IEnumerable<string>, DateTime>>((x, y) => { callback = y; });
                var controller = CreateControllerWithMoqs(moqChatRoom: moqChatRoom);
                controller.GetMessagesAsync(new GetMessagesRequest());

                callback(new[] { "theFirstMessage" }, now);

                Assert.Equal(0, controller.AsyncManager.OutstandingOperations.Count);
                Assert.Equal(now, controller.AsyncManager.Parameters["since"]);
            }
        }

        static ChatController CreateControllerWithMoqs(
            Mock<IIdentity> moqIdentity = null,
            Mock<IAuthSvc> moqAuthSvc = null,
            Mock<IChatRoom> moqChatRoom = null,
            Mock<IDateTimeSvc> moqDateTimeSvc = null)
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
            moqChatRoom = moqChatRoom ?? new Mock<IChatRoom>();

            if (moqDateTimeSvc == null)
            {
                moqDateTimeSvc = new Mock<IDateTimeSvc>();
                moqDateTimeSvc.Setup(x => x.GetCurrentDateTimeAsUtc()).Returns(DateTime.UtcNow);
            }

            var moqControllerContext = new Mock<ControllerContext>();
            moqControllerContext.Setup(x => x.HttpContext).Returns(moqHttpContext.Object);
            var controller = new ChatController(
                authSvc: moqAuthSvc.Object, 
                chatRoom: moqChatRoom.Object,
                dateTimeSvc: moqDateTimeSvc.Object);

            controller.ControllerContext = moqControllerContext.Object;

            return controller;
        }
    }
}
