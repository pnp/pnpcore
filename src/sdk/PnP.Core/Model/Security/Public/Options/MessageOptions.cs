using System.Collections.Generic;

namespace PnP.Core.Model.Security
{

    /// <summary>
    /// The options for a message
    /// </summary>
    public class MessageOptions
    {
        /// <summary>
        /// The subject of the message.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The To: recipients for the message.
        /// </summary>
        public List<RecipientOptions> ToRecipients { get; set; }

        /// <summary>
        /// The Cc: recipients for the message.
        /// </summary>
        public List<RecipientOptions> CcRecipients { get; set; }

        /// <summary>
        /// The Bcc: recipients for the message.
        /// </summary>
        public List<RecipientOptions> BccRecipients { get; set; }

        /// <summary>
        /// The email addresses to use when replying.
        /// </summary>
        public List<RecipientOptions> ReplyTo { get; set; }

        /// <summary>
        /// Attachments of the mail message
        /// </summary>
        public List<MessageAttachmentOptions> Attachments { get; set; }

        /// <summary>
        /// The importance of the message. 
        /// The possible values are: low, normal, and high.
        /// </summary>
        public MessageImportance Importance { get; set; } = MessageImportance.Normal;

        /// <summary>
        /// The body of the message. 
        /// It can be in HTML or text format.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The type of the content. 
        /// The possible values are text and html.
        /// </summary>
        public MessageBodyContentType BodyContentType {get;set;}
    }
}
