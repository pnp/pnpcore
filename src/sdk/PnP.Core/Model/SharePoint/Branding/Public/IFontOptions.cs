namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to configure a the site font chrome.
    /// </summary>
    public interface IFontOptions
    {
        /// <summary>
        /// fontOptionForSiteTitle
        /// </summary>
        IFontOption SiteTitle { get; set; }

        /// <summary>
        /// fontOptionForSiteNav
        /// </summary>
        IFontOption SiteNav { get; set; }

        /// <summary>
        /// fontOptionForSiteFooterTitle
        /// </summary>
        IFontOption SiteFooterTitle { get; set; }

        /// <summary>
        /// fontOptionForSiteFooterNav
        /// </summary>
        IFontOption SiteFooterNav { get; set; }
    }
}
