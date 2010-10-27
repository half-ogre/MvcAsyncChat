using System;
using System.Collections.Generic;

namespace MvcAsyncChat.Domain
{
    public interface IMessageRepo
    {
        DateTime Add(string message);
        IEnumerable<string> GetSince(DateTime since);
    }
}