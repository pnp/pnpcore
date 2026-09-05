using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Csom
{
    /// <summary>
    /// Live round trips for the two user resource CSOM requests.
    /// </summary>
    [TestClass]
    public class UserResourceRequestLiveTests : LiveTestBase
    {
        /// <summary>
        /// A culture the test site is very unlikely to be running in, so writing to it cannot
        /// change what anyone actually sees.
        /// </summary>
        private const string ProbeCulture = "fi-FI";

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task SetAndGetValueForUICulture_RoundTripsOnTheWebTitle()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                var resource = UserResourcePath.ForWeb(siteId, webId, ResourceProperty.Title);
                string original = null;

                try
                {
                    original = await CsomRequestSender.SendAsync(context,
                        new GetValueForUICultureRequest(resource, ProbeCulture)).ConfigureAwait(false);

                    string localized = $"{TestPrefix}Otsikko_{DateTime.UtcNow:HHmmss}";

                    await CsomRequestSender.SendAsync(context,
                        new SetValueForUICultureRequest(resource, ProbeCulture, localized)).ConfigureAwait(false);

                    string readBack = await CsomRequestSender.SendAsync(context,
                        new GetValueForUICultureRequest(resource, ProbeCulture)).ConfigureAwait(false);

                    Assert.AreEqual(localized, readBack,
                        "The localized web title did not round-trip. T6 is what a dozen phase 6 handlers depend on.");
                }
                finally
                {
                    if (original != null)
                    {
                        try
                        {
                            await CsomRequestSender.SendAsync(context,
                                new SetValueForUICultureRequest(resource, ProbeCulture, original)).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Could not restore the original {ProbeCulture} web title: {ex.Message}");
                        }
                    }
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task SetAndGetValueForUICulture_RoundTripsOnAListTitle()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                string listTitle = $"{TestPrefix}Loc_{DateTime.UtcNow:HHmmssfff}";
                IList list = null;

                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList).ConfigureAwait(false);

                    var resource = UserResourcePath.ForList(siteId, webId, list.Id, ResourceProperty.Title);

                    string localized = $"{TestPrefix}Lista_{DateTime.UtcNow:HHmmss}";

                    await CsomRequestSender.SendAsync(context,
                        new SetValueForUICultureRequest(resource, ProbeCulture, localized)).ConfigureAwait(false);

                    string readBack = await CsomRequestSender.SendAsync(context,
                        new GetValueForUICultureRequest(resource, ProbeCulture)).ConfigureAwait(false);

                    Assert.AreEqual(localized, readBack, "The localized list title did not round-trip.");
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

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task SetValueForUICulture_DoesNotDisturbOtherCultures()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                string listTitle = $"{TestPrefix}MultiLoc_{DateTime.UtcNow:HHmmssfff}";
                IList list = null;

                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList).ConfigureAwait(false);
                    var resource = UserResourcePath.ForList(siteId, webId, list.Id, ResourceProperty.Title);

                    await CsomRequestSender.SendAsync(context,
                        new SetValueForUICultureRequest(resource, "fi-FI", "Suomeksi")).ConfigureAwait(false);
                    await CsomRequestSender.SendAsync(context,
                        new SetValueForUICultureRequest(resource, "de-DE", "Auf Deutsch")).ConfigureAwait(false);

                    Assert.AreEqual("Suomeksi", await CsomRequestSender.SendAsync(context,
                        new GetValueForUICultureRequest(resource, "fi-FI")).ConfigureAwait(false));

                    Assert.AreEqual("Auf Deutsch", await CsomRequestSender.SendAsync(context,
                        new GetValueForUICultureRequest(resource, "de-DE")).ConfigureAwait(false));
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

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("UserResources")]
        public async Task SetValueForUICulture_BatchesSeveralCulturesInOneRoundTrip()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                string listTitle = $"{TestPrefix}BatchLoc_{DateTime.UtcNow:HHmmssfff}";
                IList list = null;

                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList).ConfigureAwait(false);
                    var resource = UserResourcePath.ForList(siteId, webId, list.Id, ResourceProperty.Title);

                    var batch = new System.Collections.Generic.List<PnP.Core.Services.Core.CSOM.Requests.IRequest<object>>
                    {
                        new SetValueForUICultureRequest(resource, "fi-FI", "Yksi"),
                        new SetValueForUICultureRequest(resource, "de-DE", "Zwei"),
                        new SetValueForUICultureRequest(resource, "fr-FR", "Trois"),
                    };

                    await CsomRequestSender.SendManyAsync(context, batch).ConfigureAwait(false);

                    Assert.AreEqual("Yksi", await CsomRequestSender.SendAsync(context,
                        new GetValueForUICultureRequest(resource, "fi-FI")).ConfigureAwait(false));
                    Assert.AreEqual("Zwei", await CsomRequestSender.SendAsync(context,
                        new GetValueForUICultureRequest(resource, "de-DE")).ConfigureAwait(false));
                    Assert.AreEqual("Trois", await CsomRequestSender.SendAsync(context,
                        new GetValueForUICultureRequest(resource, "fr-FR")).ConfigureAwait(false));
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
    }
}
