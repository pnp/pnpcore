using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a TranslationNotificationRecipientUsers object
    /// </summary>
    [ConcreteType(typeof(TranslationNotificationRecipientUsers))]
    public interface ITranslationNotificationRecipientUsers : IDataModel<ITranslationNotificationRecipientUsers>, IDataModelGet<ITranslationNotificationRecipientUsers>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserCollection Recipients { get; }

        #endregion

    }
}
