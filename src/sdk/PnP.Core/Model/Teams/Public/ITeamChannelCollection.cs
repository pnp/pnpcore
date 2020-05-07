using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a collection of Team Channels
    /// </summary>
    public interface ITeamChannelCollection : IDataModelCollection<ITeamChannel>, ISupportPaging
    {

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public Task<ITeamChannel> AddAsync(string name, string description = null);

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel Add(Batch batch, string name, string description = null);

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel Add(string name, string description = null);

    }
}
