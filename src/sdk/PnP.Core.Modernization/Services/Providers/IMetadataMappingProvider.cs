using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.Providers
{
    /// <summary>
    /// Provides the basic interface for a Metadata mapping provider
    /// </summary>
    public interface IMetadataMappingProvider
    {
        /// <summary>
        /// Maps a Metadata Field Value from the source platform to the target platform
        /// </summary>
        /// <param name="sourceFieldName">The internal name of the Metadata Field in the source platform</param>
        /// <param name="sourceValue">The value of the Metadata Field in the source platform</param>
        /// <param name="targetFieldName">The internal name of the Metadata Field in the target platform</param>
        /// <returns>The value of the Metadata Field in the target platform</returns>
        Task<Object> MapMetadataFieldAsync(string sourceFieldName, string targetFieldName, object sourceValue);
    }
}
