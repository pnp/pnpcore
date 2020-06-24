using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RecentList object
    /// </summary>
    [ConcreteType(typeof(RecentList))]
    public interface IRecentList : IDataModel<IRecentList>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CreatedByName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CreatedByUpn { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CreatedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LastModifiedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int LastViewDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LastviewDateTime { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ListId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ListTitle { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ListUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShouldRemove { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteColor { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteTitle { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebTemplateConfiguration { get; set; }

        #endregion

    }
}
