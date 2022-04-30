namespace PnP.Core.Model.Security
{
    /// <summary>
    /// The sharingLink resource groups link-related data items into a single structure.
    /// </summary>
    [ConcreteType(typeof(SharingLink))]
    public interface ISharingLink
    {
        /// <summary>
        /// If true then the user can only use this link to view the item on the web, and cannot use it to download the contents of the item. Only for OneDrive for Business and SharePoint.
        /// </summary>
        public bool PreventsDownload { get; set; }

        /// <summary>
        /// The type of the link created.
        /// </summary>
        public ShareType Type { get; set; }

        /// <summary>
        /// The scope of the link represented by this permission. Value anonymous indicates the link is usable by anyone, organization indicates the link is only usable for users signed into the same tenant.
        /// </summary>
        public ShareScope Scope { get; set; }

        /// <summary>
        /// For embed links, this property contains the HTML code for an iframe element that will embed the item in a webpage.
        /// </summary>
        public string WebHtml { get; set; }

        /// <summary>
        /// A URL that opens the item in the browser on the OneDrive website.
        /// </summary>
        public string WebUrl { get; set; }
    }
}
