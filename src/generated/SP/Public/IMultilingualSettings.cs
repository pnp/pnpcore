using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a MultilingualSettings object
    /// </summary>
    [ConcreteType(typeof(MultilingualSettings))]
    public interface IMultilingualSettings : IDataModel<IMultilingualSettings>, IDataModelGet<IMultilingualSettings>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public ITranslationNotificationRecipientUsersCollection Recipients { get; }

        #endregion

    }
}
