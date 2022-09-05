using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    internal sealed class TeamChannelCollection : QueryableDataModelCollection<ITeamChannel>, ITeamChannelCollection
    {
        public TeamChannelCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add methods

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public async Task<ITeamChannel> AddAsync(string name, string description = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            // TODO: validate name restrictions

            var newChannel = CreateNewAndAdd() as TeamChannel;

            // Assign field values
            newChannel.DisplayName = name;
            newChannel.Description = description;

            return await newChannel.AddAsync().ConfigureAwait(false) as TeamChannel;
        }

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel Add(string name, string description = null)
        {
            return AddAsync(name, description).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public async Task<ITeamChannel> AddBatchAsync(Batch batch, string name, string description = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newChannel = CreateNewAndAdd() as TeamChannel;

            // Assign field values
            newChannel.DisplayName = name;
            newChannel.Description = description;

            return await newChannel.AddBatchAsync(batch).ConfigureAwait(false) as TeamChannel;
        }

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel AddBatch(Batch batch, string name, string description = null)
        {
            return AddBatchAsync(batch, name, description).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public async Task<ITeamChannel> AddBatchAsync(string name, string description = null)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, name, description).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel AddBatch(string name, string description = null)
        {
            return AddBatchAsync(name, description).GetAwaiter().GetResult();
        }

        #endregion

        #region GetByDisplayName methods

        /// <summary>
        /// Get channel by display name
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="selectors"></param>
        /// <returns></returns>
        public ITeamChannel GetByDisplayName(string displayName, params Expression<Func<ITeamChannel, object>>[] selectors)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException(nameof(displayName));
            }

            return GetByDisplayNameAsync(displayName, selectors).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously Get channel by display name
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="selectors"></param>
        /// <returns></returns>
        /// <remarks>This does not work due with a $top=1 limitation in the graph when calling tests</remarks>
        public async Task<ITeamChannel> GetByDisplayNameAsync(string displayName, params Expression<Func<ITeamChannel, object>>[] selectors)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException(nameof(displayName));
            }

            return await this.QueryProperties(selectors).FirstOrDefaultAsync(c => c.DisplayName == displayName).ConfigureAwait(false);
        }

        #endregion
    }
}
