using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Web;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using RoleAssignmentModel = PnP.Core.Provisioning.Model.RoleAssignment;
using RoleDefinitionModel = PnP.Core.Provisioning.Model.RoleDefinition;
using SiteGroupModel = PnP.Core.Provisioning.Model.SiteGroup;
using UserModel = PnP.Core.Provisioning.Model.User;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Provisions and extracts the site's <c>&lt;pnp:Security&gt;</c> element - its SharePoint
    /// groups and their members, the associated owner/member/visitor groups, the site collection
    /// administrators, and the web's permission levels and grants.
    /// </summary>
    internal class ObjectSiteSecurity : ObjectHandlerBase
    {
        public override string Name => "Site security";

        public override string InternalName => "SiteSecurity";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.Security != null && HasAnything(template.Security);
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            _willExtract ??= true;
            return _willExtract.Value;
        }

        private static bool HasAnything(SiteSecurity security)
        {
            return security.AdditionalAdministrators.Any()
                || security.AdditionalOwners.Any()
                || security.AdditionalMembers.Any()
                || security.AdditionalVisitors.Any()
                || security.SiteGroups.Any()
                || security.ClearExistingAdministrators
                || security.ClearExistingOwners
                || security.ClearExistingMembers
                || security.ClearExistingVisitors
                || security.BreakRoleInheritance
                || !string.IsNullOrEmpty(security.AssociatedOwnerGroup)
                || !string.IsNullOrEmpty(security.AssociatedMemberGroup)
                || !string.IsNullOrEmpty(security.AssociatedVisitorGroup)
                || (security.SiteSecurityPermissions != null
                    && (security.SiteSecurityPermissions.RoleDefinitions.Any()
                        || security.SiteSecurityPermissions.RoleAssignments.Any()));
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            if (!WillProvision(context, template, configuration))
            {
                return parser;
            }

            SiteSecurity security = template.Security;
            IWeb web = context.Web;

            await web.LoadAsync(w => w.Id, w => w.Url, w => w.HasUniqueRoleAssignments).ConfigureAwait(false);
            bool isNoScriptSite = await web.IsNoScriptSiteAsync().ConfigureAwait(false);
            bool isSubSite = await IsSubSiteAsync(context).ConfigureAwait(false);

            if (isSubSite && security.BreakRoleInheritance)
            {
                await web.BreakRoleInheritanceAsync(security.CopyRoleAssignments, security.ClearSubscopes).ConfigureAwait(false);
            }

            // Groups first: an association can only point at a group that exists, and the template
            // routinely defines the group it then associates.
            await ProvisionSiteGroupsAsync(context, security, parser).ConfigureAwait(false);

            await ProvisionAssociatedGroupsAsync(context, security, parser, isNoScriptSite).ConfigureAwait(false);

            await ProvisionAssociatedMembershipAsync(context, security, parser).ConfigureAwait(false);

            await ProvisionAdministratorsAsync(context, security, parser).ConfigureAwait(false);

            await ProvisionPermissionsAsync(context, security, parser).ConfigureAwait(false);

            WriteMessage("Done processing site security", ProvisioningMessageType.Completed);

            return parser;
        }

        /// <summary>
        /// Creates or updates the template's SharePoint groups and fills their membership.
        /// </summary>
        private async Task ProvisionSiteGroupsAsync(PnPContext context, SiteSecurity security, TokenParser parser)
        {
            if (!security.SiteGroups.Any())
            {
                return;
            }

            IWeb web = context.Web;
            await LoadGroupsAsync(web).ConfigureAwait(false);

            List<ISharePointGroup> existingGroups = web.SiteGroups.AsRequested().ToList();
            int index = 0;

            foreach (SiteGroupModel siteGroup in SortByOwnerDependency(security.SiteGroups, parser))
            {
                index++;
                string title = parser.ParseString(siteGroup.Title);
                WriteSubProgress("Site group", title, index, security.SiteGroups.Count);

                ISharePointGroup group = existingGroups
                    .FirstOrDefault(g => string.Equals(g.Title, title, StringComparison.OrdinalIgnoreCase));

                try
                {
                    if (group == null)
                    {
                        group = await web.SiteGroups.AddAsync(title).ConfigureAwait(false);
                        existingGroups.Add(group);
                    }
                    else if (siteGroup.ClearExistingMembers)
                    {
                        await ClearMembersAsync(group).ConfigureAwait(false);
                    }

                    await ApplyGroupSettingsAsync(context, group, siteGroup, parser).ConfigureAwait(false);

                    parser.AddToken(new GroupIdToken(context, group.Title, group.Id.ToString(CultureInfo.InvariantCulture)));

                    await AddUsersAsync(context, group, siteGroup.Members, parser).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"The group '{title}' could not be provisioned: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        /// <summary>
        /// Orders the template's groups so that a group named as another's owner is created first.
        /// </summary>
        private static IEnumerable<SiteGroupModel> SortByOwnerDependency(IEnumerable<SiteGroupModel> groups, TokenParser parser)
        {
            List<SiteGroupModel> remaining = groups.ToList();
            var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ordered = new List<SiteGroupModel>();

            while (remaining.Count > 0)
            {
                List<SiteGroupModel> ready = remaining.Where(g =>
                {
                    string owner = parser.ParseString(g.Owner);

                    if (string.IsNullOrWhiteSpace(owner)
                        || emitted.Contains(owner)
                        || string.Equals(owner, parser.ParseString(g.Title), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    // An owner that is not one of the template's own groups already exists on the
                    // site, or is a user - either way it is not something to wait for.
                    return !remaining.Any(other =>
                        string.Equals(parser.ParseString(other.Title), owner, StringComparison.OrdinalIgnoreCase));
                }).ToList();

                if (ready.Count == 0)
                {
                    // A cycle. Emit what is left in template order; SharePoint will reject the
                    // impossible owner and the handler reports that per group.
                    ordered.AddRange(remaining);
                    break;
                }

                foreach (SiteGroupModel group in ready)
                {
                    ordered.Add(group);
                    emitted.Add(parser.ParseString(group.Title));
                    remaining.Remove(group);
                }
            }

            return ordered;
        }

        private static async Task ApplyGroupSettingsAsync(PnPContext context, ISharePointGroup group,
            SiteGroupModel siteGroup, TokenParser parser)
        {
            bool dirty = false;

            // SharePoint refuses a description longer than 512 characters, and the schema does not
            // stop a template from carrying one.
            string description = Truncate(parser.ParseString(siteGroup.Description), 511);

            if (description != null)
            {
                dirty |= SetIfChanged(group.Description, description, v => group.Description = v);
            }

            dirty |= SetIfChanged(group.AllowMembersEditMembership, siteGroup.AllowMembersEditMembership,
                v => group.AllowMembersEditMembership = v);
            dirty |= SetIfChanged(group.AllowRequestToJoinLeave, siteGroup.AllowRequestToJoinLeave,
                v => group.AllowRequestToJoinLeave = v);
            dirty |= SetIfChanged(group.AutoAcceptRequestToJoinLeave, siteGroup.AutoAcceptRequestToJoinLeave,
                v => group.AutoAcceptRequestToJoinLeave = v);
            dirty |= SetIfChanged(group.OnlyAllowMembersViewMembership, siteGroup.OnlyAllowMembersViewMembership,
                v => group.OnlyAllowMembersViewMembership = v);

            if (!string.IsNullOrEmpty(siteGroup.RequestToJoinLeaveEmailSetting))
            {
                dirty |= SetIfChanged(group.RequestToJoinLeaveEmailSetting,
                    parser.ParseString(siteGroup.RequestToJoinLeaveEmailSetting),
                    v => group.RequestToJoinLeaveEmailSetting = v);
            }

            if (dirty)
            {
                await group.UpdateAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Points the web's owner, member and visitor associations at the named groups.
        /// </summary>
        private async Task ProvisionAssociatedGroupsAsync(PnPContext context, SiteSecurity security,
            TokenParser parser, bool isNoScriptSite)
        {
            var wanted = new List<(AssociatedGroupKind Kind, string Title)>
            {
                (AssociatedGroupKind.Owners, parser.ParseString(security.AssociatedOwnerGroup)),
                (AssociatedGroupKind.Members, parser.ParseString(security.AssociatedMemberGroup)),
                (AssociatedGroupKind.Visitors, parser.ParseString(security.AssociatedVisitorGroup)),
            };

            wanted = wanted.Where(w => !string.IsNullOrEmpty(w.Title)).ToList();

            if (wanted.Count == 0)
            {
                return;
            }

            if (isNoScriptSite)
            {
                string warning = "This is a NoScript site, so the associated owner, member and visitor groups were not changed.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            IWeb web = context.Web;
            await LoadGroupsAsync(web).ConfigureAwait(false);
            List<ISharePointGroup> groups = web.SiteGroups.AsRequested().ToList();

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            foreach ((AssociatedGroupKind kind, string title) in wanted)
            {
                ISharePointGroup group = groups
                    .FirstOrDefault(g => string.Equals(g.Title, title, StringComparison.OrdinalIgnoreCase));

                if (group == null)
                {
                    string warning = $"'{title}' cannot be made the associated {kind.ToString().ToLowerInvariant()} group because no such group exists.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                try
                {
                    await CsomRequestSender.SendAsync(context,
                        new SetAssociatedGroupRequest(siteId, webId, group.Id, kind)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"'{title}' could not be made the associated {kind.ToString().ToLowerInvariant()} group: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }

            // The tokens cache the association, and it has just changed underneath them.
            foreach (AssociatedGroupToken token in parser.Tokens.OfType<AssociatedGroupToken>())
            {
                token.ClearCache();
            }
        }

        /// <summary>
        /// Clears and fills the owner, member and visitor groups.
        /// </summary>
        private async Task ProvisionAssociatedMembershipAsync(PnPContext context, SiteSecurity security, TokenParser parser)
        {
            IWeb web = context.Web;

            await web.LoadAsync(
                w => w.AssociatedOwnerGroup.QueryProperties(g => g.Id, g => g.Title),
                w => w.AssociatedMemberGroup.QueryProperties(g => g.Id, g => g.Title),
                w => w.AssociatedVisitorGroup.QueryProperties(g => g.Id, g => g.Title)).ConfigureAwait(false);

            var groups = new[]
            {
                (Group: SafeGroup(() => web.AssociatedOwnerGroup), Clear: security.ClearExistingOwners,
                    Users: security.AdditionalOwners, Kind: AssociatedGroupToken.AssociatedGroupType.owners),
                (Group: SafeGroup(() => web.AssociatedMemberGroup), Clear: security.ClearExistingMembers,
                    Users: security.AdditionalMembers, Kind: AssociatedGroupToken.AssociatedGroupType.members),
                (Group: SafeGroup(() => web.AssociatedVisitorGroup), Clear: security.ClearExistingVisitors,
                    Users: security.AdditionalVisitors, Kind: AssociatedGroupToken.AssociatedGroupType.visitors),
            };

            foreach (var entry in groups)
            {
                if (entry.Group == null)
                {
                    // A site with no associated group of that kind - common on a communication site.
                    continue;
                }

                if (entry.Clear)
                {
                    await ClearMembersAsync(entry.Group).ConfigureAwait(false);
                }

                await AddUsersAsync(context, entry.Group, entry.Users, parser).ConfigureAwait(false);

                parser.AddToken(new AssociatedGroupToken(context, entry.Kind));
            }
        }

        private async Task ProvisionAdministratorsAsync(PnPContext context, SiteSecurity security, TokenParser parser)
        {
            if (!security.ClearExistingAdministrators && !security.AdditionalAdministrators.Any())
            {
                return;
            }

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            if (security.ClearExistingAdministrators)
            {
                await ClearAdministratorsAsync(context, siteId, webId).ConfigureAwait(false);
            }

            foreach (UserModel admin in security.AdditionalAdministrators)
            {
                string name = parser.ParseString(admin.Name);

                try
                {
                    ISharePointUser user = await context.Web.EnsureUserAsync(name).ConfigureAwait(false);

                    await CsomRequestSender.SendAsync(context,
                        new SetSiteAdministratorRequest(siteId, webId, user.Id, true)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"'{name}' could not be made a site collection administrator: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        /// <summary>
        /// Removes site collection administrator rights from everyone who currently has them.
        /// </summary>
        private async Task ClearAdministratorsAsync(PnPContext context, Guid siteId, Guid webId)
        {
            await context.Web.LoadAsync(w => w.CurrentUser.QueryProperties(u => u.Id),
                w => w.SiteUsers.QueryProperties(u => u.Id, u => u.LoginName, u => u.IsSiteAdmin)).ConfigureAwait(false);

            int currentUserId = context.Web.CurrentUser.Id;

            foreach (ISharePointUser user in context.Web.SiteUsers.AsRequested()
                .Where(u => u.IsSiteAdmin && u.Id != currentUserId).ToList())
            {
                try
                {
                    await CsomRequestSender.SendAsync(context,
                        new SetSiteAdministratorRequest(siteId, webId, user.Id, false)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    context.Logger?.LogWarning(ex, "{Source}: '{User}' could not be removed as a site collection administrator.",
                        Constants.LOGGING_SOURCE, user.LoginName);
                }
            }
        }

        /// <summary>
        /// Creates the template's permission levels and applies its web-level grants.
        /// </summary>
        private async Task ProvisionPermissionsAsync(PnPContext context, SiteSecurity security, TokenParser parser)
        {
            SiteSecurityPermissions permissions = security.SiteSecurityPermissions;

            if (permissions == null)
            {
                return;
            }

            IWeb web = context.Web;

            if (permissions.RoleDefinitions.Any())
            {
                await ProvisionRoleDefinitionsAsync(context, permissions, parser).ConfigureAwait(false);
            }

            if (!permissions.RoleAssignments.Any())
            {
                return;
            }

            await web.LoadAsync(w => w.RoleDefinitions.QueryProperties(r => r.Id, r => r.Name)).ConfigureAwait(false);
            List<IRoleDefinition> roleDefinitions = web.RoleDefinitions.AsRequested().ToList();

            await LoadGroupsAsync(web).ConfigureAwait(false);
            List<ISharePointGroup> groups = web.SiteGroups.AsRequested().ToList();

            foreach (RoleAssignmentModel assignment in permissions.RoleAssignments)
            {
                string principalName = parser.ParseString(assignment.Principal);
                string roleName = parser.ParseString(assignment.RoleDefinition);

                IRoleDefinition roleDefinition = roleDefinitions
                    .FirstOrDefault(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));

                if (roleDefinition == null)
                {
                    string warning = $"The permission level '{roleName}' does not exist on this site.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                int principalId = await ResolvePrincipalAsync(context, groups, principalName).ConfigureAwait(false);

                if (principalId == 0)
                {
                    WriteMessage($"Principal '{assignment.Principal}' not found, so it was not granted '{roleName}'.",
                        ProvisioningMessageType.Warning);
                    continue;
                }

                try
                {
                    if (assignment.Remove)
                    {
                        await web.RemoveRoleDefinitionAsync(principalId, roleDefinition).ConfigureAwait(false);
                    }
                    else
                    {
                        await web.AddRoleDefinitionAsync(principalId, roleDefinition).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    string warning = $"'{principalName}' could not be granted '{roleName}': {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        private async Task ProvisionRoleDefinitionsAsync(PnPContext context, SiteSecurityPermissions permissions, TokenParser parser)
        {
            IWeb web = context.Web;

            await web.LoadAsync(w => w.RoleDefinitions.QueryProperties(r => r.Id, r => r.Name, r => r.Description,
                r => r.BasePermissions)).ConfigureAwait(false);

            List<IRoleDefinition> existing = web.RoleDefinitions.AsRequested().ToList();

            foreach (RoleDefinitionModel definition in permissions.RoleDefinitions)
            {
                string name = parser.ParseString(definition.Name);
                string description = parser.ParseString(definition.Description);

                IRoleDefinition onSite = existing
                    .FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

                // The template model reuses PnP Core's own PermissionKind, so there is nothing to convert.
                PermissionKind[] wanted = definition.Permissions.ToArray();

                try
                {
                    if (onSite == null)
                    {
                        IRoleDefinition created = await web.RoleDefinitions
                            .AddAsync(name, RoleType.None, wanted, description).ConfigureAwait(false);

                        existing.Add(created);
                        parser.AddToken(new RoleDefinitionIdToken(context, created.Name, created.Id));
                        continue;
                    }

                    bool dirty = SetIfChanged(onSite.Description, description, v => onSite.Description = v);

                    // Permissions are added, not replaced: a template naming three permissions is
                    // saying "these as well", and resetting the rest would silently narrow an
                    // existing level that other things already depend on.
                    foreach (PermissionKind permission in wanted.Where(p => !onSite.BasePermissions.Has(p)))
                    {
                        onSite.BasePermissions.Set(permission);
                        dirty = true;
                    }

                    if (dirty)
                    {
                        await onSite.UpdateAsync().ConfigureAwait(false);
                    }

                    parser.AddToken(new RoleDefinitionIdToken(context, onSite.Name, onSite.Id));
                }
                catch (Exception ex)
                {
                    string warning = $"The permission level '{name}' could not be provisioned: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            IWeb web = context.Web;

            await web.LoadAsync(w => w.HasUniqueRoleAssignments,
                w => w.AssociatedOwnerGroup.QueryProperties(g => g.Id, g => g.Title),
                w => w.AssociatedMemberGroup.QueryProperties(g => g.Id, g => g.Title),
                w => w.AssociatedVisitorGroup.QueryProperties(g => g.Id, g => g.Title)).ConfigureAwait(false);

            // A subsite that inherits has no security of its own to describe, and emitting an
            // element for it would make a re-apply break inheritance.
            if (await IsSubSiteAsync(context).ConfigureAwait(false) && !web.HasUniqueRoleAssignments)
            {
                return template;
            }

            ProvisioningTemplateCreationInformation creationInfo =
                configuration?.ToCreationInformation() ?? new ProvisioningTemplateCreationInformation();

            var security = new SiteSecurity
            {
                AssociatedOwnerGroup = SafeGroup(() => web.AssociatedOwnerGroup)?.Title,
                AssociatedMemberGroup = SafeGroup(() => web.AssociatedMemberGroup)?.Title,
                AssociatedVisitorGroup = SafeGroup(() => web.AssociatedVisitorGroup)?.Title,
            };

            await ExtractAssociatedMembershipAsync(context, security).ConfigureAwait(false);

            if (creationInfo.IncludeSiteGroups)
            {
                await ExtractGroupsAsync(context, security).ConfigureAwait(false);
            }

            await ExtractAdministratorsAsync(context, security).ConfigureAwait(false);
            await ExtractPermissionsAsync(context, security).ConfigureAwait(false);

            template.Security = security;

            WriteMessage("Done processing site security", ProvisioningMessageType.Completed);

            return template;
        }

        /// <summary>
        /// Reads the members of the three associated groups into the template's
        /// <c>AdditionalOwners</c>, <c>AdditionalMembers</c> and <c>AdditionalVisitors</c>.
        /// </summary>
        private static async Task ExtractAssociatedMembershipAsync(PnPContext context, SiteSecurity security)
        {
            IWeb web = context.Web;

            var groups = new[]
            {
                (Group: SafeGroup(() => web.AssociatedOwnerGroup), Target: security.AdditionalOwners),
                (Group: SafeGroup(() => web.AssociatedMemberGroup), Target: security.AdditionalMembers),
                (Group: SafeGroup(() => web.AssociatedVisitorGroup), Target: security.AdditionalVisitors),
            };

            foreach (var entry in groups.Where(g => g.Group != null))
            {
                try
                {
                    await entry.Group.LoadAsync(g => g.Users.QueryProperties(u => u.LoginName)).ConfigureAwait(false);

                    foreach (ISharePointUser user in entry.Group.Users.AsRequested())
                    {
                        entry.Target.Add(new UserModel { Name = user.LoginName });
                    }
                }
                catch (Exception ex)
                {
                    context.Logger?.LogDebug(ex, "{Source}: the members of group '{Group}' could not be read.",
                        Constants.LOGGING_SOURCE, entry.Group.Title);
                }
            }
        }

        /// <summary>
        /// Reads the site's SharePoint groups.
        /// </summary>
        private static async Task ExtractGroupsAsync(PnPContext context, SiteSecurity security)
        {
            IWeb web = context.Web;
            await LoadGroupsAsync(web).ConfigureAwait(false);

            var associated = new HashSet<int>(new[]
            {
                SafeGroup(() => web.AssociatedOwnerGroup)?.Id ?? 0,
                SafeGroup(() => web.AssociatedMemberGroup)?.Id ?? 0,
                SafeGroup(() => web.AssociatedVisitorGroup)?.Id ?? 0,
            }.Where(id => id != 0));

            foreach (ISharePointGroup group in web.SiteGroups.AsRequested())
            {
                var siteGroup = new SiteGroupModel
                {
                    Title = group.Title,
                    Description = group.Description,
                    AllowMembersEditMembership = group.AllowMembersEditMembership,
                    AllowRequestToJoinLeave = group.AllowRequestToJoinLeave,
                    AutoAcceptRequestToJoinLeave = group.AutoAcceptRequestToJoinLeave,
                    OnlyAllowMembersViewMembership = group.OnlyAllowMembersViewMembership,
                    RequestToJoinLeaveEmailSetting = group.RequestToJoinLeaveEmailSetting,
                    Owner = SafeString(() => group.OwnerTitle),
                };

                if (!associated.Contains(group.Id))
                {
                    try
                    {
                        await group.LoadAsync(g => g.Users.QueryProperties(u => u.LoginName)).ConfigureAwait(false);

                        foreach (ISharePointUser user in group.Users.AsRequested())
                        {
                            siteGroup.Members.Add(new UserModel { Name = user.LoginName });
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Logger?.LogDebug(ex, "{Source}: the members of group '{Group}' could not be read.",
                            Constants.LOGGING_SOURCE, group.Title);
                    }
                }

                security.SiteGroups.Add(siteGroup);
            }
        }

        private static async Task ExtractAdministratorsAsync(PnPContext context, SiteSecurity security)
        {
            try
            {
                await context.Web.LoadAsync(w => w.SiteUsers.QueryProperties(u => u.Id, u => u.LoginName,
                    u => u.IsSiteAdmin)).ConfigureAwait(false);

                foreach (ISharePointUser user in context.Web.SiteUsers.AsRequested().Where(u => u.IsSiteAdmin))
                {
                    security.AdditionalAdministrators.Add(new UserModel { Name = user.LoginName });
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the site collection administrators could not be read.",
                    Constants.LOGGING_SOURCE);
            }
        }

        private static async Task ExtractPermissionsAsync(PnPContext context, SiteSecurity security)
        {
            IWeb web = context.Web;

            await web.LoadAsync(w => w.RoleDefinitions.QueryProperties(r => r.Id, r => r.Name, r => r.Description,
                r => r.Hidden, r => r.RoleTypeKind, r => r.BasePermissions)).ConfigureAwait(false);

            foreach (IRoleDefinition definition in web.RoleDefinitions.AsRequested())
            {
                // Only custom levels are exported. The built-in ones exist on every site, and a
                // template that recreates them fails on apply.
                if (definition.Hidden || definition.RoleTypeKind != RoleType.None)
                {
                    continue;
                }

                var model = new RoleDefinitionModel
                {
                    Name = definition.Name,
                    Description = definition.Description,
                };

                foreach (PermissionKind permission in Enum.GetValues(typeof(PermissionKind)).Cast<PermissionKind>())
                {
                    if (permission != PermissionKind.EmptyMask && definition.BasePermissions.Has(permission))
                    {
                        model.Permissions.Add(permission);
                    }
                }

                security.SiteSecurityPermissions.RoleDefinitions.Add(model);
            }
        }

        #endregion

        #region Helpers

        private static Task LoadGroupsAsync(IWeb web)
        {
            return web.LoadAsync(w => w.SiteGroups.QueryProperties(g => g.Id, g => g.Title, g => g.LoginName,
                g => g.Description, g => g.AllowMembersEditMembership, g => g.AllowRequestToJoinLeave,
                g => g.AutoAcceptRequestToJoinLeave, g => g.OnlyAllowMembersViewMembership,
                g => g.RequestToJoinLeaveEmailSetting, g => g.OwnerTitle));
        }

        private static async Task ClearMembersAsync(ISharePointGroup group)
        {
            await group.LoadAsync(g => g.Users.QueryProperties(u => u.Id)).ConfigureAwait(false);

            foreach (ISharePointUser user in group.Users.AsRequested().ToList())
            {
                await group.RemoveUserAsync(user.Id).ConfigureAwait(false);
            }
        }

        private async Task AddUsersAsync(PnPContext context, ISharePointGroup group, IEnumerable<UserModel> users, TokenParser parser)
        {
            foreach (UserModel user in users ?? Enumerable.Empty<UserModel>())
            {
                string name = parser.ParseString(user.Name);

                try
                {
                    await group.AddUserAsync(name).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"'{name}' could not be added to group '{group.Title}': {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        /// <summary>
        /// Resolves a principal name to its id, trying the site's groups before its users.
        /// </summary>
        private static async Task<int> ResolvePrincipalAsync(PnPContext context, List<ISharePointGroup> groups, string principalName)
        {
            if (string.IsNullOrEmpty(principalName))
            {
                return 0;
            }

            if (int.TryParse(principalName, NumberStyles.Integer, CultureInfo.InvariantCulture, out int principalId))
            {
                return principalId;
            }

            ISharePointGroup group = groups.FirstOrDefault(g =>
                string.Equals(g.Title, principalName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(g.LoginName, principalName, StringComparison.OrdinalIgnoreCase));

            if (group != null)
            {
                return group.Id;
            }

            try
            {
                ISharePointUser user = await context.Web.EnsureUserAsync(principalName).ConfigureAwait(false);
                return user?.Id ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Reads an associated group, treating "there isn't one" as null rather than an exception.
        /// </summary>
        private static ISharePointGroup SafeGroup(Func<ISharePointGroup> read)
        {
            try
            {
                ISharePointGroup group = read();
                return group != null && group.Id > 0 ? group : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string SafeString(Func<string> read)
        {
            try
            {
                return read();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string Truncate(string value, int maxLength)
        {
            if (value == null)
            {
                return null;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static bool SetIfChanged<T>(T current, T wanted, Action<T> set)
        {
            if (EqualityComparer<T>.Default.Equals(current, wanted))
            {
                return false;
            }

            set(wanted);
            return true;
        }

        #endregion
    }
}
