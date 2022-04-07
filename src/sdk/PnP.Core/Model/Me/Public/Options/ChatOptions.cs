using System.Collections.Generic;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// 
    /// </summary>
    public class ChatOptions
    {
        /// <summary>
        /// Type of chat
        /// </summary>
        public ChatType ChatType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ChatMemberOptions> Members { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Topic { get; set; }
    }
}
