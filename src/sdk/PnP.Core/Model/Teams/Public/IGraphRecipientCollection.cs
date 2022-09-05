namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a collection of recipients for an event
    /// </summary>
    [ConcreteType(typeof(GraphRecipientCollection))]
    public interface IGraphRecipientCollection : IDataModelCollection<IGraphRecipient>
    {
    }
}
