using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Tags on the team
    /// </summary>
    [ConcreteType(typeof(TeamTagCollection))]
    public interface ITeamTagCollection : IDataModelCollection<ITeamTag>, IDataModelCollectionLoad<ITeamTag>, ISupportQuery<ITeamTag>, ISupportModules<ITeamTagCollection>
    {
        /// <summary>
        /// Adds a new tag to a team
        /// </summary>
        /// <param name="options">Tag creation options</param>
        /// <returns>Newly created tag</returns>
        public Task<ITeamTag> AddAsync(TeamTagOptions options);

        /// <summary>
        /// Adds a new tag to a team
        /// </summary>
        /// <param name="options">Tag creation options</param>
        /// <returns>Newly created tag</returns>
        public ITeamTag Add(TeamTagOptions options);
    }
}
