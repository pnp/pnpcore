using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Teams;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Teams
{
    [TestClass]
    public class TeamChatMessageTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }
                
        [TestMethod]
        public async Task GetChatMessageAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Messages);
                var chatMessages = channel.Messages;

                Assert.IsNotNull(chatMessages);

                // assume as if there are no chat messages
                // There appears to be no remove option yet in this feature - so add a recognisable message
                var body = "Hello, this is a unit test (GetChatMessageAsyncTest) posting a message - PnP Rocks!";
                if(!chatMessages.Any(o=> o.Body.Content == body))
                {
                    await chatMessages.AddAsync(body);
                }

                channel = await channel.GetAsync(o => o.Messages);
                var updateMessages = channel.Messages;

                var message = updateMessages.First(o => o.Body.Content == body);
                Assert.IsNotNull(message.CreatedDateTime);
                Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
                Assert.IsNotNull(message.Etag);
                Assert.IsNotNull(message.Importance);
                Assert.IsNotNull(message.LastModifiedDateTime);
                Assert.IsNotNull(message.Locale);
                Assert.IsNotNull(message.MessageType);
                Assert.IsNotNull(message.WebUrl);

                Assert.IsTrue(message.IsPropertyAvailable(o => o.ReplyToId));
                Assert.IsNull(message.ReplyToId);
                Assert.IsTrue(message.IsPropertyAvailable(o => o.Subject));
                Assert.IsNull(message.Subject);
                Assert.IsTrue(message.IsPropertyAvailable(o => o.Summary));
                Assert.IsNull(message.Summary);
                
            }
        }


        [TestMethod]
        public void AddChatMessageTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Messages);
                var chatMessages = channel.Messages;

                Assert.IsNotNull(chatMessages);

                // assume as if there are no chat messages
                // There appears to be no remove option yet in this feature - so add a recognisable message
                var body = $"Hello, this is a unit test (AddChatMessageTest) posting a message - PnP Rocks! - Woah...";
                if (!chatMessages.Any(o => o.Body.Content == body))
                {
                    chatMessages.Add(body);
                }

                channel = channel.Get(o => o.Messages);
                var updateMessages = channel.Messages;

                var message = updateMessages.First(o => o.Body.Content == body);
                Assert.IsNotNull(message.CreatedDateTime);
                Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
                Assert.IsNotNull(message.Etag);
                Assert.IsNotNull(message.Importance);
                Assert.IsNotNull(message.LastModifiedDateTime);
                Assert.IsNotNull(message.Locale);
                Assert.IsNotNull(message.MessageType);
                Assert.IsNotNull(message.WebUrl);

                Assert.IsTrue(message.IsPropertyAvailable(o => o.ReplyToId));
                Assert.IsNull(message.ReplyToId);
                Assert.IsTrue(message.IsPropertyAvailable(o => o.Subject));
                Assert.IsNull(message.Subject);
                Assert.IsTrue(message.IsPropertyAvailable(o => o.Summary));
                Assert.IsNull(message.Summary);

            }
        }

        [TestMethod]
        public void AddChatMessageBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.GetBatch(o => o.Channels);
                context.Execute();
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.GetBatch(o => o.Messages);
                context.Execute();
                var chatMessages = channel.Messages;

                Assert.IsNotNull(chatMessages);
                                
                // assume as if there are no chat messages
                // There appears to be no remove option yet in this feature - so add a recognisable message
                var body = $"Hello, this is a unit test (AddChatMessageBatchTest) posting a message - PnP Rocks! - Woah...";
                if (!chatMessages.Any(o => o.Body.Content == body))
                {
                    chatMessages.AddBatch(body);
                    context.Execute();
                }

                channel = channel.GetBatch(o => o.Messages);
                context.Execute();
                var updateMessages = channel.Messages;

                var message = updateMessages.FirstOrDefault(o => o.Body.Content == body);

                Assert.IsFalse(message == default);
                Assert.IsNotNull(message.CreatedDateTime);
                Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
                Assert.IsNotNull(message.Etag);
                Assert.IsNotNull(message.Importance);
                Assert.IsNotNull(message.LastModifiedDateTime);
                Assert.IsNotNull(message.Locale);
                Assert.IsNotNull(message.MessageType);
                Assert.IsNotNull(message.WebUrl);

                Assert.IsTrue(message.IsPropertyAvailable(o => o.ReplyToId));
                Assert.IsNull(message.ReplyToId);
                Assert.IsTrue(message.IsPropertyAvailable(o => o.Subject));
                Assert.IsNull(message.Subject);
                Assert.IsTrue(message.IsPropertyAvailable(o => o.Summary));
                Assert.IsNull(message.Summary);

            }
        }

        [TestMethod]
        public void AddChatMessageSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.NewBatch();
                var team = context.Team.GetBatch(batch, o => o.Channels);
                context.Execute(batch);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.GetBatch(batch, o => o.Messages);
                context.Execute(batch);
                var chatMessages = channel.Messages;

                Assert.IsNotNull(chatMessages);

                // assume as if there are no chat messages
                // There appears to be no remove option yet in this feature - so add a recognisable message
                var body = $"Hello, this is a unit test (AddChatMessageSpecificBatchTest) posting a message - PnP Rocks! - Woah...";
                if (!chatMessages.Any(o => o.Body.Content == body))
                {
                    chatMessages.AddBatch(batch, body);
                    context.Execute(batch);
                }

                var batch2 = context.NewBatch();
                channel = channel.GetBatch(batch2, o => o.Messages);
                context.Execute(batch2);
                var updateMessages = channel.Messages;

                var message = updateMessages.First(o => o.Body.Content == body);
                Assert.IsNotNull(message.CreatedDateTime);
                Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
                Assert.IsNotNull(message.Etag);
                Assert.IsNotNull(message.Importance);
                Assert.IsNotNull(message.LastModifiedDateTime);
                Assert.IsNotNull(message.Locale);
                Assert.IsNotNull(message.MessageType);
                Assert.IsNotNull(message.WebUrl);

                Assert.IsTrue(message.IsPropertyAvailable(o => o.ReplyToId));
                Assert.IsNull(message.ReplyToId);
                Assert.IsTrue(message.IsPropertyAvailable(o => o.Subject));
                Assert.IsNull(message.Subject);
                Assert.IsTrue(message.IsPropertyAvailable(o => o.Summary));
                Assert.IsNull(message.Summary);

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddChatMessageBatchExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.NewBatch();
                var team = context.Team.GetBatch(batch, o => o.Channels);
                context.Execute(batch);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.GetBatch(batch, o => o.Messages);
                context.Execute(batch);
                var chatMessages = channel.Messages;

                Assert.IsNotNull(chatMessages);

                // assume as if there are no chat messages
                // There appears to be no remove option yet in this feature - so add a recognisable message
                var body = string.Empty;
                chatMessages.AddBatch(batch, body);
                context.Execute(batch);
                
            }
        }

        //TODO Exception tests...
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddChatMessageExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Messages);
                var chatMessages = channel.Messages;

                Assert.IsNotNull(chatMessages);

                // assume as if there are no chat messages
                // There appears to be no remove option yet in this feature - so add a recognisable message
                var body = string.Empty;
                chatMessages.Add(body);
                
            }
        }
    }
}
