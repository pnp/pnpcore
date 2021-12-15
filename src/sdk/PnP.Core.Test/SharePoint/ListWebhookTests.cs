using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ListWebhookTests
    {
        // function app hosted to allow live test runs
        private const string webhookHost = "https://pnpcoresdkwebhooktest.azurewebsites.net";

        #region Instructions for online tests with local function app
        //private const string webhookHost = "https://6e4569a36d94.ngrok.io";

        /*        
        1. Deploy an Azure Function host containing below 2 functions with anonymous access
        2. Launch the function on localhost port 7071
        3. Launch ngrok: ngrok http 7071 --host-header=localhost
        4. Update the ngrok url in the webhookHost variable

        public static class Function1
        {
	        [Function("HandleEvent")]
	        public static HttpResponseData RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
		        FunctionContext executionContext)
	        {
		        var logger = executionContext.GetLogger(nameof(Function1));
		        logger.LogInformation("C# HTTP trigger function processed a request.");

		        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
		        var token = query["validationtoken"];

		        if (!string.IsNullOrEmpty(token))
		        {
			        var tokenResponse = req.CreateResponse(HttpStatusCode.OK);
			        tokenResponse.Headers.Add("Content-Type", "text/plain");
			        tokenResponse.WriteString(token);
			        return tokenResponse;
		        }

		        var response = req.CreateResponse(HttpStatusCode.OK);
		        response.Headers.Add("Content-Type", "text/plain");
		        return response;
	        }

	        [Function("HandleEventNew")]
	        public static HttpResponseData RunAsync2([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
		        FunctionContext executionContext)
	        {
		        var logger = executionContext.GetLogger(nameof(Function1));
		        logger.LogInformation("C# HTTP trigger function processed a request.");

		        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
		        var token = query["validationtoken"];

		        if (!string.IsNullOrEmpty(token))
		        {
			        var tokenResponse = req.CreateResponse(HttpStatusCode.OK);
			        tokenResponse.Headers.Add("Content-Type", "text/plain");
			        tokenResponse.WriteString(token);
			        return tokenResponse;
		        }

		        var response = req.CreateResponse(HttpStatusCode.OK);
		        response.Headers.Add("Content-Type", "text/plain");

		        return response;
	        }
        }
        */
        #endregion

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task CreateNewWebhookTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1), "state");

                Assert.IsTrue(webhook.ClientState.Equals("state"));
            }
        }

        [TestMethod]
        public async Task CreateNewWebhookDefaultBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.AddBatch($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1), "state");
                await context.ExecuteAsync();

                Assert.IsTrue(webhook.ClientState.Equals("state"));
            }
        }

        [TestMethod]
        public async Task CreateNewWebhookCustomBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var batch = context.NewBatch();
                var webhook = list.Webhooks.AddBatch(batch, $"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1), "state");
                await context.ExecuteAsync(batch);

                Assert.IsTrue(webhook.ClientState.Equals("state"));
            }
        }

        [TestMethod]
        public async Task CreateNewWebhookWithoutstateTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1));

                Assert.IsTrue(webhook != null);
            }
        }

        [TestMethod]
        public async Task GetAllWebhooksTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents", l => l.Webhooks);

                Assert.IsTrue(list.Webhooks.AsRequested().Count() > 0);
            }
        }

        [TestMethod]
        public async Task GetAllWebhooksLoadTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                list.Webhooks.Load();

                Assert.IsTrue(list.Webhooks.AsRequested().Count() > 0);
            }
        }

        [TestMethod]
        public async Task GetAllWebhooksLoadListTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                list.Load(l => l.Webhooks);

                Assert.IsTrue(list.Webhooks.AsRequested().Count() > 0);
            }
        }

        [TestMethod]
        public async Task RemoveWebhookTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1));
                list.Webhooks.Load();
                var count = list.Webhooks.AsRequested().Count();

                webhook.Delete();

                list.Webhooks.Load();
                var count2 = list.Webhooks.AsRequested().Count();
                Assert.AreEqual(count - 1, count2);
            }
        }

        [TestMethod]
        public async Task UpdateWebhookUrlTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1));

                webhook.NotificationUrl = $"{webhookHost}/api/HandleEventNew";

                webhook.Update();

                var wh = webhook.Get();

                Assert.IsTrue(wh.NotificationUrl.Contains("HandleEventNew"));
            }
        }

        [TestMethod]
        public async Task GetWebhookByIdTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1));
                var wh = list.Webhooks.GetById(webhook.Id);

                Assert.IsTrue(wh.NotificationUrl == $"{webhookHost}/api/HandleEvent");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task WebhookArgumentOutOfRangeTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddMonths(7));

                Assert.Fail("Should throw expected exception");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WebhookNotificationNullExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add(string.Empty, DateTime.UtcNow.AddMonths(1));

                Assert.Fail("Should throw expected exception");
            }
        }

        [TestMethod]
        public async Task CreateNewWebhookWithMonthsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", 1, "state");

                Assert.IsTrue(webhook != null);
                Assert.IsTrue(webhook.ClientState.Equals("state"));
            }
        }

        [TestMethod]
        public async Task CreateNewWebhookWithMonthsDefaultBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.AddBatch($"{webhookHost}/api/HandleEvent", 1, "state");

                context.Execute();

                Assert.IsTrue(webhook != null);
                Assert.IsTrue(webhook.ClientState.Equals("state"));
            }
        }

        [TestMethod]
        public async Task CreateNewWebhookWithMonthsBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var batch = context.NewBatch();

                var webhook = list.Webhooks.AddBatch(batch, $"{webhookHost}/api/HandleEvent", 1, "state");

                context.Execute(batch);

                Assert.IsTrue(webhook != null);
                Assert.IsTrue(webhook.ClientState.Equals("state"));
            }
        }

        [TestMethod]
        public async Task GetWebhookByIdLinqTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1));
                var wh = list.Webhooks.FirstOrDefault(w => w.Id == webhook.Id);

                Assert.IsTrue(wh != null);
            }
        }

        [TestMethod]
        public async Task GetWebhookByClientStateTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1), "state");
                var wh = list.Webhooks.Where(w => w.ClientState.Contains("state")).ToList();

                Assert.IsTrue(wh.Count > 0);
            }
        }

        [TestMethod]
        public async Task GetWebhookByUrlTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents");
                var webhook = list.Webhooks.Add($"{webhookHost}/api/HandleEvent", DateTime.UtcNow.AddDays(1), "state");
                var wh = list.Webhooks.Where(w => w.NotificationUrl.Contains("pnpcoresdkwebhooktest")).ToList();

                Assert.IsTrue(wh.Count > 0);
            }
        }
    }
}
