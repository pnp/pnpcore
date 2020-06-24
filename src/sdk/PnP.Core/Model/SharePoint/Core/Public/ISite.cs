using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Site object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Site))]
    public interface ISite : IDataModel<ISite>, IDataModelUpdate
    {
        /// <summary>
        /// The Unique ID of the Site object
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The unique ID of the connected Microsoft 365 Group (if any)
        /// </summary>
        public Guid GroupId { get; }

        /// <summary>
        /// The URL of the Site object
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// The Classification of the Site object
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// Defines whether social bar is disabled on Site Pages in this site collection
        /// </summary>
        public bool SocialBarOnSitePagesDisabled { get; set; }

        /// <summary>
        /// Define if the suitebar search box should show or not 
        /// </summary>
        public SearchBoxInNavBar SearchBoxInNavBar { get; set; }

        /// <summary>
        /// Defines the Search Center URL
        /// </summary>
        public string SearchCenterUrl { get; set; }
        /// <summary>
        /// The RootWeb of the Site object
        /// </summary>
        public IWeb RootWeb { get; }

        /// <summary>
        /// Collection of sub-webs in the current Site object
        /// </summary>
        public IWebCollection AllWebs { get; }

        /// <summary>
        /// Collection of features enabled for the site
        /// </summary>
        public IFeatureCollection Features { get; }
    }
}
