using System;

namespace MvcAsyncChat.Domain
{
    public interface IMessageRepo
    {
        void Add(string message);
    }
}