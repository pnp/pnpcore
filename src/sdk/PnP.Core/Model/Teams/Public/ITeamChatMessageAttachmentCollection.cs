namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Collection of chat messages
    /// </summary>
    [ConcreteType(typeof(TeamChatMessageAttachmentCollection))]
    public interface ITeamChatMessageAttachmentCollection : IDataModelCollection<ITeamChatMessageAttachment>
    {
    }
}
