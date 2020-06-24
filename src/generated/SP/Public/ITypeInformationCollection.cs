using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of TypeInformation objects
    /// </summary>
    public interface ITypeInformationCollection : IQueryable<ITypeInformation>, IDataModelCollection<ITypeInformation>
    {
    }
}