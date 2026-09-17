using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live
{
    /// <summary>
    /// Reports what each configured site actually is, so verification gaps can be attributed to the
    /// right cause.
    /// </summary>
    [TestClass]
    public class SiteCapabilityReportTests : LiveTestBase
    {
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Report")]
        public async Task ReportConfiguredSiteCapabilities()
        {
            await ReportAsync(TestCommon.TestSite, "TestSite").ConfigureAwait(false);
            await ReportAsync(TestCommon.ClassicSTS0TestSite, "ClassicSTS0TestSite").ConfigureAwait(false);
            await ReportAsync(TestCommon.NoGroupTestSite, "NoGroupTestSite").ConfigureAwait(false);
        }

        private static async Task ReportAsync(string configurationName, string label)
        {
            Console.WriteLine();
            Console.WriteLine($"=== {label} ===");

            try
            {
                using (PnPContext context = await TestCommon.Instance.GetContextAsync(configurationName).ConfigureAwait(false))
                {
                    IWeb web = await context.Web.GetAsync(
                        w => w.Url, w => w.Title, w => w.WebTemplate, w => w.WebTemplateConfiguration,
                        w => w.Language, w => w.IsMultilingual).ConfigureAwait(false);

                    bool noScript = await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false);
                    bool isComm = await SiteTypeHelper.IsCommunicationSiteAsync(context).ConfigureAwait(false);
                    bool isTeam = await SiteTypeHelper.IsModernTeamSiteAsync(context).ConfigureAwait(false);

                    Console.WriteLine($"  Url          : {web.Url}");
                    Console.WriteLine($"  Template     : {web.WebTemplateConfiguration} ({web.WebTemplate})");
                    Console.WriteLine($"  Kind         : {(isComm ? "communication" : isTeam ? "modern team" : "classic")}");
                    Console.WriteLine($"  NoScript     : {noScript}   <-- blocks property bag / web part / custom action WRITES");
                    Console.WriteLine($"  Multilingual : {web.IsMultilingual} (default LCID {web.Language})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  COULD NOT REACH: {Describe(ex)}");
            }
        }
    }
}
