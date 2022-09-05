using System;

namespace PnP.Core.Admin.Model.Teams
{
    /// <summary>
    /// Options to configure the created Team
    /// </summary>
    public class TeamForGroupOptions : TeamOptions
    {
        /// <summary>
        /// Default constructor used for creating a Team
        /// </summary>
        /// <param name="groupId">Id of the Microsoft 365 group to create the Team for</param>
        public TeamForGroupOptions(Guid groupId)
        {
            GroupId = groupId;
        }

        /// <summary>
        /// Id of the Microsoft 365 group to create the Team for
        /// </summary>
        public Guid GroupId { get; set; }

    }
}
