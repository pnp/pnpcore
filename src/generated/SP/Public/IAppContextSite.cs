using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a AppContextSite object
    /// </summary>
    [ConcreteType(typeof(AppContextSite))]
    public interface IAppContextSite : IDataModel<IAppContextSite>, IDataModelGet<IAppContextSite>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public ISite Site { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWeb Web { get; }

        #endregion

    }
}
