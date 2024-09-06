using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class PipelineTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task PipelineForGraphExpandableRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Dictionary<string, string> extraHeaders = new Dictionary<string, string>() { { "header1", "value1" } };
                GenericRequestModule testModule = new GenericRequestModule()
                {

                    RequestHeaderHandler = (headers) =>
                    {
                        Assert.IsTrue(headers.ContainsKey("header1"));
                    },
                    RequestUrlHandler = (url) =>
                    {
                        return url;
                    },
                    RequestBodyHandler = (body) =>
                    {
                        return body;
                    },
                    ResponseHandler = (statusCode, headers, responseContent, batchRequestId) =>
                    {
                        return responseContent;
                    }
                };

                var team = await context.Team.WithHeaders(extraHeaders).WithModule(testModule).GetAsync(p => p.Description, p => p.Channels, p => p.Owners);

                Assert.IsTrue(team.IsPropertyAvailable(p => p.Description));
            }
        }

        [TestMethod]
        public async Task PipelineForMixedRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Dictionary<string, string> extraHeaders = new Dictionary<string, string>() { { "header1", "value1" } };
                GenericRequestModule testModule = new GenericRequestModule()
                {

                    RequestHeaderHandler = (headers) =>
                    {
                        Assert.IsTrue(headers.ContainsKey("header1"));
                    },
                    RequestUrlHandler = (url) =>
                    {
                        return url;
                    },
                    RequestBodyHandler = (body) =>
                    {
                        return body;
                    },
                    ResponseHandler = (statusCode, headers, responseContent, batchRequestId) =>
                    {
                        return responseContent;
                    }
                };

                await context.Team.WithHeaders(extraHeaders).WithModule(testModule).LoadBatchAsync(p => p.Description);
                await context.Web.WithHeaders(extraHeaders).WithModule(testModule).LoadBatchAsync(p => p.All);
                await context.ExecuteAsync();

                Assert.IsTrue(context.Team.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsTrue(context.RequestModules.Any() == false);
            }
        }

        [TestMethod]
        public async Task BatchingRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Dictionary<string, string> extraHeaders = new Dictionary<string, string>() { { "header1", "value1" } };
                GenericRequestModule testModule = new GenericRequestModule()
                {

                    RequestHeaderHandler = (headers) =>
                    {
                        Assert.IsTrue(headers.ContainsKey("header1"));
                    },
                    RequestUrlHandler = (url) =>
                    {
                        return url;
                    },
                    RequestBodyHandler = (body) =>
                    {
                        return body;
                    },
                    ResponseHandler = (statusCode, headers, responseContent, batchRequestId) =>
                    {
                        return responseContent;
                    }
                };

                await context.Web.WithHeaders(extraHeaders).WithModule(testModule).LoadBatchAsync(p => p.All);
                await context.ExecuteAsync();

                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsTrue(context.RequestModules.Any() == false);
            }
        }

        [TestMethod]
        public async Task PipelineForGraphRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Dictionary<string, string> extraHeaders = new Dictionary<string, string>() { { "header1", "value1" } };
                GenericRequestModule testModule = new GenericRequestModule()
                {

                    RequestHeaderHandler = (headers) =>
                    {
                        Assert.IsTrue(headers.ContainsKey("header1"));
                    },
                    RequestUrlHandler = (url) =>
                    {
                        return url;
                    },
                    RequestBodyHandler = (body) =>
                    {
                        return body;
                    },
                    ResponseHandler = (statusCode, headers, responseContent, batchRequestId) =>
                    {
                        return responseContent;
                    }
                };

                var team = await context.Team.WithHeaders(extraHeaders).WithModule(testModule).GetAsync(p => p.Description);

                Assert.IsTrue(team.IsPropertyAvailable(p => p.Description));
            }
        }

        [TestMethod]
        public async Task PipelineForInteractiveRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Dictionary<string, string> extraHeaders = new Dictionary<string, string>() { { "header1", "value1" } };
                GenericRequestModule testModule = new GenericRequestModule()
                {

                    RequestHeaderHandler = (headers) =>
                    {
                        Assert.IsTrue(headers.ContainsKey("header1"));
                    },
                    RequestUrlHandler = (url) =>
                    {
                        return url;
                    },
                    RequestBodyHandler = (body) =>
                    {
                        return body;
                    },
                    ResponseHandler = (statusCode, headers, responseContent, batchRequestId) =>
                    {
                        return responseContent;
                    }
                };

                var api = new ApiCall($"_api/web?$select=Id%2cWelcomePage", ApiType.SPORest)
                {
                    Interactive = true
                };

                var apiResponse = await (context.Web.WithHeaders(extraHeaders).WithModule(testModule) as Web).RequestAsync(api, HttpMethod.Get);

                Assert.IsTrue(apiResponse.Executed);
                Assert.IsTrue(!string.IsNullOrEmpty(apiResponse.Requests.First().Value.ResponseJson));
                Assert.IsTrue(apiResponse.Requests.First().Value.ResponseHttpStatusCode == System.Net.HttpStatusCode.OK);
                Assert.IsTrue(!string.IsNullOrEmpty(context.Web.WelcomePage));
            }
        }

        [TestMethod]
        public async Task PipelinePerRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Dictionary<string, string> extraHeaders = new Dictionary<string, string>() { { "header1", "value1" } };
                CustomHeadersRequestModule customHeadersRequestModule = new CustomHeadersRequestModule(extraHeaders, null);

                GenericRequestModule testModule = new GenericRequestModule()
                {

                    RequestHeaderHandler = (headers) =>
                    {
                        Assert.IsTrue(headers.ContainsKey("header1"));
                    },
                    RequestUrlHandler = (url) =>
                    {
                        return url;
                    },
                    RequestBodyHandler = (body) =>
                    {
                        return body;
                    },
                    ResponseHandler = (statusCode, headers, responseContent, batchRequestId) =>
                    {
                        return responseContent;
                    }
                };

                var result = await context.Web.WithModule(customHeadersRequestModule).WithModule(testModule).GetAsync(p => p.All);
                Assert.IsTrue(result.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsTrue(context.RequestModules.Any() == false);

                context.Web.WithHeaders(extraHeaders, (responseHeaders) => { Assert.IsTrue(responseHeaders.Count > 0); }).WithModule(testModule).Load(p => p.All);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsTrue(context.RequestModules.Any() == false);

                await context.Web.WithHeaders(extraHeaders).WithModule(testModule).LoadBatchAsync(p => p.All);
                await context.ExecuteAsync();
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsTrue(context.RequestModules.Any() == false);

                var lists = await context.Web.Lists.WithHeaders(extraHeaders).WithModule(testModule).Where(p => p.Title == "Site Pages").ToListAsync();
                Assert.IsTrue(lists.Count == 1);
                Assert.IsTrue(context.RequestModules.Any() == false);

                await foreach (var list in context.Web.Lists.WithHeaders(extraHeaders).WithModule(testModule))
                {
                    // Use List
                }
                Assert.IsTrue(context.RequestModules.Any() == false);

            }
        }

    }
}
