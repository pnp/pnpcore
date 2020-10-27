using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a PushNotificationSubscriber object
    /// </summary>
    [ConcreteType(typeof(PushNotificationSubscriber))]
    public interface IPushNotificationSubscriber : IDataModel<IPushNotificationSubscriber>, IDataModelGet<IPushNotificationSubscriber>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string CustomArgs { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid DeviceAppInstanceId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastModifiedTimeStamp { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime RegistrationTimeStamp { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServiceToken { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SubscriberType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser User { get; }

        #endregion

    }
}
