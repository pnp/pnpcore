using System;
using System.Collections.Generic;
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
        public SharePointSourceProvider(PnPContext sourceContext)
        {
            
        }

        /// <summary>
        /// Gets the id of each available items
        /// </summary>
        /// <returns></returns>
        public IAsyncEnumerable<ISourceItemId> GetItemsIdsAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the item and related information based on its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ISourceItem> GetItemAsync(ISourceItemId id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
