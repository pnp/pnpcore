using PnP.Core.Services;
using System;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("Microsoft.SharePoint.Webhooks.Subscription", Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/Subscriptions(guid'{Id}')", LinqGet = "_api/web/lists/getbyid(guid'{Parent.Id}')/Subscriptions")]
    internal class ListSubscription
        : BaseDataModel<IListSubscription>, IListSubscription
    {
        public ListSubscription()
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var entity = EntityManager.GetClassInfo(GetType(), this);

                var addParameters = new
                {
                    __metadata = new { type = entity.SharePointType },
                    Resource = ((IList)Parent).Id.ToString(),
                    NotificationUrl,
                    ExpirationDateTime,
                    ClientState
                };

                var body = JsonSerializer.Serialize(addParameters, PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase);

                return new ApiCall($"_api/web/lists(guid'{((IList)Parent).Id}')/Subscriptions", ApiType.SPORest, body);
            };
        }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        [SharePointProperty("clientState")]
        public string ClientState { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("expirationDateTime")]
        public DateTime ExpirationDateTime { get => GetValue<DateTime>(); set => SetValue(value); }

        [SharePointProperty("notificationUrl")]
        public string NotificationUrl { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("resource")]
        public string Resource { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }
    }
}
