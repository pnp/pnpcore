using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM.Requests.ListItems;
using PnP.Core.Services.Core.CSOM.Requests.Web;
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
    public class UpdatePropertyBagTests
    {
        [TestMethod]
        public void UpdatePropertyBag_Test_BuildUpdateWebProperty()
        {
            UpdatePropertyBagRequest updatePropertyBagRequest = new UpdatePropertyBagRequest()
            {
                SiteId = "test-site-id",
                WebId = "test-web-id"
            };
            updatePropertyBagRequest.FieldsToUpdate.Add(new CSOMItemField()
            {
                FieldName = "TestProperty",
                FieldValue = "TestPropertyValue",
                FieldType = "String"
            });
            IIdProvider idProvider = new IteratorIdProvider();
            var request = updatePropertyBagRequest.GetRequest(idProvider);

            var firstPart = request[0];
            var secondPart = request[1];

            string setValuePart = "<Method Name=\"SetFieldValue\" Id=\"1\" ObjectPathId=\"3\"><Parameters><Parameter Type=\"String\">TestProperty</Parameter><Parameter Type=\"String\">TestPropertyValue</Parameter></Parameters></Method>";
            string allPropertiesPath = "<Property Id=\"3\" ParentId=\"2\" Name=\"AllProperties\" />";
            string updatePropertiesMethod = "<Method Name=\"Update\" Id=\"1\" ObjectPathId=\"2\"></Method>";
            string webIdentity = "<Identity Id=\"2\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:test-site-id:web:test-web-id\" />";

            Assert.AreEqual(setValuePart, firstPart.Action.ToString());
            Assert.AreEqual(allPropertiesPath, firstPart.ObjectPath.ToString());
            Assert.AreEqual(updatePropertiesMethod, secondPart.Action.ToString());
            Assert.AreEqual(webIdentity, secondPart.ObjectPath.ToString());
        }
        [TestMethod]
        public void UpdatePropertyBag_Test_BuildUpdateFolderProperty()
        {
            FolderPropertyBagUpdateRequest updatePropertyBagRequest = new FolderPropertyBagUpdateRequest("/sites/test-sites/some-test-folder")
            {
                SiteId = "test-site-id",
                WebId = "test-web-id"
            };
            updatePropertyBagRequest.FieldsToUpdate.Add(new CSOMItemField()
            {
                FieldName = "TestProperty",
                FieldValue = "TestPropertyValue",
                FieldType = "String"
            });
            IIdProvider idProvider = new IteratorIdProvider();
            var request = updatePropertyBagRequest.GetRequest(idProvider);

            var firstPart = request[0];
            var secondPart = request[1];

            string setValuePart = "<Method Name=\"SetFieldValue\" Id=\"1\" ObjectPathId=\"3\"><Parameters><Parameter Type=\"String\">TestProperty</Parameter><Parameter Type=\"String\">TestPropertyValue</Parameter></Parameters></Method>";
            string allPropertiesPath = "<Property Id=\"3\" ParentId=\"2\" Name=\"Properties\" />";
            string updatePropertiesMethod = "<Method Name=\"Update\" Id=\"1\" ObjectPathId=\"2\"></Method>";
            string webIdentity = "<Identity Id=\"2\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:test-site-id:web:test-web-id:folder:/sites/test-sites/some-test-folder\" />";

            Assert.AreEqual(setValuePart, firstPart.Action.ToString());
            Assert.AreEqual(allPropertiesPath, firstPart.ObjectPath.ToString());
            Assert.AreEqual(updatePropertiesMethod, secondPart.Action.ToString());
            Assert.AreEqual(webIdentity, secondPart.ObjectPath.ToString());
        }
        [TestMethod]
        public void UpdatePropertyBag_Test_BuildUpdateFileProperty()
        {
            FilePropertyBagUpdateRequest updatePropertyBagRequest = new FilePropertyBagUpdateRequest("/sites/test-sites/some-test-folder/test-file.docx")
            {
                SiteId = "test-site-id",
                WebId = "test-web-id"
            };
            updatePropertyBagRequest.FieldsToUpdate.Add(new CSOMItemField()
            {
                FieldName = "TestProperty",
                FieldValue = "TestPropertyValue",
                FieldType = "String"
            });
            IIdProvider idProvider = new IteratorIdProvider();
            var request = updatePropertyBagRequest.GetRequest(idProvider);

            var firstPart = request[0];
            var secondPart = request[1];

            string setValuePart = "<Method Name=\"SetFieldValue\" Id=\"1\" ObjectPathId=\"3\"><Parameters><Parameter Type=\"String\">TestProperty</Parameter><Parameter Type=\"String\">TestPropertyValue</Parameter></Parameters></Method>";
            string allPropertiesPath = "<Property Id=\"3\" ParentId=\"2\" Name=\"Properties\" />";
            string updatePropertiesMethod = "<Method Name=\"Update\" Id=\"1\" ObjectPathId=\"2\"></Method>";
            string webIdentity = "<Identity Id=\"2\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:test-site-id:web:test-web-id:file:/sites/test-sites/some-test-folder/test-file.docx\" />";

            Assert.AreEqual(setValuePart, firstPart.Action.ToString());
            Assert.AreEqual(allPropertiesPath, firstPart.ObjectPath.ToString());
            Assert.AreEqual(updatePropertiesMethod, secondPart.Action.ToString());
            Assert.AreEqual(webIdentity, secondPart.ObjectPath.ToString());
        }
    }
}
