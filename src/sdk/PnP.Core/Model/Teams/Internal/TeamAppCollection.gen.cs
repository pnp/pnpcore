using System.ComponentModel;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamAppCollection : BaseDataModelCollection<ITeamApp>, ITeamAppCollection
    {
        public override ITeamApp CreateNew()
        {
            return NewTeamApp();
        }

        // PAOLO: It looks like we can remove the following method
        //private TeamApp AddNewTeamApp()
        //{
        //    var newTeamApp = NewTeamApp();
        //    this.items.Add(newTeamApp);
        //    return newTeamApp;
        //}

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
