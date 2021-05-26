using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.SharePoint
{
    /// <summary>
    /// <see cref="ISourceProvider"/> implementation which supports SharePoint Online
    /// </summary>
    public class SharePointSourceProvider : ISourceProvider
    {
        /// <summary>
        /// Gets the source context used by the provider
        /// </summary>
        public PnPContext SourceContext { get; }

        /// <summary>
        /// Creates a new instance for the context
        /// </summary>
        /// <param name="sourceContext"></param>
        public SharePointSourceProvider(PnPContext sourceContext)
        {
            SourceContext = sourceContext ?? throw new ArgumentNullException(nameof(sourceContext));
        }

        /// <summary>
        /// Gets the id of each available items
        /// </summary>
        /// <returns></returns>
        public async IAsyncEnumerable<ISourceItemId> GetItemsIdsAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            for (int x = 0; x < 100; x++)
            {
                yield return new SharePointSourceItemId(new Uri(new Uri(SourceContext.Uri + "/"), $"{x}"));
            }
        }

        /// <summary>
        /// Get the item and related information based on its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token">The cancellation token to use</param>
        /// <returns></returns>
        public Task<ISourceItem> GetItemAsync(ISourceItemId id, CancellationToken token = default)
        {
            if (!(id is SharePointSourceItemId sid))
            {
                throw new ArgumentException($"Only id of type {typeof(SharePointSourceItemId)} is supported");
            }

            ISourceItem result = new SharePointSourceItem(sid);
            return Task.FromResult(result);
        }
    }
}
