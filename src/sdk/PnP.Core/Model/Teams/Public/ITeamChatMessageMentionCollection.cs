namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Chat mentions
    /// </summary>
    [ConcreteType(typeof(TeamChatMessageMentionCollection))]
    public interface ITeamChatMessageMentionCollection : IDataModelCollection<ITeamChatMessageMention>, IDataModelCollectionLoad<ITeamChatMessageMention>, ISupportQuery<ITeamChatMessageMention>
    {
    }
}
