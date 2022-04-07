namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Attachments connected to a chat message
    /// </summary>
    [ConcreteType(typeof(ChatMessageHostedContent))]
    public interface IChatMessageHostedContent : IDataModel<IChatMessageHostedContent>
    {
        /// <summary>
        /// Read-only. Unique id of the attachment.
        /// </summary>
        [GraphProperty("@microsoft.graph.temporaryId")]
        public string Id { get; }

        /// <summary>
        /// The media type of the content attachment.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The content bytes of the attachment as Base64-encoded bytes
        /// </summary>
        public string ContentBytes { get; set; }

    }
}
