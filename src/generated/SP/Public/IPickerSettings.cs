using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a PickerSettings object
    /// </summary>
    [ConcreteType(typeof(PickerSettings))]
    public interface IPickerSettings : IDataModel<IPickerSettings>, IDataModelGet<IPickerSettings>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowEmailAddresses { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowOnlyEmailAddresses { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PrincipalAccountType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int PrincipalSource { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool UseSubstrateSearch { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int VisibleSuggestions { get; set; }

        #endregion

    }
}
