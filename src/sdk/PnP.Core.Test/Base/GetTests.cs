using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        #region Tests that use REST to hit SharePoint

        [TestMethod]
        public async Task GetSinglePropertyViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
        public async Task GetMultiplePropertiesViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            }
        }

        [TestMethod]
        public async Task GetSingleExpandableCollectionTwiceViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Turn off graphfirst behaviour so that we force this one to use REST
                context.GraphFirst = false;

                // First load
                var web = await context.Web.GetAsync(p => p.Lists);
                Assert.IsTrue(web.Lists.Length > 0);

                var numberOfLists = web.Lists.Length;
                // Load the expandable collection again
                await context.Web.GetAsync(p => p.Lists);
                
                // Loading a collection again should not result in more rows in the collection, assuming 
                // the collection has a key like is the case for lists
                Assert.IsTrue(numberOfLists == context.Web.Lists.Length);
            }
        }

        [TestMethod]
        public async Task GetMultipleExpandableCollectionViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.WelcomePage);

                // Is the metadata collection correctly populated?
                var webImplementation = web as Web;
                Assert.IsTrue(!string.IsNullOrEmpty(webImplementation.GetMetadata("type")));
                Assert.IsTrue(webImplementation.GetMetadata("type") == "SP.Web");
                
                Assert.IsTrue(!string.IsNullOrEmpty(webImplementation.GetMetadata("uri")));
                //Assert.IsTrue(new Uri($"{context.Uri.ToString()}/_api/Web") == new Uri(webImplementation.GetMetadata("uri")));

                Assert.IsTrue(!string.IsNullOrEmpty(webImplementation.GetMetadata("id")));
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var site = await context.Site.GetAsync(p => p.RootWeb);

                // Was the rootweb model property loaded
                Assert.IsTrue(context.Site.IsPropertyAvailable(p => p.RootWeb));
                // Do we we have the key property loaded on the model property
                Assert.IsTrue(site.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(site.Id != Guid.Empty);
            }
        }
        #endregion

        #region Tests that use Graph to hit SharePoint
        [TestMethod]
        public async Task GetSinglePropertyViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Description);

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(!string.IsNullOrEmpty(web.Description));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task GetMultiplePropertiesViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Description, p => p.Title);

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(!string.IsNullOrEmpty(web.Description));
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // First load
                var web = await context.Web.GetAsync(p => p.Lists);
                Assert.IsTrue(web.Lists.Length > 0);

                var numberOfLists = web.Lists.Length;
                // Load the expandable collection again
                await context.Web.GetAsync(p => p.Lists);

                // Loading a collection again should not result in more rows in the collection, assuming 
                // the collection has a key like is the case for lists
                Assert.IsTrue(numberOfLists == context.Web.Lists.Length);
            }
        }

        [TestMethod]
        public async Task GetSinglePropertyViaGraphOnly()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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

        [TestMethod]
        public async Task GetExpandedByDefaultCollectionViaGraphOnly()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync();

                // Are collections available and populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Channels));
                Assert.IsTrue(team.Channels.Length > 0);
                // Are other collections not loaded
                Assert.IsFalse(team.IsPropertyAvailable(p => p.Owners));
            }
        }
        #endregion
    }
}
