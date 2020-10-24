using PnP.Core.QueryModel;
using PnP.Core.Services;
using System.Threading.Tasks;

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