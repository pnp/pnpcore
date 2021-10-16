using PnP.Core.Model;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Represents an interace for an app located at the site collection app catalog.
    /// </summary>
    [ConcreteType(typeof (SiteCollectionApp))]
    public interface ISiteCollectionApp : IApp
    {
    }
}
