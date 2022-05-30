using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Contains the available options for creating a group connected team site collection
    /// </summary>
    public class TeamSiteOptions : CommonGroupSiteOptions
    {
        /// <summary>
        /// Default constuctor for creating a <see cref="TeamSiteOptions"/> object used to define a team site collection creation
        /// </summary>
        /// <param name="alias">Alias for the group to create</param>
        /// <param name="displayName">Displayname for the group to create</param>
        public TeamSiteOptions(string alias, string displayName) : base(alias, displayName)
        {
            WebTemplate = PnPAdminConstants.TeamSiteTemplate;
        }

        /// <summary>
        /// Set the owners of the team site. Specify the UPN values in a string array.
        /// </summary>
        public string[] Owners { get; set; }

        /// <summary>
        /// The ID of the Site Design to apply, if any (not applicable when application permissions are used)
        /// </summary>
        public Guid? SiteDesignId { get; set; }
    }
}
