using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Test.Services.Core.CSOM.Requests;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class ExceptionTests
    {
        /*
        Sample Microsoft Graph error - see also https://docs.microsoft.com/en-us/graph/errors
        {
            "error": 
            {
                "code": "BadRequest",
                "message": "teamId needs to be a valid GUID.",
                "innerError": 
                {
                    "request-id": "14fa2fd2-8551-4dd1-83eb-2a3b881a27be",
                    "date": "2020-05-08T11:42:43"
                }
            }
        }
        */
        private static readonly string sampleGraphError = "{\"error\":{\"code\":\"BadRequest\",\"message\":\"teamId needs to be a valid GUID.\",\"innerError\":{\"request-id\":\"14fa2fd2-8551-4dd1-83eb-2a3b881a27be\",\"date\":\"2020-05-08T11:42:43\"}}}";
        private static readonly string sampleGraphAuthError = "{\"error\":\"unauthorized_client\",\"error_description\":\"AADSTS700016: Application with identifier '11111c7f-bd7e-475c-86db-fdb8c937548e' was not found in the directory 'bertonline.onmicrosoft.com'. This can happen if the application has not been installed by the administrator of the tenant or consented to by any user in the tenant. You may have sent your authentication request to the wrong tenant. Trace ID: abb724fe-4d60-4898-a4d4-4ac9d7b01800 Correlation ID: b9b3d413-4226-4397-9612-9c213c9d68c9 Timestamp: 2020-05-28 17:38:11Z\",\"error_codes\":[700016],\"timestamp\":\"2020-05-28 17:38:11Z\",\"trace_id\":\"abb724fe-4d60-4898-a4d4-4ac9d7b01800\",\"correlation_id\":\"b9b3d413-4226-4397-9612-9c213c9d68c9\",\"error_uri\":\"https://login.microsoftonline.com/error?code=700016\"}";
        private static readonly string sampleGraphAuthResponse = "{\"token_type\":\"Bearer\",\"scope\":\"AllSites.FullControl AppCatalog.ReadWrite.All Directory.AccessAsUser.All Directory.ReadWrite.All Group.ReadWrite.All IdentityProvider.ReadWrite.All Mail.Send Reports.Read.All TermStore.ReadWrite.All User.Invite.All User.Read.All\",\"expires_in\":\"3599\",\"ext_expires_in\":\"3599\",\"expires_on\":\"1590694546\",\"not_before\":\"1590690646\",\"resource\":\"https://graph.microsoft.com\",\"access_token\":\"eyJ0eXAiOiJKV1QiLCJub25jZSI6ImxRMEdRSTdaWGNyc3p4Vk9nQWtSSFJzWU50emNDdkFnVEFjUmZaWmxnMUkiLCJhbGciOiJSUzI1NiIsIng1dCI6IkN0VHVoTUptRDVNN0RMZHpEMnYyeDNRS1NSWSIsImtpZCI6IkN0VHVoTUptRDVNN0RMZHpEMnYyeDNRS1NSWSJ9.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9kODYyM2M5ZS0zMGM3LTQ3M2EtODNiYy1kOTA3ZGY0NGEyNmUvIiwiaWF0IjoxNTkwNjkwNjQ2LCJuYmYiOjE1OTA2OTA2NDYsImV4cCI6MTU5MDY5NDU0NiwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IjQyZGdZSGk0djk2eCtNekJoK3BYOHBaeTIzdmZqTlNkSE9TMFZhbno2V04yL3FlUkhnOEIiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IlBuUCBPZmZpY2UgMzY1IE1hbmFnZW1lbnQgU2hlbGwiLCJhcHBpZCI6IjMxMzU5YzdmLWJkN2UtNDc1Yy04NmRiLWZkYjhjOTM3NTQ4ZSIsImFwcGlkYWNyIjoiMCIsImZhbWlseV9uYW1lIjoiSmFuc2VuIiwiZ2l2ZW5fbmFtZSI6IkJlcnQiLCJpcGFkZHIiOiI4MS44Mi4yMTkuMjM3IiwibmFtZSI6IkJlcnQgSmFuc2VuIChDbG91ZCkiLCJvaWQiOiIzM2FjYTMxMC1hNDg5LTQxMjEtYjg1My02NjNkMDMyN2ZlMDgiLCJwbGF0ZiI6IjE0IiwicHVpZCI6IjEwMDMwMDAwODRFNEZGNjEiLCJzY3AiOiJBbGxTaXRlcy5GdWxsQ29udHJvbCBBcHBDYXRhbG9nLlJlYWRXcml0ZS5BbGwgRGlyZWN0b3J5LkFjY2Vzc0FzVXNlci5BbGwgRGlyZWN0b3J5LlJlYWRXcml0ZS5BbGwgR3JvdXAuUmVhZFdyaXRlLkFsbCBJZGVudGl0eVByb3ZpZGVyLlJlYWRXcml0ZS5BbGwgTWFpbC5TZW5kIFJlcG9ydHMuUmVhZC5BbGwgVGVybVN0b3JlLlJlYWRXcml0ZS5BbGwgVXNlci5JbnZpdGUuQWxsIFVzZXIuUmVhZC5BbGwiLCJzdWIiOiJjaHA4ZURUVlF2Q3VGaGZ3dUtLX3hwS2FYRFVrQkM1aGswakNYMVJBSl9RIiwidGVuYW50X3JlZ2lvbl9zY29wZSI6IkVVIiwidGlkIjoiZDg2MjNjOWUtMzBjNy00NzNhLTgzYmMtZDkwN2RmNDRhMjZlIiwidW5pcXVlX25hbWUiOiJiZXJ0LmphbnNlbkBiZXJ0b25saW5lLm9ubWljcm9zb2Z0LmNvbSIsInVwbiI6ImJlcnQuamFuc2VuQGJlcnRvbmxpbmUub25taWNyb3NvZnQuY29tIiwidXRpIjoicEFrMWZSbXUyRXFsSWFNa244QW1BQSIsInZlciI6IjEuMCIsIndpZHMiOlsiNjJlOTAzOTQtNjlmNS00MjM3LTkxOTAtMDEyMTc3MTQ1ZTEwIl0sInhtc190Y2R0IjoxMzYyMDMxMTI5fQ.sjbM300ryRVXW50q_BANgFqP0DrbEPKkOc4wvW28Y0ucdIfq4ysSwxyWkDzb3vTVkSxdhgH8VUYbjS6su9iN9-CoClMqnV-krMqzyJrSJQGBNI1yw_onbyco8kYR-c7DMx8uBKIEEock_uePQiH8AL1LNCOwq-uipKCleWzdTo7EjKn8NFzeLfsy8sDzuvNK5L6Zb0CE-vjD3chz4nGziU0hwvx8G6zx_z6jxTo4iU-eVx4LAg39W4OomV-2qqmrHZBqFbas_hu1JnwiH-JCICeS76H8nZlbpCc3CnQeKZYEKtxF5SvuuD8ybDYv8EMfjXBgJwZnpimpgj889vFh6w\",\"refresh_token\":\"AQABAAAAAAAm-06blBE1TpVMil8KPQ41kQL4vl_79jGq40JGwTkWgL0CMwSeugIyr13qdvnAEdDBbgctW0J7_8gj-_1j98aBJP_sbVk6moALzLFnvTS4AxHEDRntK4Yd8lBzMI4LBSTDAjBaEZH0Kq736wdRIbq8f6LWR1AdypQVLAvqayylVrxPOT0sl4ONqH0xtA4Bd2TcpxY6z1hjpx30eE7cTfa9-VaOoIiGZxrEUl7ZR3glqmECSVR94JFftTXwOQLkZqCTUxDH55mQiWmtxPocNkHjuBxlzUpXu6BdrwcqIUL-ddQkZ_4QU-fLINu56AJN4dcNVQXZTXLxDVTu9Q4Wn7KeyExOQYKX6YvH1B_S3f3O-9_qTVDs7Y4_S2IRYRat6SZZx3LYuFz5AplU8Yijb2vwiyjq2JX2nkllmhqAf02AO55LBI4kMR8PYcFY3wdiDYkZAMIF1daTgs1ZggHUFi6M2hbqCaeOlk7Ps-X1jXfWYabWTLJRW7DBYItGYlfqC_ONhD1AqTf5AqlqFGuF9wxfAYgSa2UDezM9Tm7d6lg9PwK6jFyZbMfGo5w_3T3tv7Kt1HOb06jvcELjrXSKHFYavwL92LKNbUI1HdvcYeKv694xLTi6ZICkS03qk-KvOJ2uanOfPyYRh_e_oH4eP1LwayPvtrLVt1bU3FSyCYnjxNQEnkUh3P-k6F4e8VV09sOp9xqjtU_Xb4UzwB5Gia8QIzneMXwr8UlO-mgvkbWpxpNz1cwx2Ym4_kr0OpKK8NO4saVLTYJwoXmn63AcWV-EZMXPCLYx_ImaNaw4J2C1JM4UZnwc0H7iTAG1DsbWs58gAA\"}";
        private static readonly string sampleRestError = "{\"odata.error\":{\"code\":\"-2130575342, Microsoft.SharePoint.SPException\",\"message\":{\"lang\":\"en-US\",\"value\":\"A list, survey, discussion board, or document library with the specified title already exists in this Web site.  Please choose another title.\"}}}";
        private static readonly string sampleRestErrorBad = "{\"whatever\": \"This is not a normal error message!\"}";
        private static readonly string sampleRestSimpleError = "Plain simple error string";
        private static readonly string sampleClientError = "This is a sample client error";


        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region SharePointRestServiceException Tests

        [TestMethod]
        public void SharePointRestErrorDeserialization()
        {
            bool sharePointRestServiceExceptionThrown = false;
            SharePointRestError error = null;
            try
            {
                throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, 400, sampleRestError);
            }
            catch (ServiceException ex)
            {
                if (ex is SharePointRestServiceException)
                {
                    error = ex.Error as SharePointRestError;
                    sharePointRestServiceExceptionThrown = true;
                }
            }

            Assert.IsTrue(sharePointRestServiceExceptionThrown);
            Assert.IsTrue(!string.IsNullOrEmpty(error.Code));
            Assert.IsTrue(error.ServerErrorCode == -2130575342);
            Assert.IsTrue(!string.IsNullOrEmpty(error.Message));
            Assert.IsTrue(error.Type == ErrorType.SharePointRestServiceError);
        }

        [TestMethod]
        public void SharePointRestBadErrorDeserialization()
        {
            bool sharePointRestServiceExceptionThrown = false;
            SharePointRestError error = null;
            try
            {
                throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, 400, sampleRestErrorBad);
            }
            catch (ServiceException ex)
            {
                if (ex is SharePointRestServiceException)
                {
                    error = ex.Error as SharePointRestError;
                    sharePointRestServiceExceptionThrown = true;
                }
            }

            Assert.IsTrue(sharePointRestServiceExceptionThrown);
            Assert.IsTrue(!string.IsNullOrEmpty(error.Message));
            Assert.IsTrue(error.Message == sampleRestErrorBad);
        }

        [TestMethod]
        public void SharePointRestSimpleErrorDeserialization()
        {
            bool sharePointRestServiceExceptionThrown = false;
            SharePointRestError error = null;
            try
            {
                throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, 400, sampleRestSimpleError);
            }
            catch (ServiceException ex)
            {
                if (ex is SharePointRestServiceException)
                {
                    error = ex.Error as SharePointRestError;
                    sharePointRestServiceExceptionThrown = true;
                }
            }

            Assert.IsTrue(sharePointRestServiceExceptionThrown);
            Assert.IsTrue(string.IsNullOrEmpty(error.Code));
            Assert.IsTrue(!string.IsNullOrEmpty(error.Message));
            Assert.IsTrue(error.Type == ErrorType.SharePointRestServiceError);
        }

        [TestMethod]
        public void SharePointRestErrorToStringDeserialization()
        {
            bool sharePointRestServiceExceptionThrown = false;
            SharePointRestError error = null;

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { PnPConstants.SPRequestGuidHeader, "E91534F0-D3B4-4062-B872-41301FC1EC86" },
                { PnPConstants.XSPServerStateHeader, "0" },
                { PnPConstants.XSharePointHealthScoreHeader, "2" },
                { PnPConstants.SPClientServiceRequestDurationHeader, "13" }
            };

            try
            {
                SharePointRestServiceException restException = new SharePointRestServiceException(ErrorType.SharePointRestServiceError, 400, sampleRestSimpleError, headers);
                restException.Error.AdditionalData.Add("Extra1", "extra error data");
                restException.Error.AdditionalData.Add("Extra2", true);

                throw restException;
            }
            catch (ServiceException ex)
            {
                if (ex is SharePointRestServiceException)
                {
                    error = ex.Error as SharePointRestError;
                    sharePointRestServiceExceptionThrown = true;
                }
            }

            Assert.IsTrue(error.ClientRequestId == "E91534F0-D3B4-4062-B872-41301FC1EC86");

            var restErrorString = error.ToString();

            Assert.IsTrue(restErrorString.Contains("HttpResponseCode:"));
            Assert.IsTrue(restErrorString.Contains("Code:"));
            Assert.IsTrue(restErrorString.Contains("Message:"));
            Assert.IsTrue(restErrorString.Contains("ClientRequestId:"));
            Assert.IsTrue(restErrorString.Contains("Extra1: extra error data"));
            Assert.IsTrue(restErrorString.Contains("Extra2: True"));
            Assert.IsTrue(restErrorString.Contains("SPClientServiceRequestDuration: 13"));
            Assert.IsTrue(restErrorString.Contains("X-SharePointHealthScore: 2"));
            Assert.IsTrue(restErrorString.Contains("X-SP-SERVERSTATE: 0"));

            Assert.IsTrue(sharePointRestServiceExceptionThrown);
            Assert.IsTrue(string.IsNullOrEmpty(error.Code));
            Assert.IsTrue(!string.IsNullOrEmpty(error.Message));
            Assert.IsTrue(error.Type == ErrorType.SharePointRestServiceError);
        }

        [TestMethod]
        public async Task ThrowSharePointRestServiceException()
        {
            var listTitle = "Fail list";

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                try
                {
                    bool SharePointRestServiceExceptionThrown = false;
                    SharePointRestError error = null;
                    try
                    {
                        // try adding the same list twice, will always result in an error
                        await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                        await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }
                    catch (ServiceException ex)
                    {
                        if (ex is SharePointRestServiceException)
                        {
                            error = ex.Error as SharePointRestError;
                            SharePointRestServiceExceptionThrown = true;
                        }
                    }

                    Assert.IsTrue(SharePointRestServiceExceptionThrown);
                    Assert.IsTrue(!string.IsNullOrEmpty(error.Code));
                    Assert.IsTrue(!string.IsNullOrEmpty(error.Message));
                    Assert.IsTrue(error.Type == ErrorType.SharePointRestServiceError);
                }
                finally
                {
                    // Clean up
                    var listToDelete = context.Web.Lists.FirstOrDefault(l => l.Title == listTitle);
                    if (listToDelete != null)
                    {
                        await listToDelete.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public void SharePointRestServiceExceptionDeserializationSingleParam()
        {
            var input = "test";
            try
            {
                throw new SharePointRestServiceException(input);
            }
            catch (SharePointRestServiceException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(input, ex.Message);
            }
        }

        [TestMethod]
        public void SharePointRestServiceExceptionDeserializationExceptionParam()
        {
            var input = "test";
            try
            {
                throw new SharePointRestServiceException(sampleRestError, new Exception(input));
            }
            catch (SharePointRestServiceException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(sampleRestError, ex.Message);
                Assert.IsInstanceOfType(ex.InnerException, typeof(Exception));
            }
        }

        [TestMethod]
        public void SharePointRestServiceExceptionDeserializationToString()
        {
            var input = "test";
            try
            {
                throw new SharePointRestServiceException(input);
            }
            catch (SharePointRestServiceException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.ToString()));
                Assert.AreEqual(input, ex.Message);
            }
        }

        [TestMethod]
        public void SharePointRestServiceExceptionDeserializationNoParam()
        {
            var input = "Exception of type 'PnP.Core.SharePointRestServiceException' was thrown.";
            try
            {
                throw new SharePointRestServiceException();
            }
            catch (SharePointRestServiceException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(input, ex.Message);
            }
        }

        [TestMethod]
        public async Task RestErrorLoggingOfFailingRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string error = null;
                try
                {
                    var apiRequest = new ApiRequest(ApiRequestType.SPORest, "_api/web/lists/getbytitle('nonexisting')");
                    await context.Web.ExecuteRequestAsync(apiRequest);
                }
                catch (SharePointRestServiceException ex)
                {
                    error = ex.ToString();
                }

                Assert.IsTrue(!string.IsNullOrEmpty(error));

            }
        }
        #endregion

        #region ClientException Tests

        [TestMethod]
        public void ClientExceptionDeserialization()
        {
            ClientError error;
            bool clientExceptionThrown;
            try
            {
                throw new ClientException(ErrorType.MissingAddApiHandler, sampleClientError);
            }
            catch (ClientException ex)
            {
                error = ex.Error as ClientError;
                clientExceptionThrown = true;
            }

            Assert.IsTrue(clientExceptionThrown);
            Assert.IsTrue(!string.IsNullOrEmpty(error.Message));
            Assert.IsTrue(error.Type == ErrorType.MissingAddApiHandler);
        }

        [TestMethod]
        public void ClientExceptionDeserializationExceptionParam()
        {
            var input = "test";
            try
            {
                throw new ClientException(sampleGraphAuthError, new Exception(input));
            }
            catch (ClientException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(sampleGraphAuthError, ex.Message);
                Assert.IsInstanceOfType(ex.InnerException, typeof(Exception));
            }
        }

        [TestMethod]
        public void ClientExceptionDeserializationSingleParam()
        {
            var input = "test";
            try
            {
                throw new ClientException(input);
            }
            catch (ClientException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(input, ex.Message);
            }
        }

        [TestMethod]
        public void ClientExceptionDeserializationTypeParam()
        {
            var input = "test";
            try
            {
                throw new ClientException(ErrorType.AzureADError, sampleGraphAuthError, new Exception(input));
            }
            catch (ClientException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(sampleGraphAuthError, ex.Message);
                Assert.IsInstanceOfType(ex.InnerException, typeof(Exception));
            }
        }

        [TestMethod]
        public void ClientExceptionDeserializationToString()
        {
            var input = "test";
            try
            {
                throw new ClientException(input);
            }
            catch (ClientException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.ToString()));
                Assert.AreEqual(input, ex.Message);
            }
        }

        [TestMethod]
        public void ClientExceptionDeserializationNoParam()
        {
            var input = "Exception of type 'PnP.Core.ClientException' was thrown.";
            try
            {
                throw new ClientException();
            }
            catch (ClientException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(input, ex.Message);
            }
        }

        #endregion

        #region MicrosoftGraphServiceException Tests

        [TestMethod]
        public async Task ThrowGraphServiceException()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                bool microsoftGraphServiceExceptionThrown = false;
                MicrosoftGraphError error = null;
                try
                {
                    // Send a wrong Graph query to trigger a graph exception
                    await (context.Team as Team).RawRequestAsync(new ApiCall("sites/bla", ApiType.Graph), HttpMethod.Get);
                }
                catch (ServiceException ex)
                {
                    if (ex is MicrosoftGraphServiceException)
                    {
                        error = ex.Error as MicrosoftGraphError;
                        microsoftGraphServiceExceptionThrown = true;
                    }
                }

                Assert.IsTrue(microsoftGraphServiceExceptionThrown);
                Assert.IsTrue(!string.IsNullOrEmpty(error.Code));
                Assert.IsTrue(!string.IsNullOrEmpty(error.Message));
                Assert.IsTrue(error.Type == ErrorType.GraphServiceError);
            }
        }

        [TestMethod]
        public async Task VerifyGraphServiceExceptionToString()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                bool microsoftGraphServiceExceptionThrown = false;
                string stringRepresentation = null;
                try
                {
                    // try adding a channel while the Team was not yet loaded, should result in not correctly formatted query sent to graph
                    await context.Team.Channels.AddAsync("Fail channel");
                }
                catch (ServiceException ex)
                {
                    if (ex is MicrosoftGraphServiceException)
                    {
                        microsoftGraphServiceExceptionThrown = true;
                        stringRepresentation = ex.ToString();
                    }
                }

                Assert.IsTrue(microsoftGraphServiceExceptionThrown);
                Assert.IsFalse(string.IsNullOrEmpty(stringRepresentation));
                Assert.IsTrue(stringRepresentation.Contains("HttpResponseCode:"));
                Assert.IsTrue(stringRepresentation.Contains("Code:"));
            }
        }

        [TestMethod]
        public void GraphErrorDeserialization()
        {
            bool microsoftGraphServiceExceptionThrown = false;
            MicrosoftGraphError error = null;
            try
            {
                throw new MicrosoftGraphServiceException(ErrorType.GraphServiceError, 400, sampleGraphError);
            }
            catch (ServiceException ex)
            {
                if (ex is MicrosoftGraphServiceException)
                {
                    error = ex.Error as MicrosoftGraphError;
                    microsoftGraphServiceExceptionThrown = true;
                }
            }

            Assert.IsTrue(microsoftGraphServiceExceptionThrown);
            Assert.IsTrue(!string.IsNullOrEmpty(error.Code));
            Assert.IsTrue(!string.IsNullOrEmpty(error.Message));
            Assert.IsTrue(error.Type == ErrorType.GraphServiceError);
        }

        [TestMethod]
        public void GraphServiceExceptionTestNoParam()
        {
            bool microsoftGraphServiceExceptionThrown = false;
            try
            {
                throw new MicrosoftGraphServiceException();
            }
            catch (ServiceException ex)
            {
                if (ex is MicrosoftGraphServiceException)
                {
                    microsoftGraphServiceExceptionThrown = true;
                    Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                }
            }

            Assert.IsTrue(microsoftGraphServiceExceptionThrown);
        }

        [TestMethod]
        public void GraphServiceExceptionTestStringParam()
        {
            bool microsoftGraphServiceExceptionThrown = false;

            try
            {
                throw new MicrosoftGraphServiceException(sampleGraphError);
            }
            catch (ServiceException ex)
            {
                if (ex is MicrosoftGraphServiceException)
                {

                    microsoftGraphServiceExceptionThrown = true;
                    Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                }
            }

            Assert.IsTrue(microsoftGraphServiceExceptionThrown);
        }

        [TestMethod]
        public void GraphServiceExceptionTestWithExceptionParam()
        {
            bool microsoftGraphServiceExceptionThrown = false;

            try
            {
                throw new MicrosoftGraphServiceException(sampleGraphError, new Exception("test"));
            }
            catch (ServiceException ex)
            {
                if (ex is MicrosoftGraphServiceException)
                {

                    microsoftGraphServiceExceptionThrown = true;
                }

                Assert.IsInstanceOfType(ex.InnerException, typeof(Exception));
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
            }

            Assert.IsTrue(microsoftGraphServiceExceptionThrown);
        }

        #endregion

        #region AuthenticationException Tests

        [TestMethod]
        public void AuthenticationErrorDeserialization()
        {
            bool AuthenticationExceptionThrown;
            AuthenticationError error;
            try
            {
                throw new AuthenticationException(ErrorType.AzureADError, sampleGraphAuthError);
            }
            catch (AuthenticationException ex)
            {
                error = ex.Error as AuthenticationError;
                AuthenticationExceptionThrown = true;
            }

            Assert.IsTrue(AuthenticationExceptionThrown);
            Assert.IsTrue(!string.IsNullOrEmpty(error.Code));
            Assert.IsTrue(!string.IsNullOrEmpty(error.Message));
            Assert.IsTrue(!string.IsNullOrEmpty(error.TimeStamp));
            Assert.IsTrue(error.TraceId != Guid.Empty);
            Assert.IsTrue(error.CorrelationId != Guid.Empty);
            Assert.IsTrue(error.Type == ErrorType.AzureADError);
            Assert.IsTrue(error.ErrorCodes.Count == 1);
        }

        [TestMethod]
        public void AuthenticationErrorDeserializationSingleParam()
        {
            var input = "test";
            try
            {
                throw new AuthenticationException(input);
            }
            catch (AuthenticationException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(input, ex.Message);
            }
        }

        [TestMethod]
        public void AuthenticationErrorDeserializationTypeParam()
        {
            var input = "test";
            try
            {
                throw new AuthenticationException(ErrorType.AzureADError, sampleGraphAuthError, new Exception(input));
            }
            catch (AuthenticationException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(sampleGraphAuthError, ex.Message);
                Assert.IsInstanceOfType(ex.InnerException, typeof(Exception));
            }
        }

        [TestMethod]
        public void AuthenticationErrorDeserializationExceptionParam()
        {
            var input = "test";
            try
            {
                throw new AuthenticationException(sampleGraphAuthError, new Exception(input));
            }
            catch (AuthenticationException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(sampleGraphAuthError, ex.Message);
                Assert.IsInstanceOfType(ex.InnerException, typeof(Exception));
            }
        }

        [TestMethod]
        public void AuthenticationErrorDeserializationToString()
        {
            var input = "test";
            try
            {
                throw new AuthenticationException(input);
            }
            catch (AuthenticationException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.ToString()));
                Assert.AreEqual(input, ex.Message);
            }
        }

        [TestMethod]
        public void AuthenticationErrorDeserializationNoParam()
        {
            var input = "Exception of type 'PnP.Core.AuthenticationException' was thrown.";
            try
            {
                throw new AuthenticationException();
            }
            catch (AuthenticationException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(input, ex.Message);
            }
        }

        [TestMethod]
        public void VerifyAuthenticationErrorToString()
        {
            bool AuthenticationExceptionThrown;
            string stringRepresentation;
            try
            {
                throw new AuthenticationException(ErrorType.AzureADError, sampleGraphAuthError);
            }
            catch (AuthenticationException ex)
            {
                AuthenticationExceptionThrown = true;
                stringRepresentation = ex.ToString();
            }

            Assert.IsTrue(AuthenticationExceptionThrown);
            Assert.IsTrue(!string.IsNullOrEmpty(stringRepresentation));
            Assert.IsTrue(stringRepresentation.Contains("Error:"));
            Assert.IsTrue(stringRepresentation.Contains("Message:"));
            Assert.IsTrue(stringRepresentation.Contains("TimeStamp:"));
            Assert.IsTrue(stringRepresentation.Contains("TraceId:"));
            Assert.IsTrue(stringRepresentation.Contains("CorrelationId:"));
        }

        [TestMethod]
        public void AuthResponseParsingSuccess()
        {
            var tokenResult = JsonSerializer.Deserialize<JsonElement>(sampleGraphAuthResponse);

            string token = null;
            if (tokenResult.TryGetProperty("access_token", out JsonElement accessToken))
            {
                token = accessToken.GetString();
            }

            Assert.IsTrue(!string.IsNullOrEmpty(token));
            Assert.IsTrue(token == "eyJ0eXAiOiJKV1QiLCJub25jZSI6ImxRMEdRSTdaWGNyc3p4Vk9nQWtSSFJzWU50emNDdkFnVEFjUmZaWmxnMUkiLCJhbGciOiJSUzI1NiIsIng1dCI6IkN0VHVoTUptRDVNN0RMZHpEMnYyeDNRS1NSWSIsImtpZCI6IkN0VHVoTUptRDVNN0RMZHpEMnYyeDNRS1NSWSJ9.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9kODYyM2M5ZS0zMGM3LTQ3M2EtODNiYy1kOTA3ZGY0NGEyNmUvIiwiaWF0IjoxNTkwNjkwNjQ2LCJuYmYiOjE1OTA2OTA2NDYsImV4cCI6MTU5MDY5NDU0NiwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IjQyZGdZSGk0djk2eCtNekJoK3BYOHBaeTIzdmZqTlNkSE9TMFZhbno2V04yL3FlUkhnOEIiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IlBuUCBPZmZpY2UgMzY1IE1hbmFnZW1lbnQgU2hlbGwiLCJhcHBpZCI6IjMxMzU5YzdmLWJkN2UtNDc1Yy04NmRiLWZkYjhjOTM3NTQ4ZSIsImFwcGlkYWNyIjoiMCIsImZhbWlseV9uYW1lIjoiSmFuc2VuIiwiZ2l2ZW5fbmFtZSI6IkJlcnQiLCJpcGFkZHIiOiI4MS44Mi4yMTkuMjM3IiwibmFtZSI6IkJlcnQgSmFuc2VuIChDbG91ZCkiLCJvaWQiOiIzM2FjYTMxMC1hNDg5LTQxMjEtYjg1My02NjNkMDMyN2ZlMDgiLCJwbGF0ZiI6IjE0IiwicHVpZCI6IjEwMDMwMDAwODRFNEZGNjEiLCJzY3AiOiJBbGxTaXRlcy5GdWxsQ29udHJvbCBBcHBDYXRhbG9nLlJlYWRXcml0ZS5BbGwgRGlyZWN0b3J5LkFjY2Vzc0FzVXNlci5BbGwgRGlyZWN0b3J5LlJlYWRXcml0ZS5BbGwgR3JvdXAuUmVhZFdyaXRlLkFsbCBJZGVudGl0eVByb3ZpZGVyLlJlYWRXcml0ZS5BbGwgTWFpbC5TZW5kIFJlcG9ydHMuUmVhZC5BbGwgVGVybVN0b3JlLlJlYWRXcml0ZS5BbGwgVXNlci5JbnZpdGUuQWxsIFVzZXIuUmVhZC5BbGwiLCJzdWIiOiJjaHA4ZURUVlF2Q3VGaGZ3dUtLX3hwS2FYRFVrQkM1aGswakNYMVJBSl9RIiwidGVuYW50X3JlZ2lvbl9zY29wZSI6IkVVIiwidGlkIjoiZDg2MjNjOWUtMzBjNy00NzNhLTgzYmMtZDkwN2RmNDRhMjZlIiwidW5pcXVlX25hbWUiOiJiZXJ0LmphbnNlbkBiZXJ0b25saW5lLm9ubWljcm9zb2Z0LmNvbSIsInVwbiI6ImJlcnQuamFuc2VuQGJlcnRvbmxpbmUub25taWNyb3NvZnQuY29tIiwidXRpIjoicEFrMWZSbXUyRXFsSWFNa244QW1BQSIsInZlciI6IjEuMCIsIndpZHMiOlsiNjJlOTAzOTQtNjlmNS00MjM3LTkxOTAtMDEyMTc3MTQ1ZTEwIl0sInhtc190Y2R0IjoxMzYyMDMxMTI5fQ.sjbM300ryRVXW50q_BANgFqP0DrbEPKkOc4wvW28Y0ucdIfq4ysSwxyWkDzb3vTVkSxdhgH8VUYbjS6su9iN9-CoClMqnV-krMqzyJrSJQGBNI1yw_onbyco8kYR-c7DMx8uBKIEEock_uePQiH8AL1LNCOwq-uipKCleWzdTo7EjKn8NFzeLfsy8sDzuvNK5L6Zb0CE-vjD3chz4nGziU0hwvx8G6zx_z6jxTo4iU-eVx4LAg39W4OomV-2qqmrHZBqFbas_hu1JnwiH-JCICeS76H8nZlbpCc3CnQeKZYEKtxF5SvuuD8ybDYv8EMfjXBgJwZnpimpgj889vFh6w");
        }

        [TestMethod]
        public void AuthResponseParsingFail()
        {
            bool AuthenticationExceptionThrown = false;
            try
            {
                var tokenResult = JsonSerializer.Deserialize<JsonElement>(sampleGraphAuthError);

                string token = null;
                if (tokenResult.TryGetProperty("access_token", out JsonElement accessToken))
                {
                    token = accessToken.GetString();
                }
                else
                {
                    throw new AuthenticationException(ErrorType.AzureADError, sampleGraphAuthError);
                }
            }
            catch (AuthenticationException)
            {
                AuthenticationExceptionThrown = true;
            }

            Assert.IsTrue(AuthenticationExceptionThrown);
        }

        #endregion

        #region CSOMServiceException Tests

        [TestMethod]
        public async Task ThrowCSOMServiceException()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web as Web;

                var apiCall = new ApiCall(new List<IRequest<object>>() { new GetTitleErrorRequest() });

                bool exceptionThrown = false;
                try
                {
                    var response = await web.RawRequestAsync(apiCall, HttpMethod.Post);
                }
                catch (CsomServiceException)
                {
                    exceptionThrown = true;
                }

                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestMethod]
        public async Task VerifyCSOMErrorToString()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web as Web;

                var apiCall = new ApiCall(new List<IRequest<object>>() { new GetTitleErrorRequest() });

                bool exceptionThrown = false;
                string stringRepresentation = null;
                try
                {
                    var response = await web.RawRequestAsync(apiCall, HttpMethod.Post);
                }
                catch (CsomServiceException ex)
                {
                    exceptionThrown = true;
                    stringRepresentation = ex.ToString();
                }

                Assert.IsTrue(exceptionThrown);
                Assert.IsTrue(!string.IsNullOrEmpty(stringRepresentation));
                Assert.IsTrue(stringRepresentation.Contains("Message:"));
                Assert.IsTrue(stringRepresentation.Contains("ClientRequestId:"));

            }
        }

        [TestMethod]
        public void CsomServiceExceptionDeserializationNoParam()
        {
            var input = "Exception of type 'PnP.Core.CsomServiceException' was thrown.";
            try
            {
                throw new CsomServiceException();
            }
            catch (CsomServiceException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(input, ex.Message);
            }
        }

        [TestMethod]
        public void CsomServiceExceptionExceptionParam()
        {
            var input = "test";
            try
            {
                throw new CsomServiceException(sampleGraphAuthError, new Exception(input));
            }
            catch (CsomServiceException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(sampleGraphAuthError, ex.Message);
                Assert.IsInstanceOfType(ex.InnerException, typeof(Exception));
            }
        }

        [TestMethod]
        public void CsomServiceExceptionSingleParam()
        {
            var input = "test";
            try
            {
                throw new CsomServiceException(input);
            }
            catch (CsomServiceException ex)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(ex.Message));
                Assert.AreEqual(input, ex.Message);
            }
        }


        #endregion
    }
}
