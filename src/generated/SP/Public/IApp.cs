using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a App object
    /// </summary>
    [ConcreteType(typeof(App))]
    public interface IApp : IDataModel<IApp>, IDataModelGet<IApp>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ContentMarket { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string VersionString { get; set; }

        #endregion

    }
}
