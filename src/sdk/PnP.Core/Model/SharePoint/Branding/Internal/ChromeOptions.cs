using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class ChromeOptions : IChromeOptions
    {
        internal ChromeOptions(PnPContext pnpContext)
        {
            PnPContext = pnpContext;
        }

        internal PnPContext PnPContext { get; set; }        

        public IHeaderOptions Header { get; internal set; }

        public INavigationOptions Navigation { get; internal set; }

        public IFooterOptions Footer { get; internal set; }

    }
}
