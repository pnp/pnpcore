namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Types of pages headers that a page can use
    /// </summary>
    public enum PageHeaderType
    {
        /// <summary>
        /// The page does not have a header
        /// </summary>
        None = 0,

        /// <summary>
        /// The page uses the default page header
        /// </summary>
        Default = 1,

        /// <summary>
        /// The page use a customized header (e.g. with image + offset)
        /// </summary>
        Custom = 2,

        /// <summary>
        /// All Headersettings are in the PageTitle WebPart. PageTitle-WP in a section as OneColumnFullWith (Message ID: MC791596 / Roadmap ID: 386904)
        /// </summary>
        PageTitleWebPart = 3,
    }
}
