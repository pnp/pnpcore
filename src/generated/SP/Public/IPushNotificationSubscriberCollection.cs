using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of PushNotificationSubscriber objects
    /// </summary>
    [ConcreteType(typeof(PushNotificationSubscriberCollection))]
    public interface IPushNotificationSubscriberCollection : IQueryable<IPushNotificationSubscriber>, IDataModelCollection<IPushNotificationSubscriber>
    {
    }
}