using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Lists;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Establishes what list creation can express, before <c>ObjectListInstance</c> is written on
    /// top of it.
    /// </summary>
    [TestClass]
    public class ListCreationProbeTests : LiveTestBase
    {
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Lists_PnPCoreDerivesTheUrlFromTheTitle_KnownLimitation()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string title = $"{TestPrefix}CoreProbe";

                try
                {
                    IList created = await context.Web.Lists.AddAsync(title, ListTemplateType.GenericList).ConfigureAwait(false);
                    await created.LoadAsync(l => l.Title, l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl)).ConfigureAwait(false);

                    Console.WriteLine($"Title '{created.Title}' -> url '{created.RootFolder.ServerRelativeUrl}'");

                    StringAssert.EndsWith(created.RootFolder.ServerRelativeUrl, title,
                        "PnP Core derives a new list's url from its title. If this changed, CreateListRequest may no " +
                        "longer be needed - check whether AddAsync gained a url parameter.");
                }
                finally
                {
                    await DeleteListByTitleAsync(title).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Lists_CreateListRequestHonoursAUrlThatDiffersFromTheTitle()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string title = $"{TestPrefix}Csom Probe";
                string url = $"Lists/{TestPrefix}CsomProbeUrl";

                try
                {
                    await context.Web.LoadAsync(w => w.ServerRelativeUrl).ConfigureAwait(false);
                    (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                    CreatedListInfo info = await CsomRequestSender.SendAsync(context, new CreateListRequest(
                        siteId, webId, title, url, "Created by a live probe",
                        (int)ListTemplateType.GenericList, Guid.Empty, onQuickLaunch: false)).ConfigureAwait(false);

                    Assert.IsNotNull(info, "The request produced no result at all.");
                    Guid listId = info.Id;

                    Console.WriteLine($"CreateListRequest returned list id {listId}");

                    Assert.AreNotEqual(Guid.Empty, listId,
                        "The request did not read the new list's id back out of the response.");

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        await fresh.Web.LoadAsync(w => w.ServerRelativeUrl, w => w.Lists.QueryProperties(
                            l => l.Id, l => l.Title, l => l.Description,
                            l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

                        IList created = fresh.Web.Lists.AsRequested().FirstOrDefault(l => l.Id == listId);

                        Assert.IsNotNull(created, "The list was not created, or its id was read back wrong.");
                        Console.WriteLine($"Title '{created.Title}' -> url '{created.RootFolder.ServerRelativeUrl}'");

                        Assert.AreEqual(title, created.Title, "The title was not applied.");
                        Assert.AreEqual("Created by a live probe", created.Description, "The description was not applied.");

                        string expected = $"{fresh.Web.ServerRelativeUrl.TrimEnd('/')}/{url}";
                        Assert.AreEqual(expected, created.RootFolder.ServerRelativeUrl,
                            "The url the template asked for was not used - a lookup column or {listurl:} token " +
                            "pointing at it would resolve to nothing.");
                    }
                }
                finally
                {
                    await DeleteListByTitleAsync(title).ConfigureAwait(false);
                }
            }
        }

        private static async Task DeleteListByTitleAsync(string title)
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title)).ConfigureAwait(false);

                    foreach (IList list in context.Web.Lists.AsRequested()
                        .Where(l => l.Title != null && l.Title.StartsWith(TestPrefix, StringComparison.Ordinal)).ToList())
                    {
                        try
                        {
                            string name = list.Title;
                            await list.DeleteAsync().ConfigureAwait(false);
                            Console.WriteLine($"Deleted list '{name}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT DELETE list '{list.Title}': {Describe(ex)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT SWEEP lists: {Describe(ex)}");
            }
        }
    }
}
