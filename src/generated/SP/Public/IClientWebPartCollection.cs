using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ClientWebPart objects
    /// </summary>
    public interface IClientWebPartCollection : IQueryable<IClientWebPart>, IDataModelCollection<IClientWebPart>
    {
    }
}