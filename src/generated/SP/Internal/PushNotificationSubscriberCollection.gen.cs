using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of PushNotificationSubscriber Domain Model objects
    /// </summary>
    internal partial class PushNotificationSubscriberCollection : QueryableDataModelCollection<IPushNotificationSubscriber>, IPushNotificationSubscriberCollection
    {
        public PushNotificationSubscriberCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}