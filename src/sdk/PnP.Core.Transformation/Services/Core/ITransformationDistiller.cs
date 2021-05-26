using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Abstract interface for a service that defines a list of pages to transform
    /// </summary>
    public interface ITransformationDistiller
    {
        /// <summary>
        /// Defines a list of Page Transformation Tasks
        /// </summary>
        /// <returns>A list of PageTransformationTask to transform</returns>
        IAsyncEnumerable<PageTransformationTask> GetPageTransformationTasksAsync(
            ISourceProvider sourceProvider,
            PnPContext targetContext,
            CancellationToken token = default);
    }
}
