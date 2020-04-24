using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

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

        /// <summary>
        /// Dictionary to access the domain model object Deferred properties
        /// </summary>
        Dictionary<string, string> Deferred { get; }
    }
}
