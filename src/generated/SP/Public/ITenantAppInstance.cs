using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a TenantAppInstance object
    /// </summary>
    [ConcreteType(typeof(TenantAppInstance))]
    public interface ITenantAppInstance : IDataModel<ITenantAppInstance>, IDataModelGet<ITenantAppInstance>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        #endregion

    }
}
