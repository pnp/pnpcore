﻿using PnP.Core.Services;
using System.Collections.Generic;
using System.Threading;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Abstract interface for a service that defines a list of items to transform
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
