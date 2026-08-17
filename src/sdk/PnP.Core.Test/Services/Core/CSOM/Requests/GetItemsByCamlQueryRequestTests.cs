using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.Requests.ListItems;
using PnP.Core.Services.Core.CSOM.Utils;

namespace PnP.Core.Test.Services.Core.CSOM.Requests
{
    [TestClass]
    public class GetItemsByCamlQueryRequestTests
    {
        private const string SiteId = "b1a2c3d4-0000-0000-0000-000000000001";
        private const string WebId = "b1a2c3d4-0000-0000-0000-000000000002";
        private const string ListId = "b1a2c3d4-0000-0000-0000-000000000003";

        [TestMethod]
        public void GetItemsByCamlQueryRequest_Test_GetRequest()
        {
            var request = new GetItemsByCamlQueryRequest(SiteId, WebId, ListId, new CamlQueryOptions
            {
                ViewXml = "<View><Query><Where><IsNotNull><FieldRef Name='Title'/></IsNotNull></Where></Query></View>",
                DatesInUtc = true
            });

            var actionObjectPaths = request.GetRequest(new IteratorIdProvider());

            var identity = actionObjectPaths[0].ObjectPath.ToString();
            Assert.AreEqual($"<Identity Id=\"1\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{SiteId}:web:{WebId}:list:{ListId}\" />", identity);

            var objectPathAction = actionObjectPaths[0].Action.ToString();
            Assert.AreEqual("<ObjectPath Id=\"3\" ObjectPathId=\"2\" />", objectPathAction);

            var method = actionObjectPaths[1].ObjectPath.ToString();
            Assert.AreEqual(
                "<Method Id=\"2\" ParentId=\"1\" Name=\"GetItems\"><Parameters>" +
                "<Parameter TypeId=\"{3d248d7b-fc86-40a3-aa97-02a75d69fb8a}\">" +
                "<Property Name=\"AllowIncrementalResults\" Type=\"Null\" />" +
                "<Property Name=\"DatesInUtc\" Type=\"Boolean\">true</Property>" +
                "<Property Name=\"FolderServerRelativeUrl\" Type=\"Null\" />" +
                "<Property Name=\"ListItemCollectionPosition\" Type=\"Null\" />" +
                "<Property Name=\"ViewXml\" Type=\"String\">&lt;View&gt;&lt;Query&gt;&lt;Where&gt;&lt;IsNotNull&gt;&lt;FieldRef Name='Title'/&gt;&lt;/IsNotNull&gt;&lt;/Where&gt;&lt;/Query&gt;&lt;/View&gt;</Property>" +
                "</Parameter></Parameters></Method>",
                method);

            var queryAction = actionObjectPaths[1].Action.ToString();
            Assert.AreEqual(
                "<Query Id=\"4\" ObjectPathId=\"2\" ><Query SelectAllProperties=\"true\"><Properties /></Query><ChildItemQuery SelectAllProperties=\"true\"><Properties /></ChildItemQuery></Query>",
                queryAction);
        }

        [TestMethod]
        public void GetItemsByCamlQueryRequest_Test_GetRequest_WithPagingInfo()
        {
            var request = new GetItemsByCamlQueryRequest(SiteId, WebId, ListId, new CamlQueryOptions
            {
                ViewXml = "<View/>",
                AllowIncrementalResults = true,
                PagingInfo = "Paged=TRUE&p_ID=100"
            });

            var actionObjectPaths = request.GetRequest(new IteratorIdProvider());
            var method = actionObjectPaths[1].ObjectPath.ToString();

            Assert.IsTrue(method.Contains("<Property Name=\"AllowIncrementalResults\" Type=\"Boolean\">true</Property>"));
            Assert.IsTrue(method.Contains(
                "<Property Name=\"ListItemCollectionPosition\" TypeId=\"{922354eb-c56a-4d88-ad59-67496854efe1}\">" +
                "<Property Name=\"PagingInfo\" Type=\"String\">Paged=TRUE&amp;p_ID=100</Property></Property>"));
        }

