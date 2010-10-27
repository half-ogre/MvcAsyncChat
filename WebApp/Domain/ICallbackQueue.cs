using System;
using System.Collections.Generic;

namespace MvcAsyncChat.Domain
{
    public interface ICallbackQueue
    {
        void Enqueue(Action<IEnumerable<string>, DateTime> callback);
        IEnumerable<Action<IEnumerable<string>, DateTime>> DequeueAll();
        IEnumerable<Action<IEnumerable<string>, DateTime>> DequeueExpired(DateTime expiry);
    }
}