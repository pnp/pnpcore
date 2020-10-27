using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a AppInstance object
    /// </summary>
    [ConcreteType(typeof(AppInstance))]
    public interface IAppInstance : IDataModel<IAppInstance>, IDataModelGet<IAppInstance>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AppPrincipalId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AppWebFullUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ImageFallbackUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool InError { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string StartPage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string RemoteAppUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SettingsPageUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        #endregion

    }
}
