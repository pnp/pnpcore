using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.SharePoint
{
    /// <summary>
    /// Extensions for <see cref="IPageTransformator"/>
    /// </summary>
    public static class PageTransformatorExtensions
    {
        /// <summary>
        /// Transforms a SharePoint page uri
        /// </summary>
        /// <param name="pageTransformator">The page transformator to use</param>
        /// <param name="sourceContext">The source context</param>
        /// <param name="targetContext">The destination context</param>
        /// <param name="sourceUri">The source URI</param>
        /// <returns>The resulting URI</returns>
        public static Task<Uri> TransformSharePointAsync(this IPageTransformator pageTransformator, PnPContext sourceContext, PnPContext targetContext, Uri sourceUri)
        {
            if (pageTransformator == null) throw new ArgumentNullException(nameof(pageTransformator));
            if (sourceContext == null) throw new ArgumentNullException(nameof(sourceContext));
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));
            if (sourceUri == null) throw new ArgumentNullException(nameof(sourceUri));

            var sourceItemId = new SharePointSourceItemId(sourceUri);

            var sourceProvider = new SharePointSourceProvider(sourceContext);
            return pageTransformator.TransformAsync(new PageTransformationTask(sourceProvider, sourceItemId, targetContext));
        }
    }
}
