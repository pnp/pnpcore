using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
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
        private static readonly string sampleRestError = "{\"error\":{\"code\":\"-2130575342, Microsoft.SharePoint.SPException\",\"message\":{\"lang\":\"en-US\",\"value\":\"A list, survey, discussion board, or document library with the specified title already exists in this Web site.  Please choose another title.\"}}}";
        private static readonly string sampleRestSimpleError = "Plain simple error string";
        private static readonly string sampleClientError = "This is a sample client error";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
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
        public void ClientErrorDeserialization()
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
        public async Task ThrowGraphServiceException()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                bool microsoftGraphServiceExceptionThrown = false;
                MicrosoftGraphError error = null;
                try
                {
                    // try adding a channel while the Team was not yet loaded, should result in not correctly formatted query sent to graph
                    await context.Team.Channels.AddAsync("Fail channel");
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
            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
        public async Task ThrowSharePointRestServiceException()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                bool SharePointRestServiceExceptionThrown = false;
                SharePointRestError error = null;
                try
                {
                    // try adding the same list twice, will always result in an error
                    await context.Web.Lists.AddAsync("Fail list", ListTemplateType.GenericList);
                    await context.Web.Lists.AddAsync("Fail list", ListTemplateType.GenericList);
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
        }

    }
}
