using PnP.Core.QueryModel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// TeamChannel extension methods
    /// </summary>
    public static class TeamChannelExtensions
    {
        #region GetByDisplayName implementation

        /// <summary>
        /// Extension method to select a channel (ITeamChannel) by displayName
        /// </summary>
        /// <param name="source">The collection of channels to get the channel by displayName from</param>
        /// <param name="displayName">The displayName to search for</param>
        /// <returns>The resulting channel instance, if any</returns>
        public static ITeamChannel GetByDisplayName(this IQueryable<ITeamChannel> source, string displayName)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetByDisplayName(displayName, null);
        }

        /// <summary>
        /// Extension method to select a channel (ITeamChannel) by displayName
        /// </summary>
        /// <param name="source">The collection of channels to get the channel by displayName from</param>
        /// <param name="displayName">The displayName to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting channel instance, if any</returns>
        public static ITeamChannel GetByDisplayName(this IQueryable<ITeamChannel> source, string displayName, params Expression<Func<ITeamChannel, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (displayName == null)
            {
                throw new ArgumentNullException(nameof(displayName));
            }

            return BaseDataModelExtensions.BaseLinqGet(source, c => c.DisplayName == displayName, selectors);
        }

        /// <summary>
        /// Extension method to select a channel (ITeamChannel) by displayName asynchronously
        /// </summary>
        /// <param name="source">The collection of channels to get the channel by displayName from</param>
        /// <param name="displayName">The displayName to search for</param>
        /// <returns>The resulting channel instance, if any</returns>
        public static async Task<ITeamChannel> GetByDisplayNameAsync(this IQueryable<ITeamChannel> source, string displayName)
        {
            // Just rely on the below overload, without providing any selector
            return await source.GetByDisplayNameAsync(displayName, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Extension method to select a channel (ITeamChannel) by displayName asynchronously
        /// </summary>
        /// <param name="source">The collection of channels to get the channel by displayName from</param>
        /// <param name="displayName">The displayName to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting channel instance, if any</returns>
        public static async Task<ITeamChannel> GetByDisplayNameAsync(this IQueryable<ITeamChannel> source, string displayName, params Expression<Func<ITeamChannel, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (displayName == null)
            {
                throw new ArgumentNullException(nameof(displayName));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(source, c => c.DisplayName == displayName, selectors).ConfigureAwait(false);
        }

        #endregion

    }
}
