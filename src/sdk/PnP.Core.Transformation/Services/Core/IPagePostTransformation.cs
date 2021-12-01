using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Interface used to intercept a page after a transformation
    /// </summary>
    public interface IPagePostTransformation
    {
        /// <summary>
        /// Method called when a page has been transformed
        /// </summary>
        /// <param name="context">The context contained all information related to the transformation</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns></returns>
        Task PostTransformAsync(PagePostTransformationContext context, CancellationToken token = default);
    }
}
