using System;
using System.Web.Security;

namespace MvcAsyncChat
{
    public class FormsAuthSvc : IAuthSvc
    {
        public void Authenticate(string name)
        {
            FormsAuthentication.SetAuthCookie(name, false);
        }
    }
}