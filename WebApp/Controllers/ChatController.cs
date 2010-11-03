using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using MvcAsyncChat.Domain;
using MvcAsyncChat.RequestModels;
using MvcAsyncChat.ResponseModels;
using MvcAsyncChat.Svcs;

namespace MvcAsyncChat.Controllers
{
    public class ChatController : AsyncController
    {
        readonly IAuthSvc authSvc;
        readonly IChatRoom chatRoom;
        readonly IDateTimeSvc dateTimeSvc;

        public ChatController(
            IAuthSvc authSvc,
            IChatRoom chatRoom,
            IDateTimeSvc dateTimeSvc)
        {
            this.authSvc = authSvc;
            this.chatRoom = chatRoom;
            this.dateTimeSvc = dateTimeSvc;
        }
        
        [ActionName("enter"), HttpGet]
        public ActionResult ShowEnterForm()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToRoute(RouteName.Room);
            
            return View();
        }

        [ActionName("enter"), HttpPost]
        public ActionResult EnterRoom(EnterRequest enterRequest)
        {
            if (!ModelState.IsValid)
                return View(enterRequest);

            authSvc.Authenticate(enterRequest.Name);
            chatRoom.AddParticipant(enterRequest.Name);

            return RedirectToRoute(RouteName.Room);
        }

        [ActionName("room"), HttpGet, Authorize]
        public ActionResult ShowRoom()
        {
            return View();
        }

        [ActionName("leave"), HttpGet, Authorize]
        public ActionResult LeaveRoom()
        {
            authSvc.Unauthenticate();
            chatRoom.RemoveParticipant(User.Identity.Name);

            return RedirectToRoute(RouteName.Enter);
        }

        [HttpPost, Authorize]
        public ActionResult Say(SayRequest sayRequest)
        {
            if (!ModelState.IsValid)
                return Json(new SayResponse() { error = "The say request was invalid." });

            chatRoom.AddMessage(sayRequest.Text);

            return Json(new SayResponse());
        }

        [ActionName("messages"), HttpPost, Authorize]
        public void GetMessagesAsync(GetMessagesRequest getMessagesRequest)
        {
            AsyncManager.OutstandingOperations.Increment();

            if (!ModelState.IsValid)
            {
                AsyncManager.Parameters["error"] = "The messages request was invalid.";
                AsyncManager.Parameters["since"] = null;
                AsyncManager.Parameters["messages"] = null;
                AsyncManager.OutstandingOperations.Decrement();
                return;
            }

            var since = dateTimeSvc.GetCurrentDateTimeAsUtc();
            if (!string.IsNullOrEmpty(getMessagesRequest.since))
                since = DateTime.Parse(getMessagesRequest.since).ToUniversalTime();

            chatRoom.GetMessages(since, (newMessages, timestamp) => 
            {
                AsyncManager.Parameters["error"] = null;
                AsyncManager.Parameters["since"] = timestamp;
                AsyncManager.Parameters["messages"] = newMessages;
                AsyncManager.OutstandingOperations.Decrement();
            });
        }

        public ActionResult GetMessagesCompleted(
            string error, 
            DateTime? since, 
            IEnumerable<string> messages)
        {
            if (!string.IsNullOrWhiteSpace(error))
                return Json(new GetMessagesResponse() { error = error });
            
            var data = new GetMessagesResponse();
            data.since = since.Value.ToString("o");
            data.messages = messages;

            return Json(data);
        }
    }
}
