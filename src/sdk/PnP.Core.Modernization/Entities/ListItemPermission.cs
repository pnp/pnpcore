using Microsoft.SharePoint.Client;
using System.Collections.Generic;

namespace PnP.Core.Modernization.Entities
{
    /// <summary>
    /// Class used to temporarily hold list item level permissions that need to be re-applied
    /// </summary>
    public class ListItemPermission
    {
        /// <summary>
        /// Roles assigned to the list item
        /// </summary>
        public RoleAssignmentCollection RoleAssignments { get; set; }

        /// <summary>
        /// Resolved principals used in those roles, kept for performance reasons
        /// </summary>
        public Dictionary<string, Principal> Principals { get; set; }

    }
}