        [TestMethod]
        public void GetItemsByCamlQueryRequest_Test_ProcessResponse()
        {
            var request = new GetItemsByCamlQueryRequest(SiteId, WebId, ListId, new CamlQueryOptions { ViewXml = "<View/>" });
            request.GetRequest(new IteratorIdProvider());

            // The query action gets id 4 when using the IteratorIdProvider
            request.ProcessResponse(
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23019.12004\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"2f4f75a0-c0ea-5000-7bec-0d4c83c2b811\"}," +
                "3,{\"IsNull\":false}," +
                "4,{\"_ObjectType_\":\"SP.ListItemCollection\",\"_Child_Items_\":[" +
                "{\"_ObjectType_\":\"SP.ListItem\",\"_ObjectIdentity_\":\"2f4f75a0|...:list:x:item:1,1\",\"_ObjectVersion_\":\"1\",\"Id\":1,\"ID\":1,\"Title\":\"Item1\"," +
                "\"JoinLookup\":{\"_ObjectType_\":\"SP.FieldLookupValue\",\"LookupId\":2,\"LookupValue\":\"Lookup item\",\"IsSecretFieldValue\":false}," +
                "\"ProjectedText\":{\"_ObjectType_\":\"SP.FieldLookupValue\",\"LookupId\":2,\"LookupValue\":\"Projected text\",\"IsSecretFieldValue\":false}}," +
                "{\"_ObjectType_\":\"SP.ListItem\",\"_ObjectIdentity_\":\"2f4f75a0|...:list:x:item:2,1\",\"_ObjectVersion_\":\"1\",\"Id\":2,\"ID\":2,\"Title\":\"Item2\",\"ProjectedText\":null}" +
                "],\"ListItemCollectionPosition\":{\"_ObjectType_\":\"SP.ListItemCollectionPosition\",\"PagingInfo\":\"Paged=TRUE&p_ID=2\"}}]");

            Assert.AreEqual(2, request.Result.Items.Count);
            Assert.AreEqual("Paged=TRUE&p_ID=2", request.Result.PagingInfo);

            var firstItem = request.Result.Items[0];
            Assert.AreEqual(1, firstItem.GetProperty("Id").GetInt32());
            Assert.AreEqual("SP.FieldLookupValue", firstItem.GetProperty("ProjectedText").GetProperty("_ObjectType_").GetString());
        }

        [TestMethod]
        public void GetItemsByCamlQueryRequest_Test_ProcessResponse_LastPage()
        {
            var request = new GetItemsByCamlQueryRequest(SiteId, WebId, ListId, new CamlQueryOptions { ViewXml = "<View/>" });
            request.GetRequest(new IteratorIdProvider());

            request.ProcessResponse(
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23019.12004\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"2f4f75a0-c0ea-5000-7bec-0d4c83c2b811\"}," +
                "3,{\"IsNull\":false}," +
                "4,{\"_ObjectType_\":\"SP.ListItemCollection\",\"_Child_Items_\":[" +
                "{\"_ObjectType_\":\"SP.ListItem\",\"Id\":7,\"ID\":7,\"Title\":\"Last\"}" +
                "],\"ListItemCollectionPosition\":null}]");

            Assert.AreEqual(1, request.Result.Items.Count);
            Assert.IsNull(request.Result.PagingInfo);
        }

        [TestMethod]
        public void GetItemsByCamlQueryRequest_Test_ProcessResponse_NoMatchingActionId()
        {
            var request = new GetItemsByCamlQueryRequest(SiteId, WebId, ListId, new CamlQueryOptions { ViewXml = "<View/>" });
            request.GetRequest(new IteratorIdProvider());

            request.ProcessResponse("[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23019.12004\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"x\"}]");

            Assert.AreEqual(0, request.Result.Items.Count);
            Assert.IsNull(request.Result.PagingInfo);
        }

        [TestMethod]
        public void GetItemsByCamlQueryRequest_Test_ProcessResponse_ActionIdIsLastElement()
        {
            var request = new GetItemsByCamlQueryRequest(SiteId, WebId, ListId, new CamlQueryOptions { ViewXml = "<View/>" });
            request.GetRequest(new IteratorIdProvider());

            // A truncated response where the SP.ListItemCollection following the query action id is missing
            request.ProcessResponse(
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23019.12004\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"x\"}," +
                "3,{\"IsNull\":false},4]");

            Assert.AreEqual(0, request.Result.Items.Count);
            Assert.IsNull(request.Result.PagingInfo);
        }

        [TestMethod]
        public void GetItemsByCamlQueryRequest_Test_ProcessResponse_ItemCollectionNotAnObject()
        {
            var request = new GetItemsByCamlQueryRequest(SiteId, WebId, ListId, new CamlQueryOptions { ViewXml = "<View/>" });
            request.GetRequest(new IteratorIdProvider());

            request.ProcessResponse(
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23019.12004\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"x\"}," +
                "3,{\"IsNull\":false},4,null]");

            Assert.AreEqual(0, request.Result.Items.Count);
            Assert.IsNull(request.Result.PagingInfo);
        }

        [TestMethod]
        public void GetItemsByCamlQueryRequest_Test_ProcessResponse_PagingInfoNotAString()
        {
            var request = new GetItemsByCamlQueryRequest(SiteId, WebId, ListId, new CamlQueryOptions { ViewXml = "<View/>" });
            request.GetRequest(new IteratorIdProvider());

            request.ProcessResponse(
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23019.12004\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"x\"}," +
                "3,{\"IsNull\":false}," +
                "4,{\"_ObjectType_\":\"SP.ListItemCollection\",\"_Child_Items_\":[{\"_ObjectType_\":\"SP.ListItem\",\"Id\":1,\"Title\":\"Item1\"}]," +
                "\"ListItemCollectionPosition\":{\"_ObjectType_\":\"SP.ListItemCollectionPosition\",\"PagingInfo\":42}}]");

            Assert.AreEqual(1, request.Result.Items.Count);
            Assert.IsNull(request.Result.PagingInfo);
        }
    }
}
