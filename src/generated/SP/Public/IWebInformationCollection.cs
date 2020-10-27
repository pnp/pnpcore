using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of WebInformation objects
    /// </summary>
    [ConcreteType(typeof(WebInformationCollection))]
    public interface IWebInformationCollection : IQueryable<IWebInformation>, IDataModelCollection<IWebInformation>
    {
    }
}