using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.Me;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Me
{
    [TestClass]
    public class ChatTest
    {
        // Replace these values with Graph user ids in your own environment
        private readonly string UserId1 = "225f4ed7-524b-4a6d-8e37-51802e87b3ad";
        private readonly string UserId2 = "7cffa5d0-513a-4f44-b40c-c3b0dba47e07";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetChatsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Me.Chats.LoadAsync();
                Assert.IsNotNull(context.Me.Chats.Length);
            }
        }

        [TestMethod]
        public async Task AddChatTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var chatMemberOptions = new List<ChatMemberOptions>
                {
                    new ChatMemberOptions()
                    {
                        Roles = new List<string> { "owner" },
                        UserId = UserId1,
                    }
                };
                var chat = context.Me.Chats.Add(new ChatOptions 
                {
                    ChatType = ChatType.OneOnOne,
                    Members = chatMemberOptions,
                });
                Assert.IsNotNull(chat);
                Assert.IsNotNull(chat.Id);
                chat.Load(y => y.Members);
                var member = chat.Members.AsRequested().FirstOrDefault(y => y.UserId == UserId1);
                Assert.IsNotNull(member);
            }
        }

        [TestMethod]
        public async Task AddChatIncludingOwnIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Me.LoadAsync(m => m.Id);
                var chatMemberOptions = new List<ChatMemberOptions>
                {
                    new ChatMemberOptions()
                    {
                        Roles = new List<string> { "owner" },
                        UserId = UserId1,
                    },
                    new ChatMemberOptions()
                    {
                        Roles = new List<string> { "owner" },
                        UserId = context.Me.Id.ToString(),
                    }
                };
                var chat = context.Me.Chats.Add(new ChatOptions
                {
                    ChatType = ChatType.OneOnOne,
                    Members = chatMemberOptions,
                });
                Assert.IsNotNull(chat);
                Assert.IsNotNull(chat.Id);
                await chat.LoadAsync(y => y.Members);
                var member = chat.Members.AsRequested().FirstOrDefault(y => y.UserId == UserId1);
                Assert.IsNotNull(member);
            }
        }

        [TestMethod]
        public async Task AddGroupChatTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var topic = "Test PnP Group Chat!";
                var chatMemberOptions = new List<ChatMemberOptions>
                {
                    new ChatMemberOptions()
                    {
                        Roles = new List<string> { "owner" },
                        UserId = UserId1
                    },
                    new ChatMemberOptions()
                    {
                        Roles = new List<string> { "owner" },
                        UserId = UserId2
                    }
                };
                var chat = context.Me.Chats.Add(new ChatOptions
                {
                    ChatType = ChatType.Group,
                    Members = chatMemberOptions,
                    Topic = topic
                });

                await chat.LoadAsync(y => y.Members);

                Assert.IsNotNull(chat);
                Assert.AreEqual(chat.Members.Length, 3);
                Assert.AreEqual(chat.Topic, topic);
                Assert.AreEqual(chat.ChatType, ChatTypeConstants.Group);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AddChatMeetingExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var chatMemberOptions = new List<ChatMemberOptions>
                {
                    new ChatMemberOptions()
                    {
                        Roles = new List<string> { "owner" },
                        UserId = UserId1
                    }
                };
                var chat = context.Me.Chats.Add(new ChatOptions
                {
                    ChatType = ChatType.Meeting,
                    Members = chatMemberOptions,
                });
            }
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddChatNoMembersExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var chat = context.Me.Chats.Add(new ChatOptions
                {
                    ChatType = ChatType.Meeting,
                    Members = null,
                });
            }
        }
    }


}
