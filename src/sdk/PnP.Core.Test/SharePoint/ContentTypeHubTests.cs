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
        public async Task GetContentTypesFromHubAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.ContentTypeHub.LoadAsync(y => y.ContentTypes);

                Assert.IsNotNull(context.ContentTypeHub.ContentTypes);
            }
        }

        [TestMethod]
        public async Task AddContentTypeToHubAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", "AddContentTypeToHubAsyncTest", "TESTING", "TESTING");

                // Test the created object
                Assert.IsNotNull(newContentType);
                Assert.AreEqual("0x0100302EF0D1F1DB4C4EBF58251BCCF5966F", newContentType.StringId);
                Assert.AreEqual("AddContentTypeToHubAsyncTest", newContentType.Name);
                Assert.AreEqual("TESTING", newContentType.Description);
                Assert.AreEqual("TESTING", newContentType.Group);

                await context.ContentTypeHub.LoadAsync(y => y.ContentTypes);

                var matchingCt = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(y => y.Id == newContentType.StringId);

                Assert.IsNotNull(matchingCt);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishContentTypeAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5959D", "IsContentTypePublished", "TESTING", "TESTING");

                var isPublished = await newContentType.IsPublishedAsync();

                Assert.IsFalse(isPublished);

                await newContentType.PublishAsync();

                isPublished = await newContentType.IsPublishedAsync();

                Assert.IsTrue(isPublished);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UnpublishContentTypeAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968D", "TEST ADD Unpublish", "TESTING", "TESTING");

                // First we have to publish the content type

                await newContentType.PublishAsync();

                var isPublished = await newContentType.IsPublishedAsync();

                Assert.IsTrue(isPublished);

                await newContentType.UnpublishAsync();

                isPublished = await newContentType.IsPublishedAsync();

                Assert.IsFalse(isPublished);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CheckIfContentTypeIsPublishedAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968A", "TEST ADD IsPublished", "TESTING", "TESTING");

                var isPublished = await newContentType.IsPublishedAsync();

                Assert.IsFalse(isPublished);

                await newContentType.PublishAsync();

                isPublished = await newContentType.IsPublishedAsync();

                Assert.IsTrue(isPublished);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateContentTypeAndAddFieldTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5961A", "CreateContentTypeC", "TESTING", "TESTING");

                IField newField = await context.ContentTypeHub.Fields.AddBooleanAsync("TestField", new FieldBooleanOptions {});

                await newContentType.AddFieldAsync(newField);
                await newContentType.LoadAsync(y => y.Fields);

                // Check if field is created on ct
                var matchingField = newContentType.Fields.AsRequested().FirstOrDefault(y => y.Id == newField.Id);

                Assert.IsNotNull(matchingField);

                // Clean up
                await newContentType.DeleteAsync();
                await newField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UnpublishContentTypeAsyncExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968D", "TEST ADD Unpublish", "TESTING", "TESTING");

                await Assert.ThrowsExceptionAsync<Exception>(async () =>
                {
                    await newContentType.UnpublishAsync();
                });

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishContentTypeAsyncTargetExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968D", "TEST ADD Publish", "TESTING", "TESTING");

                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                {
                    await newContentType.PublishAsync();
                });

                await newContentType.DeleteAsync();
            }
        }

        #region Document Sets

        // Ensure the document set site collection feature is enabled before running test tests live

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

                IDocumentSet newDocumentSet = await context.ContentTypeHub.ContentTypes.AddDocumentSetAsync(docSetId, "Document set name", "TESTING", "TESTING", documentSetOptions);
                IContentType newContentType = newDocumentSet.Parent as IContentType;
                // Test the created object
                Assert.IsNotNull(newContentType);
                Assert.IsNotNull(newDocumentSet);

                Assert.AreEqual(newDocumentSet.SharedColumns.Count, documentSetOptions.SharedColumns.Count);
                Assert.AreEqual(newDocumentSet.WelcomePageColumns.Count, documentSetOptions.WelcomePageColumns.Count);
                Assert.AreEqual(newDocumentSet.AllowedContentTypes.Count, documentSetOptions.AllowedContentTypes.Count);
                Assert.AreEqual(newDocumentSet.ShouldPrefixNameToFile, documentSetOptions.ShouldPrefixNameToFile);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateContentTypeAsDocumentSet()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitle("Documents").GetAsync();
                var rootFolder = await list.RootFolder.GetAsync();

                (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(parentFolder: rootFolder);

                var categoriesField = await context.Web.Fields.FirstAsync(y => y.InternalName == "Categories").ConfigureAwait(false);
                var managersField = await context.Web.Fields.FirstAsync(y => y.InternalName == "ManagersName").ConfigureAwait(false);
                var documentCt = await context.Web.ContentTypes.FirstAsync(y => y.Name == "Document").ConfigureAwait(false);

                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

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
                IContentType newContentType = newDocumentSet.Parent as IContentType;

                Assert.IsNotNull(newDocumentSet);
                Assert.AreEqual(newDocumentSet.SharedColumns.Count, documentSetOptionsUpdate.SharedColumns.Count + documentSetOptions.SharedColumns.Count);
                Assert.AreEqual(newDocumentSet.WelcomePageColumns.Count, documentSetOptionsUpdate.WelcomePageColumns.Count + documentSetOptions.WelcomePageColumns.Count);
                Assert.AreEqual(newDocumentSet.DefaultContents.Count, documentSetOptionsUpdate.DefaultContents.Count + documentSetOptions.DefaultContents.Count);

                await newContentType.DeleteAsync();
                await file.DeleteAsync();
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
