using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of PushNotificationSubscriber objects
    /// </summary>
    public interface IPushNotificationSubscriberCollection : IQueryable<IPushNotificationSubscriber>, IDataModelCollection<IPushNotificationSubscriber>
    {
    }
}