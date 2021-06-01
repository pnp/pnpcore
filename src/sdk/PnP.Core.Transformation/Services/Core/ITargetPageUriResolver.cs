using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Interface used to resolve the SharePoint target uri
    /// </summary>
    public interface ITargetPageUriResolver
    {
        /// <summary>
        /// Resolves the SharePoint target uri for the specified source item
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="targetContext"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Uri> ResolveAsync(ISourceItem sourceItem, PnPContext targetContext, CancellationToken token = default);
    }
}
