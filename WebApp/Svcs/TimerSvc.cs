using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace MvcAsyncChat.Svcs
{
    public class TimerSvc : ITimerSvc
    {
        readonly IDictionary<string, Timer> timers = new Dictionary<string, Timer>();
        
        public void AddPeriodicTimer(
            string id, 
            double interval, 
            Action callback)
        {
            var timer = new Timer(interval);
            timer.Elapsed += (sender, e) =>
            {
                callback();
            };
            timers.Add(id, timer);
            timer.Start();
        }

        public void RemovePeriodicTimer(string id)
        {
            Timer timer;
            if (timers.TryGetValue(id, out timer))
            {
                timer.Stop();
                timers.Remove(id);
            }
        }

        static TimerSvc current;

        public static TimerSvc Current
        {
            get
            {
                if (current == null)
                    current = new TimerSvc();

                return current;
            }
        }
    }
}