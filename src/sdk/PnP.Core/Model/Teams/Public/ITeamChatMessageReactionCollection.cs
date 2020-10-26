namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Reactions on this chat
    /// </summary>
    [ConcreteType(typeof(TeamChatMessageReactionCollection))]
    public interface ITeamChatMessageReactionCollection: IDataModelCollection<ITeamChatMessageReaction>
    {
    }
}
