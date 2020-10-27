using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a AppCatalog object
    /// </summary>
    [ConcreteType(typeof(AppCatalog))]
    public interface IAppCatalog : IDataModel<IAppCatalog>, IDataModelGet<IAppCatalog>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        #endregion

    }
}
