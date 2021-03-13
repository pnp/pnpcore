using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Chat Message Hosted Content options
    /// </summary>
    public class ChatMessageHostedContentOptions
    {
        /// <summary>
        /// Hosted Content Id
        /// </summary>
        public string Id { get; set; }
            
        /// <summary>
        /// Hosted content type
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// Hosted content bytes
        /// </summary>
        public string ContentBytes { get; set; }
    }
}
