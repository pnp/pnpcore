using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunicationSiteCollectionModel = PnP.Core.Provisioning.Model.CommunicationSiteCollection;
using TermGroupModel = PnP.Core.Provisioning.Model.TermGroup;
using ThemeModel = PnP.Core.Provisioning.Model.Theme;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Phase 8's exit gate: one tenant template that provisions tenant settings, a term group and a
    /// site collection, in one apply.
    /// </summary>
    [TestClass]
    public class TenantHierarchyLiveTests : LiveTestBase
    {
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        [Timeout(30 * 60 * 1000)]
        public async Task Hierarchy_AppliesTenantSettingsThenTermGroupsThenSites()
        {
            string fixture = Guid.NewGuid().ToString("N").Substring(0, 12);

            string themeName = $"{TestPrefix}HierarchyTheme_{fixture}";
            string termGroupName = $"{TestPrefix}HierarchyGroup_{fixture}";
            var termGroupId = Guid.NewGuid();
            string listTitle = $"{TestPrefix}HierarchyList";

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
                    siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoreprovisioningtesthier{fixture}");

                    try
                    {
                        ProvisioningHierarchy hierarchy = BuildHierarchy(
                            siteUrl, themeName, termGroupName, termGroupId, listTitle,
                            await SiteOwnerAsync(context).ConfigureAwait(false));

                        var problems = new List<string>();

                        Console.WriteLine($"Applying hierarchy: theme '{themeName}', term group " +
                            $"'{termGroupName}', site {siteUrl}");

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
                            $"The hierarchy reported problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");

                        HashSet<string> themes = await TenantThemes.GetNamesAsync(admin).ConfigureAwait(false);
                        Assert.IsTrue(themes.Contains(themeName),
                            "ObjectHierarchyTenant did not apply the hierarchy's tenant settings.");

                        ITermGroup group = await admin.TermStore.Groups
                            .GetByIdAsync(termGroupId.ToString()).ConfigureAwait(false);

                        Assert.IsNotNull(group, "ObjectHierarchySequenceTermGroups did not create the term group.");
                        Assert.AreEqual(termGroupName, group.Name, "The term group was created with the wrong name.");

                        using (PnPContext seed = await GetContextAsync(1).ConfigureAwait(false))
                        using (PnPContext created = await seed.CloneAsync(siteUrl).ConfigureAwait(false))
                        {
                            await created.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Title)).ConfigureAwait(false);

                            Assert.IsTrue(created.Web.Lists.AsRequested().Any(l => l.Title == listTitle),
                                "ObjectHierarchySequenceSites created the site but its template was not applied.");
                        }

                        Console.WriteLine("All three hierarchy handlers ran, in order.");
                    }
                    finally
                    {
                        await CleanUpAsync(admin, siteUrl, themeName, termGroupId).ConfigureAwait(false);
                    }
                }
            }
        }

        private static ProvisioningHierarchy BuildHierarchy(Uri siteUrl, string themeName, string termGroupName,
            Guid termGroupId, string listTitle, string owner)
        {
            var hierarchy = new ProvisioningHierarchy
            {
                Tenant = new ProvisioningTenant(),
            };

            hierarchy.Tenant.Themes.Add(new ThemeModel
            {
                Name = themeName,
                IsInverted = false,
                Overwrite = true,
                Palette = "{\"themePrimary\":\"#0078d4\",\"themeDark\":\"#005a9e\",\"white\":\"#ffffff\"}",
            });

            var template = new ProvisioningTemplate { Id = "HIERARCHY-TEMPLATE" };

            template.Lists.Add(new ListInstance
            {
                Title = listTitle,
                Url = "Lists/PnPCoreProvisioningTestHierarchyList",
                TemplateType = (int)ListTemplateType.GenericList,
            });

            hierarchy.Templates.Add(template);

            var sequence = new ProvisioningSequence
            {
                ID = "TENANTSEQUENCE",
                TermStore = new ProvisioningTermStore(),
            };

            sequence.TermStore.TermGroups.Add(new TermGroupModel
            {
                Id = termGroupId,
                Name = termGroupName,
                Description = "Created by the PnP Core provisioning tests",
            });

            var siteCollection = new CommunicationSiteCollectionModel
            {
                Url = siteUrl.ToString(),
                Title = $"{TestPrefix}Hierarchy",
                Description = "Created by the PnP Core provisioning tests",
                Language = 1033,
                Owner = owner,
                ProvisioningId = "HIER-SITE",
            };

            siteCollection.Templates.Add("HIERARCHY-TEMPLATE");
            sequence.SiteCollections.Add(siteCollection);
            hierarchy.Sequences.Add(sequence);

            return hierarchy;
        }

        /// <summary>
        /// Removes everything the hierarchy created, whatever failed.
        /// </summary>
        private static async Task CleanUpAsync(PnPContext admin, Uri siteUrl, string themeName, Guid termGroupId)
        {
            try
            {
                await TenantThemes.DeleteAsync(admin, themeName).ConfigureAwait(false);
                Console.WriteLine($"Deleted theme {themeName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE THEME {themeName}: {Describe(ex)}");
            }

            try
            {
                await DeleteTermGroupDeepAsync(admin, termGroupId.ToString()).ConfigureAwait(false);
                Console.WriteLine($"Deleted term group {termGroupId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE TERM GROUP {termGroupId}: {Describe(ex)}");
            }

            if (siteUrl == null)
            {
                return;
            }

            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    ISiteCollectionManager manager = context.GetSiteCollectionManager();

                    if (await manager.SiteExistsAsync(siteUrl).ConfigureAwait(false))
                    {
                        await manager.DeleteSiteCollectionAsync(siteUrl).ConfigureAwait(false);
                        Console.WriteLine($"Deleted {siteUrl}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE {siteUrl} - delete it by hand.{Environment.NewLine}{Describe(ex)}");
            }
        }
    }
}
