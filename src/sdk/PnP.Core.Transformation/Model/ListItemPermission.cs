using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Model
{
    /// <summary>
    /// Class used to temporarily hold list item level permissions that need to be re-applied
    /// </summary>
    public class ListItemPermission
    {
        /// <summary>
        /// Roles assigned to the list item
        /// </summary>
        public object RoleAssignments { get; set; }
//        public RoleAssignmentCollection RoleAssignments { get; set; }

        /// <summary>
        /// Resolved principals used in those roles, kept for performance reasons
        /// </summary>
        public Dictionary<string, object> Principals { get; } = new Dictionary<string, object>();
//        public Dictionary<string, Principal> Principals { get; } = new Dictionary<string, Principal>();

    }
}
