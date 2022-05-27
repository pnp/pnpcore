namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents information about a user in the sending or receiving end of an event, message or group post.
    /// </summary>
    [ConcreteType(typeof(GraphRecipient))]
    public interface IGraphRecipient : IDataModel<IGraphRecipient>
    {
        /// <summary>
        /// The recipient's email address.
        /// </summary>
        public IGraphEmailAddress EmailAddress { get; set; }
    }
}
