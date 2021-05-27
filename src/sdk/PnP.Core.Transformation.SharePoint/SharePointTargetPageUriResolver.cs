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
    /// Implementation of <see cref="ITargetPageUriResolver"/> for SharePoint sources
    /// </summary>
    public class SharePointTargetPageUriResolver : ITargetPageUriResolver
    {
        /// <summary>
        /// Resolves the SharePoint target uri for the specified source item
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="targetContext"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<Uri> ResolveAsync(ISourceItem sourceItem, PnPContext targetContext, CancellationToken token = default)
        {
            if (sourceItem == null) throw new ArgumentNullException(nameof(sourceItem));
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));
            if (!(sourceItem is SharePointSourceItem spItem))
                throw new ArgumentException($"Only source item of type {typeof(SharePointSourceItem)} is supported", nameof(sourceItem));

            var uri = new Uri(targetContext.Uri + "/" + spItem.Id.Id);
            return Task.FromResult(uri);
        }
    }
}
