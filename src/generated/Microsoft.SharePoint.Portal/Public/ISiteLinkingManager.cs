using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SiteLinkingManager object
    /// </summary>
    [ConcreteType(typeof(SiteLinkingManager))]
    public interface ISiteLinkingManager : IDataModel<ISiteLinkingManager>, IDataModelGet<ISiteLinkingManager>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        #endregion

    }
}
