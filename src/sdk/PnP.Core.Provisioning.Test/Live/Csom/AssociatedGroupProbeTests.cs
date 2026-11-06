using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Web;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Csom
{
    /// <summary>
    /// Establishes whether a NoScript site really refuses an associated-group assignment.
    /// </summary>
    [TestClass]
    public class AssociatedGroupProbeTests : LiveTestBase
    {
        private static string ProbeGroupTitle => $"{TestPrefix}ProbeOwners";

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Diagnostics")]
        public async Task AssociatedGroups_CanTheyBeAssignedOnANoScriptSite()
        {
            using (PnPContext context = await GetNoGroupContextAsync().ConfigureAwait(false))
            {
                bool isNoScript = await IsNoScriptAsync(context).ConfigureAwait(false);
                Console.WriteLine($"Site is NoScript: {isNoScript}");

                if (!isNoScript)
                {
                    Assert.Inconclusive("The probe needs a NoScript site and this one is not.");
                }

                await context.Web.LoadAsync(w => w.AssociatedOwnerGroup.QueryProperties(g => g.Id, g => g.Title))
                    .ConfigureAwait(false);

                int originalId = context.Web.AssociatedOwnerGroup.Id;
                string originalTitle = context.Web.AssociatedOwnerGroup.Title;
                Console.WriteLine($"Associated owner group is '{originalTitle}' ({originalId})");

                ISharePointGroup probe = null;
                bool assignmentApplied = false;

                try
                {
                    probe = await context.Web.SiteGroups.AddAsync(ProbeGroupTitle).ConfigureAwait(false);
                    Console.WriteLine($"Created probe group '{probe.Title}' ({probe.Id})");

                    (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                    try
                    {
                        await CsomRequestSender.SendAsync(context,
                            new SetAssociatedGroupRequest(siteId, webId, probe.Id, AssociatedGroupKind.Owners))
                            .ConfigureAwait(false);

                        assignmentApplied = true;
                        Console.WriteLine("The request was ACCEPTED on a NoScript site.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"The request was REFUSED on a NoScript site: {Describe(ex)}");
                        Assert.Inconclusive("SharePoint refused the assignment on a NoScript site - PnP Framework's guard is correct.");
                    }

                    using (PnPContext fresh = await GetNoGroupContextAsync(1).ConfigureAwait(false))
                    {
                        await fresh.Web.LoadAsync(w => w.AssociatedOwnerGroup.QueryProperties(g => g.Id, g => g.Title))
                            .ConfigureAwait(false);

                        Console.WriteLine($"After the request, the associated owner group is '{fresh.Web.AssociatedOwnerGroup.Title}'");

                        Assert.AreEqual(ProbeGroupTitle, fresh.Web.AssociatedOwnerGroup.Title,
                            "The request was accepted but the association did not change.");
                    }
                }
                finally
                {
                    if (assignmentApplied)
                    {
                        await RestoreAsync(originalId).ConfigureAwait(false);
                    }

                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        private static async Task RestoreAsync(int originalGroupId)
        {
            try
            {
                using (PnPContext context = await GetNoGroupContextAsync(2).ConfigureAwait(false))
                {
                    (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                    await CsomRequestSender.SendAsync(context,
                        new SetAssociatedGroupRequest(siteId, webId, originalGroupId, AssociatedGroupKind.Owners))
                        .ConfigureAwait(false);

                    Console.WriteLine($"Restored the associated owner group to {originalGroupId}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT RESTORE the associated owner group to {originalGroupId}: {Describe(ex)}");
            }
        }

        private static async Task SweepAsync()
        {
            try
            {
                using (PnPContext context = await GetNoGroupContextAsync(2).ConfigureAwait(false))
                {
                    await context.Web.LoadAsync(w => w.SiteGroups.QueryProperties(g => g.Id, g => g.Title))
                        .ConfigureAwait(false);

                    foreach (ISharePointGroup group in context.Web.SiteGroups.AsRequested()
                        .Where(g => g.Title != null && g.Title.StartsWith(TestPrefix, StringComparison.Ordinal)).ToList())
                    {
                        string name = group.Title;
                        try
                        {
                            await group.DeleteAsync().ConfigureAwait(false);
                            Console.WriteLine($"Deleted group '{name}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT DELETE group '{name}': {Describe(ex)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT SWEEP: {Describe(ex)}");
            }
        }
    }
}
