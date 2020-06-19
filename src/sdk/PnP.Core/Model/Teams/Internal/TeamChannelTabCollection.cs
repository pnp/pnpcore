using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChannelTabCollection
    {

        #region Wiki tab
        /// <summary>
        /// Adds a new channel tab
        /// </summary>
        /// <param name="name">Display name of the channel tab</param>
        /// <returns>Newly added channel tab</returns>
        public async Task<ITeamChannelTab> AddWikiTabAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelWikiTab(name);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the channel tab</param>
        /// <returns>Newly added channel tab</returns>
        public ITeamChannelTab AddWikiTab(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelWikiTab(name);

            return newTab.Add(batch, keyValuePairs) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new channel tab
        /// </summary>
        /// <param name="name">Display name of the channel tab</param>
        /// <returns>Newly added channel tab</returns>
        public ITeamChannelTab AddWikiTab(string name)
        {
            return AddWikiTab(PnPContext.CurrentBatch, name);
        }
        #endregion

        #region Document library tab
        /// <summary>
        /// Adds a new DocumentLibrary tab
        /// </summary>
        /// <param name="name">Display name of the  DocumentLibrary tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary tab</returns>
        public async Task<ITeamChannelTab> AddDocumentLibraryTabAsync(string name, Uri documentLibraryUri)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelDocumentLibraryTab(name, documentLibraryUri);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new DocumentLibrary tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the DocumentLibrary tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary tab</returns>
        public ITeamChannelTab AddDocumentLibraryTab(Batch batch, string name, Uri documentLibraryUri)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelDocumentLibraryTab(name, documentLibraryUri);

            return newTab.Add(batch, keyValuePairs) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new DocumentLibrary tab
        /// </summary>
        /// <param name="name">Display name of the DocumentLibrary tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary tab</returns>
        public ITeamChannelTab AddDocumentLibraryTab(string name, Uri documentLibraryUri)
        {
            return AddDocumentLibraryTab(PnPContext.CurrentBatch, name, documentLibraryUri);
        }
        #endregion

        /// <summary>
        /// Creates a wiki <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelDocumentLibraryTab(string displayName, Uri documentLibraryUri)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { "teamsAppId", "com.microsoft.teamspace.tab.files.sharepoint" },
            };

            newTab.Configuration = new TeamChannelTabConfiguration
            {
                EntityId = "",
                ContentUrl = documentLibraryUri.ToString()
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a wiki <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelWikiTab(string displayName)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { "teamsAppId", "com.microsoft.teamspace.tab.wiki" }
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Base tab creation logic
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <returns>Initial <see cref="TeamChannelTab"/> instance</returns>
        private TeamChannelTab CreateTeamChannelTab(string displayName)
        {
            var newChannelTab = CreateNewAndAdd() as TeamChannelTab;

            // Assign field values
            newChannelTab.DisplayName = displayName;

            return newChannelTab;
        }

    }
}
