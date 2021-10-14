namespace PnP.Core.Transformation.SharePoint.Model
{
    /// <summary>
    /// Defines the different types of pages supported
    /// </summary>
    internal enum SourcePageType
    {
        /// <summary>
        /// Undefined page type
        /// </summary>
        Undefined,
        /// <summary>
        /// Classic web part page
        /// </summary>
        WebPartPage,
        /// <summary>
        /// Classic wiki page
        /// </summary>
        WikiPage,
        /// <summary>
        /// Classic blog page
        /// </summary>
        BlogPage,
        /// <summary>
        /// Classic publishing page
        /// </summary>
        PublishingPage,
        /// <summary>
        /// Delve blog page
        /// </summary>
        DelveBlogPage,
        /// <summary>
        /// ASP.NET (ASPX) page
        /// </summary>
        AspxPage,
        /// <summary>
        /// Modern client side page
        /// </summary>
        ClientSidePage,
    }
}
