using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a PushNotificationSubscriber object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class PushNotificationSubscriber : BaseDataModel<IPushNotificationSubscriber>, IPushNotificationSubscriber
    {

        #region New properties

        public string CustomArgs { get => GetValue<string>(); set => SetValue(value); }

        public Guid DeviceAppInstanceId { get => GetValue<Guid>(); set => SetValue(value); }

        public DateTime LastModifiedTimeStamp { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime RegistrationTimeStamp { get => GetValue<DateTime>(); set => SetValue(value); }

        public string ServiceToken { get => GetValue<string>(); set => SetValue(value); }

        public string SubscriberType { get => GetValue<string>(); set => SetValue(value); }

        public IUser User
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new User
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        #endregion

    }
}
