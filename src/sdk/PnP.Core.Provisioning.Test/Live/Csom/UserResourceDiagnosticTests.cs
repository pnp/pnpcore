using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Csom
{
    [TestClass]
    public class UserResourceDiagnosticTests : LiveTestBase
    {
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task Diagnose_WhichLanguagesDoesTheSiteActuallySupport()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                IWeb web = await context.Web.GetAsync(
                    w => w.Language, w => w.IsMultilingual, w => w.SupportedUILanguageIds).ConfigureAwait(false);

                Console.WriteLine($"Web default language LCID : {web.Language} ({SafeCultureName(web.Language)})");
                Console.WriteLine($"IsMultilingual            : {web.IsMultilingual}");
                Console.WriteLine($"SupportedUILanguageIds    : " +
                    (web.SupportedUILanguageIds == null || web.SupportedUILanguageIds.Count == 0
                        ? "<none>"
                        : string.Join(", ", web.SupportedUILanguageIds.Select(l => $"{l} ({SafeCultureName(l)})"))));

                Assert.IsTrue(true, "Diagnostic only - see output.");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task RoundTrip_AgainstALanguageTheSiteActuallySupports()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                IWeb web = await context.Web.GetAsync(
                    w => w.Language, w => w.SupportedUILanguageIds).ConfigureAwait(false);

                int? targetLcid = web.SupportedUILanguageIds?
                    .Where(l => l != web.Language)
                    .Cast<int?>()
                    .FirstOrDefault();

                if (targetLcid == null)
                {
                    Assert.Inconclusive(
                        $"This site has no supported UI language other than its default ({web.Language}), so a " +
                        $"localized value cannot be stored and T6 cannot be verified here. This is a site " +
                        $"configuration matter, not a defect: enable a second language on the site (Site Settings > " +
                        $"Language Settings), or run against a multilingual site. It also confirms that backlog T2 " +
                        $"(supported UI languages) is a genuine prerequisite for T6.");
                    return;
                }

                string cultureName = SafeCultureName(targetLcid.Value);
                Console.WriteLine($"Round-tripping against supported language {targetLcid} ({cultureName})");

                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                string listTitle = $"{TestPrefix}Diag_{DateTime.UtcNow:HHmmssfff}";
                IList list = null;

                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList).ConfigureAwait(false);
                    var resource = UserResourcePath.ForList(siteId, webId, list.Id, ResourceProperty.Title);

                    string localized = $"{TestPrefix}Localized_{DateTime.UtcNow:HHmmss}";

                    await CsomRequestSender.SendAsync(context,
                        new SetValueForUICultureRequest(resource, cultureName, localized)).ConfigureAwait(false);

                    string readBack = await CsomRequestSender.SendAsync(context,
                        new GetValueForUICultureRequest(resource, cultureName)).ConfigureAwait(false);

                    Console.WriteLine($"  wrote     : {localized}");
                    Console.WriteLine($"  read back : {readBack}");

                    Assert.AreEqual(localized, readBack,
                        $"Even against a supported language ({cultureName}) the value did not round-trip. " +
                        $"That points at the CSOM request itself rather than site configuration.");
                }
                finally
                {
                    if (list != null)
                    {
                        try
                        {
                            await list.DeleteAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Could not delete test list {listTitle}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static string SafeCultureName(int lcid)
        {
            try
            {
                return new CultureInfo(lcid).Name;
            }
            catch (CultureNotFoundException)
            {
                return "<unknown>";
            }
        }
    }
}
