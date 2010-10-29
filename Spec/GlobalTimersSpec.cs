using System;
using Xunit;
using Moq;
using MvcAsyncChat.Svcs;
using MvcAsyncChat.Domain;
using System.Collections.Generic;

namespace MvcAsyncChat
{
    public class GlobalTimersSpec
    {
        public class The_StartTimers_method
        {
            [Fact]
            void will_start_a_periodic_timer_to_call_expired_callbacks()
            {
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                var moqTimerSvc = new Mock<ITimerSvc>();
                var moqDateTimeSvc = new Mock<IDateTimeSvc>();

                WebApp.StartTimers(moqCallbackQueue.Object, moqTimerSvc.Object, moqDateTimeSvc.Object);

                moqTimerSvc.Verify(x => x.AddPeriodicTimer(Const.CallIdleCallbacksTimerId, It.IsAny<double>(), It.IsAny<Action>()));
            }

            [Fact]
            void will_use_a_timer_period_equal_to_half_the_idle_callback_limit()
            {
                double period = 0;
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.DequeueExpired(It.IsAny<DateTime>()))
                    .Returns(new Action<IEnumerable<string>, DateTime>[] { (messages, since) => { } });
                var moqTimerSvc = new Mock<ITimerSvc>();
                moqTimerSvc.Setup(x => x.AddPeriodicTimer(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Action>()))
                    .Callback<string, double, Action>((x, y, z) => period = y);
                var moqDateTimeSvc = new Mock<IDateTimeSvc>();
                moqDateTimeSvc.Setup(x => x.GetCurrentDateTimeAsUtc())
                    .Returns(DateTime.UtcNow);

                WebApp.StartTimers(moqCallbackQueue.Object, moqTimerSvc.Object, moqDateTimeSvc.Object);

                Assert.Equal(Math.Floor(Const.IdleCallbackLimit / 2) * 1000, period);
            }

            [Fact]
            void will_use_the_configured_limit_for_idle_callback()
            {
                DateTime now = DateTime.UtcNow;
                DateTime expiry = DateTime.MinValue;
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.DequeueExpired(It.IsAny<DateTime>()))
                    .Returns(new Action<IEnumerable<string>, DateTime>[] { (messages, since) => { } })
                    .Callback<DateTime>(x => expiry = x);
                var moqTimerSvc = new Mock<ITimerSvc>();
                moqTimerSvc.Setup(x => x.AddPeriodicTimer(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Action>()))
                    .Callback<string, double, Action>((x, y, z) => z());
                var moqDateTimeSvc = new Mock<IDateTimeSvc>();
                moqDateTimeSvc.Setup(x => x.GetCurrentDateTimeAsUtc())
                    .Returns(now);

                WebApp.StartTimers(moqCallbackQueue.Object, moqTimerSvc.Object, moqDateTimeSvc.Object);

                Assert.Equal(now.AddSeconds(Const.IdleCallbackLimit * -1), expiry);
            }

            [Fact]
            void will_send_an_empty_message_list_to_expired_callbacks_when_the_timer_period_elapses()
            {
                IEnumerable<string> newMessages = null;
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.DequeueExpired(It.IsAny<DateTime>()))
                    .Returns(new Action<IEnumerable<string>, DateTime>[] { (messages, since) => { newMessages = messages; } });
                var moqTimerSvc = new Mock<ITimerSvc>();
                moqTimerSvc.Setup(x => x.AddPeriodicTimer(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Action>()))
                    .Callback<string, double, Action>((x, y, z) => z());
                var moqDateTimeSvc = new Mock<IDateTimeSvc>();
                moqDateTimeSvc.Setup(x => x.GetCurrentDateTimeAsUtc())
                    .Returns(DateTime.UtcNow);

                WebApp.StartTimers(moqCallbackQueue.Object, moqTimerSvc.Object, moqDateTimeSvc.Object);

                Assert.Empty(newMessages);
            }

            [Fact]
            void will_send_a_new_since_date_of_one_second_ago_to_expired_callbacks_when_the_timer_period_elapses()
            {
                DateTime now = DateTime.UtcNow;
                DateTime newSince = DateTime.MinValue;
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.DequeueExpired(It.IsAny<DateTime>()))
                    .Returns(new Action<IEnumerable<string>, DateTime>[] { (messages, since) => { newSince = since; } });
                var moqTimerSvc = new Mock<ITimerSvc>();
                moqTimerSvc.Setup(x => x.AddPeriodicTimer(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<Action>()))
                    .Callback<string, double, Action>((x, y, z) => z());
                var moqDateTimeSvc = new Mock<IDateTimeSvc>();
                moqDateTimeSvc.Setup(x => x.GetCurrentDateTimeAsUtc())
                    .Returns(now);

                WebApp.StartTimers(moqCallbackQueue.Object, moqTimerSvc.Object, moqDateTimeSvc.Object);

                Assert.Equal(now.AddSeconds(-1), newSince);
            }
        }

        public class The_StopTimers_method
        {
            [Fact]
            void will_stop_the_period_timer_that_calls_expired_callbacks()
            {
                var moqTimerSvc = new Mock<ITimerSvc>();

                WebApp.StopTimers(moqTimerSvc.Object);

                moqTimerSvc.Verify(x => x.RemovePeriodicTimer(Const.CallIdleCallbacksTimerId));
            }
        }
    }
}
