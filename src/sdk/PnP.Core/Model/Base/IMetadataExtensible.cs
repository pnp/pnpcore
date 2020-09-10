using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// Method that's being called by the serializer to complete the model for Graph to Rest transitions
        /// </summary>
        /// <returns></returns>
        internal Task SetGraphToRestMetadataAsync();

        /// <summary>
        /// Method that's being called by the serializer to complete the model for Rest to Graph transitions
        /// </summary>
        /// <returns></returns>
        internal Task SetRestToGraphMetadataAsync();
    }
}
