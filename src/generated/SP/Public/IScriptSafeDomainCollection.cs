using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ScriptSafeDomain objects
    /// </summary>
    [ConcreteType(typeof(ScriptSafeDomainCollection))]
    public interface IScriptSafeDomainCollection : IQueryable<IScriptSafeDomain>, IDataModelCollection<IScriptSafeDomain>
    {
    }
}