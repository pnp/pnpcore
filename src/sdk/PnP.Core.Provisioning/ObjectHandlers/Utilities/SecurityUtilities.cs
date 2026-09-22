using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using RoleAssignmentModel = PnP.Core.Provisioning.Model.RoleAssignment;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Applies and reads back the <c>&lt;pnp:Security&gt;</c> element that a list, a folder or a
    /// list item can carry.
    /// </summary>
    internal static class SecurityUtilities
    {
        /// <summary>
        /// Writes a template's security element onto a securable object.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="securable">The list, folder or item to secure</param>
        /// <param name="security">The template's security element, or null to do nothing</param>
        /// <param name="parser">The token parser, applied to principal names</param>
        /// <param name="describe">Text naming the target, used in warnings</param>
        /// <param name="reportWarning">Called when a principal cannot be resolved</param>
        internal static async Task ApplyAsync(PnPContext context, ISecurableObject securable, ObjectSecurity security,
            TokenParser parser, string describe, Action<string> reportWarning = null)
        {
            if (security == null || securable == null)
            {
                return;
            }

            await securable.BreakRoleInheritanceAsync(security.CopyRoleAssignments, security.ClearSubscopes).ConfigureAwait(false);

            if (security.RoleAssignments == null || security.RoleAssignments.Count == 0)
            {
                return;
            }

            PrincipalDirectory directory = await PrincipalDirectory.LoadAsync(context).ConfigureAwait(false);

            IWeb web = context.Web;
            await web.LoadAsync(w => w.RoleDefinitions.QueryProperties(r => r.Id, r => r.Name)).ConfigureAwait(false);
            List<IRoleDefinition> roleDefinitions = web.RoleDefinitions.AsRequested().ToList();

            foreach (RoleAssignmentModel assignment in security.RoleAssignments)
            {
                string principalName = parser.ParseString(assignment.Principal);
                string roleName = parser.ParseString(assignment.RoleDefinition);

                int principalId = await directory.ResolveAsync(context, principalName).ConfigureAwait(false);
                if (principalId == 0)
                {
                    Warn(context, reportWarning,
                        $"The principal '{principalName}' could not be found, so it was not granted '{roleName}' on {describe}.");
                    continue;
                }

                IRoleDefinition roleDefinition = roleDefinitions
                    .FirstOrDefault(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));

                if (roleDefinition == null)
                {
                    Warn(context, reportWarning,
                        $"The permission level '{roleName}' does not exist on this site, so '{principalName}' was not granted it on {describe}.");
                    continue;
                }

                if (assignment.Remove)
                {
                    await securable.RemoveRoleDefinitionAsync(principalId, roleDefinition).ConfigureAwait(false);
                }
                else
                {
                    await securable.AddRoleDefinitionAsync(principalId, roleDefinition).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Reads a securable object's unique permissions back into a template element, or returns
        /// null when it still inherits.
        /// </summary>
        internal static async Task<ObjectSecurity> ExtractAsync(PnPContext context, ISecurableObject securable)
        {
            if (securable == null || !securable.HasUniqueRoleAssignments)
            {
                return null;
            }

            if (securable is IDataModelLoad<IList> list)
            {
                await list.LoadAsync(l => l.RoleAssignments.QueryProperties(r => r.PrincipalId,
                    r => r.RoleDefinitions.QueryProperties(d => d.Id, d => d.Name, d => d.RoleTypeKind)))
                    .ConfigureAwait(false);
            }

            PrincipalDirectory directory = await PrincipalDirectory.LoadAsync(context).ConfigureAwait(false);
            await directory.LoadAssociatedGroupTokensAsync(context).ConfigureAwait(false);

            var security = new ObjectSecurity
            {
                CopyRoleAssignments = false,
                ClearSubscopes = false,
            };

            foreach (IRoleAssignment assignment in securable.RoleAssignments.AsRequested())
            {
                string principal = directory.TemplateNameOf(assignment.PrincipalId);
                if (string.IsNullOrEmpty(principal))
                {
                    continue;
                }

                foreach (IRoleDefinition roleDefinition in assignment.RoleDefinitions.AsRequested())
                {
                    if (roleDefinition.RoleTypeKind == RoleType.Guest)
                    {
                        continue;
                    }

                    security.RoleAssignments.Add(new RoleAssignmentModel
                    {
                        Principal = principal,
                        RoleDefinition = roleDefinition.Name,
                    });
                }
            }

            return security;
        }

        private static void Warn(PnPContext context, Action<string> reportWarning, string message)
        {
            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            reportWarning?.Invoke(message);
        }

        /// <summary>
        /// The site's users and groups, loaded once and used in both directions.
        /// </summary>
        private sealed class PrincipalDirectory
        {
            private readonly Dictionary<int, string> namesById = new Dictionary<int, string>();
            private readonly Dictionary<string, int> idsByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<int, string> tokensById = new Dictionary<int, string>();

            internal static async Task<PrincipalDirectory> LoadAsync(PnPContext context)
            {
                var directory = new PrincipalDirectory();

                await context.Web.LoadAsync(
                    w => w.SiteUsers.QueryProperties(u => u.Id, u => u.LoginName, u => u.Title),
                    w => w.SiteGroups.QueryProperties(g => g.Id, g => g.LoginName, g => g.Title)).ConfigureAwait(false);

                foreach (ISharePointGroup group in context.Web.SiteGroups.AsRequested())
                {
                    directory.Add(group.Id, group.Title, group.Title);
                }

                foreach (ISharePointUser user in context.Web.SiteUsers.AsRequested())
                {
                    directory.Add(user.Id, user.LoginName, user.LoginName);
                    directory.idsByName[user.Title] = user.Id;
                }

                return directory;
            }

            private void Add(int id, string canonicalName, string lookupName)
            {
                if (string.IsNullOrEmpty(canonicalName))
                {
                    return;
                }

                namesById[id] = canonicalName;
                idsByName[lookupName] = id;
            }

            internal string NameOf(int principalId)
            {
                return namesById.TryGetValue(principalId, out string name) ? name : null;
            }

            /// <summary>
            /// Reads which of the site's groups are its associated owners, members and visitors, so a
            /// template refers to them by token rather than by the source site's group names.
            /// </summary>
            internal async Task LoadAssociatedGroupTokensAsync(PnPContext context)
            {
                IWeb web = context.Web;

                try
                {
                    await web.LoadAsync(
                        w => w.AssociatedOwnerGroup.QueryProperties(g => g.Id),
                        w => w.AssociatedMemberGroup.QueryProperties(g => g.Id),
                        w => w.AssociatedVisitorGroup.QueryProperties(g => g.Id)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    context.Logger?.LogDebug(ex, "{Source}: the associated groups could not be read, so they are named as they are.",
                        Constants.LOGGING_SOURCE);
                    return;
                }

                AddToken(() => web.AssociatedOwnerGroup, "{associatedownergroupid}");
                AddToken(() => web.AssociatedMemberGroup, "{associatedmembergroupid}");
                AddToken(() => web.AssociatedVisitorGroup, "{associatedvisitorgroupid}");
            }

            private void AddToken(Func<ISharePointGroup> read, string token)
            {
                try
                {
                    ISharePointGroup group = read();
                    if (group != null && group.Id > 0)
                    {
                        tokensById[group.Id] = token;
                    }
                }
                catch (Exception)
                {
                    // The site has no such associated group.
                }
            }

            /// <summary>
            /// The name a template records a principal under: a token for an associated group, otherwise its name.
            /// </summary>
            internal string TemplateNameOf(int principalId)
            {
                return tokensById.TryGetValue(principalId, out string token) ? token : NameOf(principalId);
            }

            /// <summary>
            /// Finds a principal by name, falling back to provisioning the user if the name looks
            /// like one and the site has not seen it yet.
            /// </summary>
            internal async Task<int> ResolveAsync(PnPContext context, string principalName)
            {
                if (string.IsNullOrEmpty(principalName))
                {
                    return 0;
                }

                if (idsByName.TryGetValue(principalName, out int id))
                {
                    return id;
                }

                // What {associatedownergroupid} and the other group id tokens resolve to.
                if (int.TryParse(principalName, NumberStyles.None, CultureInfo.InvariantCulture, out int principalId)
                    && namesById.ContainsKey(principalId))
                {
                    return principalId;
                }

                try
                {
                    ISharePointUser ensured = await context.Web.EnsureUserAsync(principalName).ConfigureAwait(false);
                    if (ensured != null)
                    {
                        Add(ensured.Id, ensured.LoginName, principalName);
                        return ensured.Id;
                    }
                }
                catch (Exception)
                {
                }

                return 0;
            }
        }
    }
}
