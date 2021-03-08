using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.Requests.ListItems;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Services.Core.CSOM.Requests
{
    [TestClass]
    public class UpdateListItemRequestTests
    {
        [TestMethod]
        public void UpdateListItemRequest_Test_GetRequest()
        {
            UpdateListItemRequest request = new UpdateListItemRequest("test-site-id", "test-web-id", "test-list-id", 1);
            request.FieldsToUpdate.Add(new CSOMItemField()
            {
                FieldName = "Test Field",
                FieldType = "String",
                FieldValue = "Test field value"
            });
            IIdProvider idProvider = new IteratorIdProvider();
            List<ActionObjectPath> actionObjectPaths =  request.GetRequest(idProvider);

            ActionObjectPath setFieldsActionPath = actionObjectPaths[0];
            Assert.AreEqual("<Method Name=\"SetFieldValue\" Id=\"4\" ObjectPathId=\"1\"><Parameters><Parameter Type=\"String\">Test Field</Parameter><Parameter Type=\"String\">Test field value</Parameter></Parameters></Method>", setFieldsActionPath.Action.ToString());

            ActionObjectPath actionObjectPath = actionObjectPaths[1];
            Assert.AreEqual("<Method Name=\"Update\" Id=\"5\" ObjectPathId=\"1\"></Method>", actionObjectPath.Action.ToString());
            Assert.AreEqual("<Identity Id=\"1\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:test-site-id:web:test-web-id:list:test-list-id:item:1,1\" />", actionObjectPath.ObjectPath.ToString());

        }
        [TestMethod]
        public void SystemUpdateListItemRequest_Test_GetRequest()
        {
            UpdateListItemRequest request = new SystemUpdateListItemRequest("test-site-id", "test-web-id", "test-list-id", 1);
            request.FieldsToUpdate.Add(new CSOMItemField()
            {
                FieldName = "Test Field",
                FieldType = "String",
                FieldValue = "Test field value"
            });
            IIdProvider idProvider = new IteratorIdProvider();
            List<ActionObjectPath> actionObjectPaths = request.GetRequest(idProvider);

            ActionObjectPath setFieldsActionPath = actionObjectPaths[0];
            Assert.AreEqual("<Method Name=\"SetFieldValue\" Id=\"4\" ObjectPathId=\"1\"><Parameters><Parameter Type=\"String\">Test Field</Parameter><Parameter Type=\"String\">Test field value</Parameter></Parameters></Method>", setFieldsActionPath.Action.ToString());

            ActionObjectPath actionObjectPath = actionObjectPaths[1];
            Assert.AreEqual("<Method Name=\"SystemUpdate\" Id=\"5\" ObjectPathId=\"1\"></Method>", actionObjectPath.Action.ToString());
            Assert.AreEqual("<Identity Id=\"1\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:test-site-id:web:test-web-id:list:test-list-id:item:1,1\" />", actionObjectPath.ObjectPath.ToString());

        }
        [TestMethod]
        public void UpdateOvervriteVersionRequest_Test_GetRequest()
        {
            UpdateListItemRequest request = new UpdateOvervriteVersionRequest("test-site-id", "test-web-id", "test-list-id", 1);
            request.FieldsToUpdate.Add(new CSOMItemField()
            {
                FieldName = "Test Field",
                FieldType = "String",
                FieldValue = "Test field value"
            });
            IIdProvider idProvider = new IteratorIdProvider();
            List<ActionObjectPath> actionObjectPaths = request.GetRequest(idProvider);

            ActionObjectPath setFieldsActionPath = actionObjectPaths[0];
            Assert.AreEqual("<Method Name=\"SetFieldValue\" Id=\"4\" ObjectPathId=\"1\"><Parameters><Parameter Type=\"String\">Test Field</Parameter><Parameter Type=\"String\">Test field value</Parameter></Parameters></Method>", setFieldsActionPath.Action.ToString());

            ActionObjectPath actionObjectPath = actionObjectPaths[1];
            Assert.AreEqual("<Method Name=\"UpdateOverwriteVersion\" Id=\"5\" ObjectPathId=\"1\"></Method>", actionObjectPath.Action.ToString());
            Assert.AreEqual("<Identity Id=\"1\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:test-site-id:web:test-web-id:list:test-list-id:item:1,1\" />", actionObjectPath.ObjectPath.ToString());
        }
        [TestMethod]
        public void UpdateOvervriteVersionRequest_Test_ProcessRequest()
        {
            UpdateListItemRequest request = new UpdateOvervriteVersionRequest("test-site-id", "test-web-id", "test-list-id", 1);
            request.FieldsToUpdate.Add(new CSOMItemField()
            {
                FieldName = "Title",
                FieldType = "String",
                FieldValue = "Test field value"
            });
            IIdProvider idProvider = new IteratorIdProvider();
            request.GetRequest(idProvider);

            request.ProcessResponse("[1,{\"_ObjectType_\":\"SP.ListItem\", \"_ObjectIdentity_\":\"10f1b19f-b09b-2000-a0c6-8d8859432e0d|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:69399214-4542-4f93-b658-328a712a834f:web:f5ad2fe7-d9f1-4b96-8dce-884fa6cc49cd:list:98f73a1b-9869-4024-af86-9e63da385636:item:1,1\", \"_ObjectVersion_\":\"2\", \"Title\":\"Test field value\"}]");
            Assert.AreEqual("Test field value", request.Result.Title);
        }
    }
}
