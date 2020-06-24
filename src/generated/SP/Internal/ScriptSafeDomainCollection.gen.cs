using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of ScriptSafeDomain Domain Model objects
    /// </summary>
    internal partial class ScriptSafeDomainCollection : QueryableDataModelCollection<IScriptSafeDomain>, IScriptSafeDomainCollection
    {
        public ScriptSafeDomainCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}