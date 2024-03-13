namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a image (thumbnail) field value
    /// </summary>
    public interface IFieldThumbnailValue
        : IFieldValue
    {
        /// <summary>
        /// Filename identifiying this image
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// Server relative URL to access this image
        /// </summary>
        public string ServerRelativeUrl { get; }
        /// <summary>
        /// Server URL
        /// </summary>
        public string ServerUrl { get; }
        /// <summary>
        /// Thumbnail renderer
        /// </summary>
        public object ThumbnailRenderer { get; }
    }
}
