using System;

namespace MvcAsyncChat.Svcs
{
    public interface ITimerSvc
    {
        void AddPeriodicTimer(
            string id, 
            double interval, 
            Action callback);

        void RemovePeriodicTimer(string id);
    }
}