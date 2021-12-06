namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the options to configure an image
    /// </summary>
    public class PageImageOptions
    {
        /// <summary>
        /// Defines the alignment of the image
        /// </summary>
        public PageImageAlignment Alignment { get; set; }

        /// <summary>
        /// Defines the width of the image
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Defines the height of the image
        /// </summary>
        public int? Height { get; set; }
    }
}
