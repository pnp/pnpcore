using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PnP.Core.Auth.Test.Base
{
    [TestClass]
    public class SecurityTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Install the debug cert in the certstore ~ this works on Linux as well
            string path = $"TestAssets{Path.DirectorySeparatorChar}pnp.pfx";
            using (X509Certificate2 certificate = new X509Certificate2(path, "PnPRocks!", X509KeyStorageFlags.PersistKeySet))
            {
                X509Store xstore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                xstore.Open(OpenFlags.ReadWrite);
                xstore.Add(certificate);
                xstore.Close();
            }
        }

        #region Security Exceptions Class Tests

        [TestMethod]
        public void SecurityExtEncryptFailTest()
        {
            var result = "encryptthisstring".Encrypt("3e69491285a9a26efea5c3aeddc75b0148040000"); //Fake
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void SecurityExtEncryptTest()
        {
            var thumbPrint = TestCommon.GetX509CertificateThumbprint();

            if (string.IsNullOrEmpty(thumbPrint) || thumbPrint.Length != 40)
            {
                Assert.Inconclusive("No valid certificate thumbprint was set in the X509CertificateThumbprint element of the json settings file");
            }

            var encryptSampleText = "EncryptThisString";
            var encryptResult = encryptSampleText.Encrypt(thumbPrint); //Fake
            Assert.IsTrue(!string.IsNullOrEmpty(encryptResult));
            Assert.IsTrue(encryptResult.Contains("=="));

            var decryptResult = encryptResult.Decrypt(thumbPrint);
            Assert.AreEqual(decryptResult, encryptSampleText);

        }

        [TestMethod]
        public void SecurityExtSecureStringTest()
        {
            var sampleText = "ThisIsACoolSDK";
            var secureString = sampleText.ToSecureString();

            Assert.AreNotEqual(sampleText, secureString.ToString());
            Assert.IsTrue(secureString.Length > 0);

            var backToInSecure = secureString.ToInsecureString();
            Assert.AreEqual(sampleText, backToInSecure);
        }

        [TestMethod]
        public void SecurityExtDecryptFailTest()
        {
            var result = "decryptthisstring".Decrypt("3e69491285a9a26efea5c3aeddc75b0148040000"); //Fake
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        #endregion

        #region X509 Certificate Utility Tests

        [TestMethod]
        public void X509CertUtilityEncryptNothingTest()
        {
            var certificate = X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.CurrentUser, TestCommon.GetX509CertificateThumbprint());
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Encrypt(null, certificate));
        }

        [TestMethod]
        public void X509CertUtilityEncryptNoCertTest()
        {
            byte[] encoded = Encoding.UTF8.GetBytes("test");
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Encrypt(encoded, null));
        }

        [TestMethod]
        public void X509CertUtilityDecryptNothingTest()
        {
            var certificate = X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.CurrentUser, TestCommon.GetX509CertificateThumbprint());
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Decrypt(null, certificate));
        }

        [TestMethod]
        public void X509CertUtilityDecryptNoCertTest()
        {
            byte[] encoded = Encoding.UTF8.GetBytes("doesntmatterwhatthisis");
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Decrypt(encoded, null));
        }

        #endregion
    }
}
