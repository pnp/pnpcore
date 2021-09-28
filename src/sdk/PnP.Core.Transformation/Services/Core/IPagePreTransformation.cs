using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Interface used to intercept a page before a transformation
    /// </summary>
    public interface IPagePreTransformation
    {
        /// <summary>
        /// Method called when a page is going to be transformed
        /// </summary>
        /// <param name="context">The context contained all information related to the transformation</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns></returns>
        Task PreTransformAsync(PagePreTransformationContext context, CancellationToken token);
    }
}
