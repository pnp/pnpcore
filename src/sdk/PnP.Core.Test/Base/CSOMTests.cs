using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.QueryModel;
using System.Collections.Generic;
using PnP.Core.Test.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Requests;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Test cases for the BatchClient class
    /// </summary>
    [TestClass]
    public class CSOMTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext testContext)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
            //TestCommon.Instance.GenerateMockingDebugFiles = true;
        }

        
        [TestMethod]
        public async Task SimplePropertyRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web;

                // Get the title value via non CSOM
                await web.EnsurePropertiesAsync(p => p.Title);

                var apiCall = new ApiCall(new List<IRequest<object>>() { new GetTitleRequest() });

                var response = await (web as Web).RawRequestAsync(apiCall, HttpMethod.Post);

                Assert.IsTrue((response.ApiCall.CSOMRequests[0].Result as IWeb).Title == web.Title);
            }
        }
        
        [TestMethod]
        public async Task MultipleSimplePropertyRequests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web;

                // Get the title value via non CSOM
                await web.EnsurePropertiesAsync(p => p.Title, p => p.Description);

                var apiCall1 = new ApiCall(new List<IRequest<object>>() { new GetTitleRequest() });
                var apiCall2 = new ApiCall(new List<IRequest<object>>() { new GetDescriptionRequest() });

                var batch = context.BatchClient.EnsureBatch();
                await (web as Web).RawRequestBatchAsync(batch, apiCall1, HttpMethod.Post);
                await (web as Web).RawRequestBatchAsync(batch, apiCall2, HttpMethod.Post);
                await context.ExecuteAsync(batch);

                var response1 = batch.Requests.First().Value;
                var response2 = batch.Requests.Last().Value;

                Assert.IsTrue((response1.ApiCall.CSOMRequests[0].Result as IWeb).Title == web.Title);
                Assert.IsTrue((response2.ApiCall.CSOMRequests[0].Result as IWeb).Description == web.Description);
            }
        }

        [TestMethod]
        public async Task CSOMPartitySubSequentModelLoads()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Subsequent loads of the same model will "enrich" the loaded model
                await context.Web.LoadAsync(p => p.Title);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.MasterUrl));

                await context.Web.LoadAsync(p => p.MasterUrl, p => p.AlternateCssUrl);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.CustomMasterUrl));
            }
        }

        [TestMethod]
        public async Task CSOMPartitySubSequentModelLoads2()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Subsequent loads of the same model will "enrich" the loaded model:
                // Loading another child model with default properties
                await context.Web.LoadAsync(p => p.Title);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.MasterUrl));

                await context.Web.LoadAsync(p => p.AssociatedOwnerGroup, p => p.AlternateCssUrl);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AssociatedOwnerGroup));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.CustomMasterUrl));

                Assert.IsTrue(context.Web.AssociatedOwnerGroup.Requested);
                Assert.IsTrue(context.Web.AssociatedOwnerGroup.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task CSOMPartitySubSequentModelLoads3()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Subsequent loads of the same model will "enrich" the loaded model:
                // Loading another child model via subsequent loads ==> CSOM does
                // not support this capability for child model loads, but it makes 
                // sense to behave identical to regular subsequent model loads
                await context.Web.LoadAsync(p => p.Title);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.MasterUrl));

                await context.Web.LoadAsync(p => p.AssociatedOwnerGroup.QueryProperties(p => p.Title), p => p.AlternateCssUrl);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AssociatedOwnerGroup));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.CustomMasterUrl));
                Assert.IsTrue(context.Web.AssociatedOwnerGroup.Requested);
                Assert.IsTrue(context.Web.AssociatedOwnerGroup.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(context.Web.AssociatedOwnerGroup.IsPropertyAvailable(p => p.Description));

                await context.Web.LoadAsync(p => p.AssociatedOwnerGroup.QueryProperties(p => p.Description, p => p.LoginName), p => p.AlternateCssUrl);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AssociatedOwnerGroup));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.CustomMasterUrl));
                Assert.IsTrue(context.Web.AssociatedOwnerGroup.Requested);
                Assert.IsTrue(context.Web.AssociatedOwnerGroup.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(context.Web.AssociatedOwnerGroup.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(context.Web.AssociatedOwnerGroup.IsPropertyAvailable(p => p.LoginName));
                Assert.IsFalse(context.Web.AssociatedOwnerGroup.IsPropertyAvailable(p => p.AllowMembersEditMembership));
            }
        }

        [TestMethod]
        public async Task CSOMPartitySubSequentModelLoads4()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Subsequent loads of the same model will "enrich" the loaded model:
                // Loading another model collection with default properties
                await context.Web.LoadAsync(p => p.Title);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.MasterUrl));

                await context.Web.LoadAsync(p => p.Lists, p => p.AlternateCssUrl);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.CustomMasterUrl));

                Assert.IsTrue(context.Web.Lists.Requested);
                Assert.IsTrue(context.Web.Lists.AsRequested().First().IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task CSOMPartitySubSequentModelLoads5()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Subsequent loads of the same model will "enrich" the loaded model:
                // Loading another model collection with specific properties
                await context.Web.LoadAsync(p => p.Title);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.MasterUrl));

                await context.Web.LoadAsync(p => p.Lists.QueryProperties(p => p.Title), p => p.AlternateCssUrl);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.CustomMasterUrl));

                Assert.IsTrue(context.Web.Lists.Requested);
                Assert.IsTrue(context.Web.Lists.AsRequested().First().IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(context.Web.Lists.AsRequested().First().IsPropertyAvailable(p => p.DefaultEditFormUrl));
            }
        }

        [TestMethod]
        public async Task CSOMPartitySubSequentModelLoads6()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Subsequent loads of the same model will "enrich" the loaded model:
                // Loading another model collection with specific properties followed by
                // a subsequent load requesting other properties ==> in this case the initially loaded collection model 
                // is replaced with the freshly loaded one, using the properties coming with the new one
                await context.Web.LoadAsync(p => p.Title);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.MasterUrl));

                await context.Web.LoadAsync(p => p.Lists.QueryProperties(p => p.Title), p => p.AlternateCssUrl);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.CustomMasterUrl));

                Assert.IsTrue(context.Web.Lists.Requested);
                Assert.IsTrue(context.Web.Lists.AsRequested().First().IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(context.Web.Lists.AsRequested().First().IsPropertyAvailable(p => p.DefaultEditFormUrl));

                await context.Web.LoadAsync(p => p.Lists.QueryProperties(p => p.Description), p => p.MasterUrl);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.CustomMasterUrl));

                Assert.IsTrue(context.Web.Lists.Requested);
                Assert.IsTrue(context.Web.Lists.AsRequested().First().IsPropertyAvailable(p => p.Description));
                Assert.IsFalse(context.Web.Lists.AsRequested().First().IsPropertyAvailable(p => p.DefaultEditFormUrl));
            }
        }

        [TestMethod]
        public async Task CSOMUpdateWebPropertyBag()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.ClassicSTS0TestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.ClassicSTS0TestSite))
            {
                context.Web.AllProperties["TestPnPProperty"] = "TestPropertyValue";
                context.Web.AllProperties.Update();
                Assert.AreEqual("TestPropertyValue", context.Web.AllProperties.GetString("TestPnPProperty", ""));
            }
        }

        [TestMethod]
        public async Task CSOMBatching()
        {
            //TestCommon.Instance.Mocking = false;            

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = TestCommon.GetPnPSdkTestAssetName("CSOMBatching");
                var list = web.Lists.AsRequested().FirstOrDefault(p => p.Title == listTitle);

                if (list != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    list = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Add items
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new()
                    {
                        { "Title", $"Item {i}" }
                    };

                    await list.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                // update first 4 items
                var firstItems = list.Items.Take(4).ToList();
                foreach(var item in firstItems)
                {
                    item.Title = "Updated!";
                    // SytemUpdate uses a CSOM call under the covers
                    await item.SystemUpdateBatchAsync();
                }

                // Execute the batch
                await context.ExecuteAsync();

                firstItems = list.Items.Take(4).ToList();
                foreach (var item in firstItems)
                {
                    Assert.AreEqual(item.Title, "Updated!"); 
                }

                await list.DeleteAsync();
            }
        }
    }
}
