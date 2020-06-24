using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of ActivityEntity Domain Model objects
    /// </summary>
    internal partial class ActivityEntityCollection : QueryableDataModelCollection<IActivityEntity>, IActivityEntityCollection
    {
        public ActivityEntityCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}