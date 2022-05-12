namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a collection of phones for an event
    /// </summary>
    [ConcreteType(typeof(GraphPhoneCollection))]
    public interface IGraphPhoneCollection : IDataModelCollection<IGraphPhone>
    {
    }
}
