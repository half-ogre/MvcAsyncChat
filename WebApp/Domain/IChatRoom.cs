using System;
using System.Collections.Generic;

namespace MvcAsyncChat.Domain
{
    public interface IChatRoom
    {
        void AddMessage(string message);
        void AddParticipant(string name);
        void GetMessages(
            DateTime since, 
            Action<IEnumerable<string>, DateTime> callback);
        void RemoveParticipant(string name);
    }
}