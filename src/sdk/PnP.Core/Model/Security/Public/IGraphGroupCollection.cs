using System.Linq;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of Microsoft 365 Groups
    /// </summary>
    public interface IGraphGroupCollection : IQueryable<IGraphGroup>, IDataModelCollection<IGraphGroup>
    {
    }
}
