using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of TypeInformation objects
    /// </summary>
    [ConcreteType(typeof(TypeInformationCollection))]
    public interface ITypeInformationCollection : IQueryable<ITypeInformation>, IDataModelCollection<ITypeInformation>
    {
    }
}