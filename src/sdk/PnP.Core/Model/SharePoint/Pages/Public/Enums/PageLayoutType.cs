namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Types of pages that can be created
    /// </summary>
    public enum PageLayoutType
    {
        /// <summary>
        /// Custom article page, used for user created pages
        /// </summary>
        Article = 0,

        /// <summary>
        /// Home page of modern team sites
        /// </summary>
        Home = 1,

        /// <summary>
        /// Page is an app page, hosting a single SPFX web part full screen
        /// </summary>
        SingleWebPartAppPage = 2,

        /// <summary>
        /// Page is a repost / link page
        /// </summary>
        RepostPage = 3,

        /// <summary>
        /// Page is a custom search result page
        /// </summary>
        HeaderlessSearchResults = 4,

        /// <summary>
        /// Page is a spaces page
        /// </summary>
        Spaces = 5,

        /// <summary>
        /// Page is a Microsoft Syntex Topic page
        /// </summary>
        Topic = 6,

        /// <summary>
        /// Page is a Viva Dashboard page
        /// </summary>
        Dashboard = 7,

        /// <summary>
        /// News digest page (https://support.microsoft.com/en-us/office/create-and-send-a-news-digest-42efc3c6-605f-4a9a-85d5-1f9ff46019bf)
        /// </summary>
        NewsDigest = 8,
    }
}
