using System.Threading;
using System.Threading.Tasks;


namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Provides the basic interface for a Page Layout mapping provider
    /// </summary>
    public interface IPageLayoutMappingProvider
    {
        /// <summary>
        /// Maps a classic Page Layout into a modern Page Layout
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The output of the mapping activity</returns>
        Task<PageLayoutMappingProviderOutput> MapPageLayoutAsync(PageLayoutMappingProviderInput input, CancellationToken token = default);
    }
}
