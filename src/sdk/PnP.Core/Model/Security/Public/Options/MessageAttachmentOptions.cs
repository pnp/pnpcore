using System.Collections.Generic;

namespace PnP.Core.Model.Security
{

    /// <summary>
    /// Options for attachments for a message
    /// </summary>
    public class MessageAttachmentOptions
    {
        /// <summary>
        /// Name of the attachment
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Content-Type of the attachment
        /// </summary>
        public string ContentType { get; set; } = "text/plain";

        /// <summary>
        /// Bytes of the attachment
        /// </summary>
        public string ContentBytes { get; set; }
    }
}
