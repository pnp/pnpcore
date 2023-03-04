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
        /// <param name="options">Options for creating the channel</param>
        /// <returns>Newly added channel</returns>
        public async Task<ITeamChannel> AddAsync(string name, TeamChannelOptions options)
        {
            var newChannel = CreateNewAndAdd(name, options);
            return await newChannel.AddAsync().ConfigureAwait(false) as TeamChannel;
        }

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public async Task<ITeamChannel> AddAsync(string name, string description = null)
        {
            return await AddAsync(name, new TeamChannelOptions(description)).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="options">Options for creating the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel Add(string name, TeamChannelOptions options)
        {
            return AddAsync(name, options).GetAwaiter().GetResult();
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
        /// <param name="options">Options for creating the channel</param>
        /// <returns>Newly added channel</returns>
        public async Task<ITeamChannel> AddBatchAsync(Batch batch, string name, TeamChannelOptions options)
        {
            var newChannel = CreateNewAndAdd(name, options);
            return await newChannel.AddBatchAsync(batch).ConfigureAwait(false) as TeamChannel;
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
            return await AddBatchAsync(name, new TeamChannelOptions(description)).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the channel</param>
        /// <param name="options">Options for creating the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel AddBatch(Batch batch, string name, TeamChannelOptions options)
        {
            return AddBatchAsync(batch, name, options).GetAwaiter().GetResult();
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
        /// <param name="options">Options for creating the channel</param>
        /// <returns>Newly added channel</returns>
        public async Task<ITeamChannel> AddBatchAsync(string name, TeamChannelOptions options)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, name, options).ConfigureAwait(false);
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
        /// <param name="options">Options for creating the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel AddBatch(string name, TeamChannelOptions options)
        {
            return AddBatchAsync(name, options).GetAwaiter().GetResult();
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

        /// <summary>
        /// Creates a new `TeamChannel` instance,
        /// adds it to the collection and configures it.
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="options">Options for creating the channel</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Missing name argument</exception>
        private TeamChannel CreateNewAndAdd(string name, TeamChannelOptions options)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            
            // TODO: validate name restrictions
            
            var newChannel = CreateNewAndAdd() as TeamChannel;
            
            newChannel.DisplayName = name;
            newChannel.Description = options.Description;
            if (options.MembershipType.HasValue)
            {
                newChannel.MembershipType = options.MembershipType.Value;
            }

            return newChannel;
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
