using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on getting data via REST or Microsoft Graph - used to test the core data retrieval/mapping logic of the engine.
    /// Specific domain model testing will be implemented in the domain model tests
    /// </summary>
    [TestClass]
    public class GetTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;

            // Configure the test cases to use application permissions instead of delegated permissions
            //TestCommon.Instance.UseApplicationPermissions = true;
        }

        #region Tests that use REST to hit SharePoint

        [TestMethod]
        public async Task GetSinglePropertyViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            //TestCommon.Instance.UseApplicationPermissions = true;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.WelcomePage);

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ClientException))]
        public async Task GetSingleBadPropertyViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.GetAsync(p => p.RootFolder.ServerRelativeUrl);
            }
        }

        [TestMethod]
        public async Task GetMultiplePropertiesViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.WelcomePage, p => p.Title, p => p.QuickLaunchEnabled);

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(!string.IsNullOrEmpty(web.Title));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.QuickLaunchEnabled));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.NoCrawl));
            }
        }

        [TestMethod]
        public async Task IsTheKeyfieldLoadedWhenLoadingAPropertyViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.WelcomePage);

                // Is the web keyfield property (=Id) populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(web.Id != Guid.Empty);
            }
        }

        [TestMethod]
        public async Task GetSingleExpandableCollectionViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Turn off graphfirst behaviour so that we force this one to use REST
                context.GraphFirst = false;

                var web = await context.Web.GetAsync(p => p.Lists);

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Length > 0);
                // Is the collection requested flag set
                Assert.IsTrue(web.Lists.Requested);

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
                // Are other expandable collections still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Webs));
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Fields));
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AvailableFields));
                Assert.IsFalse(web.IsPropertyAvailable(p => p.ContentTypes));
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AvailableContentTypes));
            }
        }

        [TestMethod]
        public async Task GetSingleExpandableCollectionTwiceViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Turn off graphfirst behaviour so that we force this one to use REST
                context.GraphFirst = false;

                // First load
                var web = await context.Web.GetAsync(p => p.Lists);
                Assert.IsTrue(web.Lists.Length > 0);

                var numberOfLists = web.Lists.Length;
                // Load the expandable collection again
                await context.Web.LoadAsync(p => p.Lists);

                // Loading a collection again should not result in more rows in the collection, assuming 
                // the collection has a key like is the case for lists
                Assert.AreEqual(numberOfLists, context.Web.Lists.Length);
            }
        }

        [TestMethod]
        public async Task GetMultipleExpandableCollectionViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Turn off graphfirst behaviour so that we force this one to use REST
                context.GraphFirst = false;

                var web = await context.Web.GetAsync(p => p.Lists, p => p.Webs);

                // Are the properties populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Length > 0);
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Webs));
                Assert.IsTrue(web.Webs.Length > 0);

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task GetMultiplePropertiesAndExpandableCollectionViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.WelcomePage, p => p.Title, p => p.QuickLaunchEnabled, p => p.Lists, p => p.Webs);

                // Are the properties populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(!string.IsNullOrEmpty(web.Title));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.QuickLaunchEnabled));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Length > 0);
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Webs));
                Assert.IsTrue(web.Webs.Length > 0);

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.NoCrawl));
            }
        }

        [TestMethod]
        public async Task CheckMetadataWhenPropertyViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.WelcomePage);

                // Is the metadata collection correctly populated?
                var webImplementation = web as Web;
                Assert.IsTrue(!string.IsNullOrEmpty(webImplementation.GetMetadata("type")));
                Assert.IsTrue(webImplementation.GetMetadata("type") == "SP.Web");

                //Assert.IsTrue(!string.IsNullOrEmpty(webImplementation.GetMetadata("uri")));
                //Assert.IsTrue(new Uri($"{context.Uri.ToString()}/_api/Web") == new Uri(webImplementation.GetMetadata("uri")));

                //Assert.IsTrue(!string.IsNullOrEmpty(webImplementation.GetMetadata("id")));
                //Assert.IsTrue(new Uri($"{context.Uri.ToString()}/_api/Web") == new Uri(webImplementation.GetMetadata("id")));

                Assert.IsTrue(!string.IsNullOrEmpty(webImplementation.GetMetadata("restId")));
                Assert.IsTrue(Guid.Parse(webImplementation.GetMetadata("restId")) == web.Id);

                if (context.GraphFirst)
                {
                    Assert.IsTrue(!string.IsNullOrEmpty(webImplementation.GetMetadata("graphId")));
                    Assert.IsTrue($"{context.Uri.DnsSafeHost},{context.Site.Id.ToString()},{context.Web.Id.ToString()}" == webImplementation.GetMetadata("graphId"));
                }
            }
        }

        [TestMethod]
        public async Task GetSingleModelPropertyViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var site = await context.Site.GetAsync(p => p.RootWeb);

                // Was the rootweb property loaded
                Assert.IsTrue(site.IsPropertyAvailable(p => p.RootWeb));
                // Do we we have the key property loaded on the model property
                Assert.IsTrue(site.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(site.Id != Guid.Empty);
            }
        }

        [TestMethod]
        public async Task GetSingleModelPropertyWithExpandViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var site = await context.Site.GetAsync(
                    p => p.RootWeb.QueryProperties(p => p.Title, p => p.NoCrawl,
                    p => p.Lists.QueryProperties(p => p.Title)));

                // Was the rootweb property loaded
                Assert.IsTrue(site.IsPropertyAvailable(p => p.RootWeb));
                // Do we we have the key property loaded on the model property
                Assert.IsTrue(site.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(site.Id != Guid.Empty);

                // Check the root web properties
                Assert.IsTrue(site.RootWeb.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(site.RootWeb.IsPropertyAvailable(p => p.NoCrawl));
                Assert.IsTrue(site.RootWeb.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(site.RootWeb.Lists.Length > 0);

                var firstList = site.RootWeb.Lists.AsRequested().First();
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task ExpandWithIncludeViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                var web = await context.Web.GetAsync(p => p.Title,
                                                     p => p.ContentTypes.QueryProperties(p => p.Name),
                                                     p => p.Lists.QueryProperties(p => p.Id, p => p.Title, p => p.DocumentTemplate));
                Assert.IsTrue(web.Lists.Requested);
                Assert.IsTrue(web.Lists.Length > 0);

                var firstList = web.Lists.AsRequested().First();
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.DocumentTemplate));
                Assert.IsFalse(firstList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(web.ContentTypes.Requested);
                Assert.IsTrue(web.ContentTypes.Length > 0);

                var firstWebContentType = web.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstWebContentType.IsPropertyAvailable(p => p.StringId));
                Assert.IsTrue(firstWebContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsFalse(firstWebContentType.IsPropertyAvailable(p => p.SchemaXml));
            }
        }

        [TestMethod]
        public async Task ExpandWithCollectionIncludeViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                var web = await context.Web.GetAsync(p => p.Title,
                                                     p => QueryableExtensions.QueryProperties(p.ContentTypes, p => p.Name),
                                                     p => p.Lists.QueryProperties(p => p.Id, p => p.Title, p => p.DocumentTemplate, p => p.ContentTypes));
                Assert.IsTrue(web.Lists.Requested);
                Assert.IsTrue(web.Lists.Length > 0);

                var firstList = web.Lists.AsRequested().First();
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.DocumentTemplate));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.ContentTypes));

                var firstListContentType = firstList.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstListContentType.IsPropertyAvailable(p => p.StringId));
                Assert.IsFalse(firstList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(web.ContentTypes.Requested);
                Assert.IsTrue(web.ContentTypes.Length > 0);

                var firstWebContentType = web.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstWebContentType.IsPropertyAvailable(p => p.StringId));
                Assert.IsTrue(firstWebContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsFalse(firstWebContentType.IsPropertyAvailable(p => p.SchemaXml));
            }
        }

        [TestMethod]
        public async Task ExpandRecursivelyWithCollectionIncludeViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                var web = await context.Web.GetAsync(p => p.Title,
                                                     p => p.ContentTypes.QueryProperties(p => p.Name),
                                                     p => p.Lists.QueryProperties(p => p.Id, p => p.Title, p => p.DocumentTemplate,
                                                          p => p.ContentTypes.QueryProperties(p => p.Name,
                                                               p => p.FieldLinks.QueryProperties(p => p.Name)))
                                                    );
                Assert.IsTrue(web.Lists.Requested);
                Assert.IsTrue(web.Lists.Length > 0);

                var firstList = web.Lists.AsRequested().First();
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.DocumentTemplate));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.ContentTypes));
                Assert.IsFalse(firstList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(firstList.ContentTypes.Requested);

                var firstListContentType = firstList.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstListContentType.IsPropertyAvailable(p => p.StringId));
                Assert.IsFalse(firstListContentType.IsPropertyAvailable(p => p.SchemaXml));
                Assert.IsTrue(firstListContentType.FieldLinks.Requested);

                var firstFieldLink = firstListContentType.FieldLinks.AsRequested().First();
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Id));
                Assert.IsFalse(firstFieldLink.IsPropertyAvailable(p => p.Hidden));
                Assert.IsTrue(web.ContentTypes.Requested);
                Assert.IsTrue(web.ContentTypes.Length > 0);

                var firstWebContentType = web.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstWebContentType.IsPropertyAvailable(p => p.StringId));
                Assert.IsTrue(firstWebContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsFalse(firstWebContentType.IsPropertyAvailable(p => p.SchemaXml));
            }
        }

        [TestMethod]
        public async Task ExpandRecursivelyUnorderedWithCollectionIncludeViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                var web = await context.Web.GetAsync(p => QueryableExtensions.QueryProperties(p.ContentTypes, p => p.Name),
                                                     p => p.Title,
                                                     p => p.Lists.QueryProperties(p => p.DocumentTemplate,
                                                                          p => p.ContentTypes.QueryProperties(p => p.Name, p => p.FieldLinks, p => p.NewFormUrl),
                                                                          p => p.Id, p => p.Title),
                                                     p => p.AlternateCssUrl
                                                    );
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(web.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsTrue(web.Lists.Requested);
                Assert.IsTrue(web.Lists.Length > 0);

                var firstList = web.Lists.AsRequested().First();
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.DocumentTemplate));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.ContentTypes));
                Assert.IsFalse(firstList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(firstList.ContentTypes.Requested);

                var firstListContentType = firstList.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstListContentType.IsPropertyAvailable(p => p.StringId));
                Assert.IsFalse(firstListContentType.IsPropertyAvailable(p => p.SchemaXml));
                Assert.IsTrue(firstListContentType.FieldLinks.Requested);

                var firstFieldLink = firstListContentType.FieldLinks.AsRequested().First();
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Hidden));
                Assert.IsTrue(web.ContentTypes.Requested);
                Assert.IsTrue(web.ContentTypes.Length > 0);

                var firstWebContentType = web.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstWebContentType.IsPropertyAvailable(p => p.StringId));
                Assert.IsTrue(firstWebContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsFalse(firstWebContentType.IsPropertyAvailable(p => p.SchemaXml));
            }
        }

        [TestMethod]
        public async Task GetCollectionFilterExpressionViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                // Do a get with filter and custom properties specifying the data to load
                var foundLists = await context.Web.Lists.Where(
                            p => p.TemplateType == ListTemplateType.GenericList)
                        .QueryProperties(
                            p => p.Title, p => p.TemplateType,
                            p => p.ContentTypes.QueryProperties(
                            p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                        .ToArrayAsync();

                Assert.IsTrue(foundLists.Any());

                var firstList = foundLists.First();
                Assert.IsTrue(firstList.Requested);
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.ContentTypes));
                Assert.IsTrue(firstList.ContentTypes.Requested);
                Assert.IsTrue(firstList.ContentTypes.Length > 0);

                var firstContentType = firstList.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.FieldLinks));

                var firstFieldLink = firstContentType.FieldLinks.AsRequested().First();
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Name));
            }
        }

        [TestMethod]
        public async Task GetCollectionFilterExpressionViaRestBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                // Do a get with filter and custom properties specifying the data to load
                var result = await context.Web.Lists
                    .Where(p => p.TemplateType == ListTemplateType.GenericList)
                    .QueryProperties(
                        p => p.Title, p => p.TemplateType,
                        p => p.ContentTypes.QueryProperties(
                            p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                    .AsBatchAsync();

                await context.ExecuteAsync();

                var firstList = result.First();
                Assert.IsTrue(firstList.Requested);
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.ContentTypes));
                Assert.IsTrue(firstList.ContentTypes.Requested);
                Assert.IsTrue(firstList.ContentTypes.Length > 0);

                var firstContentType = firstList.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.FieldLinks));

                var firstFieldLink = firstContentType.FieldLinks.AsRequested().First();
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Name));
            }
        }

        [TestMethod]
        public async Task GetCollectionFilterViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                // Do a get with filter and custom properties specifying the data to load
                var foundLists = await context.Web.Lists.Where(p => p.TemplateType == ListTemplateType.GenericList).ToArrayAsync();

                Assert.IsTrue(foundLists.Length > 0);

                var firstList = foundLists.First();
                Assert.IsTrue(firstList.Requested);
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task GetCollectionExpressionViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                // Load the lists in the current context
                await context.Web.LoadAsync(w => w.Lists.QueryProperties(
                        p => p.Title, p => p.TemplateType,
                        p => p.ContentTypes.QueryProperties(
                        p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name))));

                // Do a get with filter and custom properties specifying the data to load
                var foundLists = await context.Web.Lists.QueryProperties(
                        p => p.Title, p => p.TemplateType,
                        p => p.ContentTypes.QueryProperties(
                        p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                        .ToArrayAsync();

                Assert.IsTrue(foundLists.Any());
                Assert.AreEqual(foundLists.Length, context.Web.Lists.Length);
                Assert.IsTrue(context.Web.Lists.Requested);

                var firstList = context.Web.Lists.AsRequested().First();
                Assert.IsTrue(firstList.Requested);
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.ContentTypes));
                Assert.IsTrue(firstList.ContentTypes.Requested);
                Assert.IsTrue(firstList.ContentTypes.Length > 0);

                var firstContentType = firstList.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.FieldLinks));

                var firstFieldLink = firstContentType.FieldLinks.AsRequested().First();
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Name));
            }
        }

        [TestMethod]
        public async Task GetCollectionFirstFilterExpressionViaRest()
        {
            var listTitle = "Site Assets";

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                await context.Web.LoadAsync(w => w.Lists.QueryProperties(
                        p => p.Title, p => p.TemplateType,
                        p => p.ContentTypes.QueryProperties(p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name))));

                // Do a get with filter and custom properties specifying the data to load
                var foundList = await context.Web.Lists.QueryProperties(
                        p => p.Title, p => p.TemplateType,
                        p => p.ContentTypes.QueryProperties(p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                    .FirstOrDefaultAsync(p => p.Title == listTitle);

                Assert.IsTrue(foundList != null);
                Assert.AreEqual(foundList.Title, listTitle);
                Assert.IsTrue(context.Web.Lists.Requested);

                var firstList = context.Web.Lists.AsRequested().FirstOrDefault(l => l.Title == listTitle);
                Assert.IsNotNull(firstList);
                Assert.AreEqual(firstList.Id, foundList.Id);
                Assert.IsTrue(foundList.Requested);
                Assert.IsTrue(foundList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(foundList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(foundList.IsPropertyAvailable(p => p.ContentTypes));
                Assert.IsTrue(foundList.ContentTypes.Requested);
                Assert.IsTrue(foundList.ContentTypes.Length > 0);

                var firstContentType = foundList.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.FieldLinks));

                var firstFieldLink = firstContentType.FieldLinks.AsRequested().First();
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Name));
            }
        }

        [TestMethod]
        public async Task GetCollectionFirstFilterExpressionViaRestBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                // Do a get with filter and custom properties specifying the data to load
                var siteAssets = await context.Web.Lists.QueryProperties(
                        p => p.Title, p => p.TemplateType,
                        p => p.ContentTypes.QueryProperties(p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                    .Where(p => p.Title == "Site Assets")
                    .AsBatchAsync();

                var sitePages = await context.Web.Lists.QueryProperties(p => p.Title, p => p.TemplateType,
                        p => p.ContentTypes.QueryProperties(p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                    .Where(p => p.Title == "Site Pages")
                    .AsBatchAsync();

                // Batch results are not available right now
                Assert.IsFalse(siteAssets.IsAvailable);
                Assert.IsFalse(sitePages.IsAvailable);

                await context.ExecuteAsync();

                // Batch results are now available
                Assert.IsTrue(siteAssets.IsAvailable);
                Assert.IsTrue(sitePages.IsAvailable);

                Assert.IsNotNull(siteAssets);
                Assert.IsNotNull(sitePages);

                var siteAssetsList = siteAssets.AsEnumerable().First();
                var sitePagesList = sitePages.AsEnumerable().First();

                Assert.IsTrue(siteAssetsList.Requested);
                Assert.IsTrue(siteAssetsList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(siteAssetsList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(siteAssetsList.IsPropertyAvailable(p => p.ContentTypes));
                Assert.IsTrue(siteAssetsList.ContentTypes.Requested);
                Assert.IsTrue(siteAssetsList.ContentTypes.Length > 0);

                var firstContentType = siteAssetsList.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.FieldLinks));

                var firstFieldLink = firstContentType.FieldLinks.AsRequested().First();
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Name));
            }
        }

        [TestMethod]
        public async Task GetCollectionFirstFilterViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                // Do a get with filter and custom properties specifying the data to load
                var foundList = await context.Web.Lists.FirstOrDefaultAsync(p => p.Title == "Site Assets");

                Assert.IsTrue(foundList != null);
                Assert.IsTrue(foundList.Title == "Site Assets");
                Assert.IsTrue(foundList.Requested);
                Assert.IsTrue(foundList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(foundList.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task GetCollectionFirstExpressionViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                await context.Web.LoadAsync(w => w.Lists.QueryProperties(p => p.Title, p => p.TemplateType,
                        p => p.ContentTypes.QueryProperties(p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name))));

                // Do a get with filter and custom properties specifying the data to load
                var foundList = await context.Web.Lists.QueryProperties(p => p.Title, p => p.TemplateType,
                        p => p.ContentTypes.QueryProperties(p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                    .FirstOrDefaultAsync();

                Assert.IsTrue(foundList != null);
                Assert.IsTrue(context.Web.Lists.Requested);

                var firstList = context.Web.Lists.AsRequested().First();
                Assert.AreEqual(firstList.Id, foundList.Id);
                Assert.IsTrue(foundList.Requested);
                Assert.IsTrue(foundList.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsTrue(foundList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(foundList.IsPropertyAvailable(p => p.ContentTypes));
                Assert.IsTrue(foundList.ContentTypes.Requested);
                Assert.IsTrue(foundList.ContentTypes.Length > 0);

                var firstContentType = foundList.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.FieldLinks));

                var firstFieldLink = firstContentType.FieldLinks.AsRequested().First();
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Name));
            }
        }

        [TestMethod]
        public async Task ExecuteRestRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Relative url for current site
                var apiRequest = new ApiRequest(ApiRequestType.SPORest, "_api/web");
                var response = context.Web.WithSPResponseHeaders((responseHeaders) => {
                    Assert.IsTrue(!string.IsNullOrEmpty(responseHeaders["SPRequestGuid"]));
                }).ExecuteRequest(apiRequest);

                Assert.IsTrue(response.ApiRequest == apiRequest);
                Assert.IsTrue(!string.IsNullOrEmpty(response.Response));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);

                // fully qualified url for current site
                apiRequest = new ApiRequest(ApiRequestType.SPORest, $"{context.Uri}/_api/web");
                response = context.Web.ExecuteRequest(apiRequest);

                Assert.IsTrue(response.ApiRequest == apiRequest);
                Assert.IsTrue(!string.IsNullOrEmpty(response.Response));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);

                // fully qualified url for other site
                Uri testUrl = TestCommon.Instance.TestUris[TestCommon.NoGroupTestSite];
                apiRequest = new ApiRequest(ApiRequestType.SPORest, $"{testUrl}/_api/web");
                response = context.Web.ExecuteRequest(apiRequest);

                Assert.IsTrue(response.ApiRequest == apiRequest);
                Assert.IsTrue(!string.IsNullOrEmpty(response.Response));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);
            }
        }

        [TestMethod]
        public async Task ExecuteBatchRestRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Relative url for current site
                var batch = context.NewBatch();
                var webResponse = context.Web.ExecuteRequestBatch(batch, new ApiRequest(ApiRequestType.SPORest, "_api/web"));
                Assert.IsTrue(webResponse.IsAvailable == false);
                var siteResponse = context.Web.ExecuteRequestBatch(batch, new ApiRequest(ApiRequestType.SPORest, "_api/site"));
                Assert.IsTrue(siteResponse.IsAvailable == false);

                await context.ExecuteAsync(batch);

                Assert.IsTrue(webResponse.IsAvailable);
                Assert.IsFalse(string.IsNullOrEmpty(webResponse.Result.Value));
                Assert.IsTrue(siteResponse.IsAvailable);
                Assert.IsFalse(string.IsNullOrEmpty(siteResponse.Result.Value));

                // In case of a batch the there's only one SPRequestGuid for the entire batch, but it's linked to each individual request
                Assert.IsTrue(batch.Requests[0].SPRequestGuid != null);
                Assert.IsTrue(batch.Requests[1].SPRequestGuid != null);
            }
        }

        #endregion

        #region Tests that use Graph to hit SharePoint
        [TestMethod]
        public async Task GetSinglePropertyViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Description);

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Description));
                Assert.IsNotNull(web.Description);

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task GetMultiplePropertiesViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Description, p => p.Title);

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Description));
                Assert.IsNotNull(web.Description);
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(!string.IsNullOrEmpty(web.Title));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.NoCrawl));
            }
        }

        [TestMethod]
        public async Task IsTheKeyfieldLoadedWhenLoadingAPropertyViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Title);

                // Is the web keyfield property (=Id) populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(web.Id != Guid.Empty);
            }
        }

        [TestMethod]
        public async Task GetSingleExpandableCollectionViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Length > 0);
                // Is the collection requested flag set
                Assert.IsTrue(web.Lists.Requested);

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
                // Are other expandable collections still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Webs));
            }
        }

        [TestMethod]
        public async Task GetSingleExpandableCollectionTwiceViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // First load
                var web = await context.Web.GetAsync(p => p.Lists);
                Assert.IsTrue(web.Lists.Length > 0);

                var numberOfLists = web.Lists.Length;
                // Load the expandable collection again
                await context.Web.LoadAsync(p => p.Lists);

                // Loading a collection again should not result in more rows in the collection, assuming 
                // the collection has a key like is the case for lists
                Assert.AreEqual(numberOfLists, context.Web.Lists.Length);
            }
        }

        [TestMethod]
        public async Task GetSinglePropertyViaGraphOnly()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Description);

                // Is the property populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Description));

                // Are other properties not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.InternalId));
                // Are other collections not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.Owners));
                // Are other complex models not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.FunSettings));
            }
        }

        [TestMethod]
        public async Task GetMultiplePropertiesViaGraphOnly()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Description, p => p.InternalId);

                // Is the property populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(team.IsPropertyAvailable(p => p.InternalId));

                // Are other properties not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.DisplayName));
                // Are other collections not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.Owners));
                // Are other complex models not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.FunSettings));
            }
        }

        [TestMethod]
        public async Task GetMultiplePropertiesAndComplexModelsViaGraphOnly()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Description, p => p.InternalId, p => p.FunSettings, p => p.DiscoverySettings);

                // Is the property populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(team.IsPropertyAvailable(p => p.InternalId));
                // Are complex model property loaded
                Assert.IsTrue(team.IsPropertyAvailable(p => p.FunSettings));
                var allowGiphy = team.FunSettings.AllowGiphy;
                Assert.IsTrue(allowGiphy == true || allowGiphy == false);

                Assert.IsTrue(team.IsPropertyAvailable(p => p.DiscoverySettings));

                // Are other properties not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.DisplayName));
                // Are other collections not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.Owners));
                // Are other complex models not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.GuestSettings));
            }
        }

        [TestMethod]
        public async Task GetMultiplePropertiesComplexModelsAndCollectionsViaGraphOnly()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Description, p => p.InternalId, p => p.FunSettings, p => p.DiscoverySettings, p => p.InstalledApps, p => p.Members);

                // Is the property populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(team.IsPropertyAvailable(p => p.InternalId));
                // Are complex model property loaded
                Assert.IsTrue(team.IsPropertyAvailable(p => p.FunSettings));
                Assert.IsTrue(team.IsPropertyAvailable(p => p.DiscoverySettings));
                // Are collections available and populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.InstalledApps));
                Assert.IsTrue(team.InstalledApps.Length > 0);
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Members));
                Assert.IsTrue(team.Members.Length > 0);

                // Are other properties not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.DisplayName));
                // Are other collections not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.Owners));
                // Are other complex models not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.GuestSettings));
            }
        }

        // we don't have by default expandable collections at this point
        //[TestMethod]
        //public async Task GetExpandedByDefaultCollectionViaGraphOnly()
        //{
        //    // TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var team = await context.Team.GetAsync();

        //        // Are collections available and populated
        //        Assert.IsTrue(team.IsPropertyAvailable(p => p.Channels));
        //        Assert.IsTrue(team.Channels.Length > 0);
        //        // Are other collections not loaded
        //        Assert.IsFalse(team.IsPropertyAvailable(p => p.Owners));
        //    }
        //}

        [TestMethod]
        public async Task LoadViaGraph()
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                TeamChannel teamChannel = new TeamChannel()
                {
                    PnPContext = context
                };

                string newQuery = "";
                string query = "";

                // Selecting one regular property on expand
                (newQuery, query) = BuildGraphExpandSelect(new Expression<Func<ITeamChannel, object>>[] { p => p.Tabs.QueryProperties(p => p.DisplayName) },
                                               new TeamChannel() { PnPContext = context });
                Assert.IsTrue(query.Equals("($select%3DdisplayName,id)", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(newQuery.Equals("$select=displayName,id", StringComparison.InvariantCultureIgnoreCase));

                // Selecting multiple regular properties on expand
                (newQuery, query) = BuildGraphExpandSelect(new Expression<Func<ITeamChannel, object>>[] { p => p.Tabs.QueryProperties(p => p.DisplayName, p => p.WebUrl) },
                                               new TeamChannel() { PnPContext = context });
                Assert.IsTrue(query.Equals("($select%3DdisplayName,weburl,id)", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(newQuery.Equals("$select=displayName,weburl,id", StringComparison.InvariantCultureIgnoreCase));

                // Selecting multiple regular properties on expand including a complex type
                (newQuery, query) = BuildGraphExpandSelect(new Expression<Func<ITeamChannel, object>>[] { p => p.Tabs.QueryProperties(p => p.DisplayName, p => p.WebUrl, p => p.Configuration) },
                                               new TeamChannel() { PnPContext = context });
                Assert.IsTrue(query.Equals("($select%3DdisplayName,weburl,configuration,id)", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(newQuery.Equals("$select=displayName,weburl,configuration,id", StringComparison.InvariantCultureIgnoreCase));

                // Selecting multiple regular properties on expand including a complex type and DataModel type (non expandable)
                (newQuery, query) = BuildGraphExpandSelect(new Expression<Func<ITeamChannel, object>>[] { p => p.Tabs.QueryProperties(p => p.DisplayName, p => p.WebUrl, p => p.Configuration, p => p.TeamsApp) },
                                               new TeamChannel() { PnPContext = context });
                Assert.IsTrue(query.Equals("($select%3DdisplayName,weburl,configuration,teamsapp,id)", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(newQuery.Equals("$select=displayName,weburl,configuration,teamsapp,id", StringComparison.InvariantCultureIgnoreCase));

                // Selecting multiple regular properties on expand including a complex type and DataModel type (non expandable)
                (newQuery, query) = BuildGraphExpandSelect(new Expression<Func<ITeam, object>>[] { p => p.PrimaryChannel.QueryProperties(p => p.DisplayName) },
                                               new Team() { PnPContext = context });
                Assert.IsTrue(query.Equals("($select%3DdisplayName,id)", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(newQuery.Equals("$select=displayName,id", StringComparison.InvariantCultureIgnoreCase));

                /* commenting out as Lists are loaded via an additional query in the batch 
                // Selecting expandable property on expand
                (newQuery, query) = BuildGraphExpandSelect(new Expression<Func<IWeb, object>>[] { p => p.Lists.Load(p => p.Title, p=>p.Items.Load(p=>p.Id)) },
                                               new Web() { PnPContext = context });
                Assert.IsTrue(query.Equals("($select%3DdisplayName,id;$expand%3Ditems($select%3Did))", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(newQuery.Equals("$select=displayName,id;$expand%3Ditems($select%3Did)", StringComparison.InvariantCultureIgnoreCase));

                // Selecting expandable property on expand + more fields
                (newQuery, query) = BuildGraphExpandSelect(new Expression<Func<IWeb, object>>[] { p => p.Lists.Load(p => p.Title, p => p.Items.Load(p => p.Id) , p=>p.Description ) },
                                               new Web() { PnPContext = context });
                Assert.IsTrue(query.Equals("($select%3DdisplayName,description,id;$expand%3Ditems($select%3Did))", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(newQuery.Equals("$select=displayName,description,id;$expand%3Ditems($select%3Did)", StringComparison.InvariantCultureIgnoreCase));
                */
            }
        }

        [TestMethod]
        public async Task LoadTeamPropertiesViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Expand for a property that is implemented using it's own graph query
                // Url for loading channels is teams/{Site.GroupId}/channels
                var team = await context.Team.GetAsync(
                    p => p.Channels.QueryProperties(p => p.DisplayName));

                foreach (var channel in team.Channels.AsRequested())
                {
                    Assert.IsTrue(channel.IsPropertyAvailable(p => p.DisplayName));
                    Assert.IsTrue(!string.IsNullOrEmpty(channel.DisplayName));
                    Assert.IsFalse(channel.IsPropertyAvailable(p => p.Description));
                }

                // Expand for a property that is implemented using it's own graph query which has url parameters defined and which uses a JsonPath
                // Url for loading installed apps is teams/{Site.GroupId}/installedapps?$expand=TeamsApp
                team = await context.Team.GetAsync(p => p.InstalledApps.QueryProperties(p => p.DistributionMethod));

                foreach (var installedApp in team.InstalledApps.AsRequested())
                {
                    Assert.IsTrue(installedApp.IsPropertyAvailable(p => p.DistributionMethod));
                    Assert.IsFalse(installedApp.IsPropertyAvailable(p => p.DisplayName));
                }
            }
        }

        [TestMethod]
        public async Task LoadTeamPropertiesWithExpandViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Expand for an expandable property 
                var team = await context.Team.GetAsync(p => p.PrimaryChannel.QueryProperties(p => p.DisplayName));

                Assert.IsTrue(team.IsPropertyAvailable(p => p.PrimaryChannel));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.DisplayName));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ClientException))]
        public async Task LoadTeamPropertiesWithExpandExceptionViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Expand for an expandable property using a property that depends it's own graph query ==> 
                // Not supported in Graph
                var team = await context.Team.GetAsync(p => p.PrimaryChannel.QueryProperties(p => p.DisplayName, p => p.Messages));
            }
        }

        [TestMethod]
        public async Task LoadTeamPropertiesComplexViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get primary channel
                await context.Team.LoadAsync(p => p.PrimaryChannel);

                // Load tabs with only the WebUrl property 
                // Special case since the listed url is teams/{Site.GroupId}/channels/{GraphId}/tabs?$expand=teamsApp
                await context.Team.PrimaryChannel.LoadAsync(p => p.Tabs.QueryProperties(p => p.WebUrl));

                foreach (var tab in context.Team.PrimaryChannel.Tabs.AsRequested())
                {
                    Assert.IsTrue(tab.IsPropertyAvailable(p => p.WebUrl));
                    Assert.IsFalse(tab.IsPropertyAvailable(p => p.DisplayName));
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ClientException))]
        public async Task LoadTeamPropertiesRecursiveViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Team.GetAsync(
                    p => p.Channels.QueryProperties(
                        p => p.Messages.QueryProperties(p => p.Body)));
            }
        }


        private Tuple<string, string> BuildGraphExpandSelect<TModel>(Expression<Func<TModel, object>>[] testExpression, BaseDataModel<TModel> instance)
        {
            var entityInfo = EntityManager.GetClassInfo(instance.GetType(), instance, expressions: testExpression);
            var expandProperty = entityInfo.Fields.FirstOrDefault(p => p.ExpandFieldInfo != null);
            StringBuilder sb = new StringBuilder();
            QueryClient.AddExpandableSelectGraph(true, sb, expandProperty, null, "");
            var newQuery = sb.ToString();

            sb = new StringBuilder();
            QueryClient.AddExpandableSelectGraph(false, sb, expandProperty, null, "");
            var query = sb.ToString();

            return new Tuple<string, string>(newQuery, query);
        }

        [TestMethod]
        public async Task ExecuteGraphRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var apiRequest = new ApiRequest(ApiRequestType.Graph, "me");
                var response = context.Team.ExecuteRequest(apiRequest);

                Assert.IsTrue(response.ApiRequest == apiRequest);
                Assert.IsTrue(!string.IsNullOrEmpty(response.Response));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);

                apiRequest = new ApiRequest(ApiRequestType.Graph, "/me");
                response = context.Team.ExecuteRequest(apiRequest);

                Assert.IsTrue(response.ApiRequest == apiRequest);
                Assert.IsTrue(!string.IsNullOrEmpty(response.Response));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);
            }
        }

        [TestMethod]
        public async Task ExecuteGraphRequestWithCustomHeaders()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Dictionary<string, string> extraHeaders = new()
                {
                    { "random", "random" },
                    { "random2", "random2" }
                };

                var apiRequest = new ApiRequest(HttpMethod.Get, ApiRequestType.Graph, "me", null, extraHeaders);
                var response = context.Team.ExecuteRequest(apiRequest);

                Assert.IsTrue(response.ApiRequest == apiRequest);
                Assert.IsTrue(!string.IsNullOrEmpty(response.Response));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);
            }
        }

        [TestMethod]
        public async Task ExecuteGraphSingleBatchRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Relative url for current site
                var batch = context.NewBatch();
                var drivesResponse = context.Web.ExecuteRequestBatch(batch, new ApiRequest(ApiRequestType.Graph, "drives"));
                Assert.IsTrue(drivesResponse.IsAvailable == false);
                await context.ExecuteAsync(batch);

                Assert.IsTrue(drivesResponse.IsAvailable);
                Assert.IsFalse(string.IsNullOrEmpty(drivesResponse.Result.Value));
            }
        }

        [TestMethod]
        public async Task ExecuteGraphBatchRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Relative url for current site
                var batch = context.NewBatch();
                var meResponse = context.Web.ExecuteRequestBatch(batch, new ApiRequest(ApiRequestType.Graph, "me"));
                Assert.IsTrue(meResponse.IsAvailable == false);
                var drivesResponse = context.Web.ExecuteRequestBatch(batch, new ApiRequest(ApiRequestType.Graph, "drives"));
                Assert.IsTrue(drivesResponse.IsAvailable == false);

                await context.ExecuteAsync(batch);

                Assert.IsTrue(meResponse.IsAvailable);
                Assert.IsFalse(string.IsNullOrEmpty(meResponse.Result.Value));
                Assert.IsTrue(drivesResponse.IsAvailable);
                Assert.IsFalse(string.IsNullOrEmpty(drivesResponse.Result.Value));
            }
        }

        [TestMethod]
        public async Task ExecuteGraphBatchRequestWithErrors()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Relative url for current site
                var batch = context.NewBatch();
                var meResponse = context.Web.ExecuteRequestBatch(batch, new ApiRequest(ApiRequestType.Graph, "me"));
                Assert.IsTrue(meResponse.IsAvailable == false);
                var drivesResponse = context.Web.ExecuteRequestBatch(batch, new ApiRequest(ApiRequestType.Graph, "thiswillgiveanerror"));
                Assert.IsTrue(drivesResponse.IsAvailable == false);

                var batchErrors = await context.ExecuteAsync(batch, false);

                Assert.IsTrue(meResponse.IsAvailable);
                Assert.IsFalse(string.IsNullOrEmpty(meResponse.Result.Value));

                // The last request in our batch failed!
                var driveBatchResult = batchErrors.First();
                Assert.IsTrue(driveBatchResult.StatusCode != System.Net.HttpStatusCode.OK);
                Assert.IsTrue(drivesResponse.IsAvailable);
                Assert.IsTrue(string.IsNullOrEmpty(drivesResponse.Result.Value));
            }
        }

        #endregion
    }
}
