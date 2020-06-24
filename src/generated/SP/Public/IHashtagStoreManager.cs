using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a HashtagStoreManager object
    /// </summary>
    [ConcreteType(typeof(HashtagStoreManager))]
    public interface IHashtagStoreManager : IDataModel<IHashtagStoreManager>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        #endregion

    }
}
