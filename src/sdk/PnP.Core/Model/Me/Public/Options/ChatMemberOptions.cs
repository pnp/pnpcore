using System.Collections.Generic;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Options for a member participating in a chat
    /// </summary>
    public class ChatMemberOptions
    {

        /// <summary>
        /// Member roles
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// User id of the member
        /// </summary>
        public string UserId { get; set; }
    }
}
