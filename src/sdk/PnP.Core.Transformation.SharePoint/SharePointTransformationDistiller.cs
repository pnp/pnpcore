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
    /// Default implementation of transformation distiller which returns all items from source provider
    /// </summary>
    public class SharePointTransformationDistiller : ITransformationDistiller
    {
        /// <summary>
        /// Defines a list of Page Transformation Tasks
        /// </summary>
        /// <returns>A list of PageTransformationTask to transform</returns>
        public async IAsyncEnumerable<PageTransformationTask> GetPageTransformationTasksAsync(
            ISourceProvider sourceProvider,
            PnPContext targetContext,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            if (sourceProvider == null) throw new ArgumentNullException(nameof(sourceProvider));
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));

            await foreach (var sourceItemId in sourceProvider.GetItemsIdsAsync(token).WithCancellation(token))
            {
                var sourceItem = await sourceProvider.GetItemAsync(sourceItemId, token).ConfigureAwait(false);

                var spSourceItem = sourceItem as SharePointSourceItem;
                if (spSourceItem == null) throw new InvalidOperationException($"Only source item of type {typeof(SharePointSourceItem)} is supported");

                // TODO: compute the target uri
                var targetPageUri = new Uri(targetContext.Uri, sourceItemId.Id);
                yield return new PageTransformationTask(sourceItem, targetContext, targetPageUri);
            }
        }
    }
}
