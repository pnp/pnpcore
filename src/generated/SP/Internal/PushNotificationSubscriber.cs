using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// PushNotificationSubscriber class, write your custom code here
    /// </summary>
    [SharePointType("SP.PushNotificationSubscriber", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class PushNotificationSubscriber : BaseDataModel<IPushNotificationSubscriber>, IPushNotificationSubscriber
    {
        #region Construction
        public PushNotificationSubscriber()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string CustomArgs { get => GetValue<string>(); set => SetValue(value); }

        public Guid DeviceAppInstanceId { get => GetValue<Guid>(); set => SetValue(value); }

        public DateTime LastModifiedTimeStamp { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime RegistrationTimeStamp { get => GetValue<DateTime>(); set => SetValue(value); }

        public string ServiceToken { get => GetValue<string>(); set => SetValue(value); }

        public string SubscriberType { get => GetValue<string>(); set => SetValue(value); }

        public IUser User { get => GetModelValue<IUser>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
