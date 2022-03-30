namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to set when creating a new navigation node
    /// </summary>
    public class NavigationNodeOptions
    {
        /// <summary>
        /// Title of the new node (e.g. Home)
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Url of the navigationnode (e.g. https://contoso.sharepoint.com/sites/testsite)
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Defines the parent node of the navigation node
        /// </summary>
        public INavigationNode ParentNode { get; set; }
    }
}
