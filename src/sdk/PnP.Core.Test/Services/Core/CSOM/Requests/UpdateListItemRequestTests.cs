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
            request.FieldsToUpdate = new List<CSOMItemField>()
            {
                new CSOMItemField()
                {
                    FieldName = "Test Field",
                    FieldType = "String",
                    FieldValue = "Test field value"
                }
            };
            IIdProvider idProvider = new IteratorIdProvider();
            List<ActionObjectPath> actionObjectPaths =  request.GetRequest(idProvider);

            ActionObjectPath setFieldsActionPath = actionObjectPaths[0];
            Assert.AreEqual("<Method Name=\"SetFieldValue\" Id=\"1\" ObjectPathId=\"2\"><Parameters><Parameter Type=\"String\">Test Field</Parameter><Parameter Type=\"String\">Test field value</Parameter></Parameters></Method>", setFieldsActionPath.Action.ToString());
            Assert.AreEqual("<Identity Id=\"2\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:test-site-id:web:test-web-id:list:test-list-id:item:1,1\" />", setFieldsActionPath.ObjectPath.ToString());

            ActionObjectPath actionObjectPath = actionObjectPaths[1];
            Assert.AreEqual("<Method Name=\"Update\" Id=\"1\" ObjectPathId=\"2\"></Method>", actionObjectPath.Action.ToString());

        }
        [TestMethod]
        public void SystemUpdateListItemRequest_Test_GetRequest()
        {
            UpdateListItemRequest request = new SystemUpdateListItemRequest("test-site-id", "test-web-id", "test-list-id", 1);
            request.FieldsToUpdate = new List<CSOMItemField>()
            {
                new CSOMItemField()
                {
                    FieldName = "Test Field",
                    FieldType = "String",
                    FieldValue = "Test field value"
                }
            };
            IIdProvider idProvider = new IteratorIdProvider();
            List<ActionObjectPath> actionObjectPaths = request.GetRequest(idProvider);

            ActionObjectPath setFieldsActionPath = actionObjectPaths[0];
            Assert.AreEqual("<Method Name=\"SetFieldValue\" Id=\"1\" ObjectPathId=\"2\"><Parameters><Parameter Type=\"String\">Test Field</Parameter><Parameter Type=\"String\">Test field value</Parameter></Parameters></Method>", setFieldsActionPath.Action.ToString());
            Assert.AreEqual("<Identity Id=\"2\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:test-site-id:web:test-web-id:list:test-list-id:item:1,1\" />", setFieldsActionPath.ObjectPath.ToString());

            ActionObjectPath actionObjectPath = actionObjectPaths[1];
            Assert.AreEqual("<Method Name=\"SystemUpdate\" Id=\"1\" ObjectPathId=\"2\"></Method>", actionObjectPath.Action.ToString());

        }
        [TestMethod]
        public void UpdateOvervriteVersionRequest_Test_GetRequest()
        {
            UpdateListItemRequest request = new UpdateOvervriteVersionRequest("test-site-id", "test-web-id", "test-list-id", 1);
            request.FieldsToUpdate = new List<CSOMItemField>()
            {
                new CSOMItemField()
                {
                    FieldName = "Test Field",
                    FieldType = "String",
                    FieldValue = "Test field value"
                }
            };
            IIdProvider idProvider = new IteratorIdProvider();
            List<ActionObjectPath> actionObjectPaths = request.GetRequest(idProvider);

            ActionObjectPath setFieldsActionPath = actionObjectPaths[0];
            Assert.AreEqual("<Method Name=\"SetFieldValue\" Id=\"1\" ObjectPathId=\"2\"><Parameters><Parameter Type=\"String\">Test Field</Parameter><Parameter Type=\"String\">Test field value</Parameter></Parameters></Method>", setFieldsActionPath.Action.ToString());
            Assert.AreEqual("<Identity Id=\"2\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:test-site-id:web:test-web-id:list:test-list-id:item:1,1\" />", setFieldsActionPath.ObjectPath.ToString());

            ActionObjectPath actionObjectPath = actionObjectPaths[1];
            Assert.AreEqual("<Method Name=\"UpdateOverwriteVersion\" Id=\"1\" ObjectPathId=\"2\"></Method>", actionObjectPath.Action.ToString());

        }
    }
}
