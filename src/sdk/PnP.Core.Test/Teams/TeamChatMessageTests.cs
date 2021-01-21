using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;
using PnP.Core.Test.Utilities;
using System;
using System.IO;
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
                // Depending on regional settings this check might fail
                //Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
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
                // Depending on regional settings this check might fail
                //Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
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
        public async Task AddChatMessageHtmlAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.PrimaryChannel);
                var channel = team.PrimaryChannel;
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Messages);
                var chatMessages = channel.Messages;

                Assert.IsNotNull(chatMessages);

                // assume as if there are no chat messages
                // There appears to be no remove option yet in this feature - so add a recognisable message
                var body = $"<h1>Hello</h1><br />This is a unit test (AddChatMessageHtmlAsyncTest) posting a message - <strong>PnP Rocks!</strong> - Woah...";
                if (!chatMessages.Any(o => o.Body.Content == body))
                {
                    //TODO: Fix that null
                    await chatMessages.AddAsync(body, ChatMessageContentType.Html, null);
                }

                channel =  await channel.GetAsync(o => o.Messages);
                var updateMessages = channel.Messages;

                var message = updateMessages.Last();
                Assert.IsNotNull(message.CreatedDateTime);
                // Depending on regional settings this check might fail
                //Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
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
        public async Task AddChatMessageFileAttachmentAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.PrimaryChannel);
                var channel = team.PrimaryChannel;
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Messages);
                var chatMessages = channel.Messages;

                Assert.IsNotNull(chatMessages);

                // Upload File to SharePoint Library - it will have to remain i guess as onetime upload.
                IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFile existingFile = await folder.Files.GetFirstOrDefaultAsync(o => o.Name == "test_added.docx");
                if(existingFile == default)
                {
                    existingFile = await folder.Files.AddAsync("test_added.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));
                }
                
                Assert.IsNotNull(existingFile);
                Assert.AreEqual("test_added.docx", existingFile.Name);
                
                // Useful reference - https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-4-file-attachments
                // assume as if there are no chat messages
                var attachmentId = existingFile.ETag.Replace("{","").Replace("}","").Replace("\"","").Split(',').First(); // Needs to be the documents eTag - just the GUID part
                var body = $"<h1>Hello</h1><br />This is a unit test with a file attachment (AddChatMessageHtmlAsyncTest) posting a message - <attachment id=\"{attachmentId}\"></attachment>";
                if (!chatMessages.Any(o => o.Body.Content == body))
                {

                    var fileUri = new Uri(existingFile.LinkingUrl);

                    ITeamChatMessageAttachmentCollection coll = new TeamChatMessageAttachmentCollection
                    {
                        new TeamChatMessageAttachment
                        {
                           Id = attachmentId,
                           ContentType = "reference",
                           // Cannot have the extension with a query graph doesnt recognise and think its part of file extension - include in docs.
                           ContentUrl = new Uri(fileUri.ToString().Replace(fileUri.Query, "")),
                           Name = $"{existingFile.Name}",
                           ThumbnailUrl = null,
                           Content = null
                        }
                    };

                    await chatMessages.AddAsync(body, ChatMessageContentType.Html, coll);
                }

                channel = await channel.GetAsync(o => o.Messages);
                var updateMessages = channel.Messages;

                var message = updateMessages.Last();
                Assert.IsNotNull(message.CreatedDateTime);
                // Depending on regional settings this check might fail
                //Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
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

                //delete file
                await existingFile.DeleteAsync(); //Note this will break the link in the Teams chat.

            }
        }


        //Disabled Test until subject support is implemented        
        //public async Task AddChatMessageSubjectAsyncTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var team = await context.Team.GetAsync(o => o.PrimaryChannel);
        //        var channel = team.PrimaryChannel;
        //        Assert.IsNotNull(channel);

        //        channel = await channel.GetAsync(o => o.Messages);
        //        var chatMessages = channel.Messages;

        //        Assert.IsNotNull(chatMessages);

        //        // assume as if there are no chat messages
        //        // There appears to be no remove option yet in this feature - so add a recognisable message
        //        var body = $"Hello, This is a unit test (AddChatMessageSubjectAsyncTest) posting a message - PnP Rocks! - Woah...";
        //        if (!chatMessages.Any(o => o.Body.Content == body))
        //        {
        //            //TODO: Fix that null
        //            //TODO: Add Subject to method

        //            await chatMessages.AddAsync(body, subject: "This is a subject test");
        //        }

        //        channel = await channel.GetAsync(o => o.Messages);
        //        var updateMessages = channel.Messages;

        //        var message = updateMessages.Last();
        //        Assert.IsNotNull(message.CreatedDateTime);
        //        // Depending on regional settings this check might fail
        //        //Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
        //        Assert.IsNotNull(message.Etag);
        //        Assert.IsNotNull(message.Importance);
        //        Assert.IsNotNull(message.LastModifiedDateTime);
        //        Assert.IsNotNull(message.Locale);
        //        Assert.IsNotNull(message.MessageType);
        //        Assert.IsNotNull(message.WebUrl);

        //        Assert.IsTrue(message.IsPropertyAvailable(o => o.ReplyToId));
        //        Assert.IsNull(message.ReplyToId);
        //        Assert.IsTrue(message.IsPropertyAvailable(o => o.Subject));
        //        Assert.IsNull(message.Subject);
        //        Assert.IsTrue(message.IsPropertyAvailable(o => o.Summary));
        //        Assert.IsNull(message.Summary);

        //    }
        //}

        [TestMethod]
        public async Task AddChatMessageAdaptiveAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.PrimaryChannel);
                var channel = team.PrimaryChannel;
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Messages);
                var chatMessages = channel.Messages;

                Assert.IsNotNull(chatMessages);

                // assume as if there are no chat messages
                // There appears to be no remove option yet in this feature - so add a recognisable message
                var attachmentId = "74d20c7f34aa4a7fb74e2b30004247c5";
                var body = $"<attachment id=\"{attachmentId}\"></attachment>";
                if (!chatMessages.Any(o => o.Body.Content == body))
                {
                    ITeamChatMessageAttachmentCollection coll = new TeamChatMessageAttachmentCollection
                    {
                        new TeamChatMessageAttachment
                        {
                           Id = attachmentId,
                           ContentType = "application/vnd.microsoft.card.thumbnail",
                           // Adaptive Card
                           Content = "{\r\n  \"title\": \"Unit Test posting a card\",\r\n  \"subtitle\": \"<h3>This is the subtitle</h3>\",\r\n  \"text\": \"Here is some body text. <br>\\r\\nAnd a <a href=\\\"http://microsoft.com/\\\">hyperlink</a>. <br>\\r\\nAnd below that is some buttons:\",\r\n  \"buttons\": [\r\n    {\r\n      \"type\": \"messageBack\",\r\n      \"title\": \"Login to FakeBot\",\r\n      \"text\": \"login\",\r\n      \"displayText\": \"login\",\r\n      \"value\": \"login\"\r\n    }\r\n  ]\r\n}",
                           ContentUrl = null,
                           Name = null,
                           ThumbnailUrl = null
                        }
                    };

                    await chatMessages.AddAsync(body, ChatMessageContentType.Html, coll);
                }

                channel = await channel.GetAsync(o => o.Messages);
                var updateMessages = channel.Messages;

                var message = updateMessages.Last();
                Assert.IsNotNull(message.CreatedDateTime);
                // Depending on regional settings this check might fail
                //Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
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
                // Depending on regional settings this check might fail
                //Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
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

                // Depending on regional settings this check might fail
                //Assert.AreEqual(message.DeletedDateTime, DateTime.MinValue);
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

        //TODO: There is no option to add reactions within a chat message in the SDK therefore cannot automate the testing for this at this time.
        //[TestMethod]
        //public void AddChatMessageReactionTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
        //    {
        //        var team = context.Team.Get(o => o.Channels);
        //        Assert.IsTrue(team.Channels.Length > 0);

        //        var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
        //        Assert.IsNotNull(channel);

        //        channel = channel.Get(o => o.Messages);
        //        var chatMessages = channel.Messages;

        //        Assert.IsNotNull(chatMessages);

        //        // assume as if there are no chat messages
        //        // There appears to be no remove option yet in this feature - so add a recognisable message
        //        var body = $"Hello, this is a unit test (AddChatMessageReactionTest) posting a message - PnP Rocks! - Woah...";
        //        var result = chatMessages.Add(body);

                
        //    }
        //}
    }
}
