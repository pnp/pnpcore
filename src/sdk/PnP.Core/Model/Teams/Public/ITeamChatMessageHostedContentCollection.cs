namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Collection of chat messages
    /// </summary>
    [ConcreteType(typeof(TeamChatMessageHostedContentCollection))]
    public interface ITeamChatMessageHostedContentCollection : IDataModelCollection<ITeamChatMessageHostedContent>, ISupportQuery<ITeamChatMessageHostedContent>
    {
    }
}
