namespace PnP.Core.Admin.Model.Microsoft365
{
    /// <summary>
    /// Contains the available resourceBehaviorOptions for creating a group with Graph Api
    /// See https://learn.microsoft.com/en-us/graph/group-set-options#configure-groups
    /// </summary>
    public class GraphGroupResourceBehaviorOptions
    {
        /// <summary>
        /// If true, only group members can post conversations to the group.
        /// </summary>
        public bool AllowOnlyMembersToPost { get; set; } = false;

        /// <summary>
        /// If true, members can view the group calendar in Outlook but cannot make changes.
        /// </summary>
        public bool CalendarMemberReadOnly { get; set; } = false;

        /// <summary>
        /// If true, changes made to the group in Exchange Online are not synced back to on-premises Active Directory.
        /// </summary>
        public bool ConnectorsDisabled { get; set; } = false;

        /// <summary>
        /// If true, this group is hidden in Outlook experiences.
        /// </summary>
        public bool HideGroupInOutlook { get; set; } = false;

        /// <summary>
        /// If true, members are not subscribed to the group's calendar events in Outlook.
        /// </summary>
        public bool SubscribeMembersToCalendarEventsDisabled { get; set; } = true;

        /// <summary>
        /// If true, group members are subscribed to receive group conversations.
        /// </summary>
        public bool SubscribeNewGroupMembers { get; set; } = false;

        /// <summary>
        /// If true, welcome emails are not sent to new members.
        /// </summary>
        public bool WelcomeEmailDisabled { get; internal set; } = false;
    }
}
