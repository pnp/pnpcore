using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a MultilingualSettings object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class MultilingualSettings : BaseDataModel<IMultilingualSettings>, IMultilingualSettings
    {

        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Recipients", Expandable = true)]
        public ITranslationNotificationRecipientUsersCollection Recipients
        {
            get
            {
                if (!HasValue(nameof(Recipients)))
                {
                    var collection = new TranslationNotificationRecipientUsersCollection(this.PnPContext, this, nameof(Recipients));
                    SetValue(collection);
                }
                return GetValue<ITranslationNotificationRecipientUsersCollection>();
            }
        }

        #endregion

    }
}
