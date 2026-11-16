using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Transform;
using PnP.Core.Modernization.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Tests.Transform.Mapping
{
    [TestClass]
    [TestCategory("Integration")]
    public class UserMappingTests
    {

        [TestMethod]
        public void UserMappingFileLoadTest()
        {
            FileManager fm = new FileManager();
            var mapping = fm.LoadUserMappingFile(@"..\..\Transform\Mapping\usermapping_sample.csv");

            Assert.IsTrue(mapping.Count > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void UserMappingFileNotFoundTest()
        {
            FileManager fm = new FileManager();
            var mapping = fm.LoadUrlMappingFile(@"..\..\Transform\Mapping\idontexist_sample.csv");
        }

        [TestMethod]
        public void GetUPNFromAccountTest()
        {


            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext())
                {
                    PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        //Permissions are should work given cross domain with mapping
                        KeepPageSpecificPermissions = true,

                        // Replace User Mapping
                        UserMappingFile = @"..\..\Transform\Mapping\usermapping_sample.csv"
                    };

                    UserTransformator userTransformator = new UserTransformator(pti, sourceClientContext, targetClientContext, null);

                    var result = userTransformator.SearchSourceDomainForUPN(AccountType.User, "test.user3");
                    Console.WriteLine(result);

                    Assert.IsTrue(!string.IsNullOrEmpty(result));

                }
            }
        }

        [TestMethod]
        public void GetUPNFromGroupTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext())
                {
                    PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,
                        // Don't log test runs
                        SkipTelemetry = true,
                        //Permissions are should work given cross domain with mapping
                        KeepPageSpecificPermissions = true,
                        // Replace User Mapping
                        UserMappingFile = @"..\..\Transform\Mapping\usermapping_sample.csv"
                    };

                    UserTransformator userTransformator = new UserTransformator(pti, sourceClientContext, targetClientContext, null);

                    var result = userTransformator.SearchSourceDomainForUPN(AccountType.Group, "SharePoint-Editors"); //My test rig has this setup
                    Console.WriteLine(result);

                    Assert.IsTrue(!string.IsNullOrEmpty(result));

                }
            }
        }

        [TestMethod]
        public void GetUPNFromGroupWithSIDTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext())
                {
                    PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,
                        // Don't log test runs
                        SkipTelemetry = true,
                        //Permissions are should work given cross domain with mapping
                        KeepPageSpecificPermissions = true,
                        // Replace User Mapping
                        UserMappingFile = @"..\..\Transform\Mapping\usermapping_sample.csv"
                    };

                    UserTransformator userTransformator = new UserTransformator(pti, sourceClientContext, targetClientContext, null);

                    // SharePoint-Readers (note specific to PBs test rig)
                    var result = userTransformator.SearchSourceDomainForUPN(AccountType.Group, "s-1-5-21-2364077317-3999105188-691961326-1129"); 
                    Console.WriteLine(result);

                    Assert.IsTrue(!string.IsNullOrEmpty(result));

                }
            }
        }

        [TestMethod]
        public void ResolveDomainFriendlyNameTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext())
                {
                    PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        //Permissions are should work given cross domain with mapping
                        KeepPageSpecificPermissions = true,

                        // Replace User Mapping
                        UserMappingFile = @"..\..\Transform\Mapping\usermapping_sample.csv"
                    };

                    UserTransformator userTransformator = new UserTransformator(pti, sourceClientContext, targetClientContext, null);

                    var result = userTransformator.ResolveFriendlyDomainToLdapDomain("ALPHADELTA");
                    Console.WriteLine(result);

                    Assert.IsTrue(!string.IsNullOrEmpty(result));

                }
            }
        }

        [TestMethod]
        public void GetComputerDomainTest()
        {

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext())
                {
                    //Doesnt matter what the settings are.
                    PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
                    {
                        // Don't log test runs
                        SkipTelemetry = true,
                    };


                    UserTransformator userTransformator = new UserTransformator(pti, sourceClientContext, targetClientContext, null);

                    var result = userTransformator.GetFriendlyComputerDomain();
                    Console.WriteLine(result);

                    Assert.IsTrue(!string.IsNullOrEmpty(result));

                }
            }


        }

        [TestMethod]
        public void GetLDAPConnectingStringTest()
        {
            //Doesnt matter what the settings are.
            PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
            {
                // Don't log test runs
                SkipTelemetry = true,
            };

            UserTransformator userTransformator = new UserTransformator(pti, null, null, null);

            var result = userTransformator.GetLDAPConnectionString();
            Console.WriteLine(result);

            Assert.IsTrue(!string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetLDAPConnectingStringProvidedByParamTest()
        {
            var ldap = "LDAP://OU=CDT,OU=Demo Users,DC=AlphaDelta,DC=Local";

            //Doesnt matter what the settings are.
            PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
            {
                // Don't log test runs
                SkipTelemetry = true,

                LDAPConnectionString = ldap
            };

            UserTransformator userTransformator = new UserTransformator(pti, null, null, null);

            var result = userTransformator.GetLDAPConnectionString();
            Console.WriteLine(result);

            Assert.IsTrue(!string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Equals(ldap, StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void GetUPNFromAccountWithCustomLDAPNegativeTest()
        {
            var ldap = "LDAP://OU=CDT,OU=Demo Users,DC=AlphaDelta,DC=Local";

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext())
                {
                    PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        //Permissions are should work given cross domain with mapping
                        KeepPageSpecificPermissions = true,

                        LDAPConnectionString = ldap
                    };

                    UserTransformator userTransformator = new UserTransformator(pti, sourceClientContext, targetClientContext, null);

                    var result = userTransformator.SearchSourceDomainForUPN(AccountType.User, "Brown.Bromley"); //User Outside of LDAP Connection String
                    Console.WriteLine(result);

                    Assert.IsTrue(string.IsNullOrEmpty(result));

                }
            }
        }

        [TestMethod]
        public void GetUPNFromAccountWithCustomLDAPPositiveTest()
        {
            var ldap = "LDAP://OU=CDT,OU=Demo Users,DC=AlphaDelta,DC=Local";

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext())
                {
                    PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        //Permissions are should work given cross domain with mapping
                        KeepPageSpecificPermissions = true,

                        LDAPConnectionString = ldap
                    };

                    UserTransformator userTransformator = new UserTransformator(pti, sourceClientContext, targetClientContext, null);

                    var result = userTransformator.SearchSourceDomainForUPN(AccountType.User, "Cara.Irvine"); //User Outside of LDAP Connection String
                    Console.WriteLine(result);

                    Assert.IsTrue(!string.IsNullOrEmpty(result));

                }
            }
        }

        [TestMethod]
        public void PrincipalAccountTest()
        {


            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext())
                {
                    PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        //Permissions are should work given cross domain with mapping
                        KeepPageSpecificPermissions = true,

                        SourceVersion = SPVersion.SP2010 //example only
                    };

                    UserTransformator userTransformator = new UserTransformator(pti, sourceClientContext, targetClientContext, null);

                    var result = userTransformator.RemapPrincipal("test.user3");
                    Console.WriteLine(result);

                    Assert.IsTrue(!string.IsNullOrEmpty(result));

                }
            }
        }
    }
}
