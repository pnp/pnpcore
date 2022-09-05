using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A collection of list webhook subscriptions
    /// </summary>
    [ConcreteType(typeof(ListSubscriptionCollection))]
    public interface IListSubscriptionCollection
        : IQueryable<IListSubscription>, IAsyncEnumerable<IListSubscription>, IDataModelCollection<IListSubscription>, IDataModelCollectionLoad<IListSubscription>, IDataModelCollectionDeleteByGuidId, ISupportModules<IListSubscriptionCollection>
    {
        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="expirationDate">A date, when the subscription will expire. Expiration date should not be greater than 6 months period.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        Task<IListSubscription> AddAsync(string notificationUrl, DateTime expirationDate, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="expirationDate">A date, when the subscription will expire. Expiration date should not be greater than 6 months period.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        IListSubscription Add(string notificationUrl, DateTime expirationDate, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="expirationDate">A date, when the subscription will expire. Expiration date should not be greater than 6 months period.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        Task<IListSubscription> AddBatchAsync(string notificationUrl, DateTime expirationDate, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="expirationDate">A date, when the subscription will expire. Expiration date should not be greater than 6 months period.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        IListSubscription AddBatch(string notificationUrl, DateTime expirationDate, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="expirationDate">A date, when the subscription will expire. Expiration date should not be greater than 6 months period.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        Task<IListSubscription> AddBatchAsync(Batch batch, string notificationUrl, DateTime expirationDate, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="expirationDate">A date, when the subscription will expire. Expiration date should not be greater than 6 months period.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        IListSubscription AddBatch(Batch batch, string notificationUrl, DateTime expirationDate, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="validityInMonths">How many months the subscription should stay valid. The maximum is 6 months.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        Task<IListSubscription> AddAsync(string notificationUrl, int validityInMonths = 6, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="validityInMonths">How many months the subscription should stay valid. The maximum is 6 months.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        IListSubscription Add(string notificationUrl, int validityInMonths = 6, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="validityInMonths">How many months the subscription should stay valid. The maximum is 6 months.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        Task<IListSubscription> AddBatchAsync(string notificationUrl, int validityInMonths = 6, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="validityInMonths">How many months the subscription should stay valid. The maximum is 6 months.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        IListSubscription AddBatch(string notificationUrl, int validityInMonths = 6, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="validityInMonths">How many months the subscription should stay valid. The maximum is 6 months.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        Task<IListSubscription> AddBatchAsync(Batch batch, string notificationUrl, int validityInMonths = 6, string clientState = null);

        /// <summary>
        /// Adds a new webhook subscription
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="notificationUrl">A url, where the notification request will be sent</param>
        /// <param name="clientState">A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.</param>
        /// <param name="validityInMonths">How many months the subscription should stay valid. The maximum is 6 months.</param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        IListSubscription AddBatch(Batch batch, string notificationUrl, int validityInMonths = 6, string clientState = null);

        /// <summary>
        /// Gets webhook subscription
        /// </summary>
        /// <param name="subscriptionId">Subscription's Id, a <see cref="Guid"/></param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        Task<IListSubscription> GetByIdAsync(Guid subscriptionId);

        /// <summary>
        /// Gets webhook subscription
        /// </summary>
        /// <param name="subscriptionId">Subscription's Id, a <see cref="Guid"/></param>
        /// <returns>An <see cref="IListSubscription"/> instance</returns>
        IListSubscription GetById(Guid subscriptionId);
    }
}
