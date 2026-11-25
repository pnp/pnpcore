namespace PnP.Core.Model.SharePoint
{
    internal sealed class FontOptions : IFontOptions
    {
        public IFontOption SiteTitle { get; set; } = null;
        public IFontOption SiteNav { get; set; } = null;
        public IFontOption SiteFooterTitle { get; set; } = null;
        public IFontOption SiteFooterNav { get; set; } = null;
    }
}
