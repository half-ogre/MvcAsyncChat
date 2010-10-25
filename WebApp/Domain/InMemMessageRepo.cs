using System;
using System.Collections.Generic;

namespace MvcAsyncChat.Domain
{
    public class InMemMessageRepo : IMessageRepo
    {
        public InMemMessageRepo()
        {
            Messages = new List<Tuple<string, DateTime>>();
        }
        
        public IList<Tuple<string, DateTime>> Messages { get; private set; }
        
        public void Add(string message)
        {
            Messages.Add(new Tuple<string, DateTime>(message, DateTime.UtcNow));
        }
    }
}