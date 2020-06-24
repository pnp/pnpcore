using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of FieldLink objects
    /// </summary>
    public interface IFieldLinkCollection : IQueryable<IFieldLink>, IDataModelCollection<IFieldLink>
    {
    }
}