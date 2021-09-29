namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Contains the available options for creating a team site collection without a group
    /// </summary>
    public class TeamSiteWithoutGroupOptions : CommonNoGroupSiteOptions
    {
        /// <summary>
        /// Default constuctor for creating a <see cref="TeamSiteWithoutGroupOptions"/> object used to define a team site collection creation
        /// </summary>
        /// <param name="url">Url of the team site without group</param>
        /// <param name="title">Title of the team site without group</param>
        public TeamSiteWithoutGroupOptions(string url, string title) : base(url, title)
        {
            WebTemplate = PnPAdminConstants.TeamSiteWithoutGroupTemplate;
        }

    }
}
