using System.ComponentModel;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamAppCollection : BaseDataModelCollection<ITeamApp>, ITeamAppCollection
    {
        public override ITeamApp CreateNew()
        {
            return NewTeamApp();
        }

        private TeamApp NewTeamApp()
        {
            var newTeamApp = new TeamApp
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newTeamApp;
        }
    }
}
