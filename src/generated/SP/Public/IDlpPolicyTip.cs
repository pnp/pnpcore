using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a DlpPolicyTip object
    /// </summary>
    [ConcreteType(typeof(DlpPolicyTip))]
    public interface IDlpPolicyTip : IDataModel<IDlpPolicyTip>, IDataModelGet<IDlpPolicyTip>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AppliedActionsText { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ComplianceUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string GeneralText { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastProcessedTime { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int OverrideOptions { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string TwoLetterISOLanguageName { get; set; }

        #endregion

    }
}
