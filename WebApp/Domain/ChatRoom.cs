using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MvcAsyncChat.Svcs;

namespace MvcAsyncChat.Domain
{
    public class ChatRoom : IChatRoom
    {
        readonly ICallbackQueue callbackQueue;
        readonly IDateTimeSvc dateTimeSvc;
        readonly IMessageRepo messageRepo;
        
        public ChatRoom(
            ICallbackQueue callbackQueue,
            IDateTimeSvc dateTimeSvc,
            IMessageRepo messageRepo)
        {
            this.callbackQueue = callbackQueue;
            this.dateTimeSvc = dateTimeSvc;
            this.messageRepo = messageRepo;
        }
        
        public void AddMessage(string message)
        {
            var timestamp = messageRepo.Add(message);

            foreach (var callback in callbackQueue.DequeueAll())
                callback(new[] { message }, timestamp);
        }

        public void AddParticipant(string name)
        {
            AddMessage(string.Format("{0} has entered the room.", name));
        }

        public void GetMessages(
            DateTime since,
            Action<IEnumerable<string>, DateTime> callback)
        {
            var messages = messageRepo.GetSince(since);

            if (messages.Count() > 0)
                callback(messages, since);
            else
                callbackQueue.Enqueue(callback);
        }

        public void RemoveParticipant(string name)
        {
            AddMessage(string.Format("{0} left the room.", name));
        }
    }
}