using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcAsyncChat.RequestModels;
using MvcAsyncChat.ResponseModels;
using MvcAsyncChat.Domain;

namespace MvcAsyncChat.Controllers
{
    public class ChatController : Controller
    {
        readonly IAuthSvc authSvc;
        readonly IMessageRepo messageRepo;

        public ChatController() 
            : this(null) { }
        
        public ChatController(
            IAuthSvc authSvc = null,
            IMessageRepo messageRepo = null)
        {
            this.authSvc = authSvc ?? new FormsAuthSvc();
            this.messageRepo = messageRepo ?? new InMemMessageRepo();
        }
        
        [ActionName("enter"), HttpGet]
        public ActionResult ShowEnterForm()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToRoute(RouteName.Room);
            
            return View();
        }

        [ActionName("enter"), HttpPost]
        public ActionResult EnterRoom(EnterRequest enterAttempt)
        {
            if (!ModelState.IsValid)
                return View(enterAttempt);

            authSvc.Authenticate(enterAttempt.Name);
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
        public ActionResult Say(SayRequest sayAttempt)
        {
            if (!ModelState.IsValid)
                return Json(new SayResponse() { error = "The say request was invalid." });

            messageRepo.Add(sayAttempt.Text);

            return Json(new SayResponse());
        }
    }
}
