using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Modernization.Model.Classic;
using PnP.Core.Modernization.Model.Modern;

namespace PnP.Core.Modernization.Services.Providers
{
    /// <summary>
    /// Provides the basic interface for a Web Part mapping provider
    /// </summary>
    public interface IWebPartMappingProvider
    {
        /// <summary>
        /// Maps a classic Web Part into a modern Web Part
        /// </summary>
        /// <param name="source">The classic Web Part to modernize</param>
        /// <returns>The modern Web Part mapping to the source classic one</returns>
        Task<ClientSideWebPart> MapWebPartAsync(WebPart source);
    }
}
