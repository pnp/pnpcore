using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Managed property used for search configuration
    /// </summary>
    public interface IManagedProperty
    {
        /// <summary>
        /// Name of the managed property
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Aliases used for this managed property
        /// </summary>
        List<string> Aliases { get; }

        /// <summary>
        /// Mappings with crawled properties
        /// </summary>
        List<string> Mappings { get; }

        /// <summary>
        /// Type of managed property
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Property pid
        /// </summary>
        string Pid { get; }
    }
}
