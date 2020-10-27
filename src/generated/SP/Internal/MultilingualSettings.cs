using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// MultilingualSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.MultilingualSettings", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class MultilingualSettings : BaseDataModel<IMultilingualSettings>, IMultilingualSettings
    {
        #region Construction
        public MultilingualSettings()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        public ITranslationNotificationRecipientUsersCollection Recipients { get => GetModelCollectionValue<ITranslationNotificationRecipientUsersCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
