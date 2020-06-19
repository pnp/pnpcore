using PnP.Core.Model.AzureActiveDirectory;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Identity information about a Teams element
    /// </summary>
    public interface ITeamIdentitySet : IComplexType
    {
        /// <summary>
        /// Optional. The application associated with this action.
        /// </summary>
        public IIdentity Application { get; set; }

        /// <summary>
        /// Optional. The team or channel associated with this action.
        /// </summary>
        public IIdentity Conversation { get; set; }

        /// <summary>
        /// Optional. Indicates whether the conversation property identifies a team or channel.
        /// </summary>
        public IIdentity ConversationIdentityType { get; set; }

        /// <summary>
        /// Optional. The device associated with this action.
        /// </summary>
        public IIdentity Device { get; set; }

        /// <summary>
        /// Optional. The phone number associated with this action.
        /// </summary>
        public IIdentity Phone { get; set; }

        /// <summary>
        /// Optional. The user associated with this action.
        /// </summary>
        public IIdentity User { get; set; }
    }
}
