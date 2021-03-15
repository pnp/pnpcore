using System;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Chat message attachment
    /// </summary>
    public class ChatMessageAttachmentOptions
    {
        /// <summary>
        /// Unique id of the attachment.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The media type of the content attachment.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// URL for the content of the attachment. Supported protocols: http, https, file and data.
        /// </summary>
        public Uri ContentUrl { get; set; }

        /// <summary>
        /// The content of the attachment. If the attachment is a rich card, set the property to the rich card object. 
        /// This property and contentUrl are mutually exclusive.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Name of the attachment.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL to a thumbnail image that the channel can use if it supports using an alternative, smaller form of content or contentUrl. 
        /// For example, if you set contentType to application/word and set contentUrl to the location of the Word document, you might 
        /// include a thumbnail image that represents the document. The channel could display the thumbnail image instead of the document. 
        /// When the user clicks the image, the channel would open the document.
        /// </summary>
        public Uri ThumbnailUrl { get; set; }
    }
}
