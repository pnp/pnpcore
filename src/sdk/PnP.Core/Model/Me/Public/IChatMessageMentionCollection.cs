namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Chat mentions
    /// </summary>
    [ConcreteType(typeof(ChatMessageMentionCollection))]
    public interface IChatMessageMentionCollection : IDataModelCollection<IChatMessageMention>, IDataModelCollectionLoad<IChatMessageMention>, ISupportQuery<IChatMessageMention>, ISupportModules<IChatMessageMentionCollection>
    {
    }
}
