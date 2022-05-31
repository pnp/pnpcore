namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to define a custom thumbnail to retrieve
    /// </summary>
    public class CustomThumbnailOptions
    {
        /// <summary>
        /// Generate a thumbnail that fits inside a Width x Height pixel box, maintaining aspect ratio
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Generate a thumbnail that fits inside a Width x Height pixel box, maintaining aspect ratio
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Generate a thumbnail that fits inside a Width x Height pixel box, maintaining aspect ratio, followed by 
        /// resizing the image to fill the Width x Height box and cropping whatever spills outside the box.
        /// </summary>
        public bool Cropped { get; set; }
    }
}
