using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Assigned tasks ACE
    /// </summary>
    public class AssignedTasksACE : AdaptiveCardExtension
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public AssignedTasksACE(CardSize cardSize = CardSize.Medium) : base(cardSize)
        {
            Id = VivaDashboard.DefaultACEToId(DefaultACE.AssignedTasks);
            Title = "Assigned Tasks";
            Properties = new object();
        }
    }
}
