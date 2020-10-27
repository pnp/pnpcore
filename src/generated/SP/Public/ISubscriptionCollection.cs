using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Subscription objects
    /// </summary>
    [ConcreteType(typeof(SubscriptionCollection))]
    public interface ISubscriptionCollection : IQueryable<ISubscription>, IDataModelCollection<ISubscription>
    {
    }
}