using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using MvcAsyncChat.Domain;

namespace MvcAsyncChat
{
    public class CallbackQueueSpec
    {
        public class The_Enqueue_method
        {
            [Fact]
            void will_add_the_callback_to_the_queue()
            {
                var queue = new CallbackQueue();
                Action<IEnumerable<string>, DateTime> callback = (messages, since) => { }; 

                queue.Enqueue(callback);

                Assert.Same(callback, queue.Callbacks.First().Item1);
            }

            [Fact]
            void will_add_a_timestamp_to_the_callback()
            {
                var queue = new CallbackQueue();
                Action<IEnumerable<string>, DateTime> callback = (messages, since) => { };

                queue.Enqueue(callback);

                Assert.NotEqual(DateTime.MinValue, queue.Callbacks.First().Item2);
            }
        }

        public class The_DequeueAll_method
        {
            [Fact]
            void will_dequeue_all_callbacks()
            {
                var queue = new CallbackQueue();
                Action<IEnumerable<string>, DateTime> firstCallback = (messages, since) => { };
                Action<IEnumerable<string>, DateTime> secondCallback = (messages, since) => { };
                queue.Callbacks.Enqueue(new Tuple<Action<IEnumerable<string>, DateTime>, DateTime>(firstCallback, DateTime.UtcNow));
                queue.Callbacks.Enqueue(new Tuple<Action<IEnumerable<string>, DateTime>, DateTime>(secondCallback, DateTime.UtcNow));
                List<Action<IEnumerable<string>, DateTime>> callbacks = new List<Action<IEnumerable<string>, DateTime>>();

                foreach (var callback in queue.DequeueAll())
                    callbacks.Add(callback);

                Assert.Same(firstCallback, callbacks[0]);
                Assert.Same(secondCallback, callbacks[1]);
            }
        }

        public class The_DequeueExpired_method
        {
            [Fact]
            void will_dequeue_all_callbacks_older_than_the_expiry()
            {
                var queue = new CallbackQueue();
                Action<IEnumerable<string>, DateTime> firstCallback = (messages, since) => { };
                Action<IEnumerable<string>, DateTime> secondCallback = (messages, since) => { };
                Action<IEnumerable<string>, DateTime> thirdCallback = (messages, since) => { };
                var now = DateTime.UtcNow;
                queue.Callbacks.Enqueue(new Tuple<Action<IEnumerable<string>, DateTime>, DateTime>(firstCallback, now.AddHours(-1)));
                queue.Callbacks.Enqueue(new Tuple<Action<IEnumerable<string>, DateTime>, DateTime>(secondCallback, now.AddSeconds(-1)));
                queue.Callbacks.Enqueue(new Tuple<Action<IEnumerable<string>, DateTime>, DateTime>(thirdCallback, now.AddSeconds(1)));
                List<Action<IEnumerable<string>, DateTime>> callbacks = new List<Action<IEnumerable<string>, DateTime>>();

                foreach (var callback in queue.DequeueExpired(now))
                    callbacks.Add(callback);

                Assert.Equal(2, callbacks.Count);
                Assert.Same(firstCallback, callbacks[0]);
                Assert.Same(secondCallback, callbacks[1]);
            }

            [Fact]
            void will_dequeue_callbacks_exactly_the_same_as_the_expiry()
            {
                var queue = new CallbackQueue();
                Action<IEnumerable<string>, DateTime> firstCallback = (messages, since) => { };
                var now = DateTime.UtcNow;
                queue.Callbacks.Enqueue(new Tuple<Action<IEnumerable<string>, DateTime>, DateTime>(firstCallback, now));
                List<Action<IEnumerable<string>, DateTime>> callbacks = new List<Action<IEnumerable<string>, DateTime>>();

                foreach (var callback in queue.DequeueExpired(now))
                    callbacks.Add(callback);

                Assert.Equal(1, callbacks.Count);
            }

            [Fact]
            void will_not_dequeue_callbacks_more_recent_than_the_expiry()
            {
                var queue = new CallbackQueue();
                Action<IEnumerable<string>, DateTime> firstCallback = (messages, since) => { };
                var now = DateTime.UtcNow;
                queue.Callbacks.Enqueue(new Tuple<Action<IEnumerable<string>, DateTime>, DateTime>(firstCallback, now.AddHours(1)));
                List<Action<IEnumerable<string>, DateTime>> callbacks = new List<Action<IEnumerable<string>, DateTime>>();

                foreach (var callback in queue.DequeueExpired(now))
                    callbacks.Add(callback);

                Assert.Equal(0, callbacks.Count);
            }

            [Fact]
            void will_return_an_empty_result_when_there_are_no_queued_callbacks()
            {
                var queue = new CallbackQueue();
                List<Action<IEnumerable<string>, DateTime>> callbacks = new List<Action<IEnumerable<string>, DateTime>>();

                foreach (var callback in queue.DequeueExpired(DateTime.UtcNow))
                    callbacks.Add(callback);

                Assert.Equal(0, callbacks.Count);
            }
        }
    }
}
