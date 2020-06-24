using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ScriptSafeDomain objects
    /// </summary>
    public interface IScriptSafeDomainCollection : IQueryable<IScriptSafeDomain>, IDataModelCollection<IScriptSafeDomain>
    {
    }
}