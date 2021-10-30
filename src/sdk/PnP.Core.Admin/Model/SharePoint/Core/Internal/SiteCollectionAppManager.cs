using PnP.Core.Services;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class SiteCollectionAppManager : AppManager<ISiteCollectionApp>, ISiteCollectionAppManager
    {
        protected override string Scope => "sitecollection";

        internal SiteCollectionAppManager(PnPContext pnpContext) : base(pnpContext)
        {
        }
    }
}
