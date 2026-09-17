using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Audit;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.InformationPolicy;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Publishing;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Csom
{
    /// <summary>
    /// Live round trips for the smaller classic CSOM requests
    /// </summary>
    [TestClass]
    public class ClassicRequestLiveTests : LiveTestBase
    {
        #region T21 - audit settings

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Classic")]
        public async Task GetAudit_ReadsTheSiteAuditFlags()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                AuditSettingsInfo audit = await CsomRequestSender.SendAsync(context,
                    new GetAuditRequest(siteId, webId)).ConfigureAwait(false);

                Assert.IsNotNull(audit, "GetAuditRequest returned no result - the response was not parsed.");

                Console.WriteLine($"Current audit flags: {(int)audit.AuditFlags} ({audit.AuditFlags})");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Classic")]
        public async Task UpdateAudit_RoundTripsTheAuditFlags()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                AuditSettingsInfo before = await CsomRequestSender.SendAsync(context,
                    new GetAuditRequest(siteId, webId)).ConfigureAwait(false);

                Assert.IsNotNull(before);
                AuditMaskType original = before.AuditFlags;

                try
                {
                    AuditMaskType wanted = AuditMaskType.CheckOut | AuditMaskType.ObjectDelete;

                    await CsomRequestSender.SendAsync(context,
                        new UpdateAuditRequest(siteId, webId, wanted)).ConfigureAwait(false);

                    AuditSettingsInfo after = await CsomRequestSender.SendAsync(context,
                        new GetAuditRequest(siteId, webId)).ConfigureAwait(false);

                    Assert.IsNotNull(after);
                    Assert.AreEqual(wanted, after.AuditFlags,
                        "The audit flags did not round-trip. Audit.Update() is what persists them.");
                }
                finally
                {
                    try
                    {
                        await CsomRequestSender.SendAsync(context,
                            new UpdateAuditRequest(siteId, webId, original)).ConfigureAwait(false);
                        Console.WriteLine($"Restored audit flags to {(int)original}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE audit flags to {(int)original}: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region T22 - site policy

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Classic")]
        public async Task GetProjectPolicies_ListsTheAvailablePolicies()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                List<SitePolicyInfo> policies;
                try
                {
                    policies = await CsomRequestSender.SendAsync(context,
                        new GetProjectPoliciesRequest(siteId, webId)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("Information management site policy", ex);
                    return;
                }

                Assert.IsNotNull(policies, "GetProjectPoliciesRequest returned null rather than an empty list.");

                Console.WriteLine($"Site policies available: {policies.Count}");
                foreach (SitePolicyInfo policy in policies)
                {
                    Console.WriteLine($"  {policy.Name}");
                }

                if (policies.Count == 0)
                {
                    Assert.Inconclusive(
                        "The request succeeded but this site collection has no site policies defined, so the " +
                        "response parsing is unverified. Define a policy in the Content Type Hub's Policy " +
                        "Definitions to exercise it fully.");
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Classic")]
        public async Task GetCurrentlyAppliedProjectPolicy_ReadsTheAppliedPolicy()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                try
                {
                    SitePolicyInfo applied = await CsomRequestSender.SendAsync(context,
                        new GetCurrentlyAppliedProjectPolicyRequest(siteId, webId)).ConfigureAwait(false);

                    Console.WriteLine(applied == null
                        ? "No site policy is applied to this site (request succeeded)."
                        : $"Applied site policy: {applied.Name}");
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("Information management site policy", ex);
                }
            }
        }

        #endregion

        #region T20 - image renditions

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Classic")]
        public async Task GetImageRenditions_ReadsTheSiteRenditions()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    List<ImageRenditionInfo> renditions = await CsomRequestSender.SendAsync(context,
                        new GetImageRenditionsRequest()).ConfigureAwait(false);

                    Assert.IsNotNull(renditions);

                    Console.WriteLine($"Image renditions: {renditions.Count}");
                    foreach (ImageRenditionInfo rendition in renditions)
                    {
                        Console.WriteLine($"  {rendition.Id} {rendition.Name} {rendition.Width}x{rendition.Height}");
                    }
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("Publishing (image renditions)", ex);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Classic")]
        public async Task SetImageRenditions_RoundTripsARendition()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                List<ImageRenditionInfo> original;
                try
                {
                    original = await CsomRequestSender.SendAsync(context,
                        new GetImageRenditionsRequest()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("Publishing (image renditions)", ex);
                    return;
                }

                string renditionName = $"{TestPrefix}Rendition";

                try
                {
                    var wanted = new List<ImageRenditionInfo>(original)
                    {
                        new ImageRenditionInfo { Name = renditionName, Width = 640, Height = 480 },
                    };

                    await CsomRequestSender.SendAsync(context,
                        new SetImageRenditionsRequest(wanted)).ConfigureAwait(false);

                    List<ImageRenditionInfo> after = await CsomRequestSender.SendAsync(context,
                        new GetImageRenditionsRequest()).ConfigureAwait(false);

                    ImageRenditionInfo created = after.FirstOrDefault(r => r.Name == renditionName);
                    Assert.IsNotNull(created, "The new image rendition was not persisted.");
                    Assert.AreEqual(640, created.Width);
                    Assert.AreEqual(480, created.Height);

                    Assert.AreEqual(original.Count + 1, after.Count,
                        "SetRenditions replaces the whole list - the pre-existing renditions should still be there.");
                }
                finally
                {
                    try
                    {
                        await CsomRequestSender.SendAsync(context,
                            new SetImageRenditionsRequest(original)).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE image renditions: {ex.Message}");
                    }
                }
            }
        }

        #endregion
    }
}
