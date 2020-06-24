using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a TenantSettings object
    /// </summary>
    [ConcreteType(typeof(TenantSettings))]
    public interface ITenantSettings : IDataModel<ITenantSettings>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string CorporateCatalogUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public ITenantSettings Current { get; }

        #endregion

    }
}
