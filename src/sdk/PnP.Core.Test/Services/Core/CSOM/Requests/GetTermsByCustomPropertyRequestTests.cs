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

namespace PnP.Core.Test.Services.Core.CSOM.Requests
{
    [TestClass]
    public class GetTermsByCustomPropertyRequestTests
    {
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
