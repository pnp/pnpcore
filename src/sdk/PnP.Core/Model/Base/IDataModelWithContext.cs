using PnP.Core.Services;

namespace PnP.Core.Model
{
    /// <summary>
    /// Interface to implement PnPContext concept on all model objects
    /// </summary>
    public interface IDataModelWithContext
    {
        PnPContext PnPContext { get; set; }
    }
}