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
        /// Relative url of the navigationnode (e.g. https://contoso.sharepoint.com/sites/testsite)
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Defines whether the navigation node links to an external site or not
        /// </summary>
        public bool IsVisible { get; set; }

    }
}
