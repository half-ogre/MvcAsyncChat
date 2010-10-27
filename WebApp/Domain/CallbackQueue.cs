using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcAsyncChat.Domain
{
    public class CallbackQueue : ICallbackQueue
    {
        public CallbackQueue()
        {
            Callbacks = new Queue<Tuple<Action<IEnumerable<string>, DateTime>, DateTime>>();
        }

        public Queue<Tuple<Action<IEnumerable<string>, DateTime>, DateTime>> Callbacks { get; private set; }

        public void Enqueue(Action<IEnumerable<string>, DateTime> callback)
        {
            Callbacks.Enqueue(new Tuple<Action<IEnumerable<string>, DateTime>, DateTime>(callback, DateTime.UtcNow));
        }

        public IEnumerable<Action<IEnumerable<string>, DateTime>> DequeueAll()
        {
            while (Callbacks.Count > 0)
                yield return Callbacks.Dequeue().Item1;
        }

        public IEnumerable<Action<IEnumerable<string>, DateTime>> DequeueExpired(DateTime expiry)
        {
            if (Callbacks.Count == 0)
                yield break;
            
            var oldest = Callbacks.Peek();
            while (Callbacks.Count > 0 && oldest.Item2 <= expiry)
            {
                yield return Callbacks.Dequeue().Item1;
                
                if (Callbacks.Count > 0)
                    oldest = Callbacks.Peek();
            }
        }

        static CallbackQueue current;

        public static CallbackQueue Current
        {
            get
            {
                if (current == null)
                    current = new CallbackQueue();

                return current;
            }
        }
    }
}