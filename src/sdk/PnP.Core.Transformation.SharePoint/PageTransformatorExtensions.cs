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
        /// Transform a SharePoint page uri
        /// </summary>
        /// <param name="pageTransformator"></param>
        /// <param name="sourceContext"></param>
        /// <param name="targetContext"></param>
        /// <param name="sourceUri"></param>
        /// <returns></returns>
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
