using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// SPDataLeakagePreventionStatusInfo class, write your custom code here
    /// </summary>
    [SharePointType("SP.SPDataLeakagePreventionStatusInfo", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class SPDataLeakagePreventionStatusInfo : BaseDataModel<ISPDataLeakagePreventionStatusInfo>, ISPDataLeakagePreventionStatusInfo
    {
        #region Construction
        public SPDataLeakagePreventionStatusInfo()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool ContainsConfidentialInfo { get => GetValue<bool>(); set => SetValue(value); }

        public string ContainsConfidentialInfoLearnMoreUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ExternalSharingTipsEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string ExternalSharingTipsLearnMoreUrl { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
