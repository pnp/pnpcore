using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a SPDataLeakagePreventionStatusInfo object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class SPDataLeakagePreventionStatusInfo : BaseDataModel<ISPDataLeakagePreventionStatusInfo>, ISPDataLeakagePreventionStatusInfo
    {

        #region New properties

        public bool ContainsConfidentialInfo { get => GetValue<bool>(); set => SetValue(value); }

        public string ContainsConfidentialInfoLearnMoreUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ExternalSharingTipsEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string ExternalSharingTipsLearnMoreUrl { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
