namespace PnP.Core.Model.SharePoint
{
    internal sealed class ChromeOptions : IChromeOptions
    {
        public IHeaderOptions Header { get; internal set; }

        public INavigationOptions Navigation { get; internal set; }

        public IFooterOptions Footer { get; internal set; }
    }
}
