using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    internal sealed class TeamTagCollection : BaseDataModelCollection<ITeamTag>, ITeamTagCollection
    {
        #region Methods

        public async Task<ITeamTag> AddAsync(TeamTagOptions options)
        {
            var newAndAdd = CreateNewAndAdd() as TeamTag;

            newAndAdd.DisplayName = options.DisplayName;
            var members = new TeamTagMemberCollection();

            foreach (var user in options.Members)
            {
                members.Add(new TeamTagMember { UserId = user.UserId });
            }
            newAndAdd.Members = members;
            return await newAndAdd.AddAsync().ConfigureAwait(false) as TeamTag;
        }

        public ITeamTag Add(TeamTagOptions options)
        {
            return AddAsync(options).GetAwaiter().GetResult();
        }

        #endregion
    }
}
