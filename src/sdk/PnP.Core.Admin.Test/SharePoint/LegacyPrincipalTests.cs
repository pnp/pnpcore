using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class LegacyPrincipalTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;

            // Configure the test cases to use application permissions instead of delegated permissions
            //TestCommon.Instance.UseApplicationPermissions = true;
        }

        [TestMethod]
        public async Task GetACSPrincipals()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var principals = context.GetSiteCollectionManager().GetSiteCollectionACSPrincipals();

                Assert.IsTrue(principals.Any());
            }
        }

        [TestMethod]
        public async Task GetLegacyACSPrincipalsFromAAD()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var result = context.GetSiteCollectionManager().GetLegacyServicePrincipals();
                Assert.IsNotNull(result);   
                Assert.IsTrue(result.Any());    
            }
        }

        [TestMethod]
        public async Task GetAllACSPrincipals()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var legacyServicePrincipals = context.GetSiteCollectionManager().GetLegacyServicePrincipals();
                var principals = context.GetSiteCollectionManager().GetTenantAndSiteCollectionACSPrincipals(legacyServicePrincipals);

                Assert.IsTrue(principals.Any());
            }
        }

        [TestMethod]
        public async Task GetTenantACSPrincipals()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var legacyServicePrincipals = context.GetSiteCollectionManager().GetLegacyServicePrincipals();
                var principals = context.GetSiteCollectionManager().GetTenantACSPrincipals(legacyServicePrincipals);

                Assert.IsTrue(principals.Any());
            }
        }

        [TestMethod]
        public async Task GetSharePointAddIns()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var addIns = context.GetSiteCollectionManager().GetSiteCollectionSharePointAddIns();

                Assert.IsTrue(addIns.Any());
            }
        }

        [TestMethod]
        public async Task GetSharePointAddInsVanity()
        {
            //TestCommon.Instance.Mocking = false;
            using (var contextAdmin = await TestCommon.Instance.GetContextAsync(TestCommon.TenantAdminCenterSite))
            {
                var vanityUrls = new VanityUrlOptions
                {
                    AdminCenterUri = contextAdmin.Uri
                };

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var addInsVanity = context.GetSiteCollectionManager().GetSiteCollectionSharePointAddIns(vanityUrlOptions: vanityUrls);
                    var addIns = context.GetSiteCollectionManager().GetSiteCollectionSharePointAddIns();

                    Assert.IsTrue(addInsVanity.Any());
                    Assert.IsTrue(addIns.Any());
                    Assert.IsTrue(addInsVanity.Count == addIns.Count);

                    int counter = 0;
                    foreach (var addIn in addInsVanity)
                    {
                        var addInOriginal = addIns[counter];
                        counter++;
                        Assert.IsNotNull(addInOriginal);
                        Assert.AreEqual(addIn.AppWebFullUrl, addInOriginal.AppWebFullUrl);
                        Assert.AreEqual(addIn.AppWebId, addInOriginal.AppWebId);
                        Assert.AreEqual(addIn.AssetId, addInOriginal.AssetId);
                        Assert.AreEqual(addIn.CreationTime, addInOriginal.CreationTime);
                        Assert.AreEqual(addIn.InstalledBy, addInOriginal.InstalledBy);
                        Assert.AreEqual(addIn.InstalledSiteId, addInOriginal.InstalledSiteId);
                        Assert.AreEqual(addIn.InstalledWebId, addInOriginal.InstalledWebId);
                        Assert.AreEqual(addIn.InstalledWebFullUrl, addInOriginal.InstalledWebFullUrl);
                        Assert.AreEqual(addIn.InstalledWebName, addInOriginal.InstalledWebName);
                        Assert.AreEqual(addIn.CurrentSiteId, addInOriginal.CurrentSiteId);
                        Assert.AreEqual(addIn.CurrentWebId, addInOriginal.CurrentWebId);
                        Assert.AreEqual(addIn.CurrentWebFullUrl, addInOriginal.CurrentWebFullUrl);
                        Assert.AreEqual(addIn.CurrentWebName, addInOriginal.CurrentWebName);
                        Assert.AreEqual(addIn.LaunchUrl, addInOriginal.LaunchUrl);
                        Assert.AreEqual(addIn.LicensePurchaseTime, addInOriginal.LicensePurchaseTime);
                        Assert.AreEqual(addIn.PurchaserIdentity, addInOriginal.PurchaserIdentity);
                        Assert.AreEqual(addIn.Locale, addInOriginal.Locale);
                        Assert.AreEqual(addIn.ProductId, addInOriginal.ProductId);
                        Assert.AreEqual(addIn.Status, addInOriginal.Status);
                        Assert.AreEqual(addIn.TenantAppData, addInOriginal.TenantAppData);
                        Assert.AreEqual(addIn.TenantAppDataUpdateTime, addInOriginal.TenantAppDataUpdateTime);
                        Assert.AreEqual(addIn.AppIdentifier, addInOriginal.AppIdentifier);
                        Assert.AreEqual(addIn.ServerRelativeUrl, addInOriginal.ServerRelativeUrl);
                        Assert.AreEqual(addIn.AbsoluteUrl, addInOriginal.AbsoluteUrl);
                        Assert.AreEqual(addIn.Title, addInOriginal.Title);
                    }
                }
            }
        }


        [TestMethod]
        public async Task GetACSPrincipalsVanity()
        {
            //TestCommon.Instance.Mocking = false;
            using (var contextAdmin = await TestCommon.Instance.GetContextAsync(TestCommon.TenantAdminCenterSite))
            {
                var vanityUrls = new VanityUrlOptions
                {
                    AdminCenterUri = contextAdmin.Uri
                };

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var principalsVanity = context.GetSiteCollectionManager().GetSiteCollectionACSPrincipals(vanityUrlOptions: vanityUrls);
                    var principals = context.GetSiteCollectionManager().GetSiteCollectionACSPrincipals();

                    Assert.IsTrue(principalsVanity.Any());
                    Assert.IsTrue(principals.Any());
                    Assert.IsTrue(principalsVanity.Count ==  principals.Count);

                    int counter = 0;
                    foreach (var principal in principalsVanity)
                    {
                        var principalOriginal = principals[counter];
                        counter++;
                        Assert.IsNotNull(principalOriginal);
                        Assert.AreEqual(principal.AppId, principalOriginal.AppId);
                        Assert.AreEqual(principal.Title, principalOriginal.Title);
                        Assert.AreEqual(principal.RedirectUri, principalOriginal.RedirectUri);
                        Assert.AreEqual(principal.AppDomains.Count(), principalOriginal.AppDomains.Count());
                        Assert.AreEqual(principal.ValidUntil, principalOriginal.ValidUntil);
                        Assert.AreEqual(principal.ServerRelativeUrl, principalOriginal.ServerRelativeUrl);
                        Assert.AreEqual(principal.AbsoluteUrl, principalOriginal.AbsoluteUrl);
                        Assert.AreEqual(principal.AppIdentifier, principalOriginal.AppIdentifier);
                        Assert.AreEqual(principal.AllowAppOnly, principalOriginal.AllowAppOnly);
                        Assert.AreEqual(principal.SiteCollectionScopedPermissions.Count(), principalOriginal.SiteCollectionScopedPermissions.Count());
                        Assert.AreEqual(principal.TenantScopedPermissions.Count(), principalOriginal.TenantScopedPermissions.Count());
                    }

                }
            }
        }

        [TestMethod]
        public async Task GetTenantACSPrincipalsVanity()
        {
            //TestCommon.Instance.Mocking = false;
            using (var contextAdmin = await TestCommon.Instance.GetContextAsync(TestCommon.TenantAdminCenterSite))
            {
                var vanityUrls = new VanityUrlOptions
                {
                    AdminCenterUri = contextAdmin.Uri
                };

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var legacyServicePrincipals = context.GetSiteCollectionManager().GetLegacyServicePrincipals();
                    var principalsVanity = context.GetSiteCollectionManager().GetTenantACSPrincipals(legacyServicePrincipals, vanityUrlOptions: vanityUrls);
                    var principals = context.GetSiteCollectionManager().GetTenantACSPrincipals(legacyServicePrincipals);

                    Assert.IsTrue(principalsVanity.Any());
                    Assert.IsTrue(principals.Any());
                    Assert.IsTrue(principalsVanity.Count == principals.Count);

                    int counter = 0;
                    foreach (var principal in principalsVanity)
                    {
                        var principalOriginal = principals[counter];
                        counter++;
                        Assert.IsNotNull(principalOriginal);
                        Assert.AreEqual(principal.AppId, principalOriginal.AppId);
                        Assert.AreEqual(principal.Title, principalOriginal.Title);
                        Assert.AreEqual(principal.RedirectUri, principalOriginal.RedirectUri);
                        Assert.AreEqual(principal.AppDomains.Count(), principalOriginal.AppDomains.Count());
                        Assert.AreEqual(principal.ValidUntil, principalOriginal.ValidUntil);
                        Assert.AreEqual(principal.ServerRelativeUrl, principalOriginal.ServerRelativeUrl);
                        Assert.AreEqual(principal.AbsoluteUrl, principalOriginal.AbsoluteUrl);
                        Assert.AreEqual(principal.AppIdentifier, principalOriginal.AppIdentifier);
                        Assert.AreEqual(principal.AllowAppOnly, principalOriginal.AllowAppOnly);
                        Assert.AreEqual(principal.SiteCollectionScopedPermissions.Count(), principalOriginal.SiteCollectionScopedPermissions.Count());
                        Assert.AreEqual(principal.TenantScopedPermissions.Count(), principalOriginal.TenantScopedPermissions.Count());
                    }
                }
            }
        }

    }
}
