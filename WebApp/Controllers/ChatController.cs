using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcAsyncChat.Controllers
{
    public class ChatController : Controller
    {
        [ActionName("enter"), HttpGet]
        public ActionResult ShowEnterForm()
        {
            return View();
        }
    }
}
