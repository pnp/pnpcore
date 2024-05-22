using PnP.Core.Model.Teams;

namespace PnP.Core.Admin.Model.Teams
{
    /// <summary>
    /// Options to configure the created Team
    /// </summary>
    public class TeamOptions
    {
        /// <summary>
        /// Allows to specify the specialization for the Team creation
        /// </summary>
        public TeamSpecialization Specialization { get; set; }
    }
}
