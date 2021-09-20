using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests.Web;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Test.Services.Core.CSOM.Requests
{
    [TestClass]
    public class CreateContentTypeRequestTests
    {
        [TestMethod]
        public void CreateContentTypeRequest_Test_GenerateCorrectRequest()
        {
            IteratorIdProvider idProvider = new IteratorIdProvider();
            ContentTypeCreationInfo creationInfo = new ContentTypeCreationInfo()
            {
                Id = "0x10023213123123",
                Description = "Test Description",
                Group = "Test Group",
                Name = "Test Name",
            };
            CreateContentTypeRequest request = new CreateContentTypeRequest(creationInfo);
            List<ActionObjectPath> requests = request.GetRequest(idProvider);
            var actionRequests = requests.Select(r => r.Action).Where(r => r != null).ToList();
            var identities = requests.Select(r => r.ObjectPath).Where(id => id != null).ToList();

            BaseAction objectPath = actionRequests[0];
            IdentityQueryAction objectIdQuery = actionRequests[1] as IdentityQueryAction;

            ObjectPathMethod createCTAction = identities[0] as ObjectPathMethod;
            Property contentTypesProp = identities[1] as Property;
            Property webProp = identities[2] as Property;
            StaticProperty currentSiteProp = identities[3] as StaticProperty;

            Assert.AreEqual("4", objectPath.ObjectPathId);
            Assert.AreEqual(5, objectPath.Id);

            Assert.AreEqual("4", objectIdQuery.ObjectPathId);
            Assert.AreEqual(6, objectIdQuery.Id);

            Assert.AreEqual("Add", createCTAction.Name);
            Assert.AreEqual(3, createCTAction.ParentId);
            Assert.AreEqual(4, createCTAction.Id);

            Assert.AreEqual("ContentTypes", contentTypesProp.Name);
            Assert.AreEqual(2, contentTypesProp.ParentId);
            Assert.AreEqual(3, contentTypesProp.Id);

            Assert.AreEqual("Web", webProp.Name);
            Assert.AreEqual(1, webProp.ParentId);
            Assert.AreEqual(2, webProp.Id);

            Assert.AreEqual("Current", currentSiteProp.Name);
            Assert.AreEqual("{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}", currentSiteProp.TypeId);
            Assert.AreEqual(1, currentSiteProp.Id);
        }
    }
}
