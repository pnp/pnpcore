using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Subscription Domain Model objects
    /// </summary>
    internal partial class SubscriptionCollection : QueryableDataModelCollection<ISubscription>, ISubscriptionCollection
    {
        public SubscriptionCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}