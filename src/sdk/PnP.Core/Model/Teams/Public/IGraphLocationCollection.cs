namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a collection of events for a Team
    /// </summary>
    [ConcreteType(typeof(GraphLocationCollection))]
    public interface IGraphLocationCollection : IDataModelCollection<IGraphLocation>
    {
    }
}
