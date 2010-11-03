using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using MvcAsyncChat.Domain;
using MvcAsyncChat.Svcs;

namespace MvcAsyncChat
{
    public class ChatRoomSpec
    {
        public class The_AddMessage_method
        {
            [Fact]
            void will_add_the_message_the_chat_room()
            {
                var moqMessageRepo = new Mock<IMessageRepo>();
                var chatRoom = CreateChatRoomWithMoqs(moqMessageRepo: moqMessageRepo);

                chatRoom.AddMessage("theMessage");

                moqMessageRepo.Verify(x => x.Add("theMessage"));
            }

            [Fact]
            void will_call_all_waiting_callbacks_with_the_new_message_and_its_timestamp()
            {
                var firstCallbackCalled = false;
                var secondCallbackCalled = false;
                var now = DateTime.UtcNow;
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.Add("theMessage"))
                    .Returns(now);
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.DequeueAll())
                    .Returns(new Action<IEnumerable<string>, DateTime>[] { 
                        (messages, timestamp) => { if (messages.First() == "theMessage" && timestamp == now) firstCallbackCalled = true; }, 
                        (messages, timestamp) => { if (messages.First() == "theMessage" && timestamp == now) secondCallbackCalled = true; } });
                var chatRoom = CreateChatRoomWithMoqs(moqMessageRepo: moqMessagesRepo, moqCallbackQueue: moqCallbackQueue);

                chatRoom.AddMessage("theMessage");

                Assert.True(firstCallbackCalled);
                Assert.True(secondCallbackCalled);
            }
        }
        
        public class The_AddParticipant_method
        {
            [Fact]
            void will_add_a_participant_entered_message_to_the_chat_room()
            {
                var moqMessageRepo = new Mock<IMessageRepo>();
                var chatRoom = CreateChatRoomWithMoqs(moqMessageRepo: moqMessageRepo);

                chatRoom.AddParticipant("theName");

                moqMessageRepo.Verify(x => x.Add("theName has entered the room."));
            }

            [Fact]
            void will_call_all_waiting_callbacks_with_the_entered_message_and_its_timestamp()
            {
                var firstCallbackCalled = false;
                var secondCallbackCalled = false;
                var now = DateTime.UtcNow;
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.Add(It.IsAny<string>()))
                    .Returns(now);
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.DequeueAll())
                    .Returns(new Action<IEnumerable<string>, DateTime>[] { 
                        (messages, timestamp) => { if (messages.First() == "theName has entered the room." && timestamp == now) firstCallbackCalled = true; }, 
                        (messages, timestamp) => { if (messages.First() == "theName has entered the room." && timestamp == now) secondCallbackCalled = true; } });
                var chatRoom = CreateChatRoomWithMoqs(moqMessageRepo: moqMessagesRepo, moqCallbackQueue: moqCallbackQueue);

                chatRoom.AddParticipant("theName");

                Assert.True(firstCallbackCalled);
                Assert.True(secondCallbackCalled);
            }
        }

        public class The_GetMessages_method
        {
            [Fact]
            void will_immediately_call_the_callback_with_the_messages_since_the_specified_timestamp_when_there_are_some()
            {
                var now = DateTime.UtcNow;
                DateTime callbackTimestamp = DateTime.MinValue;
                IEnumerable<string> callbackMessages = null;
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.GetSince(It.IsAny<DateTime>())).Returns(new string[] { "theFirstMessage", "theSecondMessage" });
                var chatRoom = CreateChatRoomWithMoqs(moqMessageRepo: moqMessagesRepo);

                chatRoom.GetMessages(now, (messages, timestamp) => 
                {
                    callbackMessages = messages;
                    callbackTimestamp = timestamp;
                });

                Assert.Equal("theFirstMessage", callbackMessages.First());
                Assert.Equal("theSecondMessage", callbackMessages.Skip(1).First());
                Assert.Equal(now, callbackTimestamp);
            }

            [Fact]
            void will_queue_the_callback_when_there_are_no_message_since_the_specified_timestamp()
            {
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.GetSince(It.IsAny<DateTime>())).Returns(new string[] { });
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                var chatRoom = CreateChatRoomWithMoqs(moqMessageRepo: moqMessagesRepo, moqCallbackQueue: moqCallbackQueue);
                Action<IEnumerable<string>, DateTime> callback = (messages, timestamp) => { };

                chatRoom.GetMessages(DateTime.UtcNow, callback);

                moqCallbackQueue.Verify(x => x.Enqueue(callback));
            }
        }

        public class The_RemoveParticipant_method
        {
            [Fact]
            void will_add_a_participant_left_message_to_the_chat_room()
            {
                var moqMessageRepo = new Mock<IMessageRepo>();
                var chatRoom = CreateChatRoomWithMoqs(moqMessageRepo: moqMessageRepo);

                chatRoom.RemoveParticipant("theName");

                moqMessageRepo.Verify(x => x.Add("theName left the room."));
            }

            [Fact]
            void will_call_all_waiting_callbacks_with_the_left_message_and_its_timestamp()
            {
                var firstCallbackCalled = false;
                var secondCallbackCalled = false;
                var now = DateTime.UtcNow;
                var moqMessagesRepo = new Mock<IMessageRepo>();
                moqMessagesRepo.Setup(x => x.Add(It.IsAny<string>()))
                    .Returns(now);
                var moqCallbackQueue = new Mock<ICallbackQueue>();
                moqCallbackQueue.Setup(x => x.DequeueAll())
                    .Returns(new Action<IEnumerable<string>, DateTime>[] { 
                        (messages, timestamp) => { if (messages.First() == "theName left the room." && timestamp == now) firstCallbackCalled = true; }, 
                        (messages, timestamp) => { if (messages.First() == "theName left the room." && timestamp == now) secondCallbackCalled = true; } });
                var chatRoom = CreateChatRoomWithMoqs(moqMessageRepo: moqMessagesRepo, moqCallbackQueue: moqCallbackQueue);

                chatRoom.RemoveParticipant("theName");

                Assert.True(firstCallbackCalled);
                Assert.True(secondCallbackCalled);
            }
        }

        static ChatRoom CreateChatRoomWithMoqs(
            Mock<ICallbackQueue> moqCallbackQueue = null, 
            Mock<IDateTimeSvc> moqDateTimeSvc = null, 
            Mock<IMessageRepo> moqMessageRepo = null)
        {
            moqCallbackQueue = moqCallbackQueue ?? new Mock<ICallbackQueue>();
            moqDateTimeSvc = moqDateTimeSvc ?? new Mock<IDateTimeSvc>();
            moqMessageRepo = moqMessageRepo ?? new Mock<IMessageRepo>();
            
            return new ChatRoom(moqCallbackQueue.Object, moqDateTimeSvc.Object, moqMessageRepo.Object);
        }
    }
}
