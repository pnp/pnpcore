using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ClientWebPart objects
    /// </summary>
    [ConcreteType(typeof(ClientWebPartCollection))]
    public interface IClientWebPartCollection : IQueryable<IClientWebPart>, IDataModelCollection<IClientWebPart>
    {
    }
}