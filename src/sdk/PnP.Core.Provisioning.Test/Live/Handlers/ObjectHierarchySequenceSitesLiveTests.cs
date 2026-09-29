using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.Model.Configuration.Tenant.Sequence;
using PnP.Core.Provisioning.Model.Configuration.Tenant.Teams;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunicationSiteCollectionModel = PnP.Core.Provisioning.Model.CommunicationSiteCollection;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectHierarchySequenceSites</c>
    /// </summary>
    [TestClass]
    public class ObjectHierarchySequenceSitesLiveTests : LiveTestBase
    {
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        [Timeout(30 * 60 * 1000)]
        public async Task Sequence_CreatesTheSiteAndAppliesTheTemplateItNames()
        {
            Uri siteUrl = null;

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                PnPContext admin;

                try
                {
                    admin = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("The tenant admin site", ex);
                    return;
                }

                using (admin)
                {
                    string fixture = Guid.NewGuid().ToString("N").Substring(0, 12);
                    siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoreprovisioningtestseq{fixture}");

                    string listTitle = $"{TestPrefix}SequenceList";

                    try
                    {
                        var hierarchy = new ProvisioningHierarchy();

                        var template = new ProvisioningTemplate { Id = "SEQUENCE-TEMPLATE" };

                        template.Lists.Add(new ListInstance
                        {
                            Title = listTitle,
                            Url = "Lists/PnPCoreProvisioningTestSequenceList",
                            TemplateType = (int)ListTemplateType.GenericList,

                            Description = "Created by sequence {sequencesiteurl:SEQ-SITE}",
                        });

                        hierarchy.Templates.Add(template);

                        var sequence = new ProvisioningSequence { ID = "TENANTSEQUENCE" };

                        var siteCollection = new CommunicationSiteCollectionModel
                        {
                            Url = siteUrl.ToString(),
                            Title = $"{TestPrefix}Sequence",
                            Description = "Created by the PnP Core provisioning tests",
                            Language = 1033,
                            Owner = await SiteOwnerAsync(context).ConfigureAwait(false),
                            ProvisioningId = "SEQ-SITE",
                        };

                        siteCollection.Templates.Add("SEQUENCE-TEMPLATE");
                        sequence.SiteCollections.Add(siteCollection);
                        hierarchy.Sequences.Add(sequence);

                        Console.WriteLine($"Applying sequence, target {siteUrl}");

                        var problems = new List<string>();

                        await admin.GetProvisioningManager().ApplyTenantTemplateAsync(
                            hierarchy, "TENANTSEQUENCE", new ApplyConfiguration
                            {
                                MessagesDelegate = (message, type) =>
                                {
                                    Console.WriteLine($"[{type}] {message}");

                                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                                    {
                                        problems.Add(message);
                                    }
                                },
                            }).ConfigureAwait(false);

                        Assert.AreEqual(0, problems.Count,
                            $"The sequence reported problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");

                        using (PnPContext seed = await GetContextAsync(1).ConfigureAwait(false))
                        using (PnPContext created = await seed.CloneAsync(siteUrl).ConfigureAwait(false))
                        {
                            IWeb web = await created.Web.GetAsync(w => w.Title, w => w.Url).ConfigureAwait(false);

                            Console.WriteLine($"Created {web.Url}, title '{web.Title}'");
                            Assert.AreEqual($"{TestPrefix}Sequence", web.Title, "The site was created with the wrong title.");

                            await created.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Title, l => l.Description))
                                .ConfigureAwait(false);

                            IList list = created.Web.Lists.AsRequested()
                                .FirstOrDefault(l => l.Title == listTitle);

                            Assert.IsNotNull(list,
                                "The sequence created the site but its template was not applied to it - " +
                                $"no list called '{listTitle}' exists.");

                            Console.WriteLine($"List description: {list.Description}");

                            StringAssert.Contains(list.Description, siteUrl.ToString().TrimEnd('/'),
                                "The {sequencesiteurl:SEQ-SITE} token did not resolve to the created site, " +
                                "so the template was applied with a parser that did not know about it.");
                        }
                    }
                    finally
                    {
                        await DeleteSiteAsync(siteUrl).ConfigureAwait(false);
                    }
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Sequence_ExtractsTheSiteAsASiteCollectionWithItsTemplateAndTeam()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    using (await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
                    {
                    }
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("The tenant admin site", ex);
                    return;
                }

                await context.Site.LoadAsync(s => s.Id, s => s.GroupId).ConfigureAwait(false);

                var problems = new List<string>();

                var configuration = new ExtractConfiguration
                {
                    // Only what the sequence needs from the site's own template, to keep the run short.
                    Handlers = { ConfigurationHandler.WebSettings },
                    MessagesDelegate = (message, type) =>
                    {
                        Console.WriteLine($"[{type}] {message}");

                        if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                        {
                            problems.Add(message);
                        }
                    },
                };

                configuration.Tenant.Sequence = new ExtractSequenceConfiguration { SiteUrls = { context.Uri.ToString() } };
                configuration.Tenant.Teams = new ExtractTeamsConfiguration();

                ProvisioningHierarchy hierarchy = await context.GetProvisioningManager()
                    .GetTenantTemplateAsync(configuration).ConfigureAwait(false);

                Assert.AreEqual(0, problems.Count,
                    $"The extract reported problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");

                string id = context.Site.Id.ToString("N");

                ProvisioningSequence sequence = hierarchy.Sequences.Single();
                Assert.AreEqual("TENANTSEQUENCE", sequence.ID);

                SiteCollection siteCollection = sequence.SiteCollections.Single();
                Assert.AreEqual($"{{parameter:SITECOLLECTION_{id}_TITLE}}", siteCollection.Title);
                CollectionAssert.Contains(siteCollection.Templates, $"TEMPLATE-{id}");
                Assert.IsTrue(hierarchy.Templates.Any(t => t.Id == $"TEMPLATE-{id}"),
                    "The site collection refers to a template the hierarchy does not have.");

                if (context.Site.GroupId == Guid.Empty)
                {
                    Assert.IsTrue(hierarchy.Parameters.ContainsKey($"SITECOLLECTION_{id}_URL"), "The url parameter is missing.");
                    Assert.AreEqual(0, hierarchy.Teams.Teams.Count, "A site without a group has no team.");
                    return;
                }

                Assert.IsInstanceOfType(siteCollection, typeof(TeamSiteCollection));
                Assert.IsTrue(hierarchy.Parameters.TryGetValue($"SITECOLLECTION_{id}_ALIAS", out string alias),
                    "The alias parameter is missing.");

                foreach (PnP.Core.Provisioning.Model.Teams.Team team in hierarchy.Teams.Teams)
                {
                    Console.WriteLine($"Extracted team {team.DisplayName} ({team.MailNickname}), {team.Channels.Count} channel(s)");

                    Assert.AreEqual(alias, team.MailNickname, true,
                        "The team taken for the site is not the one behind the site's group.");
                    Assert.IsTrue(team.Channels.Any(), "A team always has a channel.");
                }
            }
        }

        /// <summary>
        /// Deletes the site, recycle bin included, without ever throwing.
        /// </summary>
        private static async Task DeleteSiteAsync(Uri siteUrl)
        {
            if (siteUrl == null)
            {
                return;
            }

            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    ISiteCollectionManager manager = context.GetSiteCollectionManager();

                    if (!await manager.SiteExistsAsync(siteUrl).ConfigureAwait(false))
                    {
                        Console.WriteLine($"{siteUrl} was never created, nothing to delete.");
                        return;
                    }

                    await manager.DeleteSiteCollectionAsync(siteUrl).ConfigureAwait(false);
                    Console.WriteLine($"Deleted {siteUrl}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE {siteUrl} - delete it by hand.{Environment.NewLine}{Describe(ex)}");
            }
        }
    }
}
