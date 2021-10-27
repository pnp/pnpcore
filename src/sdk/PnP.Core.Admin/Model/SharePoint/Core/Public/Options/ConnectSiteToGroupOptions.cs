using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines the information need to connect a group to an existing site
    /// </summary>
    public class ConnectSiteToGroupOptions : CommonGroupSiteOptions
    {
        /// <summary>
        /// Creates an ConnectSiteToGroupOptions class
        /// </summary>
        /// <param name="url">The url for the site to group connect</param>
        /// <param name="alias">Alias for the group that will be connected to the site</param>
        /// <param name="displayName">Name of the site</param>
        public ConnectSiteToGroupOptions(Uri url, string alias, string displayName) : base(alias, displayName)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            Url = url;
        }

        /// <summary>
        /// The url for the site to group connect
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// If the site already has a modern home page, do we want to keep it?
        /// </summary>
        public bool KeepOldHomePage { get; set; }

        /// <summary>
        /// Set the owners of the group connected site. Specify the UPN values in a string array.
        /// </summary>
        public string[] Owners { get; set; }
    }
}
