using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Modern = PnP.Core.Modernization.Model.Modern;
using Classic = PnP.Core.Modernization.Model.Classic;

namespace PnP.Core.Modernization.Services.Providers
{
    /// <summary>
    /// Provides the basic interface for a Page Layout mapping provider
    /// </summary>
    public interface IPageLayoutMappingProvider
    {
        /// <summary>
        /// Maps a classic Page Layout into a modern Page Layout
        /// </summary>
        /// <param name="source">The classic Page Layout to modernize</param>
        /// <returns>The modern Page Layout</returns>
        Task<Modern.PageLayout> MapPageLayoutAsync(Classic.PageLayout source);
    }
}
