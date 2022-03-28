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

        /// <summary>
        /// Link the image should point to on click
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Alternative text of the image
        /// </summary>
        public string AlternativeText { get; set; }

        /// <summary>
        /// Image caption to show underneath the embedded image
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Internal indicator to specify whether this image will be used as inline image or not
        /// </summary>
        internal bool IsInlineImage { get; set; }
    }
}
