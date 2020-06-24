using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of RecycleBinItem objects
    /// </summary>
    public interface IRecycleBinItemCollection : IQueryable<IRecycleBinItem>, IDataModelCollection<IRecycleBinItem>
    {
    }
}