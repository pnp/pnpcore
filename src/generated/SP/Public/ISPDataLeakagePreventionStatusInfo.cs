using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SPDataLeakagePreventionStatusInfo object
    /// </summary>
    [ConcreteType(typeof(SPDataLeakagePreventionStatusInfo))]
    public interface ISPDataLeakagePreventionStatusInfo : IDataModel<ISPDataLeakagePreventionStatusInfo>, IDataModelGet<ISPDataLeakagePreventionStatusInfo>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool ContainsConfidentialInfo { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ContainsConfidentialInfoLearnMoreUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ExternalSharingTipsEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ExternalSharingTipsLearnMoreUrl { get; set; }

        #endregion

    }
}
