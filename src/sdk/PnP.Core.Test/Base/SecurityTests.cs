using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class SecurityTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }



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
    }
}
