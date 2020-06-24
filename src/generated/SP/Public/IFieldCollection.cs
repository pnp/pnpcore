using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Field objects
    /// </summary>
    public interface IFieldCollection : IQueryable<IField>, IDataModelCollection<IField>
    {
    }
}