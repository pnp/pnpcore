using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoleDefinitionModel = PnP.Core.Provisioning.Model.RoleDefinition;
using SiteGroupModel = PnP.Core.Provisioning.Model.SiteGroup;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectSiteSecurity</c>
    /// </summary>
    [TestClass]
    public class ObjectSiteSecurityLiveTests : LiveTestBase
    {
        private static string OwnersGroupTitle => $"{TestPrefix}Owners";

        private static string ReadersGroupTitle => $"{TestPrefix}Readers";

        private static string PermissionLevelName => $"{TestPrefix}Level";

        #region Groups

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Security_CreatesSharePointGroupsWithTheirSettings()
        {
            using (PnPContext context = await GetNoGroupContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var template = new ProvisioningTemplate();
                    template.Security.SiteGroups.Add(new SiteGroupModel
                    {
                        Title = ReadersGroupTitle,
                        Description = "Created by ObjectSiteSecurityLiveTests",
                        AllowMembersEditMembership = false,
                        OnlyAllowMembersViewMembership = true,
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetNoGroupContextAsync(1).ConfigureAwait(false))
                    {
                        ISharePointGroup group = await FindGroupAsync(fresh, ReadersGroupTitle).ConfigureAwait(false);

                        Assert.IsNotNull(group, "The group was not created.");
                        Console.WriteLine($"'{group.Title}' ({group.Id}): {group.Description}");

                        Assert.AreEqual("Created by ObjectSiteSecurityLiveTests", group.Description);
                        Assert.IsTrue(group.OnlyAllowMembersViewMembership, "The membership visibility setting was not applied.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Security_AppliedTwiceDoesNotCreateASecondGroup()
        {
            using (PnPContext context = await GetNoGroupContextAsync().ConfigureAwait(false))
            {
                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    var first = new ProvisioningTemplate();
                    first.Security.SiteGroups.Add(new SiteGroupModel { Title = ReadersGroupTitle, Description = "First" });
                    await manager.ApplyTemplateAsync(first, Reporting()).ConfigureAwait(false);

                    var second = new ProvisioningTemplate();
                    second.Security.SiteGroups.Add(new SiteGroupModel { Title = ReadersGroupTitle, Description = "Second" });
                    await manager.ApplyTemplateAsync(second, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetNoGroupContextAsync(1).ConfigureAwait(false))
                    {
                        await fresh.Web.LoadAsync(w => w.SiteGroups.QueryProperties(g => g.Id, g => g.Title,
                            g => g.Description)).ConfigureAwait(false);

                        List<ISharePointGroup> matches = fresh.Web.SiteGroups.AsRequested()
                            .Where(g => g.Title == ReadersGroupTitle).ToList();

                        Console.WriteLine($"{matches.Count} group(s) named '{ReadersGroupTitle}'");

                        Assert.AreEqual(1, matches.Count, "The second apply created a duplicate group.");
                        Assert.AreEqual("Second", matches[0].Description, "The existing group was not updated.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region T3 - associated groups

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Security_AssignsTheAssociatedOwnerGroup()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                if (await IsNoScriptAsync(context).ConfigureAwait(false))
                {
                    Assert.Inconclusive("The classic test site is NoScript, so associated groups cannot be reassigned on it.");
                }

                int? originalOwnerGroupId = await ReadAssociatedOwnerGroupIdAsync(context).ConfigureAwait(false);

                try
                {
                    var template = new ProvisioningTemplate();
                    template.Security.SiteGroups.Add(new SiteGroupModel { Title = OwnersGroupTitle });
                    template.Security.AssociatedOwnerGroup = OwnersGroupTitle;

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetClassicContextAsync(1).ConfigureAwait(false))
                    {
                        await fresh.Web.LoadAsync(w => w.AssociatedOwnerGroup.QueryProperties(g => g.Id, g => g.Title))
                            .ConfigureAwait(false);

                        Console.WriteLine($"Associated owner group is now '{fresh.Web.AssociatedOwnerGroup.Title}'");
                        Assert.AreEqual(OwnersGroupTitle, fresh.Web.AssociatedOwnerGroup.Title,
                            "The associated owner group was not reassigned.");
                    }
                }
                finally
                {
                    await RestoreAssociatedOwnerGroupAsync(originalOwnerGroupId, classic: true).ConfigureAwait(false);
                    await SweepAsync(classic: true).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Permission levels

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Security_CreatesACustomPermissionLevel()
        {
            using (PnPContext context = await GetNoGroupContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var template = new ProvisioningTemplate();
                    template.Security.SiteSecurityPermissions.RoleDefinitions.Add(new RoleDefinitionModel
                    {
                        Name = PermissionLevelName,
                        Description = "Created by ObjectSiteSecurityLiveTests",
                        Permissions = { PermissionKind.ViewListItems, PermissionKind.OpenItems, PermissionKind.ViewPages, PermissionKind.Open },
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetNoGroupContextAsync(1).ConfigureAwait(false))
                    {
                        IRoleDefinition level = await FindRoleDefinitionAsync(fresh, PermissionLevelName).ConfigureAwait(false);

                        Assert.IsNotNull(level, "The permission level was not created.");
                        Console.WriteLine($"'{level.Name}' ({level.Id}): {level.Description}");

                        Assert.AreEqual("Created by ObjectSiteSecurityLiveTests", level.Description);

                        Assert.IsTrue(level.BasePermissions.Has(PermissionKind.ViewListItems),
                            "The level does not carry ViewListItems.");
                        Assert.IsTrue(level.BasePermissions.Has(PermissionKind.OpenItems),
                            "The level does not carry OpenItems.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Extract

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Security_ExtractsTheGroupsAndLevelsItJustCreated()
        {
            using (PnPContext context = await GetNoGroupContextAsync().ConfigureAwait(false))
            {
                try
                {
                    var template = new ProvisioningTemplate();
                    template.Security.SiteGroups.Add(new SiteGroupModel { Title = ReadersGroupTitle, Description = "Round trip" });
                    template.Security.SiteSecurityPermissions.RoleDefinitions.Add(new RoleDefinitionModel
                    {
                        Name = PermissionLevelName,
                        Description = "Round trip",
                        Permissions = { PermissionKind.ViewListItems, PermissionKind.ViewPages, PermissionKind.Open },
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetNoGroupContextAsync(1).ConfigureAwait(false))
                    {
                        var configuration = new ExtractConfiguration();
                        configuration.Handlers.Add(ConfigurationHandler.SiteSecurity);

                        configuration.SiteSecurity.IncludeSiteGroups = true;

                        ProvisioningTemplate extracted = await fresh.GetProvisioningManager()
                            .GetTemplateAsync(configuration).ConfigureAwait(false);

                        Console.WriteLine($"Extracted {extracted.Security.SiteGroups.Count} group(s), " +
                            $"{extracted.Security.SiteSecurityPermissions.RoleDefinitions.Count} custom level(s)");

                        Assert.IsTrue(extracted.Security.SiteGroups.Any(g => g.Title == ReadersGroupTitle),
                            "The group was not extracted.");

                        RoleDefinitionModel level = extracted.Security.SiteSecurityPermissions.RoleDefinitions
                            .FirstOrDefault(r => r.Name == PermissionLevelName);

                        Assert.IsNotNull(level, "The custom permission level was not extracted.");
                        Assert.IsTrue(level.Permissions.Contains(PermissionKind.ViewListItems),
                            "The level's permissions were not extracted.");

                        Assert.IsFalse(extracted.Security.SiteSecurityPermissions.RoleDefinitions.Any(r => r.Name == "Full Control"),
                            "A built-in permission level was extracted; applying that template would fail.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        private static ApplyConfiguration Reporting()
        {
            return new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        Console.WriteLine($"[{type}] {message}");
                    }
                },
            };
        }

        /// <summary>
        /// A second context for cleanup, against whichever site the test used.
        /// </summary>
        private static Task<PnPContext> SweepContextAsync(bool classic)
        {
            return classic ? GetClassicContextAsync(2) : GetNoGroupContextAsync(2);
        }

        private static async Task<ISharePointGroup> FindGroupAsync(PnPContext context, string title)
        {
            await context.Web.LoadAsync(w => w.SiteGroups.QueryProperties(g => g.Id, g => g.Title, g => g.Description,
                g => g.OnlyAllowMembersViewMembership, g => g.AllowMembersEditMembership)).ConfigureAwait(false);

            return context.Web.SiteGroups.AsRequested().FirstOrDefault(g => g.Title == title);
        }

        private static async Task<IRoleDefinition> FindRoleDefinitionAsync(PnPContext context, string name)
        {
            await context.Web.LoadAsync(w => w.RoleDefinitions.QueryProperties(r => r.Id, r => r.Name,
                r => r.Description, r => r.BasePermissions)).ConfigureAwait(false);

            return context.Web.RoleDefinitions.AsRequested().FirstOrDefault(r => r.Name == name);
        }

        private static async Task<int?> ReadAssociatedOwnerGroupIdAsync(PnPContext context)
        {
            try
            {
                await context.Web.LoadAsync(w => w.AssociatedOwnerGroup.QueryProperties(g => g.Id, g => g.Title))
                    .ConfigureAwait(false);

                Console.WriteLine($"Associated owner group was '{context.Web.AssociatedOwnerGroup.Title}'");
                return context.Web.AssociatedOwnerGroup.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No associated owner group to restore: {Describe(ex)}");
                return null;
            }
        }

        /// <summary>
        /// Puts the site's original owner group back before the suite's group is deleted.
        /// </summary>
        private static async Task RestoreAssociatedOwnerGroupAsync(int? originalGroupId, bool classic)
        {
            if (originalGroupId == null)
            {
                return;
            }

            try
            {
                using (PnPContext context = await SweepContextAsync(classic).ConfigureAwait(false))
                {
                    await context.Web.LoadAsync(w => w.SiteGroups.QueryProperties(g => g.Id, g => g.Title))
                        .ConfigureAwait(false);

                    ISharePointGroup original = context.Web.SiteGroups.AsRequested()
                        .FirstOrDefault(g => g.Id == originalGroupId.Value);

                    if (original == null)
                    {
                        Console.WriteLine($"COULD NOT RESTORE owner group {originalGroupId}: it no longer exists.");
                        return;
                    }

                    var template = new ProvisioningTemplate();
                    template.Security.AssociatedOwnerGroup = original.Title;

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);
                    Console.WriteLine($"Restored associated owner group to '{original.Title}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT RESTORE the associated owner group: {Describe(ex)}");
            }
        }

        private static async Task SweepAsync(bool classic = false)
        {
            try
            {
                using (PnPContext context = await SweepContextAsync(classic).ConfigureAwait(false))
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

                    await context.Web.LoadAsync(w => w.RoleDefinitions.QueryProperties(r => r.Id, r => r.Name))
                        .ConfigureAwait(false);

                    foreach (IRoleDefinition level in context.Web.RoleDefinitions.AsRequested()
                        .Where(r => r.Name != null && r.Name.StartsWith(TestPrefix, StringComparison.Ordinal)).ToList())
                    {
                        string name = level.Name;
                        try
                        {
                            await level.DeleteAsync().ConfigureAwait(false);
                            Console.WriteLine($"Deleted permission level '{name}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT DELETE permission level '{name}': {Describe(ex)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT SWEEP: {Describe(ex)}");
            }
        }

        #endregion
    }
}
