using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Subscription objects
    /// </summary>
    public interface ISubscriptionCollection : IQueryable<ISubscription>, IDataModelCollection<ISubscription>
    {
    }
}