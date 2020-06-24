using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of WebInformation objects
    /// </summary>
    public interface IWebInformationCollection : IQueryable<IWebInformation>, IDataModelCollection<IWebInformation>
    {
    }
}