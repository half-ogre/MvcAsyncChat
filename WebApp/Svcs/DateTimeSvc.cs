using System;

namespace MvcAsyncChat.Svcs
{
    public class DateTimeSvc : IDateTimeSvc
    {
        public DateTime GetCurrentDateTimeAsUtc()
        {
            return DateTime.UtcNow;
        }
    }
}