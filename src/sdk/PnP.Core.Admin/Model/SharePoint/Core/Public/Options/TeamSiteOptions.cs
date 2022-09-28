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

        /// <summary>
        /// If true, only group members can post conversations to the group.
        /// </summary>
        public bool? AllowOnlyMembersToPost { get; set; }

        /// <summary>
        /// If true, members can view the group calendar in Outlook but cannot make changes.
        /// </summary>
        public bool? CalendarMemberReadOnly { get; set; }

        /// <summary>
        /// If true, changes made to the group in Exchange Online are not synced back to on-premises Active Directory.
        /// </summary>
        public bool? ConnectorsDisabled { get; set; }

        /// <summary>
        /// If true, this group is hidden in Outlook experiences.
        /// </summary>
        public bool? HideGroupInOutlook { get; set; }

        /// <summary>
        /// If true, members are not subscribed to the group's calendar events in Outlook.
        /// </summary>
        public bool? SubscribeMembersToCalendarEventsDisabled { get; set; }

        /// <summary>
        /// If true, group members are subscribed to receive group conversations.
        /// </summary>
        public bool? SubscribeNewGroupMembers { get; set; }

        /// <summary>
        /// If true, welcome emails are not sent to new members.
        /// </summary>
        public bool? WelcomeEmailDisabled { get; internal set; }
    }
}
