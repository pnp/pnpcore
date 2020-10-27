using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Field objects
    /// </summary>
    [ConcreteType(typeof(FieldCollection))]
    public interface IFieldCollection : IQueryable<IField>, IDataModelCollection<IField>
    {
    }
}