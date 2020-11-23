namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Class that implements the modern page header
    /// </summary>
    public interface IPageHeader
    {

        /// <summary>
        /// Server relative link to page header image, set to null for default header image.
        /// Note: image needs to reside in the current site
        /// </summary>
        public string ImageServerRelativeUrl { get; set; }

        /// <summary>
        /// Image focal point X coordinate
        /// </summary>
        public double? TranslateX { get; set; }

        /// <summary>
        /// Image focal point Y coordinate
        /// </summary>
        public double? TranslateY { get; set; }

        /// <summary>
        /// Type of layout used inside the header
        /// </summary>
        public PageHeaderLayoutType LayoutType { get; set; }

        /// <summary>
        /// Alignment of the title in the header
        /// </summary>
        public PageHeaderTitleAlignment TextAlignment { get; set; }

        /// <summary>
        /// Show the topic header in the title region
        /// </summary>
        public bool ShowTopicHeader { get; set; }

        /// <summary>
        /// Show the page publication date in the title region
        /// </summary>
        public bool ShowPublishDate { get; set; }

        /// <summary>
        /// The topic header text to show if ShowTopicHeader is set to true
        /// </summary>
        public string TopicHeader { get; set; }

        /// <summary>
        /// Alternative text for the header image
        /// </summary>
        public string AlternativeText { get; set; }

        /// <summary>
        /// Page author(s) to be displayed
        /// </summary>
        public string Authors { get; set; }

        /// <summary>
        /// Page author byline
        /// </summary>
        public string AuthorByLine { get; set; }

        /// <summary>
        /// Id of the page author
        /// </summary>
        public int AuthorByLineId { get; set; }
    }
}
