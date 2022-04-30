namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Collection of chat messages
    /// </summary>
    [ConcreteType(typeof(ChatMessageHostedContentCollection))]
    public interface IChatMessageHostedContentCollection : IDataModelCollection<IChatMessageHostedContent>, IDataModelCollectionLoad<IChatMessageHostedContent>, ISupportQuery<IChatMessageHostedContent>, ISupportModules<IChatMessageHostedContentCollection>
    {
    }
}
