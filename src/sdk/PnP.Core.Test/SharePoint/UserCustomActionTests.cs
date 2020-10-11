using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class UserCustomActionTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetWebUserCustomActionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string customActionName, Guid customActionId) = await TestAssets.CreateTestUserCustomActionAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IWeb web = await context.Web.GetAsync(p => p.UserCustomActions);

                Assert.IsNotNull(web.UserCustomActions);
                Assert.IsTrue(web.UserCustomActions.Count() > 0);

                IUserCustomAction foundCustomAction = web.UserCustomActions.FirstOrDefault(uca => uca.Id == customActionId);

                Assert.IsNotNull(foundCustomAction);
                Assert.AreEqual(customActionId, foundCustomAction.Id);
                Assert.AreEqual(UserCustomActionScope.Web, foundCustomAction.Scope);
            }

            await TestAssets.CleanupTestUserCustomActionAsync(2);
        }

        [TestMethod]
        public async Task GetSiteUserCustomActionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string customActionName, Guid customActionId) = await TestAssets.CreateTestUserCustomActionAsync(0, addToSiteCollection: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                ISite site = await context.Site.GetAsync(p => p.UserCustomActions);

                Assert.IsNotNull(site.UserCustomActions);
                Assert.IsTrue(site.UserCustomActions.Count() > 0);

                IUserCustomAction foundCustomAction = site.UserCustomActions.FirstOrDefault(uca => uca.Id == customActionId);

                Assert.IsNotNull(foundCustomAction);
                Assert.AreEqual(customActionId, foundCustomAction.Id);
                Assert.AreEqual(UserCustomActionScope.Site, foundCustomAction.Scope);
            }

            await TestAssets.CleanupTestUserCustomActionAsync(2, fromSiteCollection: true);
        }

        [TestMethod]
        public async Task AddWebUserCustomActionApplicationCustomizerAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IUserCustomAction newUserCustomAction = await context.Web.UserCustomActions.AddAsync(new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ApplicationCustomizer",
                    ClientSideComponentId = new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId),
                    ClientSideComponentProperties = @"{""message"":""Added from AddWebUserCustomActionApplicationCustomizerAsyncTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomHeader",
                    Title = "Custom Header",
                    RegistrationType = UserCustomActionRegistrationType.None,
                    Description = "TESTING"
                });

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreNotEqual(default, newUserCustomAction.Id);
                Assert.AreEqual(new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId), newUserCustomAction.ClientSideComponentId);
                Assert.AreEqual(@"{""message"":""Added from AddWebUserCustomActionApplicationCustomizerAsyncTest""}", newUserCustomAction.ClientSideComponentProperties);
                Assert.AreEqual("ClientSideExtension.ApplicationCustomizer", newUserCustomAction.Location);
                Assert.AreEqual("Custom Header", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomHeader", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);

                await newUserCustomAction.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebUserCustomActionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IUserCustomAction newUserCustomAction = context.Web.UserCustomActions.Add(new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ApplicationCustomizer",
                    ClientSideComponentId = new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId),
                    ClientSideComponentProperties = @"{""message"":""Added from AddWebUserCustomActionTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomHeader",
                    Title = "Custom Header",
                    RegistrationType = UserCustomActionRegistrationType.None,
                    Description = "TESTING"
                });

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreNotEqual(default, newUserCustomAction.Id);
                Assert.AreEqual(new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId), newUserCustomAction.ClientSideComponentId);
                Assert.AreEqual(@"{""message"":""Added from AddWebUserCustomActionTest""}", newUserCustomAction.ClientSideComponentProperties);
                Assert.AreEqual("ClientSideExtension.ApplicationCustomizer", newUserCustomAction.Location);
                Assert.AreEqual("Custom Header", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomHeader", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);

                await newUserCustomAction.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebUserCustomActionBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.NewBatch();
                IUserCustomAction newUserCustomAction = await context.Web.UserCustomActions.AddBatchAsync(batch, new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ApplicationCustomizer",
                    ClientSideComponentId = new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId),
                    ClientSideComponentProperties = @"{""message"":""Added from AddWebUserCustomActionBatchAsyncTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomHeader",
                    Title = "Custom Header",
                    RegistrationType = UserCustomActionRegistrationType.None,
                    Description = "TESTING"
                });
                await context.ExecuteAsync(batch);

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreNotEqual(default, newUserCustomAction.Id);
                Assert.AreEqual(new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId), newUserCustomAction.ClientSideComponentId);
                Assert.AreEqual(@"{""message"":""Added from AddWebUserCustomActionBatchAsyncTest""}", newUserCustomAction.ClientSideComponentProperties);
                Assert.AreEqual("ClientSideExtension.ApplicationCustomizer", newUserCustomAction.Location);
                Assert.AreEqual("Custom Header", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomHeader", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);

                await newUserCustomAction.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebUserCustomActionBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.NewBatch();
                IUserCustomAction newUserCustomAction = context.Web.UserCustomActions.AddBatch(batch, new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ApplicationCustomizer",
                    ClientSideComponentId = new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId),
                    ClientSideComponentProperties = @"{""message"":""Added from AddWebUserCustomActionBatchTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomHeader",
                    Title = "Custom Header",
                    RegistrationType = UserCustomActionRegistrationType.None,
                    Description = "TESTING"
                });
                await context.ExecuteAsync(batch);

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreNotEqual(default, newUserCustomAction.Id);
                Assert.AreEqual(new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId), newUserCustomAction.ClientSideComponentId);
                Assert.AreEqual(@"{""message"":""Added from AddWebUserCustomActionBatchTest""}", newUserCustomAction.ClientSideComponentProperties);
                Assert.AreEqual("ClientSideExtension.ApplicationCustomizer", newUserCustomAction.Location);
                Assert.AreEqual("Custom Header", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomHeader", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);

                await newUserCustomAction.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebUserCustomActionCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IUserCustomAction newUserCustomAction = await context.Web.UserCustomActions.AddBatchAsync(new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ApplicationCustomizer",
                    ClientSideComponentId = new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId),
                    ClientSideComponentProperties = @"{""message"":""Added from AddWebUserCustomActionCurrentBatchAsyncTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomHeader",
                    Title = "Custom Header",
                    RegistrationType = UserCustomActionRegistrationType.None,
                    Description = "TESTING"
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreNotEqual(default, newUserCustomAction.Id);
                Assert.AreEqual(new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId), newUserCustomAction.ClientSideComponentId);
                Assert.AreEqual(@"{""message"":""Added from AddWebUserCustomActionCurrentBatchAsyncTest""}", newUserCustomAction.ClientSideComponentProperties);
                Assert.AreEqual("ClientSideExtension.ApplicationCustomizer", newUserCustomAction.Location);
                Assert.AreEqual("Custom Header", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomHeader", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);

                await newUserCustomAction.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebUserCustomActionCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IUserCustomAction newUserCustomAction = context.Web.UserCustomActions.AddBatch(new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ApplicationCustomizer",
                    ClientSideComponentId = new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId),
                    ClientSideComponentProperties = @"{""message"":""Added from AddWebUserCustomActionCurrentBatchTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomHeader",
                    Title = "Custom Header",
                    RegistrationType = UserCustomActionRegistrationType.None,
                    Description = "TESTING"
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreNotEqual(default, newUserCustomAction.Id);
                Assert.AreEqual(new Guid(TestAssets.TestApplicationCustomizerClientSideComponentId), newUserCustomAction.ClientSideComponentId);
                Assert.AreEqual(@"{""message"":""Added from AddWebUserCustomActionCurrentBatchTest""}", newUserCustomAction.ClientSideComponentProperties);
                Assert.AreEqual("ClientSideExtension.ApplicationCustomizer", newUserCustomAction.Location);
                Assert.AreEqual("Custom Header", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomHeader", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);

                await newUserCustomAction.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebUserCustomActionListViewCommandSetBothAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IUserCustomAction newUserCustomAction = await context.Web.UserCustomActions.AddAsync(new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ListViewCommandSet",
                    ClientSideComponentId = new Guid(TestAssets.TestListViewCommandSetClientSideComponentId),
                    ClientSideComponentProperties = @"{""message"":""Added from AddWebUserCustomActionListViewCommandSetBothAsyncTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomCommand",
                    Title = "Custom Command",
                    RegistrationType = UserCustomActionRegistrationType.List,
                    RegistrationId = "101",
                    Description = "TESTING"
                });

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreNotEqual(default, newUserCustomAction.Id);
                Assert.AreEqual(new Guid(TestAssets.TestListViewCommandSetClientSideComponentId), newUserCustomAction.ClientSideComponentId);
                Assert.AreEqual(@"{""message"":""Added from AddWebUserCustomActionListViewCommandSetBothAsyncTest""}", newUserCustomAction.ClientSideComponentProperties);
                Assert.AreEqual("ClientSideExtension.ListViewCommandSet", newUserCustomAction.Location);
                Assert.AreEqual("Custom Command", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomCommand", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);

                await newUserCustomAction.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebUserCustomActionListViewCommandSetContextMenuAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IUserCustomAction newUserCustomAction = await context.Web.UserCustomActions.AddAsync(new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ListViewCommandSet.ContextMenu",
                    ClientSideComponentId = new Guid(TestAssets.TestListViewCommandSetClientSideComponentId),
                    ClientSideComponentProperties = @"{""message"":""Added from AddWebUserCustomActionListViewCommandSetContextMenuAsyncTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomCommand",
                    Title = "Custom Command",
                    RegistrationType = UserCustomActionRegistrationType.List,
                    RegistrationId = "101",
                    Description = "TESTING"
                });

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreNotEqual(default, newUserCustomAction.Id);
                Assert.AreEqual(new Guid(TestAssets.TestListViewCommandSetClientSideComponentId), newUserCustomAction.ClientSideComponentId);
                Assert.AreEqual(@"{""message"":""Added from AddWebUserCustomActionListViewCommandSetContextMenuAsyncTest""}", newUserCustomAction.ClientSideComponentProperties);
                Assert.AreEqual("ClientSideExtension.ListViewCommandSet.ContextMenu", newUserCustomAction.Location);
                Assert.AreEqual("Custom Command", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomCommand", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);

                await newUserCustomAction.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebUserCustomActionListViewCommandSetCommandBarAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IUserCustomAction newUserCustomAction = await context.Web.UserCustomActions.AddAsync(new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ListViewCommandSet.CommandBar",
                    ClientSideComponentId = new Guid(TestAssets.TestListViewCommandSetClientSideComponentId),
                    ClientSideComponentProperties = @"{""message"":""Added from AddWebUserCustomActionListViewCommandSetCommandBarAsyncTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomCommand",
                    Title = "Custom Command",
                    RegistrationType = UserCustomActionRegistrationType.List,
                    RegistrationId = "101",
                    Description = "TESTING"
                });

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreNotEqual(default, newUserCustomAction.Id);
                Assert.AreEqual(new Guid(TestAssets.TestListViewCommandSetClientSideComponentId), newUserCustomAction.ClientSideComponentId);
                Assert.AreEqual(@"{""message"":""Added from AddWebUserCustomActionListViewCommandSetCommandBarAsyncTest""}", newUserCustomAction.ClientSideComponentProperties);
                Assert.AreEqual("ClientSideExtension.ListViewCommandSet.CommandBar", newUserCustomAction.Location);
                Assert.AreEqual("Custom Command", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomCommand", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);

                await newUserCustomAction.DeleteAsync();
            }
        }

        // TODO: Uncomment when support for DenyAddAndCustomizePages is implemented
        //[TestMethod]
        //public async Task AddWebUserClassicCustomActionAsyncTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        IUserCustomAction newUserCustomAction = await context.Web.UserCustomActions.AddAsync(new AddUserCustomActionOptions()
        //        {
        //            Location = "EditControlBlock",
        //            Sequence = 100,
        //            Name = "UCA_Classic_CustomCommand",
        //            Title = "Custom Clasic Command",
        //            RegistrationType = UserCustomActionRegistrationType.List,
        //            RegistrationId = "101",
        //            Description = "TESTING",
        //            Group = "TESTING",
        //            Url = "https://aka.ms/pnp/coresdk/docs",
        //            ImageUrl = "https://aka.ms/pnp/coresdk/docs"
        //        });

        //        // Test the created object
        //        Assert.IsNotNull(newUserCustomAction);
        //        Assert.AreNotEqual(default, newUserCustomAction.Id);
        //        Assert.AreEqual("https://aka.ms/pnp/coresdk/docs", newUserCustomAction.Url);
        //        Assert.AreEqual("https://aka.ms/pnp/coresdk/docs", newUserCustomAction.ImageUrl);
        //        Assert.AreEqual("Custom Clasic Command", newUserCustomAction.Title);
        //        Assert.AreEqual("UCA_Classic_CustomCommand", newUserCustomAction.Name);
        //        Assert.AreEqual("TESTING", newUserCustomAction.Description);
        //        Assert.AreEqual("TESTING", newUserCustomAction.Group);

        //        await newUserCustomAction.DeleteAsync();
        //    }
        //}

        [TestMethod]
        public async Task UpdateWebUserCustomActionTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string customActionName, Guid customActionId) = await TestAssets.CreateTestUserCustomActionAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IWeb web = await context.Web.GetAsync(p => p.UserCustomActions);

                Assert.IsNotNull(web.UserCustomActions);
                Assert.IsTrue(web.UserCustomActions.Count() > 0);

                IUserCustomAction foundCustomAction = web.UserCustomActions.FirstOrDefault(uca => uca.Id == customActionId);

                Assert.IsNotNull(foundCustomAction);
                Assert.AreEqual(customActionId, foundCustomAction.Id);

                foundCustomAction.Name = $"{customActionName}_UPDATED";
                foundCustomAction.Title = $"{customActionName}_UPDATED";
                foundCustomAction.Sequence = 200;
                foundCustomAction.Description = "UPDATED DESCRIPTION";
                await foundCustomAction.UpdateAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IWeb web = await context.Web.GetAsync(p => p.UserCustomActions);

                Assert.IsNotNull(web.UserCustomActions);
                Assert.IsTrue(web.UserCustomActions.Count() > 0);

                IUserCustomAction foundCustomAction = web.UserCustomActions.FirstOrDefault(uca => uca.Id == customActionId);

                Assert.AreEqual($"{customActionName}_UPDATED", foundCustomAction.Name);
                Assert.AreEqual($"{customActionName}_UPDATED", foundCustomAction.Title);
                Assert.AreEqual(200, foundCustomAction.Sequence);
                Assert.AreEqual("UPDATED DESCRIPTION", foundCustomAction.Description);
            }

            await TestAssets.CleanupTestUserCustomActionAsync(3, $"{customActionName}_UPDATED");
        }

        [TestMethod]
        public async Task DeleteWebUserCustomActionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string customActionName, Guid customActionId) = await TestAssets.CreateTestUserCustomActionAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IWeb web = await context.Web.GetAsync(p => p.UserCustomActions);

                Assert.IsNotNull(web.UserCustomActions);
                Assert.IsTrue(web.UserCustomActions.Count() > 0);

                IUserCustomAction foundCustomAction = web.UserCustomActions.FirstOrDefault(uca => uca.Id == customActionId);
                Assert.IsNotNull(foundCustomAction);

                await foundCustomAction.DeleteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IWeb web = await context.Web.GetAsync(p => p.UserCustomActions);


                IUserCustomAction foundCustomAction = web.UserCustomActions.FirstOrDefault(uca => uca.Id == customActionId);
                Assert.IsNull(foundCustomAction);
            }
        }

        [TestMethod]
        public async Task ContentTypesUpdateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType contentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST UPDATE", "TESTING", "TESTING");

                // Test if the content type is created
                Assert.IsNotNull(contentType);

                // Update the content type
                contentType.Name = "UPDATED";
                await contentType.UpdateAsync();

                // Test if the updated content type is still found
                IContentType contentTypeToFind = (from ct in context.Web.ContentTypes
                                                  where ct.Name == "UPDATED"
                                                  select ct).FirstOrDefault();

                Assert.IsNotNull(contentTypeToFind);

                await contentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesDeleteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType contentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST DELETE", "TESTING", "TESTING");

                // Test if the content type is created
                Assert.IsNotNull(contentType);

                // Delete the content type again
                await contentType.DeleteAsync();

                // Test if the content type is still found
                IContentType contentTypeToFind = (from ct in context.Web.ContentTypes
                                                  where ct.Name == "TEST DELETE"
                                                  select ct).FirstOrDefault();

                Assert.IsNull(contentTypeToFind);
            }
        }
    }
}
