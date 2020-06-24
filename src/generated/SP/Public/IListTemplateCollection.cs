using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ListTemplate objects
    /// </summary>
    public interface IListTemplateCollection : IQueryable<IListTemplate>, IDataModelCollection<IListTemplate>
    {
    }
}