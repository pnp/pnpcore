using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Identity information about a Teams element
    /// </summary>
    [ConcreteType(typeof(TeamIdentitySet))]
    public interface ITeamIdentitySet : IDataModel<ITeamIdentitySet>
    {
        /// <summary>
        /// Optional. The application associated with this action.
        /// </summary>
        public IIdentity Application { get; }

        /// <summary>
        /// Optional. The team or channel associated with this action.
        /// </summary>
        public IIdentity Conversation { get; }

        /// <summary>
        /// Optional. Indicates whether the conversation property identifies a team or channel.
        /// </summary>
        public IIdentity ConversationIdentityType { get; }

        /// <summary>
        /// Optional. The device associated with this action.
        /// </summary>
        public IIdentity Device { get; }

        /// <summary>
        /// Optional. The phone number associated with this action.
        /// </summary>
        public IIdentity Phone { get; }

        /// <summary>
        /// Optional. The user associated with this action.
        /// </summary>
        public IIdentity User { get; }
    }
}
