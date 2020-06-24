using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of FileVersion objects
    /// </summary>
    public interface IFileVersionCollection : IQueryable<IFileVersion>, IDataModelCollection<IFileVersion>
    {
    }
}