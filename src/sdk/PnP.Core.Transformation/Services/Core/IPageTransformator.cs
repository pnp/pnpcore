using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Provides the basic interface for a Page Transformator
    /// </summary>
    public interface IPageTransformator
    {
        /// <summary>
        /// Transforms a page from classic to modern
        /// </summary>
        /// <param name="task">The context of the transformation process</param>
        /// <returns>The URL of the transformed page</returns>
        Task<Uri> TransformAsync(PageTransformationTask task);
    }
}
