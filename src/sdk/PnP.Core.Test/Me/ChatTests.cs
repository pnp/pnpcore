using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Me;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Me
{
    [TestClass]
    public class ChatTest
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
             TestCommon.Instance.Mocking = false;
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
                var userId = "5e56f639-0e1a-4799-9673-b52d5039ea31";
                var chatMemberOptions = new List<ChatMemberOptions>
                {
                    new ChatMemberOptions()
                    {
                        Roles = new List<string> { "owner" },
                        UserId = userId,
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
                var member = chat.Members.AsRequested().FirstOrDefault(y => y.UserId == userId);
                Assert.IsNotNull(member);
            }
        }

        [TestMethod]
        public async Task AddGroupChatTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var topic = Guid.NewGuid().ToString();
                var chatMemberOptions = new List<ChatMemberOptions>
                {
                    new ChatMemberOptions()
                    {
                        Roles = new List<string> { "owner" },
                        UserId = "a857e888-b602-4790-86d9-3dca2109449e"
                    },
                    new ChatMemberOptions()
                    {
                        Roles = new List<string> { "owner" },
                        UserId = "8323f7fe-e8a4-46c4-b5ea-f4864887d160"
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
                Assert.AreEqual(chat.ChatType, ChatType.Group);
            }
        }
    }


}
