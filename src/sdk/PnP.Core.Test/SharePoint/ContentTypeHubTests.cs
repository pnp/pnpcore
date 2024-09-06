using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ContentTypeHubTests
    {
        private const string docSetId = "0x0120D520002E5AAE78D8DB1947903FC99D0979E4A5";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;            
        }

        [TestMethod]
        public void GetContentUrlReplacement()
        {
            Assert.IsTrue(ContentTypeHub.SwitchToContentTypeHubUrl(new Uri("https://bertonline.sharepoint.com/sites/demo1"), "https://bertonline.sharepoint.com/sites/demo1/_api/web/contenttypes") == "https://bertonline.sharepoint.com/sites/contenttypehub/_api/web/contenttypes");
            Assert.IsTrue(ContentTypeHub.SwitchToContentTypeHubUrl(new Uri("https://bertonline.sharepoint.com"), "https://bertonline.sharepoint.com/_api/web/contenttypes") == "https://bertonline.sharepoint.com/sites/contenttypehub/_api/web/contenttypes");
            Assert.IsTrue(ContentTypeHub.SwitchToContentTypeHubUrl(new Uri("https://bertonline.sharepoint.com/sites/demo1"), "/sites/demo1/_api/web/contenttypes") == "/sites/contenttypehub/_api/web/contenttypes");
            Assert.IsTrue(ContentTypeHub.SwitchToContentTypeHubUrl(new Uri("https://bertonline.sharepoint.com"), "/_api/web/contenttypes") == "/sites/contenttypehub/_api/web/contenttypes");

            Assert.IsTrue(ContentTypeHub.SwitchToContentTypeHubUrl(new Uri("https://bertonline.sharepoint.com/sites/demo1"), "https://bertonline.sharepoint.com/sites/demo1/_api/web/contenttypes?urlparam=a") == "https://bertonline.sharepoint.com/sites/contenttypehub/_api/web/contenttypes?urlparam=a");
            Assert.IsTrue(ContentTypeHub.SwitchToContentTypeHubUrl(new Uri("https://bertonline.sharepoint.com"), "https://bertonline.sharepoint.com/_api/web/contenttypes?urlparam=a") == "https://bertonline.sharepoint.com/sites/contenttypehub/_api/web/contenttypes?urlparam=a");
            Assert.IsTrue(ContentTypeHub.SwitchToContentTypeHubUrl(new Uri("https://bertonline.sharepoint.com/sites/demo1"), "/sites/demo1/_api/web/contenttypes?urlparam=a") == "/sites/contenttypehub/_api/web/contenttypes?urlparam=a");
            Assert.IsTrue(ContentTypeHub.SwitchToContentTypeHubUrl(new Uri("https://bertonline.sharepoint.com"), "/_api/web/contenttypes?urlparam=a") == "/sites/contenttypehub/_api/web/contenttypes?urlparam=a");
        }

        [TestMethod]
        public async Task GetContentTypesFromHubAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.ContentTypeHub.LoadAsync(y => y.ContentTypes);

                Assert.IsNotNull(context.ContentTypeHub.ContentTypes.AsRequested());
                Assert.IsTrue(context.ContentTypeHub.ContentTypes.AsRequested().Count() > 0);
            }
        }

        [TestMethod]
        public async Task AddContentTypeToHubAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;

                try
                {
                    newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", "AddContentTypeToHubAsyncTest", "TESTING", "TESTING");

                    // Test the created object
                    Assert.IsNotNull(newContentType);
                    Assert.AreEqual("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", newContentType.StringId);
                    Assert.AreEqual("AddContentTypeToHubAsyncTest", newContentType.Name);
                    Assert.AreEqual("TESTING", newContentType.Description);
                    Assert.AreEqual("TESTING", newContentType.Group);

                    await context.ContentTypeHub.LoadAsync(y => y.ContentTypes);

                    var matchingCt = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(y => y.Id == newContentType.StringId);

                    Assert.IsNotNull(matchingCt);
                }
                finally
                {
                    if (newContentType != null)
                    {
                        await newContentType.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task PublishContentTypeAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;

                try
                {
                    newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5959D", "IsContentTypePublished", "TESTING", "TESTING");

                    var isPublished = await newContentType.IsPublishedAsync();

                    Assert.IsFalse(isPublished);

                    await newContentType.PublishAsync();

                    isPublished = await newContentType.IsPublishedAsync();

                    Assert.IsTrue(isPublished);

                }
                finally
                {
                    if (newContentType != null)
                    {
                        await newContentType.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task UnpublishContentTypeAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;

                try
                {
                    newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968D", "TEST ADD Unpublish", "TESTING", "TESTING");

                    // First we have to publish the content type

                    await newContentType.PublishAsync();

                    var isPublished = await newContentType.IsPublishedAsync();

                    Assert.IsTrue(isPublished);

                    await newContentType.UnpublishAsync();

                    isPublished = await newContentType.IsPublishedAsync();

                    Assert.IsFalse(isPublished);

                }
                finally
                {
                    if (newContentType != null)
                    {
                        await newContentType.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task CreateContentTypeAndAddFieldTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;
                IField newField = null;

                try
                {
                    newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5961A", "CreateContentTypeC", "TESTING", "TESTING");

                    newField = await context.ContentTypeHub.Fields.AddBooleanAsync("TestField", new FieldBooleanOptions { });

                    await newContentType.AddFieldAsync(newField);
                    await newContentType.LoadAsync(y => y.Fields);

                    // Check if field is created on ct
                    var matchingField = newContentType.Fields.AsRequested().FirstOrDefault(y => y.Id == newField.Id);

                    Assert.IsNotNull(matchingField);
                }
                finally
                {
                    // Clean up
                    if (newContentType != null)
                    {
                        await newContentType.DeleteAsync();
                    }

                    if (newField != null)
                    {
                        await newField.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task UnpublishContentTypeAsyncExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;

                try
                {
                    newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968D", "TEST ADD Unpublish", "TESTING", "TESTING");

                    await Assert.ThrowsExceptionAsync<Exception>(async () =>
                    {
                        await newContentType.UnpublishAsync();
                    });

                }
                finally
                {
                    if (newContentType != null)
                    {
                        await newContentType.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task PublishContentTypeAsyncTargetExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;

                try
                {
                    newContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968D", "TEST ADD Publish", "TESTING", "TESTING");

                    await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                    {
                        await newContentType.PublishAsync();
                    });

                }
                finally
                {
                    if (newContentType != null)
                    {
                        await newContentType.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddContentTypeToHubAndConsumeInSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;
                IContentType matchingContentType = null;
                try
                {

                    // Add content type to the hub
                    newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", "AddContentTypeToHubAsyncTest", "TESTING", "TESTING");

                    // Test the created object
                    Assert.IsNotNull(newContentType);
                    Assert.AreEqual("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", newContentType.StringId);
                    Assert.AreEqual("AddContentTypeToHubAsyncTest", newContentType.Name);
                    Assert.AreEqual("TESTING", newContentType.Description);
                    Assert.AreEqual("TESTING", newContentType.Group);

                    // Ensure the content type is published in the hub
                    await newContentType.PublishAsync();

                    // Add content type from hub to list
                    var added = context.Web.ContentTypes.AddAvailableContentTypeFromHub(newContentType.StringId, new AddContentTypeFromHubOptions { WaitForCompletion = true });

                    //Assert.IsTrue(added);

                    // Load the list content types

                    await context.Web.LoadAsync(y => y.ContentTypes);

                    matchingContentType = context.Web.ContentTypes.AsRequested().FirstOrDefault(y => y.Id.StartsWith(newContentType.StringId));

                    Assert.IsNotNull(matchingContentType);
                }
                finally
                {
                    // Delete from site, first set it to updatable
                    if (matchingContentType != null)
                    {
                        // First remove the readonly flag
                        matchingContentType.ReadOnly = false;
                        await matchingContentType.UpdateAsync();

                        await matchingContentType.DeleteAsync();
                    }

                    if (newContentType != null)
                    {
                        // Delete from content type hub
                        await newContentType.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddContentTypeToHubAndConsumeInSubSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;
                IWeb addedWeb = null;
                try
                {
                    // Add content type to the hub
                    newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", "AddContentTypeToHubAsyncTest", "TESTING", "TESTING");

                    // Test the created object
                    Assert.IsNotNull(newContentType);
                    Assert.AreEqual("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", newContentType.StringId);
                    Assert.AreEqual("AddContentTypeToHubAsyncTest", newContentType.Name);
                    Assert.AreEqual("TESTING", newContentType.Description);
                    Assert.AreEqual("TESTING", newContentType.Group);

                    // Ensure the content type is published in the hub
                    await newContentType.PublishAsync();

                    string webTitle = "contenttypetestweb";

                    addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });

                    using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                    {

                        string listTitle = TestCommon.GetPnPSdkTestAssetName("AddContentTypeToHubAndConsumeInListTest");

                        var myList = context2.Web.Lists.GetByTitle(listTitle);

                        if (TestCommon.Instance.Mocking && myList != null)
                        {
                            Assert.Inconclusive("Test data set should be setup to not have the list available.");
                        }

                        if (myList == null)
                        {
                            myList = await context2.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                        }

                        myList.ContentTypesEnabled = true;
                        await myList.UpdateAsync();

                        // Add content type from hub to list
                        var added = myList.ContentTypes.AddAvailableContentTypeFromHub(newContentType.StringId);

                        //Assert.IsTrue(added);

                        // Load the list content types
                        await myList.LoadAsync(y => y.ContentTypes);

                        var matchingCt = myList.ContentTypes.AsRequested().FirstOrDefault(y => y.Id.StartsWith(newContentType.StringId));

                        Assert.IsNotNull(matchingCt);
                    }
                }
                finally
                {
                    // Delete the created web again
                    await addedWeb.DeleteAsync();

                    if (newContentType != null)
                    {
                        // Delete from content type hub
                        await newContentType.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task LongRunningOperationTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string location = "https://graph.microsoft.com/v1.0/sites/f92f9e40-1110-43ef-aa0e-0822e13fb7ba,2c99a486-d6c9-4a4b-8d6f-a9faa364c92c/operations/contentTypeCopy,0x0100302EF0D1F1DB4C4EBF58251BCCF5966F";

                LongRunningOperation l = new LongRunningOperation(location, context);

                l.WaitForCompletion();
            }
        }


        [TestMethod]
        public async Task AddContentTypeToHubAndConsumeInListTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;
                IList myList = null;

                try
                {
                    // Add new list
                    string listTitle = TestCommon.GetPnPSdkTestAssetName("AddContentTypeToHubAndConsumeInListTest");

                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    myList.ContentTypesEnabled = true;
                    await myList.UpdateAsync();

                    // Add content type to the hub
                    newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", "AddContentTypeToHubAsyncTest", "TESTING", "TESTING");

                    // Test the created object
                    Assert.IsNotNull(newContentType);
                    Assert.AreEqual("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", newContentType.StringId);
                    Assert.AreEqual("AddContentTypeToHubAsyncTest", newContentType.Name);
                    Assert.AreEqual("TESTING", newContentType.Description);
                    Assert.AreEqual("TESTING", newContentType.Group);

                    // Ensure the content type is published in the hub
                    await newContentType.PublishAsync();

                    // Add content type from hub to list
                    var added = myList.ContentTypes.AddAvailableContentTypeFromHub(newContentType.StringId);

                    //Assert.IsTrue(added);

                    // Load the list content types

                    await myList.LoadAsync(y => y.ContentTypes);

                    var matchingCt = myList.ContentTypes.AsRequested().FirstOrDefault(y => y.Id.StartsWith(newContentType.StringId));

                    Assert.IsNotNull(matchingCt);
                }
                finally
                {
                    if (myList != null)
                    {
                        await myList.DeleteAsync();
                    }

                    if (newContentType != null)
                    {
                        await newContentType.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddContentTypeHubFieldAndPropagateChanges()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = null;

                try
                {
                    // Create field
                    addedField = await context.ContentTypeHub.Fields.AddTextAsync("PropagateFieldChanges", new FieldTextOptions());

                    // Set default value
                    addedField.DefaultValue = "B";

                    // Push update of added field, will also trigger update of the list field
                    addedField.UpdateAndPushChanges();
                }
                finally
                {
                    await addedField.DeleteAsync();
                }
            }
        }

        #region Document Sets

        // Ensure the document set site collection feature is enabled before running test tests live (should be active by default on the content type hub site)

        [TestMethod]
        public async Task AddContentTypeAsDocumentSet()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.ContentTypeHub.LoadAsync(y => y.Fields, y => y.ContentTypes);

                var categoriesField = context.ContentTypeHub.Fields.AsRequested().FirstOrDefault(y => y.InternalName == "Categories");
                var managersField = context.ContentTypeHub.Fields.AsRequested().FirstOrDefault(y => y.InternalName == "ManagersName");
                var documentCt = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(y => y.Name == "Document");
                var formCt = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(y => y.Name == "Form");

                var documentSetOptions = new DocumentSetOptions
                {
                    AllowedContentTypes = new List<IContentType>
                    {
                        documentCt,
                        formCt
                    },
                    ShouldPrefixNameToFile = true,
                    PropagateWelcomePageChanges = true,
                    SharedColumns = new List<IField>
                    {
                        managersField,
                        categoriesField
                    },
                    WelcomePageColumns = new List<IField>
                    {
                        managersField,
                        categoriesField
                    }
                };

                IContentType newContentType = null;

                try
                {
                    IDocumentSet newDocumentSet = await context.ContentTypeHub.ContentTypes.AddDocumentSetAsync(docSetId, "Document set name", "TESTING", "TESTING", documentSetOptions);
                    newContentType = newDocumentSet.Parent as IContentType;
                    
                    // Test the created object
                    Assert.IsNotNull(newContentType);
                    Assert.IsNotNull(newDocumentSet);

                    Assert.AreEqual(newDocumentSet.SharedColumns.Count, documentSetOptions.SharedColumns.Count);
                    Assert.AreEqual(newDocumentSet.WelcomePageColumns.Count, documentSetOptions.WelcomePageColumns.Count);
                    Assert.AreEqual(newDocumentSet.AllowedContentTypes.Count, documentSetOptions.AllowedContentTypes.Count);
                    Assert.AreEqual(newDocumentSet.ShouldPrefixNameToFile, documentSetOptions.ShouldPrefixNameToFile);
                }
                finally
                {
                    if (newContentType != null)
                    {
                        await newContentType.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task UpdateContentTypeAsDocumentSet()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = null;
                IFile file = null; 
                try
                {
                    var list = await context.Web.Lists.GetByTitle("Documents").GetAsync();
                    var rootFolder = await list.RootFolder.GetAsync();

                    (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(parentFolder: rootFolder);

                    var categoriesField = await context.Web.Fields.FirstAsync(y => y.InternalName == "Categories").ConfigureAwait(false);
                    var managersField = await context.Web.Fields.FirstAsync(y => y.InternalName == "ManagersName").ConfigureAwait(false);
                    var documentCt = await context.Web.ContentTypes.FirstAsync(y => y.Name == "Document").ConfigureAwait(false);

                    file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    var documentSetOptions = new DocumentSetOptions
                    {
                        SharedColumns = new List<IField>
                        {
                            categoriesField
                        },
                        WelcomePageColumns = new List<IField>
                        {
                            categoriesField
                        },
                        DefaultContents = new List<DocumentSetContentOptions>
                        {
                            new DocumentSetContentOptions
                            {
                                FileName = "Test.docx",
                                FolderName = "FolderName",
                                File = file,
                                ContentTypeId = documentCt.StringId
                            }
                        }
                    };

                    IDocumentSet newDocumentSet = await context.Web.ContentTypes.AddDocumentSetAsync(docSetId, "Document Set Name", "TESTING", "TESTING", documentSetOptions);

                    Assert.IsNotNull(newDocumentSet);
                    Assert.AreEqual(newDocumentSet.SharedColumns.Count, documentSetOptions.SharedColumns.Count);
                    Assert.AreEqual(newDocumentSet.WelcomePageColumns.Count, documentSetOptions.WelcomePageColumns.Count);
                    Assert.AreEqual(newDocumentSet.DefaultContents.Count, documentSetOptions.DefaultContents.Count);

                    var documentSetOptionsUpdate = new DocumentSetOptions
                    {
                        SharedColumns = new List<IField>
                        {
                            managersField
                        },
                        WelcomePageColumns = new List<IField>
                        {
                            managersField
                        },
                        DefaultContents = new List<DocumentSetContentOptions>
                        {
                            new DocumentSetContentOptions
                            {
                                FileName = "Test2.docx",
                                FolderName = "FolderName2",
                                File = file,
                                ContentTypeId = documentCt.StringId
                            }
                        }
                    };
                    newDocumentSet = await newDocumentSet.UpdateAsync(documentSetOptionsUpdate);
                    newContentType = newDocumentSet.Parent as IContentType;

                    Assert.IsNotNull(newDocumentSet);
                    Assert.AreEqual(newDocumentSet.SharedColumns.Count, documentSetOptionsUpdate.SharedColumns.Count + documentSetOptions.SharedColumns.Count);
                    Assert.AreEqual(newDocumentSet.WelcomePageColumns.Count, documentSetOptionsUpdate.WelcomePageColumns.Count + documentSetOptions.WelcomePageColumns.Count);
                    Assert.AreEqual(newDocumentSet.DefaultContents.Count, documentSetOptionsUpdate.DefaultContents.Count + documentSetOptions.DefaultContents.Count);
                }
                finally
                {
                    if (newContentType != null)
                    {
                        await newContentType.DeleteAsync();
                    }

                    if (file != null)
                    {
                        await file.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddContentTypeAsDocumentSetExceptionAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IDocumentSet newDocumentSet = await context.Web.ContentTypes.AddDocumentSetAsync("0x0101mlalalala", "Document set name");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ClientException))]
        public async Task GetContentTypeAsDocumentSetExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType contentType = (from ct in context.Web.ContentTypes
                                            where ct.StringId == "0x0101"
                                            select ct)
                            .QueryProperties(ct => ct.StringId, ct => ct.Id)
                            .FirstOrDefault();
                var documentSet = contentType.AsDocumentSet();
            }
        }

        #endregion
    }
}
