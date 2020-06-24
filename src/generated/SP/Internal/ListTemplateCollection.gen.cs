using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of ListTemplate Domain Model objects
    /// </summary>
    internal partial class ListTemplateCollection : QueryableDataModelCollection<IListTemplate>, IListTemplateCollection
    {
        public ListTemplateCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}