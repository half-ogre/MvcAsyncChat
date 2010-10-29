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

            [Fact]
            void will_add_a_message_to_the_room_showing_the_participant_has_entered()
            {
                var moqMessageRepo = new Mock<IMessageRepo>();
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessageRepo);

                var result = controller.EnterRoom(new EnterRequest() { Name = "theName" }) as RedirectToRouteResult;

                moqMessageRepo.Verify(x => x.Add("theName has entered the room."));
            }

            [Fact]
            void will_call_any_queued_callbacks_with_the_entered_room_message()
            {
                var firstCallbackCalled = false;
                var secondCallbackCalled = false;
                var since = DateTime.UtcNow;
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.Add(It.IsAny<string>()))
                    .Returns(since);
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.DequeueAll())
                    .Returns(new Action<IEnumerable<string>, DateTime>[] { 
                        (messages, timestamp) => { if (messages.First() == "theName has entered the room." && timestamp == since) firstCallbackCalled = true; }, 
                        (messages, timestamp) => { if (messages.First() == "theName has entered the room." && timestamp == since) secondCallbackCalled = true; } });
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessagesRepo, moqCallbackQueue: moqCallbackQueue);

                controller.EnterRoom(new EnterRequest() { Name = "theName" });

                Assert.True(firstCallbackCalled);
                Assert.True(secondCallbackCalled);
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

            [Fact]
            void will_call_any_queued_callbacks_with_the_new_message_and_since_date()
            {
                var firstCallbackCalled = false;
                var secondCallbackCalled = false;
                var since = DateTime.UtcNow;
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.Add(It.IsAny<string>()))
                    .Returns(since);
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.DequeueAll())
                    .Returns(new Action<IEnumerable<string>, DateTime>[] { 
                        (messages, timestamp) => { if (messages.First() == "aMessage" && timestamp == since) firstCallbackCalled = true; }, 
                        (messages, timestamp) => { if (messages.First() == "aMessage" && timestamp == since) secondCallbackCalled = true; } });
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessagesRepo, moqCallbackQueue: moqCallbackQueue);

                var result = controller.Say(new SayRequest() { Text = "aMessage" }) as JsonResult;

                Assert.True(firstCallbackCalled);
                Assert.True(secondCallbackCalled);
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
            void will_use_the_specified_sicne_date()
            {
                var since = DateTime.UtcNow;
                var moqMessagesRepo = new Mock<IMessageRepo>();
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessagesRepo);

                controller.GetMessagesAsync(new GetMessagesRequest() { since = since.ToString("o") });

                moqMessagesRepo.Verify(x => x.GetSince(since));
            }

            [Fact]
            void will_use_the_current_time_as_a_new_since_date_when_one_is_not_specified()
            {
                var newSince = DateTime.UtcNow;
                var moqDateTimeSvc = new Mock<IDateTimeSvc>();
                moqDateTimeSvc.Setup(x => x.GetCurrentDateTimeAsUtc()).Returns(newSince);
                var moqMessagesRepo = new Mock<IMessageRepo>();
                var controller = CreateControllerWithMoqs(moqDateTimeSvc: moqDateTimeSvc, moqMessageRepo: moqMessagesRepo);

                controller.GetMessagesAsync(new GetMessagesRequest());

                moqMessagesRepo.Verify(x => x.GetSince(newSince));
            }

            [Fact]
            void will_send_any_new_messages()
            {
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.GetSince(It.IsAny<DateTime>())).Returns(new string[] { "theFirstMessage", "theSecondMessage" } );
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessagesRepo);

                controller.GetMessagesAsync(new GetMessagesRequest());

                Assert.Equal(0, controller.AsyncManager.OutstandingOperations.Count);
                Assert.Equal("theFirstMessage", ((IEnumerable<string>)controller.AsyncManager.Parameters["messages"]).First());
                Assert.Equal("theSecondMessage", ((IEnumerable<string>)controller.AsyncManager.Parameters["messages"]).Skip(1).First());
            }

            [Fact]
            void will_send_the_new_since_date()
            {
                var newSince = DateTime.UtcNow;
                var moqDateTimeSvc = new Mock<IDateTimeSvc>();
                moqDateTimeSvc.Setup(x => x.GetCurrentDateTimeAsUtc()).Returns(newSince);
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.GetSince(It.IsAny<DateTime>())).Returns(new string[] { "theFirstMessage", "theSecondMessage" });
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessagesRepo, moqDateTimeSvc: moqDateTimeSvc);

                controller.GetMessagesAsync(new GetMessagesRequest());

                Assert.Equal(0, controller.AsyncManager.OutstandingOperations.Count);
                Assert.Equal(newSince, controller.AsyncManager.Parameters["since"]);
            }

            [Fact]
            void will_enqueue_a_callback_to_wait_for_new_messages_when_there_are_none_currently()
            {
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.GetSince(It.IsAny<DateTime>())).Returns(new string[] { });
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessagesRepo, moqCallbackQueue: moqCallbackQueue);

                controller.GetMessagesAsync(new GetMessagesRequest());

                Assert.Equal(1, controller.AsyncManager.OutstandingOperations.Count);
                moqCallbackQueue.Verify(x => x.Enqueue(It.IsAny<Action<IEnumerable<string>, DateTime>>()));
            }

            [Fact]
            void will_send_new_messages_when_the_callback_is_called()
            {
                Action<IEnumerable<string>, DateTime> callback = null;
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.GetSince(It.IsAny<DateTime>())).Returns(new string[] { });
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.Enqueue(It.IsAny<Action<IEnumerable<string>, DateTime>>())).Callback<Action<IEnumerable<string>, DateTime>>(x => callback = x);
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessagesRepo, moqCallbackQueue: moqCallbackQueue);
                controller.GetMessagesAsync(new GetMessagesRequest());

                callback(new[] { "theFirstMessage" }, DateTime.UtcNow);

                Assert.Equal(0, controller.AsyncManager.OutstandingOperations.Count);
                Assert.Equal("theFirstMessage", ((IEnumerable<string>)controller.AsyncManager.Parameters["messages"]).First());
            }

            [Fact]
            void will_send_the_new_since_date_when_the_callback_is_called()
            {
                var newSince = DateTime.UtcNow;
                Action<IEnumerable<string>, DateTime> callback = null;
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.GetSince(It.IsAny<DateTime>())).Returns(new string[] { });
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.Enqueue(It.IsAny<Action<IEnumerable<string>, DateTime>>())).Callback<Action<IEnumerable<string>, DateTime>>(x => callback = x);
                var controller = CreateControllerWithMoqs(moqMessageRepo: moqMessagesRepo, moqCallbackQueue: moqCallbackQueue);
                controller.GetMessagesAsync(new GetMessagesRequest());

                callback(new[] { "theFirstMessage" }, newSince);

                Assert.Equal(0, controller.AsyncManager.OutstandingOperations.Count);
                Assert.Equal(newSince, controller.AsyncManager.Parameters["since"]);
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
            void will_send_the_new_since_date()
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

        static ChatController CreateControllerWithMoqs(
            Mock<IIdentity> moqIdentity = null,
            Mock<IAuthSvc> moqAuthSvc = null,
            Mock<IMessageRepo> moqMessageRepo = null,
            Mock<ICallbackQueue> moqCallbackQueue = null,
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
            moqMessageRepo = moqMessageRepo ?? new Mock<IMessageRepo>();
            moqCallbackQueue = moqCallbackQueue ?? new Mock<ICallbackQueue>();

            if (moqDateTimeSvc == null)
            {
                moqDateTimeSvc = new Mock<IDateTimeSvc>();
                moqDateTimeSvc.Setup(x => x.GetCurrentDateTimeAsUtc()).Returns(DateTime.UtcNow);
            }

            var moqControllerContext = new Mock<ControllerContext>();
            moqControllerContext.Setup(x => x.HttpContext).Returns(moqHttpContext.Object);
            var controller = new ChatController(
                authSvc: moqAuthSvc.Object, 
                messageRepo: moqMessageRepo.Object, 
                callbackQueue: moqCallbackQueue.Object,
                dateTimeSvc: moqDateTimeSvc.Object);

            controller.ControllerContext = moqControllerContext.Object;

            return controller;
        }
    }
}
