using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using PnP.Core.Services.Core.CSOM.Requests.Terms;

namespace PnP.Core.Test.Services.Core.CSOM.Requests
{
    [TestClass]
    public class GetTermsByCustomPropertyRequestTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
           //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public void GetTermsByCustomPropertyRequest_Test_ProcessResponse()
        {
            var request = new GetTermsByCustomPropertyRequest(
                "x", 
                "y", 
                false,
                "",
                ""
                );

            request.GetRequest(new IteratorIdProvider());
                
            request.ProcessResponse("[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23019.12004\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"2f4f75a0-c0ea-5000-7bec-0d4c83c2b811\"},2,{\"IsNull\":false},3,{\"_ObjectIdentity_\":\"2f4f75a0-c0ea-5000-7bec-0d4c83c2b811|fec14c62-7c3b-481b-851b-c80d7802b224:ss:\"},5,{\"IsNull\":false},6,{\"_ObjectIdentity_\":\"2f4f75a0-c0ea-5000-7bec-0d4c83c2b811|fec14c62-7c3b-481b-851b-c80d7802b224:st:EWEZqYt9YESjHmoOOqhLdg==\"},8,{\"IsNull\":false},10,{\"IsNull\":false},11,{\"_ObjectIdentity_\":\"2f4f75a0-c0ea-5000-7bec-0d4c83c2b811|fec14c62-7c3b-481b-851b-c80d7802b224:gr:EWEZqYt9YESjHmoOOqhLdqGv9rdi2CBLjdNc5oZbztU=\"},13,{\"IsNull\":false},15,{\"IsNull\":false},16,{\"_ObjectIdentity_\":\"2f4f75a0-c0ea-5000-7bec-0d4c83c2b811|fec14c62-7c3b-481b-851b-c80d7802b224:se:EWEZqYt9YESjHmoOOqhLdqGv9rdi2CBLjdNc5oZbztXUm5zj/bp7SoZ5QfomyGcp\"},18,{\"IsNull\":false},23,{\"IsNull\":false},24,{\"_ObjectType_\":\"SP.Taxonomy.TermCollection\",\"_Child_Items_\":[{\"_ObjectType_\":\"SP.Taxonomy.Term\",\"_ObjectIdentity_\":\"2f4f75a0-c0ea-5000-7bec-0d4c83c2b811|fec14c62-7c3b-481b-851b-c80d7802b224:te:EWEZqYt9YESjHmoOOqhLdqGv9rdi2CBLjdNc5oZbztXUm5zj/bp7SoZ5QfomyGcpY+0lWJsJ20O60U//2e8bGA==\",\"CreatedDate\":\"/Date(1667567579813)/\",\"Id\":\"/Guid(5825ed63-099b-43db-bad1-4fffd9ef1b18)/\",\"LastModifiedDate\":\"/Date(1667567580360)/\",\"Name\":\"T1\",\"CustomProperties\":{\"property1\":\"value1\"},\"CustomSortOrder\":null,\"IsAvailableForTagging\":true,\"Owner\":\"i:0#.f|membership|m@loitzl2.onmicrosoft.com\",\"Description\":\"Description in English\",\"IsDeprecated\":false,\"IsKeyword\":false,\"IsPinned\":false,\"IsPinnedRoot\":false,\"IsReused\":false,\"IsRoot\":true,\"IsSourceTerm\":true,\"LocalCustomProperties\":{},\"MergedTermIds\":[],\"PathOfTerm\":\"T1\",\"TermsCount\":1}]}]");
            
            Assert.AreEqual( new Guid("5825ed63-099b-43db-bad1-4fffd9ef1b18"), request.Result[0]);
        }
    }
}
