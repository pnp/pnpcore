using PnP.Core.Services;

namespace PnP.Core.Model
{
    /// <summary>
    /// Interface to implement PnPContext concept on all model objects
    /// </summary>
    public interface IDataModelWithContext
    {
        /// <summary>
        /// <see cref="PnPContext"/> linked to this model instance
        /// </summary>
        PnPContext PnPContext { get; set; }
    }
}