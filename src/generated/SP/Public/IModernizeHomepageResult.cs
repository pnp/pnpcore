using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ModernizeHomepageResult object
    /// </summary>
    [ConcreteType(typeof(ModernizeHomepageResult))]
    public interface IModernizeHomepageResult : IDataModel<IModernizeHomepageResult>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanModernizeHomepage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Reason { get; set; }

        #endregion

    }
}
