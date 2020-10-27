using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a WebInformation object
    /// </summary>
    [ConcreteType(typeof(WebInformation))]
    public interface IWebInformation : IDataModel<IWebInformation>, IDataModelGet<IWebInformation>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Language { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastItemModifiedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastItemUserModifiedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int WebTemplateId { get; set; }

        #endregion

    }
}
