using System;
using System.Web.Security;

namespace MvcAsyncChat.Svcs
{
    public class FormsAuthSvc : IAuthSvc
    {
        public void Authenticate(string name)
        {
            FormsAuthentication.SetAuthCookie(name, false);
        }


        public void Unauthenticate()
        {
            FormsAuthentication.SignOut();
        }
    }
}