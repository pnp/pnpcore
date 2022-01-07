using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class ChromeOptions : IChromeOptions
    {
        internal PnPContext PnPContext { get; set; }

        internal ChromeOptions(PnPContext pnpContext)
        {
            PnPContext = pnpContext;
        }

        public IHeaderOptions Header { get; internal set; }

        public INavigationOptions Navigation { get; internal set; }

        public IFooterOptions Footer { get; internal set; }
    }
}
