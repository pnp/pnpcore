using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of View Domain Model objects
    /// </summary>
    internal partial class ViewCollection : QueryableDataModelCollection<IView>, IViewCollection
    {
        public ViewCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}