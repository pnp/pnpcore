using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ListTemplate objects
    /// </summary>
    [ConcreteType(typeof(ListTemplateCollection))]
    public interface IListTemplateCollection : IQueryable<IListTemplate>, IDataModelCollection<IListTemplate>
    {
    }
}