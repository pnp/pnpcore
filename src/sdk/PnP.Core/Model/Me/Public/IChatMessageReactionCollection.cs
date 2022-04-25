namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Reactions on this chat
    /// </summary>
    [ConcreteType(typeof(ChatMessageReactionCollection))]
    public interface IChatMessageReactionCollection : IDataModelCollection<IChatMessageReaction>, IDataModelCollectionLoad<IChatMessageReaction>, ISupportQuery<IChatMessageReaction>, ISupportModules<IChatMessageReactionCollection>
    {
    }
}
