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
        public void SecureStringTest()
        {
            var secureString = CredentialManager.StringToSecureString("I'm a secure string");
            Assert.IsTrue(secureString != null);
            Assert.IsTrue(secureString.Length > 0);
            var insecureString = CredentialManager.SecureStringToString(secureString);
            Assert.IsTrue(insecureString == "I'm a secure string");

            var secureString2 = CredentialManager.StringToSecureString("");
            Assert.IsTrue(secureString2 != null);
            Assert.IsTrue(secureString2.Length == 0);
            var insecureString2 = CredentialManager.SecureStringToString(secureString2);
            Assert.IsTrue(insecureString2 == "");

            var secureString3 = CredentialManager.StringToSecureString(null);
            Assert.IsTrue(secureString3 != null);
            Assert.IsTrue(secureString3.Length == 0);
            var insecureString3 = CredentialManager.SecureStringToString(secureString3);
            Assert.IsTrue(insecureString3 == "");
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

            var cred = CredentialManager.GetCredential(appName);
            Assert.IsTrue(cred != null);
            Assert.IsTrue(cred.UserName == userName);
            Assert.IsTrue(cred.Password == passWord);
        }
    }
}
