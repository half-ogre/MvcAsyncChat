using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcAsyncChat.Domain
{
    public class InMemMessageRepo : IMessageRepo
    {
        public InMemMessageRepo()
        {
            Messages = new List<Tuple<string, DateTime>>();
        }
        
        public IList<Tuple<string, DateTime>> Messages { get; private set; }
        
        public DateTime Add(string message)
        {
            var timestamp = DateTime.UtcNow;

            Messages.Add(new Tuple<string, DateTime>(message, timestamp));
            
            return timestamp;
        }

        public IEnumerable<string> GetSince(DateTime since)
        {
            return Messages
                .Where(x => x.Item2 > since)
                .Select(x => x.Item1);
        }
    }
}