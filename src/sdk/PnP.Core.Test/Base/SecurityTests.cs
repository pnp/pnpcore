using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class SecurityTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // No mocking required, however a certificate thumbprint is required
        }


        #region Security Exceptions Class Tests

        [TestMethod]
        public void SecurityExtEncryptFailTest()
        {
            // Disable test in GitHub, Reason availability of certificate in Linux environments
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var result = "encryptthisstring".Encrypt("3e69491285a9a26efea5c3aeddc75b0148040000"); //Fake
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void SecurityExtEncryptTest()
        {
            // Disable test in GitHub, Reason availability of certificate in Linux environments
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var thumbPrint = TestCommon.GetX509CertificateThumbprint();

            if (string.IsNullOrEmpty(thumbPrint) || thumbPrint.Length != 40)
            {
                Assert.Inconclusive("No valid certificate thumbprint was set in the X509CertificateThumbprint element of the json settings file");
            }

            var encryptSampleText = "EncryptThisString";
            var encryptResult = encryptSampleText.Encrypt(thumbPrint); //Fake
            Assert.IsTrue(!string.IsNullOrEmpty(encryptResult));
            Assert.IsTrue(encryptResult.Contains("=="));

            //TODO: Problems decrypting - investigate
            // Reference: https://www.pkisolutions.com/accessing-and-using-certificate-private-keys-in-net-framework-net-core/ slighty different approach
            //var decryptResult = encryptResult.Decrypt(thumbPrint);
            //Assert.AreEqual(decryptResult, encryptSampleText);

        }

        [TestMethod]
        public void SecurityExtSecureStringTest()
        {
            var sampleText = "ThisIsACoolSDK";
            var secureString = sampleText.ToSecureString();

            Assert.AreNotEqual(sampleText, secureString);
            Assert.IsTrue(secureString.Length > 0);

            var backToInSecure = secureString.ToInsecureString();
            Assert.AreEqual(sampleText, backToInSecure);
        }

        [TestMethod]
        public void SecurityExtSecureEmptyStringTest()
        {
            var sampleText = "";
            Assert.ThrowsException<ArgumentException>(() => sampleText.ToSecureString());          
        }

        [TestMethod]
        public void SecurityExtDecryptFailTest()
        {
            // Disable test in GitHub, Reason availability of certificate in Linux environments
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
            
            var result = "decryptthisstring".Decrypt("3e69491285a9a26efea5c3aeddc75b0148040000"); //Fake
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        #endregion

        #region X509 Certificate Utility Tests

        [TestMethod]
        public void X509CertUtilityEncryptNothingTest()
        {
            // Disable test in GitHub, Reason availability of certificate in Linux environments
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var certificate = X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.LocalMachine, TestCommon.GetX509CertificateThumbprint());
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Encrypt(null, true, certificate));
        }

        [TestMethod]
        public void X509CertUtilityEncryptNoCertTest()
        {
            // Disable test in GitHub, Reason availability of certificate in Linux environments
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            byte[] encoded = Encoding.UTF8.GetBytes("test");
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Encrypt(encoded, true, null));
        }

        [TestMethod]
        public void X509CertUtilityGetPublicKeyNoCertTest()
        {
            // Disable test in GitHub, Reason availability of certificate in Linux environments
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
            
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.GetPublicKey(null));
            
        }

        [TestMethod]
        public void X509CertUtilityDecryptNothingTest()
        {
            // Disable test in GitHub, Reason availability of certificate in Linux environments
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var certificate = X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.LocalMachine, TestCommon.GetX509CertificateThumbprint());
                Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Decrypt(null, true, certificate));
            
        }

        [TestMethod]
        public void X509CertUtilityDecryptNoCertTest()
        {
            // Disable test in GitHub, Reason availability of certificate in Linux environments
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            byte[] encoded = Encoding.UTF8.GetBytes("doesntmatterwhatthisis");
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Decrypt(encoded, true, null));
        }

        #endregion
    }
}
