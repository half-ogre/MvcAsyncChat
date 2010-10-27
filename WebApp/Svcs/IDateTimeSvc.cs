using System;

namespace MvcAsyncChat.Svcs
{
    public interface IDateTimeSvc
    {
        DateTime GetCurrentDateTimeAsUtc();
    }
}