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
        readonly IMessageRepo messageRepo;
        readonly ICallbackQueue callbackQueue;
        readonly ITimerSvc timerSvc;
        readonly IDateTimeSvc dateTimeSvc;

        public ChatController() : this(null, null, null, null) {}

        public ChatController(
            IAuthSvc authSvc = null,
            IMessageRepo messageRepo = null,
            ICallbackQueue callbackQueue = null,
            IDateTimeSvc dateTimeSvc = null)
        {
            this.authSvc = authSvc ?? new FormsAuthSvc();
            this.messageRepo = messageRepo ?? new InMemMessageRepo();
            this.callbackQueue = callbackQueue ?? CallbackQueue.Current;
            this.dateTimeSvc = dateTimeSvc ?? new DateTimeSvc();
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

            return RedirectToRoute(RouteName.Enter);
        }

        [HttpPost, Authorize]
        public ActionResult Say(SayRequest sayRequest)
        {
            if (!ModelState.IsValid)
                return Json(new SayResponse() { error = "The say request was invalid." });

            var timestamp = messageRepo.Add(sayRequest.Text);

            foreach(var callback in callbackQueue.DequeueAll())
                callback(new [] { sayRequest.Text }, timestamp);

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

            var messages = messageRepo.GetSince(since);

            if (messages.Count() > 0)
            {
                AsyncManager.Parameters["error"] = null;
                AsyncManager.Parameters["since"] = since;
                AsyncManager.Parameters["messages"] = messages;
                AsyncManager.OutstandingOperations.Decrement();
            }
            else
            {
                callbackQueue.Enqueue((newMessages, timestamp) => {
                    AsyncManager.Parameters["error"] = null;
                    AsyncManager.Parameters["since"] = timestamp;
                    AsyncManager.Parameters["messages"] = newMessages;
                    AsyncManager.OutstandingOperations.Decrement();
                });
            }
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
