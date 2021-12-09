using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class UserProfileTests
    {
        private const string testAccountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetUserProfileTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var profile = context.Social.UserProfile.Get();

            Assert.IsNotNull(profile);
        }

        [TestMethod]
        public async Task GetUserPropertiesTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var profile = context.Social.UserProfile.GetMyProperties();

            Assert.IsNotNull(profile);
        }

        [TestMethod]
        public async Task GetUserPropertiesSelectTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var profile = context.Social.UserProfile.GetMyProperties(p => p.DisplayName);

            Assert.IsTrue(!string.IsNullOrEmpty(profile.DisplayName));
            Assert.ThrowsException<ClientException>(() => profile.UserProfileProperties);
        }

        [TestMethod]
        public async Task GetUserPropertiesSelectExceptionTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            Assert.ThrowsException<ClientException>(() => context.Social.UserProfile.GetMyProperties(p => p.DisplayName.Contains("123")));
        }

        [TestMethod]
        public async Task GetUserPropertiesForTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var profile = context.Social.UserProfile.GetPropertiesFor(testAccountName);

            Assert.IsNotNull(profile);
        }

        [TestMethod]
        public async Task GetUserProfilePropertyTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var name = context.Social.UserProfile.GetPropertyFor(testAccountName, "FirstName");

            Assert.IsTrue(!string.IsNullOrEmpty(name));
        }

        [TestMethod]
        public async Task SetSingleValueTests()
        {
            //TestCommon.Instance.Mocking = false;
            var propName = "HomePhone";

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);
            var testValue = "+54321";
            context.Social.UserProfile.SetSingleValueProfileProperty(testAccountName, propName, testValue);
            var propValue = context.Social.UserProfile.GetPropertyFor(testAccountName, propName);

            Assert.AreEqual(testValue, propValue);

            testValue = "+12345";
            context.Social.UserProfile.SetSingleValueProfileProperty(testAccountName, propName, testValue);
            propValue = context.Social.UserProfile.GetPropertyFor(testAccountName, propName);

            Assert.AreEqual(testValue, propValue);
        }

        [TestMethod]
        public async Task SetMultiValueTests()
        {
            //TestCommon.Instance.Mocking = false;
            var propName = "SPS-Skills";

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);
            var testValues = new List<string>() { "csharp", "typescript" };
            context.Social.UserProfile.SetMultiValuedProfileProperty(testAccountName, propName, testValues);
            var propValue = context.Social.UserProfile.GetPropertyFor(testAccountName, propName).Split("|").ToList();

            Assert.IsTrue(propValue.Contains("csharp"));

            testValues = new List<string>() { "python", "nodejs" };
            context.Social.UserProfile.SetMultiValuedProfileProperty(testAccountName, propName, testValues);
            propValue = context.Social.UserProfile.GetPropertyFor(testAccountName, propName).Split("|").ToList();

            Assert.IsTrue(propValue.Contains("python"));
        }

        [TestMethod]
        public async Task SetMyProfilePictureTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var bytes = System.IO.File.ReadAllBytes("TestAssets/parker-ms-300.png");
            try
            {
                context.Social.UserProfile.SetMyProfilePicture(bytes);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetUserOneDriveQuotaMaxTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var result = context.Social.UserProfile.GetUserOneDriveQuotaMax(testAccountName);

            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public async Task ResetUserOneDriveQuotaToDefaultTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var outcome = context.Social.UserProfile.ResetUserOneDriveQuotaToDefault(testAccountName);
            Assert.IsTrue(!string.IsNullOrEmpty(outcome));
        }

        [TestMethod]
        public async Task SetUserOneDriveQuotaTests()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var outcome = context.Social.UserProfile.SetUserOneDriveQuota(testAccountName, 5497558138880, 5497558138880);

            Assert.IsTrue(!string.IsNullOrEmpty(outcome));
        }
    }
}
