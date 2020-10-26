using System.Linq;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of Microsoft 365 Groups
    /// </summary>
    [ConcreteType(typeof(GraphGroupCollection))]
    public interface IGraphGroupCollection : IQueryable<IGraphGroup>, IDataModelCollection<IGraphGroup>
    {
    }
}
