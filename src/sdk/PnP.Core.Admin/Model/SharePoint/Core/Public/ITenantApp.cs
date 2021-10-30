using PnP.Core.Model;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Represents an interace for an app located at the tenant app catalog.
    /// </summary>

    [ConcreteType(typeof(TenantApp))]
    public interface ITenantApp : IApp
    {
    }
}
