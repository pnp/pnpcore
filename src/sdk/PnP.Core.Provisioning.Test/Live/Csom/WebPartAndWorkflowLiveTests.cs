using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.WebParts;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Workflows;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Csom
{
    /// <summary>
    /// Live round trips for the classic web part and SharePoint 2013 workflow CSOM requests.
    /// </summary>
    [TestClass]
    public class WebPartAndWorkflowLiveTests : LiveTestBase
    {
        /// <summary>
        /// A minimal, universally available web part - the XML a template would carry.
        /// </summary>
        private const string ContentEditorWebPartXml =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<WebPart xmlns=\"http://schemas.microsoft.com/WebPart/v2\">" +
            "<Assembly>Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c</Assembly>" +
            "<TypeName>Microsoft.SharePoint.WebPartPages.ContentEditorWebPart</TypeName>" +
            "<Title>PnPCoreProvisioningTest WebPart</Title>" +
            "<FrameType>TitleBarOnly</FrameType>" +
            "</WebPart>";

        /// <summary>
        /// Runs one step of a multi-request chain, reporting which step failed and why.
        /// </summary>
        private static async Task StepAsync(string label, Func<Task> step)
        {
            try
            {
                await step().ConfigureAwait(false);
                Console.WriteLine($"  {label}: OK");
            }
            catch (AssertFailedException)
            {
                Console.WriteLine($"  {label}: ASSERTION FAILED");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {label}: FAILED{Environment.NewLine}{Describe(ex)}");
                throw;
            }
        }

        #region T18 - classic web parts

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("WebParts")]
        public async Task WebPartLifecycle_AddEnumerateMoveUpdateDelete()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                if (await IsNoScriptAsync(context).ConfigureAwait(false))
                {
                    Assert.Inconclusive(
                        "The configured classic test site is a NoScript site, so classic web parts cannot be " +
                        "added to it. Point ClassicSTS0TestSite at a genuine STS#0 site, or disable NoScript " +
                        "(Set-PnPSite -NoScriptSite $false), to verify the web part write path.");
                    return;
                }

                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                string listTitle = $"{TestPrefix}WP_{DateTime.UtcNow:HHmmssfff}";
                IList list = null;

                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList).ConfigureAwait(false);
                    await list.LoadAsync(l => l.RootFolder).ConfigureAwait(false);

                    string pageUrl = $"{list.RootFolder.ServerRelativeUrl}/AllItems.aspx";
                    Console.WriteLine($"Targeting list form page: {pageUrl}");

                    WebPartDefinitionInfo added;
                    try
                    {
                        added = await CsomRequestSender.SendAsync(context,
                            new AddWebPartRequest(siteId, webId, pageUrl, ContentEditorWebPartXml, "Main", 0))
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        SkipIfUnavailable("Classic web parts on a list form page", ex);
                        return;
                    }

                    Assert.IsNotNull(added, "AddWebPartRequest returned no result.");
                    Assert.AreNotEqual(Guid.Empty, added.Id, "The added web part has no id.");
                    Console.WriteLine($"  added web part {added.Id} in zone '{added.ZoneId}'");

                    await StepAsync("enumerate", async () =>
                    {
                        List<WebPartDefinitionInfo> definitions = await CsomRequestSender.SendAsync(context,
                            new GetWebPartDefinitionsRequest(siteId, webId, pageUrl)).ConfigureAwait(false);

                        Assert.IsTrue(definitions.Any(d => d.Id == added.Id),
                            $"The added web part {added.Id} was not found when enumerating. Found: " +
                            string.Join(", ", definitions.Select(d => d.Id)));
                    }).ConfigureAwait(false);

                    await StepAsync("save properties", async () =>
                        await CsomRequestSender.SendAsync(context,
                            new SaveWebPartPropertiesRequest(siteId, webId, pageUrl, added.Id,
                                title: $"{TestPrefix}Renamed")).ConfigureAwait(false)).ConfigureAwait(false);

                    await StepAsync("move", async () =>
                        await CsomRequestSender.SendAsync(context,
                            new MoveWebPartToRequest(siteId, webId, pageUrl, added.Id, "Main", 2)).ConfigureAwait(false)).ConfigureAwait(false);

                    await StepAsync("delete", async () =>
                        await CsomRequestSender.SendAsync(context,
                            new DeleteWebPartRequest(siteId, webId, pageUrl, added.Id)).ConfigureAwait(false)).ConfigureAwait(false);

                    await StepAsync("verify deleted", async () =>
                    {
                        List<WebPartDefinitionInfo> afterDelete = await CsomRequestSender.SendAsync(context,
                            new GetWebPartDefinitionsRequest(siteId, webId, pageUrl)).ConfigureAwait(false);

                        Assert.IsFalse(afterDelete.Any(d => d.Id == added.Id),
                            "The web part was still present after DeleteWebPartRequest.");
                    }).ConfigureAwait(false);
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
        [TestCategory("WebParts")]
        public async Task GetWebPartDefinitions_ReadsAnExistingListFormPage()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                string listTitle = $"{TestPrefix}WPRead_{DateTime.UtcNow:HHmmssfff}";
                IList list = null;

                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList).ConfigureAwait(false);
                    await list.LoadAsync(l => l.RootFolder).ConfigureAwait(false);

                    string pageUrl = $"{list.RootFolder.ServerRelativeUrl}/AllItems.aspx";

                    List<WebPartDefinitionInfo> definitions;
                    try
                    {
                        definitions = await CsomRequestSender.SendAsync(context,
                            new GetWebPartDefinitionsRequest(siteId, webId, pageUrl)).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        SkipIfUnavailable("Classic web parts on a list form page", ex);
                        return;
                    }

                    Assert.IsNotNull(definitions);

                    Console.WriteLine($"Web parts on {pageUrl}: {definitions.Count}");
                    foreach (WebPartDefinitionInfo definition in definitions)
                    {
                        Console.WriteLine($"  {definition.Id} zone='{definition.ZoneId}'");
                    }

                    Assert.IsTrue(definitions.Count > 0,
                        "A list form page should carry at least its own list view web part. An empty result " +
                        "suggests the response was not parsed rather than that the page is empty.");
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

        #endregion

        #region T16 - SP2013 workflows

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Workflows")]
        public async Task GetWorkflowDefinitions_EnumeratesOrReportsWorkflowsRetired()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                List<WorkflowDefinitionInfo> definitions;
                try
                {
                    definitions = await CsomRequestSender.SendAsync(context,
                        new GetWorkflowDefinitionsRequest(siteId, webId)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("SharePoint 2013 workflow services", ex);
                    return;
                }

                Assert.IsNotNull(definitions);

                Console.WriteLine($"Workflow definitions: {definitions.Count}");
                foreach (WorkflowDefinitionInfo definition in definitions)
                {
                    Console.WriteLine($"  {definition.Id} {definition.DisplayName} published={definition.Published}");
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Workflows")]
        public async Task GetWorkflowSubscriptions_EnumeratesOrReportsWorkflowsRetired()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                List<WorkflowSubscriptionInfo> subscriptions;
                try
                {
                    subscriptions = await CsomRequestSender.SendAsync(context,
                        new GetWorkflowSubscriptionsRequest(siteId, webId)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("SharePoint 2013 workflow services", ex);
                    return;
                }

                Assert.IsNotNull(subscriptions);
                Console.WriteLine($"Workflow subscriptions: {subscriptions.Count}");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Workflows")]
        public async Task SaveAndPublishWorkflowDefinition_RoundTripsOrReportsWorkflowsRetired()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                var definition = new WorkflowDefinitionInfo
                {
                    DisplayName = $"{TestPrefix}Workflow",
                    Description = "Created by a live test - safe to delete",
                    Xaml = "<Activity xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" />",
                    RestrictToType = "Universal",
                };

                WorkflowDefinitionInfo saved = null;
                try
                {
                    saved = await CsomRequestSender.SendAsync(context,
                        new SaveWorkflowDefinitionRequest(siteId, webId, definition)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("SharePoint 2013 workflow services", ex);
                    return;
                }

                try
                {
                    Assert.IsNotNull(saved, "SaveWorkflowDefinitionRequest returned no result.");
                    Assert.AreNotEqual(Guid.Empty, saved.Id, "The saved workflow definition has no id.");
                    Console.WriteLine($"Saved workflow definition {saved.Id}");

                    await CsomRequestSender.SendAsync(context,
                        new PublishWorkflowDefinitionRequest(siteId, webId, saved.Id)).ConfigureAwait(false);

                    List<WorkflowDefinitionInfo> definitions = await CsomRequestSender.SendAsync(context,
                        new GetWorkflowDefinitionsRequest(siteId, webId)).ConfigureAwait(false);

                    Assert.IsTrue(definitions.Any(d => d.Id == saved.Id),
                        "The saved workflow definition was not found when enumerating.");
                }
                finally
                {
                    if (saved != null && saved.Id != Guid.Empty)
                    {
                        try
                        {
                            await CsomRequestSender.SendAsync(context,
                                new DeleteWorkflowDefinitionRequest(siteId, webId, saved.Id)).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Could not delete workflow definition {saved.Id}: {ex.Message}");
                        }
                    }
                }
            }
        }

        #endregion
    }
}
