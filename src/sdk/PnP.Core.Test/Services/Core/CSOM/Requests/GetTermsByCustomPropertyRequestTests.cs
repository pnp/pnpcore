using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests.Fields;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Requests.Terms;
using PnP.Core.Test.Utilities;

namespace PnP.Core.Test.Services.Core.CSOM.Requests
{
    [TestClass]
    public class GetTermsByCustomPropertyRequestTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetTermsByCustomProperty()
        {

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var termStore = await context.TermStore.GetAsync();
                var termGroup = await termStore.Groups.GetByNameAsync("People");
                var termSet = await termGroup.Sets.GetByIdAsync("8ed8c9ea-7052-4c1d-a4d7-b9c10bffea6f");
            }
        }

        [TestMethod]
        public void GetTermsByCustomPropertyRequest_Test_GetCorrectRequest()
        {
            GetTermsByCustomPropertyRequest request = new GetTermsByCustomPropertyRequest("key1", "value1", false);

            var requests = request.GetRequest(new IteratorIdProvider());
            var actionRequests = requests.Select(r => r.Action).ToList();
            var identities = requests.Select(r => r.ObjectPath).Where(id => id != null).ToList();

            string requestBody = "";

            CSOMApiCallBuilder csomAPICallBuilder = new CSOMApiCallBuilder();
            csomAPICallBuilder.AddRequest(request);

            requestBody = csomAPICallBuilder.SerializeCSOMRequests();

        }
    }
}
