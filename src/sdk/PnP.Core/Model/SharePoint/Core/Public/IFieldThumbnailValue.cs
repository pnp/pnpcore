using System.IO;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a image (thumbnail) field value
    /// </summary>
    public interface IFieldThumbnailValue: IFieldValue
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

        /// <summary>
        /// Uploads an image to a modern image field for the current list item
        /// </summary>
        /// <param name="item">List item to update</param>
        /// <param name="name">Image name</param>
        /// <param name="content">The content of the file.</param>
        /// <returns></returns>
        public Task UploadImageAsync(IListItem item, string name, Stream content);
    }
}
