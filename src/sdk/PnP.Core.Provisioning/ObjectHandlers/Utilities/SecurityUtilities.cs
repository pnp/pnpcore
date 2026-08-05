using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
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

            // Breaking inheritance is what makes the role assignments below meaningful, and it is
            // also the only way ClearSubscopes can be expressed - there is no separate call for it.
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

            // Loaded here rather than by the caller: expanding role assignments alongside a list's
            // other collections makes the query string long enough for SharePoint to refuse the
            // whole request.
            if (securable is IDataModelLoad<IList> list)
            {
                await list.LoadAsync(l => l.RoleAssignments.QueryProperties(r => r.PrincipalId,
                    r => r.RoleDefinitions.QueryProperties(d => d.Id, d => d.Name, d => d.RoleTypeKind)))
                    .ConfigureAwait(false);
            }

            PrincipalDirectory directory = await PrincipalDirectory.LoadAsync(context).ConfigureAwait(false);

            var security = new ObjectSecurity
            {
                // The object already has unique permissions and this element describes that state;
                // saying "copy" here reproduces it rather than starting from an empty ACL.
                CopyRoleAssignments = false,
                ClearSubscopes = false,
            };

            foreach (IRoleAssignment assignment in securable.RoleAssignments.AsRequested())
            {
                string principal = directory.NameOf(assignment.PrincipalId);
                if (string.IsNullOrEmpty(principal))
                {
                    continue;
                }

                foreach (IRoleDefinition roleDefinition in assignment.RoleDefinitions.AsRequested())
                {
                    // "Limited Access" is granted by SharePoint as a side effect of a grant further
                    // down the tree. It cannot be assigned, so writing it into a template produces
                    // an element that always fails on apply.
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

            internal static async Task<PrincipalDirectory> LoadAsync(PnPContext context)
            {
                var directory = new PrincipalDirectory();

                await context.Web.LoadAsync(
                    w => w.SiteUsers.QueryProperties(u => u.Id, u => u.LoginName, u => u.Title),
                    w => w.SiteGroups.QueryProperties(g => g.Id, g => g.LoginName, g => g.Title)).ConfigureAwait(false);

                foreach (ISharePointGroup group in context.Web.SiteGroups.AsRequested())
                {
                    // A group is named by its title in a template, never by its login name.
                    directory.Add(group.Id, group.Title, group.Title);
                }

                foreach (ISharePointUser user in context.Web.SiteUsers.AsRequested())
                {
                    // A user is named by its login name, and matched on either.
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

                // A user who has never visited the site is not in SiteUsers yet. EnsureUser creates
                // the entry; a group title that does not exist simply fails, which is the warning
                // the caller reports.
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
                    // Falls through to the caller's warning.
                }

                return 0;
            }
        }
    }
}
