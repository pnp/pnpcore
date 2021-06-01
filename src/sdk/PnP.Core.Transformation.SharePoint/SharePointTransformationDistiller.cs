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
        private readonly ITargetPageUriResolver targetPageUriResolver;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="targetPageUriResolver"></param>
        public SharePointTransformationDistiller(ITargetPageUriResolver targetPageUriResolver)
        {
            this.targetPageUriResolver = targetPageUriResolver ?? throw new ArgumentNullException(nameof(targetPageUriResolver));
        }

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
                yield return new PageTransformationTask(sourceProvider, sourceItemId, targetContext);
            }
        }
    }
}
