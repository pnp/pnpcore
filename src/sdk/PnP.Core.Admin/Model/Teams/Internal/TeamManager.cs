using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.Teams
{
    internal sealed class TeamManager : ITeamManager
    {
        private readonly PnPContext context;

        internal TeamManager(PnPContext pnpContext)
        {
            context = pnpContext;
        }

        public async Task<PnPContext> CreateTeamAsync(TeamOptions teamToCreate, TeamCreationOptions creationOptions = null)
        {
            if (teamToCreate is TeamForGroupOptions teamForGroupOptions)
            {
                return await TeamCreator.CreateTeamForGroupAsync(context, teamForGroupOptions, creationOptions).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("Coming soon");
            }
        }

        public PnPContext CreateTeam(TeamOptions teamToCreate, TeamCreationOptions creationOptions = null)
        {
            return CreateTeamAsync(teamToCreate, creationOptions).GetAwaiter().GetResult();
        }
    }
}
