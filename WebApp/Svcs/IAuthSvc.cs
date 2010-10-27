using System;

namespace MvcAsyncChat.Svcs
{
    public interface IAuthSvc
    {
        void Authenticate(string name);
        void Unauthenticate();
    }
}