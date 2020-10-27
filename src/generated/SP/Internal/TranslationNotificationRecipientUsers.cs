using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TranslationNotificationRecipientUsers class, write your custom code here
    /// </summary>
    [SharePointType("SP.TranslationNotificationRecipientUsers", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class TranslationNotificationRecipientUsers : BaseDataModel<ITranslationNotificationRecipientUsers>, ITranslationNotificationRecipientUsers
    {
        #region Construction
        public TranslationNotificationRecipientUsers()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string LanguageCode { get => GetValue<string>(); set => SetValue(value); }

        public IUserCollection Recipients { get => GetModelCollectionValue<IUserCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
