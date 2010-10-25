using System;

namespace MvcAsyncChat
{
    public interface IAuthSvc
    {
        void Authenticate(string name);
        void Unauthenticate();
    }
}