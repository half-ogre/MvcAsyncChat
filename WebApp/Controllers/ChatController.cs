using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcAsyncChat.InputModels;

namespace MvcAsyncChat.Controllers
{
    public class ChatController : Controller
    {
        readonly IAuthSvc authSvc;

        public ChatController() 
            : this(null) { }
        
        public ChatController(IAuthSvc authSvc = null)
        {
            this.authSvc = authSvc ?? new FormsAuthSvc();
        }
        
        [ActionName("enter"), HttpGet]
        public ActionResult ShowEnterForm()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToRoute(RouteName.Room);
            
            return View();
        }

        [ActionName("enter"), HttpPost]
        public ActionResult EnterRoom(EnterAttempt enterAttempt)
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
    }
}
