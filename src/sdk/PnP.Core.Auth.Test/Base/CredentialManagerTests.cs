using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace PnP.Core.Auth.Test.Base
{
    /// <summary>
    /// Tests that focus on validating the Credential Manager utility class
    /// </summary>
    [TestClass]
    public class CredentialManagerTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public void CredentialTest()
        {
            var userName = "userName";
            var passWord = "password";
            var securePassWord = "securePassword";
            var appName = "PnP.Core.Credential.Test.SecurePw";

            var result = CredentialManager.AddCredential(appName, userName, securePassWord.ToSecureString(), false);
            Assert.IsTrue(result);

            var result2 = CredentialManager.AddCredential(appName, userName, passWord, true);
            Assert.IsTrue(result2);
        }
    }
}
