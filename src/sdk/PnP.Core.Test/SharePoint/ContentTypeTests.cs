using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ContentTypeTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task ContentTypesGetTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadAsync(p => p.ContentTypes);
                IWeb web = context.Web;

                Assert.IsTrue(web.ContentTypes.Length > 0);

                IContentType contentType = web.ContentTypes.AsEnumerable().FirstOrDefault(p => p.Name == "Item");
                // Test a string property
                Assert.AreEqual(contentType.Name, "Item");
                // Test a boolean property
                Assert.IsFalse(contentType.Hidden);
            }
        }

        [TestMethod]
        public async Task ContentTypesGetPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadAsync(p => p.ContentTypes);
                IWeb web = context.Web;
                Assert.IsTrue(web.ContentTypes.Length > 0);

                IContentType contentType = web.ContentTypes.AsEnumerable().FirstOrDefault(p => p.Name == "Item");
                // Test a string property
                Assert.AreEqual(contentType.Name, "Item");

                //ClientFormCustomFormatter
                Assert.IsTrue(string.IsNullOrEmpty(contentType.ClientFormCustomFormatter));
                Assert.AreEqual(contentType.Description, "Create a new list item.");
                Assert.AreEqual(contentType.DisplayFormTemplateName, "ListForm");
                Assert.AreEqual(contentType.DisplayFormUrl, "");
                Assert.AreEqual(contentType.DocumentTemplate, "");
                Assert.AreEqual(contentType.DocumentTemplateUrl, "");
                Assert.AreEqual(contentType.EditFormTemplateName, "ListForm");
                Assert.IsTrue(string.IsNullOrEmpty(contentType.EditFormUrl));
                Assert.AreEqual(contentType.Group, "List Content Types");
                Assert.AreEqual(contentType.Hidden, false);
                Assert.AreEqual(contentType.JSLink, "");
                Assert.AreEqual(contentType.MobileDisplayFormUrl, "");
                Assert.AreEqual(contentType.MobileEditFormUrl, "");
                Assert.AreEqual(contentType.MobileNewFormUrl, "");
                Assert.AreEqual(contentType.Name, "Item");
                Assert.AreEqual(contentType.NewFormTemplateName, "ListForm");
                Assert.AreEqual(contentType.NewFormUrl, "");
                Assert.AreEqual(contentType.ReadOnly, false);
                Assert.IsTrue(!string.IsNullOrEmpty(contentType.SchemaXml));
                // Testing on site names is not possible as this will fail when the tests are running live again 
                //Assert.AreEqual(contentType.Scope, "/sites/pnpcoresdktestgroup");
                Assert.AreEqual(contentType.Sealed, false);
                Assert.AreEqual(contentType.StringId, "0x01");

                // Test a boolean property
                Assert.IsFalse(contentType.Hidden);
            }
        }

        [TestMethod]
        public async Task ContentTypesOnListGetTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                //var sitePages = context.Web.Lists.GetByTitle("Site Pages", p => p.ContentTypes);
                await context.Web.LoadAsync(p => p.Lists);
                var sitePages = context.Web.Lists.AsEnumerable().FirstOrDefault(p => p.Title == "Site Pages");
                if (sitePages != null)
                {
                    Assert.IsTrue(sitePages.Requested);

                    await sitePages.LoadAsync(p => p.ContentTypes);
                    Assert.IsTrue(sitePages.ContentTypes.Requested);

                    Assert.IsTrue(sitePages.ContentTypes.Length > 0);
                }
            }
        }

        [TestMethod]
        public async Task ContentTypesOnListGetByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                await context.Web.LoadAsync(p => p.Lists);
                var sitePages = context.Web.Lists.AsEnumerable().FirstOrDefault(p => p.Title == "Site Pages");
                if (sitePages != null)
                {
                    await sitePages.LoadAsync(p => p.ContentTypes);
                    IContentType contentType = sitePages.ContentTypes.AsEnumerable().FirstOrDefault(p => p.StringId.StartsWith("0x0101009D1CB255DA76424F860D91F20E6C4118"));

                    Assert.IsNotNull(contentType);
                    // Test Id property
                    Assert.IsTrue(contentType.Id.StartsWith("0x0101009D1CB255DA76424F860D91F20E6C4118"));
                }
            }
        }

        [TestMethod]
        public async Task ContentTypesGetByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType contentType = (from ct in context.Web.ContentTypes
                                            where ct.StringId == "0x01"
                                            select ct)
                            .QueryProperties(ct => ct.StringId, ct => ct.Id)
                            .FirstOrDefault();

                Assert.IsNotNull(contentType);
                // Test Id property
                Assert.AreEqual(contentType.Id, "0x01");
            }
        }

        [TestMethod]
        public async Task ContentTypesAddTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST ADD", "TESTING", "TESTING");

                // Test the created object
                Assert.IsNotNull(newContentType);
                Assert.AreEqual("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", newContentType.StringId);
                Assert.AreEqual("TEST ADD", newContentType.Name);
                Assert.AreEqual("TESTING", newContentType.Description);
                Assert.AreEqual("TESTING", newContentType.Group);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesAddExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IContentType newContentType = await context.Web.ContentTypes.AddAsync(string.Empty, "TEST ADD", "TESTING", "TESTING");
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IContentType newContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", string.Empty, "TESTING", "TESTING");
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IContentType newContentType = await context.Web.ContentTypes.AddAsync(string.Empty, string.Empty, "TESTING", "TESTING");
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IContentType newContentType = await context.Web.ContentTypes.AddBatchAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", string.Empty, "TESTING", "TESTING");
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IContentType newContentType = await context.Web.ContentTypes.AddBatchAsync(string.Empty, "TEST ADD", "TESTING", "TESTING");
                });
            }
        }

        #region ContentTypesOnListAddAvailable Tests

        [TestMethod]
        public async Task ContentTypesOnListAddAvailableBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = "ContentTypesOnListAddAvailableTest";
                var myList = web.Lists.AsEnumerable().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Ensure content type are enabled for the list
                myList.ContentTypesEnabled = true;
                await myList.UpdateBatchAsync();

                // Add existing content type (contact)
                var addedContentType = await myList.ContentTypes.AddAvailableContentTypeBatchAsync("0x0106");

                // send batch to server
                await context.ExecuteAsync();

                Assert.IsTrue(addedContentType != null);
                Assert.IsTrue(addedContentType.Requested);
                Assert.IsTrue(addedContentType.Id.StartsWith("0x0106"));

                // Delete list again
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesOnListAddAvailableSpecificBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();

                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = "ContentTypesOnListAddAvailableTest";
                var myList = web.Lists.AsEnumerable().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Ensure content type are enabled for the list
                myList.ContentTypesEnabled = true;
                await myList.UpdateBatchAsync(newBatch);
                await context.ExecuteAsync(newBatch);

                // Add existing content type (contact)
                var addedContentType = await myList.ContentTypes.AddAvailableContentTypeBatchAsync(newBatch, "0x0106");

                // send batch to server
                await context.ExecuteAsync(newBatch);

                Assert.IsTrue(addedContentType != null);
                Assert.IsTrue(addedContentType.Requested);
                Assert.IsTrue(addedContentType.Id.StartsWith("0x0106"));

                // Delete list again
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesOnListAddAvailableBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = "ContentTypesOnListAddAvailableTest";
                var myList = web.Lists.AsEnumerable().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Ensure content type are enabled for the list
                myList.ContentTypesEnabled = true;
                await myList.UpdateBatchAsync();
                await context.ExecuteAsync();

                // Add existing content type (contact)
                var addedContentType = myList.ContentTypes.AddAvailableContentTypeBatch("0x0106");

                // send batch to server
                await context.ExecuteAsync();

                Assert.IsTrue(addedContentType != null);
                Assert.IsTrue(addedContentType.Requested);
                Assert.IsTrue(addedContentType.Id.StartsWith("0x0106"));

                // Delete list again
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesOnListAddAvailableSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();

                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = "ContentTypesOnListAddAvailableTest";
                var myList = web.Lists.AsEnumerable().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Ensure content type are enabled for the list
                myList.ContentTypesEnabled = true;
                await myList.UpdateBatchAsync(newBatch);
                await context.ExecuteAsync(newBatch);

                // Add existing content type (contact)
                var addedContentType = myList.ContentTypes.AddAvailableContentTypeBatch(newBatch, "0x0106");

                // send batch to server
                await context.ExecuteAsync(newBatch);

                Assert.IsTrue(addedContentType != null);
                Assert.IsTrue(addedContentType.Requested);
                Assert.IsTrue(addedContentType.Id.StartsWith("0x0106"));

                // Delete list again
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesOnListAddAvailableAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = "ContentTypesOnListAddAvailableTest";
                var myList = web.Lists.AsEnumerable().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Ensure content type are enabled for the list
                myList.ContentTypesEnabled = true;
                await myList.UpdateAsync();

                // Add existing content type (contact)
                var addedContentType = await myList.ContentTypes.AddAvailableContentTypeAsync("0x0106");

                Assert.IsTrue(addedContentType != null);
                Assert.IsTrue(addedContentType.Requested);
                Assert.IsTrue(addedContentType.Id.StartsWith("0x0106"));

                // Delete list again
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesOnListAddAvailableTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = "ContentTypesOnListAddAvailableTest";
                var myList = web.Lists.AsEnumerable().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Ensure content type are enabled for the list
                myList.ContentTypesEnabled = true;
                await myList.UpdateAsync();

                // Add existing content type (contact)
                var addedContentType = myList.ContentTypes.AddAvailableContentType("0x0106");

                Assert.IsTrue(addedContentType != null);
                Assert.IsTrue(addedContentType.Requested);
                Assert.IsTrue(addedContentType.Id.StartsWith("0x0106"));

                // Delete list again
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesOnListAddAvailableExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = "ContentTypesOnListAddAvailableTest";
                var myList = web.Lists.AsEnumerable().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Ensure content type are enabled for the list
                myList.ContentTypesEnabled = true;
                await myList.UpdateAsync();

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    var addedContentType = await myList.ContentTypes.AddAvailableContentTypeAsync(null);
                });
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    var addedContentType = await myList.ContentTypes.AddAvailableContentTypeBatchAsync(null);
                });

                // Delete list again
                await myList.DeleteAsync();
            }
        }

        #endregion

        [TestMethod]
        public async Task ContentTypesOnListAddTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = "ContentTypesOnListAddTest";
                var myList = web.Lists.AsEnumerable().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Ensure content type are enabled for the list
                myList.ContentTypesEnabled = true;
                await myList.UpdateAsync();

                bool clientExceptionThrown = false;
                try
                {
                    // Add new content type as list content type
                    IContentType newContentType = await myList.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST ADD", "TESTING", "TESTING");
                }
                catch (ClientException)
                {
                    clientExceptionThrown = true;
                }

                Assert.IsTrue(clientExceptionThrown);

                // Delete list again
                await myList.DeleteAsync();
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

        [TestMethod]
        public async Task ContentTypesOnListDeleteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = "ContentTypesOnListDeleteTest";
                var myList = web.Lists.AsEnumerable().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Ensure content type are enabled for the list
                myList.ContentTypesEnabled = true;
                await myList.UpdateBatchAsync();

                // Add existing content type (contact)
                var addedContentType = await myList.ContentTypes.AddAvailableContentTypeBatchAsync("0x0106");

                // send batch to server
                await context.ExecuteAsync();

                Assert.IsTrue(addedContentType != null);
                Assert.IsTrue(addedContentType.Requested);
                Assert.IsTrue(addedContentType.Id.StartsWith("0x0106"));

                // Remove the content type again from the list
                await addedContentType.DeleteAsync();

                // Try to load the content type again, ensure it was removed
                IContentType contentType = myList.ContentTypes.AsEnumerable().FirstOrDefault(p => p.StringId.StartsWith("0x0106"));

                Assert.IsTrue(contentType == null);

                // Delete list again
                await myList.DeleteAsync();
            }
        }

    }
}
