using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChannelCollection
    {
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
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel Add(Batch batch, string name, string description = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newChannel = CreateNewAndAdd() as TeamChannel;

            // Assign field values
            newChannel.DisplayName = name;
            newChannel.Description = description;

            return newChannel.Add(batch) as TeamChannel;
        }

        /// <summary>
        /// Adds a new channel
        /// </summary>
        /// <param name="name">Display name of the channel</param>
        /// <param name="description">Optional description of the channel</param>
        /// <returns>Newly added channel</returns>
        public ITeamChannel Add(string name, string description = null)
        {
            return Add(PnPContext.CurrentBatch, name, description);
        }

    }
}
