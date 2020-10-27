using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a AppTile object
    /// </summary>
    [ConcreteType(typeof(AppTile))]
    public interface IAppTile : IDataModel<IAppTile>, IDataModelGet<IAppTile>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid AppId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AppPrincipalId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int AppSource { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int AppStatus { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int AppType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int BaseTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ChildCount { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ContentMarket { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CustomSettingsUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsCorporateCatalogSite { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LastModified { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Version { get; set; }

        #endregion

    }
}
