using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task PipelinePerRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Dictionary<string, string> extraHeaders = new Dictionary<string, string>() { { "header1", "value1" } };
                CustomHeadersRequestModule customHeadersRequestModule = new CustomHeadersRequestModule(extraHeaders);

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
                    ResponseHandler = (statusCode, headers, responseContent) =>
                    {
                        return responseContent;
                    }
                };
                
                var result = await context.Web.WithModule(customHeadersRequestModule).WithModule(testModule).GetAsync( p=> p.All);
                Assert.IsTrue(result.IsPropertyAvailable(p=>p.MasterUrl));
                Assert.IsTrue(context.RequestModules.Any() == false);

                context.Web.WithHeaders(extraHeaders).WithModule(testModule).Load(p => p.All);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsTrue(context.RequestModules.Any() == false);

                await context.Web.WithHeaders(extraHeaders).WithModule(testModule).LoadBatchAsync(p => p.All);
                await context.ExecuteAsync();
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.MasterUrl));
                Assert.IsTrue(context.RequestModules.Any() == false);

                var lists = await context.Web.Lists.WithHeaders(extraHeaders).WithModule(testModule).Where(p => p.Title == "Site Pages").ToListAsync();
                Assert.IsTrue(lists.Count == 1);
                Assert.IsTrue(context.RequestModules.Any() == false);

                // Option C: directly enumerate the lists, these lists are not loaded into the context
                await foreach (var list in context.Web.Lists.WithHeaders(extraHeaders).WithModule(testModule))
                {
                    // Use List
                }
                Assert.IsTrue(context.RequestModules.Any() == false);

            }
        }

    }
}
