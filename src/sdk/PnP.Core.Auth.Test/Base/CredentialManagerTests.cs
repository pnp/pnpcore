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
            var secureString = "I'm a secure string".ToSecureString();
            Assert.IsTrue(secureString != null);
            Assert.IsTrue(secureString.Length > 0);
            var insecureString = secureString.ToInsecureString();
            Assert.IsTrue(insecureString == "I'm a secure string");

            var secureString2 = "".ToSecureString();
            Assert.IsTrue(secureString2 != null);
            Assert.IsTrue(secureString2.Length == 0);
            var insecureString2 = secureString2.ToInsecureString();
            Assert.IsTrue(insecureString2 == "");

            string string3 = null;
            var secureString3 = string3.ToSecureString();
            Assert.IsTrue(secureString3 != null);
            Assert.IsTrue(secureString3.Length == 0);
            var insecureString3 = secureString3.ToInsecureString();
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
            if (!OperatingSystem.IsLinux())
            {
                Assert.IsTrue(cred != null);
                Assert.IsTrue(cred.UserName == userName);
                Assert.IsTrue(cred.Password == passWord);
            }
        }
    }
}
