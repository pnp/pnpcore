using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a TranslationNotificationRecipientUsers object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class TranslationNotificationRecipientUsers : BaseDataModel<ITranslationNotificationRecipientUsers>, ITranslationNotificationRecipientUsers
    {

        #region New properties

        public string LanguageCode { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Recipients", Expandable = true)]
        public IUserCollection Recipients
        {
            get
            {
                if (!HasValue(nameof(Recipients)))
                {
                    var collection = new UserCollection(this.PnPContext, this, nameof(Recipients));
                    SetValue(collection);
                }
                return GetValue<IUserCollection>();
            }
        }

        #endregion

    }
}
