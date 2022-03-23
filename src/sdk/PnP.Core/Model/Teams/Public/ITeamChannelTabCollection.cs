using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a collection of Tabs for a Team Channel
    /// </summary>
    [ConcreteType(typeof(TeamChannelTabCollection))]
    public interface ITeamChannelTabCollection : IQueryable<ITeamChannelTab>, IAsyncEnumerable<ITeamChannelTab>, IDataModelCollection<ITeamChannelTab>, IDataModelCollectionLoad<ITeamChannelTab>, IDataModelCollectionDeleteByGuidId, ISupportModules<ITeamChannelTabCollection>
    {
        /// <summary>
        /// Adds a new wiki channel tab
        /// </summary>
        /// <param name="name">Display name of the wiki channel tab</param>
        /// <returns>Newly added wiki channel tab</returns>
        public Task<ITeamChannelTab> AddWikiTabAsync(string name);

        /// <summary>
        /// Adds a new wiki channel tab
        /// </summary>
        /// <param name="name">Display name of the wiki channel tab</param>
        /// <returns>Newly added wiki channel tab</returns>
        public ITeamChannelTab AddWikiTab(string name);

        /// <summary>
        /// Adds a new wiki channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the wiki channel tab</param>
        /// <returns>Newly added wiki channel tab</returns>
        public Task<ITeamChannelTab> AddWikiTabBatchAsync(Batch batch, string name);

        /// <summary>
        /// Adds a new wiki channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the wiki channel tab</param>
        /// <returns>Newly added wiki channel tab</returns>
        public ITeamChannelTab AddWikiTabBatch(Batch batch, string name);

        /// <summary>
        /// Adds a new wiki channel tab
        /// </summary>
        /// <param name="name">Display name of the wiki channel tab</param>
        /// <returns>Newly added wiki channel tab</returns>
        public Task<ITeamChannelTab> AddWikiTabBatchAsync(string name);

        /// <summary>
        /// Adds a new wiki channel tab
        /// </summary>
        /// <param name="name">Display name of the wiki channel tab</param>
        /// <returns>Newly added wiki channel tab</returns>
        public ITeamChannelTab AddWikiTabBatch(string name);

        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public Task<ITeamChannelTab> AddWebsiteTabAsync(string name, Uri websiteUri);

        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public ITeamChannelTab AddWebsiteTab(string name, Uri websiteUri);

        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public Task<ITeamChannelTab> AddWebsiteTabBatchAsync(Batch batch, string name, Uri websiteUri);

        /// <summary>
        /// Adds a new DocumentLibrary channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public ITeamChannelTab AddWebsiteTabBatch(Batch batch, string name, Uri websiteUri);

        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the Website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public Task<ITeamChannelTab> AddWebsiteTabBatchAsync(string name, Uri websiteUri);

        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the Website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public ITeamChannelTab AddWebsiteTabBatch(string name, Uri websiteUri);

        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="name">Display name of the DocumentLibrary channel tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public Task<ITeamChannelTab> AddDocumentLibraryTabAsync(string name, Uri documentLibraryUri);

        /// <summary>
        /// Adds a new DocumentLibrary channel tab
        /// </summary>
        /// <param name="name">Display name of the DocumentLibrary channel tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary channel tab</returns>
        public ITeamChannelTab AddDocumentLibraryTab(string name, Uri documentLibraryUri);

        /// <summary>
        /// Adds a new DocumentLibrary channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the DocumentLibrary channel tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary channel tab</returns>
        public Task<ITeamChannelTab> AddDocumentLibraryTabBatchAsync(Batch batch, string name, Uri documentLibraryUri);

        /// <summary>
        /// Adds a new DocumentLibrary channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the DocumentLibrary channel tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary channel tab</returns>
        public ITeamChannelTab AddDocumentLibraryTabBatch(Batch batch, string name, Uri documentLibraryUri);

        /// <summary>
        /// Adds a new DocumentLibrary channel tab
        /// </summary>
        /// <param name="name">Display name of the DocumentLibrary channel tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary channel tab</returns>
        public Task<ITeamChannelTab> AddDocumentLibraryTabBatchAsync(string name, Uri documentLibraryUri);

        /// <summary>
        /// Adds a new DocumentLibrary channel tab
        /// </summary>
        /// <param name="name">Display name of the DocumentLibrary channel tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary channel tab</returns>
        public ITeamChannelTab AddDocumentLibraryTabBatch(string name, Uri documentLibraryUri);
    }
}
