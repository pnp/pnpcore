using System.IO;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A list item attachment
    /// </summary>
    [ConcreteType(typeof(Attachment))]
    public interface IAttachment : IDataModel<IAttachment>, IDataModelGet<IAttachment>, IDataModelUpdate, IDataModelDelete
    {

        /// <summary>
        /// Name of the attachment
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Name of the attachment as path
        /// </summary>
        public string FileNameAsPath { get; set; }

        /// <summary>
        /// Server relative URL of the attachment
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        /// <summary>
        /// Server relative URL of the attachment as path
        /// </summary>
        public string ServerRelativePath { get; set; }

        #region Get content

        /// <summary>
        /// Get the content of this attachment.
        /// </summary>
        /// <param name="streamContent">Already return the content before all bytes are read, needed for large file downloads</param>
        /// <returns>Stream containing the binary content of the attachment</returns>
        Task<Stream> GetContentAsync(bool streamContent = false);

        /// <summary>
        /// Get the content of this attachment.
        /// </summary>
        /// <param name="streamContent">Already return the content before all bytes are read, needed for large file downloads</param>
        /// <returns>Stream containing the binary content of the attachment</returns>
        Stream GetContent(bool streamContent = false);

        /// <summary>
        /// Get the content of this attachment.
        /// </summary>
        /// <returns>The binary content of the attachment</returns>
        Task<byte[]> GetContentBytesAsync();

        /// <summary>
        /// Get the content of the attachment.
        /// </summary>
        /// <returns>The binary content of the attachment</returns>
        byte[] GetContentBytes();

        #endregion

        #region Recycle

        /// <summary>
        /// Recycle this list attachment
        /// </summary>
        /// <returns></returns>
        Task RecycleAsync();

        /// <summary>
        /// Recycle this list attachment
        /// </summary>
        /// <returns></returns>
        void Recycle();

        #endregion

    }
}
