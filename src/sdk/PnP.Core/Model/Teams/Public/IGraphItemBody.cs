namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents properties of the body of an item, such as a message, event or group post.
    /// </summary>
    [ConcreteType(typeof(GraphItemBody))]
    public interface IGraphItemBody : IDataModel<IGraphItemBody>
    {
        /// <summary>
        /// The content of the item.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The type of the content.
        /// </summary>
        public EventBodyType ContentType { get; set; }
    }
}
