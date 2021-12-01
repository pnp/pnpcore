using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.Teams
{
    /// <summary>
    /// Teams management features
    /// </summary>
    public interface ITeamManager
    {

        /// <summary>
        /// Creates a Teams team and returns a <see cref="PnPContext"/> to start using the created Team
        /// </summary>
        /// <param name="teamToCreate">Information about the Team to create</param>
        /// <param name="creationOptions">Options to control the Team creation flow</param>
        Task<PnPContext> CreateTeamAsync(TeamOptions teamToCreate, TeamCreationOptions creationOptions = null);

        /// <summary>
        /// Creates a Teams team and returns a <see cref="PnPContext"/> to start using the created Team
        /// </summary>
        /// <param name="teamToCreate">Information about the Team to create</param>
        /// <param name="creationOptions">Options to control the Team creation flow</param>
        PnPContext CreateTeam(TeamOptions teamToCreate, TeamCreationOptions creationOptions = null);
    }
}
