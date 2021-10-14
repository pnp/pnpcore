using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.Services.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.SharePoint
{
    /// <summary>
    /// Implementation of <see cref="ISourceProvider"/> supporting SharePoint
    /// </summary>
    public class SharePointSourceProvider : ISourceProvider
    {
        /// <summary>
        /// Gets the source context used by the provider
        /// </summary>
        public ClientContext SourceContext { get; }

        /// <summary>
        /// Creates a new instance for the context
        /// </summary>
        /// <param name="sourceContext">The source PnP Context</param>
        public SharePointSourceProvider(ClientContext sourceContext)
        {
            SourceContext = sourceContext ?? throw new ArgumentNullException(nameof(sourceContext));
        }

        /// <summary>
        /// Gets the id of each available item
        /// </summary>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<ISourceItemId> GetItemsIdsAsync([EnumeratorCancellation] CancellationToken token = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            for (int x = 0; x < 10; x++)
            {
                yield return new SharePointSourceItemId(new Uri(new Uri(SourceContext.Web.Url + "/"), $"{x}"));
            }
        }

        /// <summary>
        /// Gets an item and its related information based on the id
        /// </summary>
        /// <param name="id">The Id of the item to retrieve</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The retrieved item</returns>
        public Task<ISourceItem> GetItemAsync(ISourceItemId id, CancellationToken token = default)
        {
            if (!(id is SharePointSourceItemId sid))
            {
                throw new ArgumentException($"Only id of type {typeof(SharePointSourceItemId)} is supported");
            }

            ISourceItem result = new SharePointSourceItem(sid, SourceContext);
            return Task.FromResult(result);
        }
    }
}
