using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SPSiteManager object
    /// </summary>
    [ConcreteType(typeof(SPSiteManager))]
    public interface ISPSiteManager : IDataModel<ISPSiteManager>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        #endregion

    }
}
