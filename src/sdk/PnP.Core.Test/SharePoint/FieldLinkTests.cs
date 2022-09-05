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
    public class FieldLinkTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetSiteContentTypeFieldLinksTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get existing content type
                IContentType contentType = (from ct in context.Web.ContentTypes
                                            where ct.Name == "Document"
                                            select ct)
                                            .QueryProperties(ct => ct.FieldLinks)
                                            .FirstOrDefault();

                Assert.IsTrue(contentType.FieldLinks.Length > 0);
                Assert.IsNotNull(contentType);

                IFieldLink titleFieldLink = contentType.FieldLinks.AsRequested().FirstOrDefault(fl => fl.Name == "Title");

                Assert.IsNotNull(titleFieldLink);
                Assert.AreEqual("Title", titleFieldLink.Name);
                Assert.AreEqual(new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247"), titleFieldLink.Id);
            }
        }

        [TestMethod]
        public async Task GetListContentTypeFieldLinksTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get existing content type

                IContentType contentType = (from ct in context.Web.Lists.GetByTitle("Documents").ContentTypes
                                            where ct.Name == "Document"
                                            select ct)
                                            .QueryProperties(ct => ct.FieldLinks)
                                            .FirstOrDefault();

                Assert.IsTrue(contentType.FieldLinks.Length > 0);
                Assert.IsNotNull(contentType);

                IFieldLink titleFieldLink = contentType.FieldLinks.AsRequested().FirstOrDefault(fl => fl.Name == "Title");

                Assert.IsNotNull(titleFieldLink);
                Assert.AreEqual("Title", titleFieldLink.Name);
                Assert.AreEqual(new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247"), titleFieldLink.Id);
            }
        }

        [TestMethod]
        public async Task ContentTypesAddFieldLinkFromWebFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST ADD", "TESTING", "TESTING");
                IField newField = null;

                try
                {
                    // Test the created object
                    Assert.IsNotNull(newContentType);
                    Assert.AreEqual("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", newContentType.StringId);
                    Assert.AreEqual("TEST ADD", newContentType.Name);
                    Assert.AreEqual("TESTING", newContentType.Description);
                    Assert.AreEqual("TESTING", newContentType.Group);

                    // Add a new field
                    newField = await context.Web.Fields.AddTextAsync("Demo1", new FieldTextOptions { Group = "Custom" });

                    // Add the field to the content type
                    var fieldLink = newContentType.FieldLinks.Add(newField, displayName: "Demo1 link", hidden: false, required: true, readOnly: false, showInDisplayForm: true);

                    // Load the field links again
                    var fieldlinks = (await newContentType.GetAsync(p => p.FieldLinks.QueryProperties(p => p.Required, p => p.Hidden, p => p.DisplayName, p => p.ReadOnly, p => p.ShowInDisplayForm))).FieldLinks;
                    var addedFieldLink = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo1 link");
                    Assert.IsTrue(addedFieldLink.Required);
                    Assert.IsFalse(addedFieldLink.Hidden);
                    Assert.IsFalse(addedFieldLink.ReadOnly);
                    Assert.IsTrue(addedFieldLink.ShowInDisplayForm);

                    // Delete the added field link
                    await addedFieldLink.DeleteAsync();

                    fieldlinks = (await newContentType.GetAsync(p => p.FieldLinks.QueryProperties(p => p.Required, p => p.Hidden, p => p.DisplayName, p => p.ReadOnly, p => p.ShowInDisplayForm))).FieldLinks;
                    addedFieldLink = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo1 link");

                    Assert.IsTrue(addedFieldLink == null);
                }
                finally
                {
                    await newContentType.DeleteAsync();
                    await newField.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task ContentTypesAddFieldLinkFromWebFieldBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST ADD", "TESTING", "TESTING");
                IField newField1 = null;
                IField newField2 = null;

                try
                {
                    // Test the created object
                    Assert.IsNotNull(newContentType);
                    Assert.AreEqual("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", newContentType.StringId);
                    Assert.AreEqual("TEST ADD", newContentType.Name);
                    Assert.AreEqual("TESTING", newContentType.Description);
                    Assert.AreEqual("TESTING", newContentType.Group);

                    // Add a new field
                    newField1 = await context.Web.Fields.AddTextBatchAsync("Demo1", new FieldTextOptions { Group = "Custom" });
                    newField2 = await context.Web.Fields.AddTextBatchAsync("Demo2", new FieldTextOptions { Group = "Custom" });

                    await context.ExecuteAsync();

                    // Add the field to the content type
                    newContentType.FieldLinks.AddBatch(newField1);
                    newContentType.FieldLinks.AddBatch(newField2);

                    await context.ExecuteAsync();

                    // Load the field links again
                    var fieldlinks = (await newContentType.GetAsync(p => p.FieldLinks.QueryProperties(p => p.Required, p => p.Hidden, p => p.DisplayName, p => p.ReadOnly, p => p.ShowInDisplayForm))).FieldLinks;
                    var addedFieldLink1 = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo1");
                    var addedFieldLink2 = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo2");

                    Assert.IsNotNull(addedFieldLink1);
                    Assert.IsNotNull(addedFieldLink2);

                    // Update the added field links
                    addedFieldLink1.Required = true;
                    addedFieldLink2.Required = true;

                    addedFieldLink1.UpdateBatch();
                    addedFieldLink2.UpdateBatch();

                    await context.ExecuteAsync();

                    fieldlinks = (await newContentType.GetAsync(p => p.FieldLinks.QueryProperties(p => p.Required, p => p.Hidden, p => p.DisplayName, p => p.ReadOnly, p => p.ShowInDisplayForm))).FieldLinks;
                    addedFieldLink1 = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo1");
                    addedFieldLink2 = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo2");

                    Assert.IsTrue(addedFieldLink1.Required);
                    Assert.IsTrue(addedFieldLink2.Required);

                    // Delete the added field link
                    await addedFieldLink1.DeleteBatchAsync();
                    await addedFieldLink2.DeleteBatchAsync();

                    await context.ExecuteAsync();

                    fieldlinks = (await newContentType.GetAsync(p => p.FieldLinks.QueryProperties(p => p.Required, p => p.Hidden, p => p.DisplayName, p => p.ReadOnly, p => p.ShowInDisplayForm))).FieldLinks;
                    addedFieldLink1 = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo1");
                    addedFieldLink2 = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo2");

                    Assert.IsNull(addedFieldLink1);
                    Assert.IsNull(addedFieldLink2);
                }
                finally
                {
                    await newContentType.DeleteAsync();
                    await newField1.DeleteAsync();
                    await newField2.DeleteAsync();
                }
            }
        }


        [TestMethod]
        public async Task ContentTypesOnListFieldLinkTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                string listTitle = TestCommon.GetPnPSdkTestAssetName("ContentTypesOnListFieldLinkTest");
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                IContentType newContentType = null;
                try
                {
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

                    // Add new content type
                    newContentType = await context.Web.ContentTypes.AddBatchAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST ADD", "TESTING", "TESTING");
                    
                    // Add field to list
                    var demoField = await myList.Fields.AddTextBatchAsync("Demo");

                    // send list update batch to server
                    await context.ExecuteAsync();

                    // And add it to the list
                    var addedContentType = await myList.ContentTypes.AddAvailableContentTypeAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F");

                    // Add existing list field
                    addedContentType.FieldLinks.Add(demoField, displayName: "Demo1 link", hidden: false, required: true, readOnly: false, showInDisplayForm: true);

                    // Load the fieldlinks again
                    var fieldlinks = (await addedContentType.GetAsync(p => p.FieldLinks.QueryProperties(p => p.Required, p => p.Hidden, p => p.DisplayName, p => p.ReadOnly, p => p.ShowInDisplayForm))).FieldLinks;

                    var addedFieldLink = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo1 link");
                    Assert.IsTrue(addedFieldLink.Required);
                    Assert.IsFalse(addedFieldLink.Hidden);
                    Assert.IsFalse(addedFieldLink.ReadOnly);
                    Assert.IsTrue(addedFieldLink.ShowInDisplayForm);

                    // Delete the added field link
                    await addedFieldLink.DeleteAsync();

                    fieldlinks = (await addedContentType.GetAsync(p => p.FieldLinks.QueryProperties(p => p.Required, p => p.Hidden, p => p.DisplayName, p => p.ReadOnly, p => p.ShowInDisplayForm))).FieldLinks;
                    addedFieldLink = fieldlinks.AsRequested().FirstOrDefault(p => p.DisplayName == "Demo1 link");

                    Assert.IsTrue(addedFieldLink == null);
                }
                finally
                {
                    // Delete list again
                    await myList.DeleteAsync();

                    // Delete the content type
                    await newContentType.DeleteAsync();
                }
            }
        }

    }
}
