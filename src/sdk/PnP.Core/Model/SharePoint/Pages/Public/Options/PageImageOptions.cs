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
        /// Defines the actual width of the image
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Defines the actual height of the image
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Defines the width of the image relative to editor control it's placed in. The <see cref="Width"/>, <see cref="Height"/> and this property need to be all set to end up
        /// to get a functional inline image. When using <see cref="Page.EditorType"/> equal to <see cref="EditorType.CK4"/> then the <see cref="WidthPercentage"/> property 
        /// is not used.
        /// </summary>
        public int? WidthPercentage { get; set; }

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
