using System.Collections.Generic;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every object that is provided with metadata
    /// eventually with deferred loading
    /// </summary>
    public interface IMetadataExtensible
    {
        /// <summary>
        /// Dictionary to access the domain model object Metadata
        /// </summary>
        Dictionary<string, string> Metadata { get; }
    }
}
