using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Model.Microsoft365;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Test.Utilities;
using PnP.Core.Test.Common.Utilities;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live
{
    /// <summary>
    /// Shared helpers for the live tests.
    /// </summary>
    public abstract class LiveTestBase
    {
        /// <summary>
        /// Prefix for every artefact these tests create, so a leaked one is obvious and can be
        /// swept up by hand.
        /// </summary>
        protected const string TestPrefix = "PnPCoreProvisioningTest_";

        /// <summary>
        /// Whether the run is authenticating as an application rather than as a user.
        /// </summary>
        /// <remarks>
        /// Read from the context rather than configured separately, so it cannot disagree with the
        /// credentials actually in use. App-only changes what the tests may assume: there is no
        /// current user to own a site, and several handlers behave differently.
        /// </remarks>
        protected static async Task<bool> IsAppOnlyAsync(PnPContext context)
        {
            return await context.GetMicrosoft365Admin().AccessTokenUsesApplicationPermissionsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// The login name to make the owner of a site this suite creates.
        /// </summary>
        /// <remarks>
        /// App-only has no current user, so the owner has to come from configuration. A site
        /// created with no owner is a site nobody can administer, so a missing setting stops the
        /// test with a reason rather than creating one.
        /// </remarks>
        protected static async Task<string> SiteOwnerAsync(PnPContext seed)
        {
            if (!await IsAppOnlyAsync(seed).ConfigureAwait(false))
            {
                return (await seed.Web.GetCurrentUserAsync().ConfigureAwait(false)).LoginName;
            }

            string owner = TestCommonBase.GetConfigurationSettings()["AppOnly:SiteOwner"];

            if (string.IsNullOrWhiteSpace(owner) || owner.StartsWith("<", StringComparison.Ordinal))
            {
                Assert.Inconclusive(
                    "This run authenticates as an application, which has no current user to own a site. "
                    + "Set \"AppOnly\": { \"SiteOwner\": \"someone@yourtenant.onmicrosoft.com\" } in the "
                    + "appsettings file named by env.txt.");
            }

            return owner;
        }

        /// <summary>
        /// The site creation options for this run, so app-only creation is not attempted as a user.
        /// </summary>
        protected static SiteCreationOptions CreationOptions(PnPContext seed)
        {
            return new SiteCreationOptions
            {
                WaitForAsyncProvisioning = true,
            };
        }

        /// <summary>
        /// Skips the test with a clear reason rather than failing it, for the capabilities a
        /// tenant may legitimately not have - SP2013 workflows, publishing, site policies.
        /// </summary>
        protected static void SkipIfUnavailable(string capability, Exception ex)
        {
            Assert.Inconclusive(
                $"{capability} appears to be unavailable on this tenant, so this request could not be verified." +
                $"{Environment.NewLine}Judge for yourself from the error below - if a *related* request succeeded, " +
                $"this is a defect rather than a missing capability.{Environment.NewLine}{Environment.NewLine}" +
                $"{Describe(ex)}");
        }

        /// <summary>
        /// Renders an exception with the detail SharePoint actually returned.
        /// </summary>
        protected static string Describe(Exception ex)
        {
            var lines = new List<string>();

            for (Exception current = ex; current != null; current = current.InnerException)
            {
                lines.Add($"{current.GetType().Name}: {current.Message}");

                if (current is ServiceException serviceException && serviceException.Error is ServiceError error)
                {
                    lines.Add($"  HTTP {error.HttpResponseCode}, code '{error.Code}'");

                    if (!string.IsNullOrEmpty(error.Message))
                    {
                        lines.Add($"  {error.Message}");
                    }

                    string rendered = error.ToString();

                    if (!string.IsNullOrEmpty(rendered) && !string.Equals(rendered, error.Message, StringComparison.Ordinal))
                    {
                        lines.Add($"  {rendered}");
                    }
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Gets a context against the configured test site.
        /// </summary>
        protected static async Task<PnPContext> GetContextAsync(int id = 0)
        {
            return await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, id).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a context against the configured classic (STS#0) test site.
        /// </summary>
        protected static async Task<PnPContext> GetClassicContextAsync(int id = 0)
        {
            return await TestCommon.Instance.GetContextAsync(TestCommon.ClassicSTS0TestSite, id).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a context against the configured non-group (communication) test site.
        /// </summary>
        protected static async Task<PnPContext> GetNoGroupContextAsync(int id = 0)
        {
            return await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, id).ConfigureAwait(false);
        }

        /// <summary>
        /// Reports whether the context's web is a NoScript site.
        /// </summary>
        protected static async Task<bool> IsNoScriptAsync(PnPContext context)
        {
            try
            {
                return await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Turns NoScript off on a site this suite created.
        /// </summary>
        /// <param name="admin">A tenant admin scoped context</param>
        /// <param name="siteUrl">The site to allow scripting on</param>
        /// <returns>Whether scripting was allowed within the wait</returns>
        protected static async Task<bool> AllowScriptingAsync(PnPContext admin, Uri siteUrl)
        {
            const int attempts = 32;
            const int reissueEvery = 8;

            await SetDenyAddAndCustomizePagesAsync(admin, siteUrl).ConfigureAwait(false);

            Console.WriteLine($"Allowed scripting on {siteUrl}, waiting for it to take effect.");

            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                using (PnPContext seed = await GetContextAsync(4).ConfigureAwait(false))
                using (PnPContext site = await seed.CloneAsync(siteUrl).ConfigureAwait(false))
                {
                    if (!await site.Web.IsNoScriptSiteAsync().ConfigureAwait(false))
                    {
                        Console.WriteLine($"  scripting allowed after {attempt} check(s).");
                        return true;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);

                if (attempt % reissueEvery == 0 && attempt < attempts)
                {
                    Console.WriteLine($"  still NoScript after {attempt} check(s) - re-issuing the setting.");
                    await SetDenyAddAndCustomizePagesAsync(admin, siteUrl).ConfigureAwait(false);
                }
            }

            Console.WriteLine($"  still NoScript after {attempts * 15 / 60} minutes.");
            return false;
        }

        private static async Task SetDenyAddAndCustomizePagesAsync(PnPContext admin, Uri siteUrl)
        {
            ISiteCollectionProperties properties = await admin.GetSiteCollectionManager()
                .GetSiteCollectionPropertiesAsync(siteUrl).ConfigureAwait(false);

            properties.DenyAddAndCustomizePages = DenyAddAndCustomizePagesStatus.Disabled;
            await properties.UpdateAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a term group, emptying it first.
        /// </summary>
        protected static async Task DeleteTermGroupDeepAsync(ITermGroup group)
        {
            if (group == null)
            {
                return;
            }

            try
            {
                await group.LoadAsync(g => g.Sets.QueryProperties(s => s.Id)).ConfigureAwait(false);

                foreach (ITermSet set in group.Sets.AsRequested().ToList())
                {
                    try
                    {
                        await set.DeleteAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  could not delete term set {set.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  could not enumerate sets in group {group.Id}: {ex.Message}");
            }

            await group.DeleteAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a term group by id, emptying it first, tolerating its absence.
        /// </summary>
        protected static async Task DeleteTermGroupDeepAsync(PnPContext context, string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                return;
            }

            try
            {
                ITermGroup group = await context.TermStore.Groups.GetByIdAsync(groupId).ConfigureAwait(false);
                await DeleteTermGroupDeepAsync(group).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Removes every term group this test suite has left behind.
        /// </summary>
        protected static async Task CleanUpLeakedTermGroupsAsync(PnPContext context)
        {
            try
            {
                await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(g => g.Id, g => g.Name)).ConfigureAwait(false);

                foreach (ITermGroup group in context.TermStore.Groups.AsRequested()
                    .Where(g => g.Name != null && g.Name.StartsWith(TestPrefix, StringComparison.Ordinal))
                    .ToList())
                {
                    try
                    {
                        await DeleteTermGroupDeepAsync(group).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
