using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests.Fields;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Linq;

namespace PnP.Core.Test.Services.Core.CSOM.Requests
{
    [TestClass]
    public class ProvisionTaxonomyFieldRequestTests
    {
        [TestMethod]
        public void ProvisionTaxonomyFieldRequest_Test_GetCorrectRequest_WithParentId()
        {
            ProvisionTaxonomyFieldRequest request = new ProvisionTaxonomyFieldRequest("site-id", "web-id", "field-id", "list-id", Guid.Parse("1e1a939f-60b2-2000-98a6-d25d3d400a3a"), Guid.Parse("740c6a0b-85e2-48a0-a494-e0f1759d4aa7"), false);

            var requests = request.GetRequest(new IteratorIdProvider());
            var actionRequests = requests.Select(r => r.Action).ToList();
            var identities = requests.Select(r => r.ObjectPath).Where(id => id != null).ToList();

            SetPropertyAction setTermStoreId = actionRequests[0] as SetPropertyAction;
            SetPropertyAction setTermSetId = actionRequests[1] as SetPropertyAction;
            SetPropertyAction setTargetTemplate = actionRequests[2] as SetPropertyAction;
            SetPropertyAction setAnchorId = actionRequests[4] as SetPropertyAction;
            MethodAction updateAction = actionRequests[5] as MethodAction;

            Identity id = identities[0];

            Assert.AreEqual("SspId", setTermStoreId.Name);
            Assert.AreEqual(2, setTermStoreId.Id);
            Assert.AreEqual("1", setTermStoreId.ObjectPathId);
            Assert.AreEqual("1e1a939f-60b2-2000-98a6-d25d3d400a3a", setTermStoreId.SetParameter.Value.ToString());

            Assert.AreEqual("TermSetId", setTermSetId.Name);
            Assert.AreEqual(3, setTermSetId.Id);
            Assert.AreEqual("1", setTermSetId.ObjectPathId);
            Assert.AreEqual("740c6a0b-85e2-48a0-a494-e0f1759d4aa7", setTermSetId.SetParameter.Value.ToString());

            Assert.AreEqual("TargetTemplate", setTargetTemplate.Name);
            Assert.AreEqual(4, setTargetTemplate.Id);
            Assert.AreEqual("1", setTargetTemplate.ObjectPathId);
            Assert.AreEqual("", setTargetTemplate.SetParameter.Value.ToString());

            Assert.AreEqual("AnchorId", setAnchorId.Name);
            Assert.AreEqual(6, setAnchorId.Id);
            Assert.AreEqual("1", setAnchorId.ObjectPathId);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", setAnchorId.SetParameter.Value.ToString());

            Assert.AreEqual("Update", updateAction.Name);
            Assert.AreEqual(7, updateAction.Id);
            Assert.AreEqual("1", updateAction.ObjectPathId);

            Assert.AreEqual("1e1a939f-60b2-2000-98a6-d25d3d400a3a|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:site-id:web:web-id:list:list-id:field:field-id", id.Name);
            Assert.AreEqual(1, id.Id);
        }

        [TestMethod]
        public void ProvisionTaxonomyFieldRequest_Test_GetCorrectRequest_WithoutParentId()
        {
            ProvisionTaxonomyFieldRequest request = new ProvisionTaxonomyFieldRequest("site-id", "web-id", "field-id", "", Guid.Parse("1e1a939f-60b2-2000-98a6-d25d3d400a3a"), Guid.Parse("740c6a0b-85e2-48a0-a494-e0f1759d4aa7"), false);

            var requests = request.GetRequest(new IteratorIdProvider());
            var actionRequests = requests.Select(r => r.Action).ToList();
            var identities = requests.Select(r => r.ObjectPath).Where(id => id != null).ToList();

            SetPropertyAction setTermStoreId = actionRequests[0] as SetPropertyAction;
            SetPropertyAction setTermSetId = actionRequests[1] as SetPropertyAction;
            SetPropertyAction setTargetTemplate = actionRequests[2] as SetPropertyAction;
            SetPropertyAction setAnchorId = actionRequests[4] as SetPropertyAction;
            MethodAction updateAction = actionRequests[5] as MethodAction;

            Identity id = identities[0];

            Assert.AreEqual("SspId", setTermStoreId.Name);
            Assert.AreEqual(2, setTermStoreId.Id);
            Assert.AreEqual("1", setTermStoreId.ObjectPathId);
            Assert.AreEqual("1e1a939f-60b2-2000-98a6-d25d3d400a3a", setTermStoreId.SetParameter.Value.ToString());

            Assert.AreEqual("TermSetId", setTermSetId.Name);
            Assert.AreEqual(3, setTermSetId.Id);
            Assert.AreEqual("1", setTermSetId.ObjectPathId);
            Assert.AreEqual("740c6a0b-85e2-48a0-a494-e0f1759d4aa7", setTermSetId.SetParameter.Value.ToString());

            Assert.AreEqual("TargetTemplate", setTargetTemplate.Name);
            Assert.AreEqual(4, setTargetTemplate.Id);
            Assert.AreEqual("1", setTargetTemplate.ObjectPathId);
            Assert.AreEqual("", setTargetTemplate.SetParameter.Value.ToString());

            Assert.AreEqual("AnchorId", setAnchorId.Name);
            Assert.AreEqual(6, setAnchorId.Id);
            Assert.AreEqual("1", setAnchorId.ObjectPathId);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", setAnchorId.SetParameter.Value.ToString());

            Assert.AreEqual("Update", updateAction.Name);
            Assert.AreEqual(7, updateAction.Id);
            Assert.AreEqual("1", updateAction.ObjectPathId);

            Assert.AreEqual("1e1a939f-60b2-2000-98a6-d25d3d400a3a|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:site-id:web:web-id:field:field-id", id.Name);
            Assert.AreEqual(1, id.Id);
        }
    }
}
