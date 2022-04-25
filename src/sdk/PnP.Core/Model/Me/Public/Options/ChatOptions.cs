using System.Collections.Generic;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Options for a chat
    /// </summary>
    public class ChatOptions
    {
        /// <summary>
        /// Type of chat
        /// </summary>
        public ChatType ChatType { get; set; }

        /// <summary>
        /// Members in the chat
        /// </summary>
        public List<ChatMemberOptions> Members { get; set; }

        /// <summary>
        /// Chat topic
        /// </summary>
        public string Topic { get; set; }
    }
}
