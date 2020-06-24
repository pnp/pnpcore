using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharePointHomeServiceContext object
    /// </summary>
    [ConcreteType(typeof(SharePointHomeServiceContext))]
    public interface ISharePointHomeServiceContext : IDataModel<ISharePointHomeServiceContext>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string CompanyPortalContext { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public ITokenResponse Token { get; }

        #endregion

    }
}
