using System;
using System.Linq;
using Xunit;
using MvcAsyncChat.Domain;

namespace MvcAsyncChat
{
    public class MessageRepoSpec
    {   
        public class The_Add_method
        {
            [Fact]
            void will_add_the_message_the_repo()
            {
                var repo = new InMemMessageRepo();

                repo.Add("aMessage");

                Assert.Equal("aMessage", repo.Messages[0].Item1);
            }

            [Fact]
            void will_add_a_timestamp_to_the_message()
            {
                var repo = new InMemMessageRepo();

                repo.Add("aMessage");

                Assert.NotEqual(DateTime.MinValue, repo.Messages[0].Item2);
            }

            [Fact]
            void will_add_the_new_message_to_the_end_of_the_list()
            {
                var repo = new InMemMessageRepo();
                repo.Messages.Add(new Tuple<string, DateTime>("aFirstMessage", DateTime.MinValue));
                repo.Messages.Add(new Tuple<string, DateTime>("aSecondMessage", DateTime.MinValue));

                repo.Add("aThirdMessage");

                Assert.Equal("aThirdMessage", repo.Messages[2].Item1);
            }
        }

        public class The_GetSince_method
        {
            [Fact]
            void will_not_get_messages_before_the_specified_since_date()
            {
                var since = DateTime.UtcNow;
                var repo = new InMemMessageRepo();
                repo.Messages.Add(new Tuple<string, DateTime>("theFirstMessageIsBeforeSince", since.AddDays(-1)));

                var messages = repo.GetSince(since);

                Assert.Equal(0, messages.Count());
            }

            [Fact]
            void will_not_get_messages_at_exactly_the_specified_since_date()
            {
                var since = DateTime.UtcNow;
                var repo = new InMemMessageRepo();
                repo.Messages.Add(new Tuple<string, DateTime>("theFirstMessageIsBeforeSince", since.AddDays(-1)));
                repo.Messages.Add(new Tuple<string, DateTime>("theSecondMessageIsExactlySince", since));

                var messages = repo.GetSince(since);

                Assert.Equal(0, messages.Count());
            }
            
            [Fact]
            void will_get_messages_after_the_specified_since_date()
            {
                var since = DateTime.UtcNow;
                var repo = new InMemMessageRepo();
                repo.Messages.Add(new Tuple<string, DateTime>("theFirstMessageIsBeforeSince", since.AddDays(-1)));
                repo.Messages.Add(new Tuple<string, DateTime>("theSecondMessageIsExactlySince", since));
                repo.Messages.Add(new Tuple<string, DateTime>("theThirdMessageIsAfterSince", since.AddHours(1)));
                repo.Messages.Add(new Tuple<string, DateTime>("theFourthMessageIsAfterSince", since.AddDays(1)));

                var messages = repo.GetSince(since);

                Assert.Equal(2, messages.Count());
                Assert.Equal("theThirdMessageIsAfterSince", messages.First());
                Assert.Equal("theFourthMessageIsAfterSince", messages.Skip(1).First());
            }
        }
    }
}
