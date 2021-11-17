using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal class ListSubscriptionCollection
        : QueryableDataModelCollection<IListSubscription>, IListSubscriptionCollection
    {
        internal const int WebhooksMaxValidityInMonths = 6;
        internal const int WebhookMaxExpirationInDays = 180;

        public ListSubscriptionCollection(PnPContext context, IDataModelParent parent, string memberName = null)
        : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        public IListSubscription Add(string notificationUrl, DateTime expirationDate, string clientState = null)
        {
            return AddAsync(notificationUrl, expirationDate, clientState).GetAwaiter().GetResult();
        }

        public IListSubscription Add(string notificationUrl, int validityInMonths = 6, string clientState = null)
        {
            return AddAsync(notificationUrl, validityInMonths, clientState).GetAwaiter().GetResult();
        }

        public async Task<IListSubscription> AddAsync(string notificationUrl, DateTime expirationDate, string clientState = null)
        {
            Func<ListSubscription, Task<IListSubscription>> create = async (newSubscription) =>
                await newSubscription.AddAsync().ConfigureAwait(false) as IListSubscription;

            return await AddNewAsync(create, notificationUrl, expirationDate, clientState).ConfigureAwait(false);
        }

        public async Task<IListSubscription> AddAsync(string notificationUrl, int validityInMonths = 6, string clientState = null)
        {
            return await AddAsync(notificationUrl, CreateDateFromExpirationMonths(validityInMonths), clientState).ConfigureAwait(false);
        }

        public IListSubscription AddBatch(string notificationUrl, DateTime expirationDate, string clientState = null)
        {
            return AddBatchAsync(notificationUrl, expirationDate, clientState).GetAwaiter().GetResult();
        }

        public IListSubscription AddBatch(Batch batch, string notificationUrl, DateTime expirationDate, string clientState = null)
        {
            return AddBatchAsync(batch, notificationUrl, expirationDate, clientState).GetAwaiter().GetResult();
        }

        public IListSubscription AddBatch(string notificationUrl, int validityInMonths = 6, string clientState = null)
        {
            return AddBatchAsync(notificationUrl, validityInMonths, clientState).GetAwaiter().GetResult();
        }

        public IListSubscription AddBatch(Batch batch, string notificationUrl, int validityInMonths = 6, string clientState = null)
        {
            return AddBatchAsync(batch, notificationUrl, validityInMonths, clientState).GetAwaiter().GetResult();
        }

        public async Task<IListSubscription> AddBatchAsync(string notificationUrl, DateTime expirationDate, string clientState = null)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, notificationUrl, expirationDate, clientState).ConfigureAwait(false);
        }

        public async Task<IListSubscription> AddBatchAsync(Batch batch, string notificationUrl, DateTime expirationDate, string clientState = null)
        {
            Func<ListSubscription, Task<IListSubscription>> create = async (newSubscription) =>
                await newSubscription.AddBatchAsync(batch).ConfigureAwait(false) as IListSubscription;

            return await AddNewAsync(create, notificationUrl, expirationDate, clientState).ConfigureAwait(false);
        }

        public async Task<IListSubscription> AddBatchAsync(string notificationUrl, int validityInMonths = 6, string clientState = null)
        {
            return await AddBatchAsync(notificationUrl, CreateDateFromExpirationMonths(validityInMonths), clientState).ConfigureAwait(false);
        }

        public async Task<IListSubscription> AddBatchAsync(Batch batch, string notificationUrl, int validityInMonths = 6, string clientState = null)
        {
            return await AddBatchAsync(batch, notificationUrl, CreateDateFromExpirationMonths(validityInMonths), clientState).ConfigureAwait(false);
        }

        public IListSubscription GetById(Guid subscriptionId)
        {
            return GetByIdAsync(subscriptionId).GetAwaiter().GetResult();
        }

        public async Task<IListSubscription> GetByIdAsync(Guid subscriptionId)
        {
            var subscription = CreateNew() as ListSubscription;
            subscription.Parent = Parent;
            subscription.AddMetadata(PnPConstants.MetaDataRestId, subscriptionId.ToString());

            return await subscription.GetAsync().ConfigureAwait(false);
        }

        private async Task<IListSubscription> AddNewAsync(Func<ListSubscription, Task<IListSubscription>> create, string notificationUrl, DateTime expirationDate, string clientState = null)
        {
            if (string.IsNullOrEmpty(notificationUrl))
            {
                throw new ArgumentNullException(nameof(notificationUrl));
            }

            ValidateExpirationDateTime(expirationDate, nameof(expirationDate));

            var newSubscription = CreateNewAndAdd() as ListSubscription;
            newSubscription.Parent = Parent;

            newSubscription.NotificationUrl = notificationUrl;
            newSubscription.ExpirationDateTime = expirationDate;
            newSubscription.ClientState = clientState;

            return await create(newSubscription).ConfigureAwait(false);
        }

        private void ValidateExpirationDateTime(DateTime expirationDateTime, string agrumentName)
        {
            var utcDateToValidate = expirationDateTime.ToUniversalTime();
            var utcNow = DateTime.UtcNow;

            var isValid = utcDateToValidate > utcNow
                && utcDateToValidate <= utcNow.AddDays(WebhookMaxExpirationInDays);

            if (!isValid)
            {
                throw new ArgumentOutOfRangeException(agrumentName, string.Format(PnPCoreResources.Exception_Invalid_WebhookExpiration, WebhooksMaxValidityInMonths));
            }
        }

        private DateTime CreateDateFromExpirationMonths(int validityInMonths)
        {
            return validityInMonths == WebhooksMaxValidityInMonths
                ? DateTime.UtcNow.AddDays(WebhookMaxExpirationInDays)
                : DateTime.UtcNow.AddMonths(validityInMonths);
        }
    }
}
