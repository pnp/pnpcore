namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Collection of chat messages
    /// </summary>
    [ConcreteType(typeof(ChatMessageAttachmentCollection))]
    public interface IChatMessageAttachmentCollection : IDataModelCollection<IChatMessageAttachment>, IDataModelCollectionLoad<IChatMessageAttachment>, ISupportQuery<IChatMessageAttachment>, ISupportModules<IChatMessageAttachmentCollection>
    {
    }
}
