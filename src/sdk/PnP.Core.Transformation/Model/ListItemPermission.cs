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
        /// List of members with role assignments
        /// </summary>
        public Dictionary<string, string[]> Members { get; } = new Dictionary<string, string[]>();
    }
}
