
using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines a structure for a webhook subscription
    /// </summary>
    [ConcreteType(typeof(ListSubscription))]
    public interface IListSubscription
        : IDataModel<IListSubscription>, IDataModelGet<IListSubscription>, IDataModelLoad<IListSubscription>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// Subscription unique Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// A string, which represents client information. You can use this for validating notifications, tagging different subscriptions, or other reasons.
        /// </summary>
        public string ClientState { get; set; }

        /// <summary>
        /// Expiration date for the webhook
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }

        /// <summary>
        /// A url, where the notification request will be sent
        /// </summary>
        public string NotificationUrl { get; set; }

        /// <summary>
        /// Resource identifier, usually a list id
        /// </summary>
        public string Resource { get; set; }
    }
}
