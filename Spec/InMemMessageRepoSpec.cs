using System;
using Xunit;
using MvcAsyncChat.Domain;

namespace MvcAsyncChat
{
    public class InMemMessageRepoSpec
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
    }
}
