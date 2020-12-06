using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of TimeZone objects
    /// </summary>
    [ConcreteType(typeof(TimeZoneCollection))]
    public interface ITimeZoneCollection : IQueryable<ITimeZone>, IDataModelCollection<ITimeZone>
    {
    }
}