using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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
            //TestCommon.Instance.Mocking = false;

            var result = "encryptthisstring".Encrypt("3e69491285a9a26efea5c3aeddc75b0148040000"); //Fake
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void SecurityExtEncryptTest()
        {
            var encryptSampleText = "EncryptThisString";
            var encryptResult = encryptSampleText.Encrypt(TestCommon.GetX509CertificateThumbprint()); //Fake
            Assert.IsTrue(!string.IsNullOrEmpty(encryptResult));
            Assert.IsTrue(encryptResult.Contains("=="));

            //TODO: Problems decrypting - investigate
            // Reference: https://www.pkisolutions.com/accessing-and-using-certificate-private-keys-in-net-framework-net-core/ slighty different approach
            //var decryptResult = encryptResult.Decrypt(TestCommon.GetX509CertificateThumbprint());
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
            var result = "decryptthisstring".Decrypt("3e69491285a9a26efea5c3aeddc75b0148040000"); //Fake
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        #endregion

        #region X509 Certificate Utility Tests

        [TestMethod]
        public void X509CertUtilityEncryptNothingTest()
        {
            var certificate = X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.LocalMachine, TestCommon.GetX509CertificateThumbprint());
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Encrypt(null, true, certificate));
        }

        [TestMethod]
        public void X509CertUtilityEncryptNoCertTest()
        {
            byte[] encoded = Encoding.UTF8.GetBytes("test");
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Encrypt(encoded, true, null));
        }

        [TestMethod]
        public void X509CertUtilityGetPublicKeyNoCertTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.GetPublicKey(null));
        }

        [TestMethod]
        public void X509CertUtilityDecryptNothingTest()
        {
            var certificate = X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.LocalMachine, TestCommon.GetX509CertificateThumbprint());
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Decrypt(null, true, certificate));
        }

        [TestMethod]
        public void X509CertUtilityDecryptNoCertTest()
        {
            byte[] encoded = Encoding.UTF8.GetBytes("doesntmatterwhatthisis");
            Assert.ThrowsException<ArgumentNullException>(() => X509CertificateUtility.Decrypt(encoded, true, null));
        }

        #endregion
    }
}
