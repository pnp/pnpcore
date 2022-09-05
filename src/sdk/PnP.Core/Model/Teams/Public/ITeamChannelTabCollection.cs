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
        /// Adds a new Website channel tab
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
        /// Adds a new document library channel tab
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

        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="name">Display name of the Word channel tab</param>
        /// <param name="fileUri">Uri to the file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Word channel tab</returns>
        public Task<ITeamChannelTab> AddWordTabAsync(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="name">Display name of the Word channel tab</param>
        /// <param name="fileUri">Uri to the website that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Word channel tab</returns>
        public ITeamChannelTab AddWordTab(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Word channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Word channel tab</returns>
        public Task<ITeamChannelTab> AddWordTabBatchAsync(Batch batch, string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Word channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param> 
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Word channel tab</returns>
        public ITeamChannelTab AddWordTabBatch(Batch batch, string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="name">Display name of the Word channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param>        
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Word channel tab</returns>
        public Task<ITeamChannelTab> AddWordTabBatchAsync(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="name">Display name of the Word channel tab</param>
        /// <param name="fileUri">Uri to the Word file that needs to be added as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Word channel tab</returns>
        public ITeamChannelTab AddWordTabBatch(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to the Excel that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Excel channel tab</returns>
        public Task<ITeamChannelTab> AddExcelTabAsync(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to the Excel that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Excel channel tab</returns>
        public ITeamChannelTab AddExcelTab(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to Excel file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Excel channel tab</returns>
        public Task<ITeamChannelTab> AddExcelTabBatchAsync(Batch batch, string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to Excel file that needs to be displayed as tab</param> 
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Excel channel tab</returns>
        public ITeamChannelTab AddExcelTabBatch(Batch batch, string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to Excel file that needs to be displayed as tab</param>        
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Excel channel tab</returns>
        public Task<ITeamChannelTab> AddExcelTabBatchAsync(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to the Excel file that needs to be added as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Excel channel tab</returns>
        public ITeamChannelTab AddExcelTabBatch(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to the Pdf file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Pdf channel tab</returns>
        public Task<ITeamChannelTab> AddPdfTabAsync(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to the Pdf that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Pdf channel tab</returns>
        public ITeamChannelTab AddPdfTab(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to Pdf file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Excel channel tab</returns>
        public Task<ITeamChannelTab> AddPdfTabBatchAsync(Batch batch, string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to Pdf file that needs to be displayed as tab</param> 
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Pdf channel tab</returns>
        public ITeamChannelTab AddPdfTabBatch(Batch batch, string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to Pdf file that needs to be displayed as tab</param>        
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Pdf channel tab</returns>
        public Task<ITeamChannelTab> AddPdfTabBatchAsync(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to the Pdf file that needs to be added as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Pdf channel tab</returns>
        public ITeamChannelTab AddPdfTabBatch(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to the file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Ppt channel tab</returns>
        public Task<ITeamChannelTab> AddPptTabAsync(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to the Ppt that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Ppt channel tab</returns>
        public ITeamChannelTab AddPptTab(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to Ppt file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Ppt channel tab</returns>
        public Task<ITeamChannelTab> AddPptTabBatchAsync(Batch batch, string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to Ppt file that needs to be displayed as tab</param> 
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Ppt channel tab</returns>
        public ITeamChannelTab AddPptTabBatch(Batch batch, string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to Ppt file that needs to be displayed as tab</param>        
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Ppt channel tab</returns>
        public Task<ITeamChannelTab> AddPptTabBatchAsync(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to the Ppt file that needs to be added as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Ppt channel tab</returns>
        public ITeamChannelTab AddPptTabBatch(string name, Uri fileUri, Guid fileId);

        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public Task<ITeamChannelTab> AddPlannerTabAsync(string name);

        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public ITeamChannelTab AddPlannerTab(string name);

        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public Task<ITeamChannelTab> AddPlannerTabBatchAsync(Batch batch, string name);

        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public ITeamChannelTab AddPlannerTabBatch(Batch batch, string name);

        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public Task<ITeamChannelTab> AddPlannerTabBatchAsync(string name);

        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public ITeamChannelTab AddPlannerTabBatch(string name);

        /// <summary>
        /// Adds a new Streams channel tab
        /// </summary>
        /// <param name="name">Display name of the Streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public Task<ITeamChannelTab> AddStreamsTabAsync(string name);

        /// <summary>
        /// Adds a new Streams channel tab
        /// </summary>
        /// <param name="name">Display name of the Streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public ITeamChannelTab AddStreamsTab(string name);

        /// <summary>
        /// Adds a new streams channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public Task<ITeamChannelTab> AddStreamsTabBatchAsync(Batch batch, string name);

        /// <summary>
        /// Adds a new streams channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public ITeamChannelTab AddStreamsTabBatch(Batch batch, string name);

        /// <summary>
        /// Adds a new streams channel tab
        /// </summary>
        /// <param name="name">Display name of the streams channel tab</param>
        /// <returns>Newly added streams channel tab</returns>
        public Task<ITeamChannelTab> AddStreamsTabBatchAsync(string name);

        /// <summary>
        /// Adds a new Streams channel tab
        /// </summary>
        /// <param name="name">Display name of the Streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public ITeamChannelTab AddStreamsTabBatch(string name);

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public Task<ITeamChannelTab> AddFormsTabAsync(string name);

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public ITeamChannelTab AddFormsTab(string name);

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public Task<ITeamChannelTab> AddFormsTabBatchAsync(Batch batch, string name);

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public ITeamChannelTab AddFormsTabBatch(Batch batch, string name);

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public Task<ITeamChannelTab> AddFormsTabBatchAsync(string name);

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public ITeamChannelTab AddFormsTabBatch(string name);

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public Task<ITeamChannelTab> AddOneNoteTabAsync(string name);

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public ITeamChannelTab AddOneNoteTab(string name);

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public Task<ITeamChannelTab> AddOneNoteTabBatchAsync(Batch batch, string name);

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public ITeamChannelTab AddOneNoteTabBatch(Batch batch, string name);

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added planner OneNote tab</returns>
        public Task<ITeamChannelTab> AddOneNoteTabBatchAsync(string name);

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public ITeamChannelTab AddOneNoteTabBatch(string name);

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public Task<ITeamChannelTab> AddPowerBiTabAsync(string name);

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public ITeamChannelTab AddPowerBiTab(string name);

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public Task<ITeamChannelTab> AddPowerBiTabBatchAsync(Batch batch, string name);

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public ITeamChannelTab AddPowerBiTabBatch(Batch batch, string name);

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public Task<ITeamChannelTab> AddPowerBiTabBatchAsync(string name);

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public ITeamChannelTab AddPowerBiTabBatch(string name);

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public Task<ITeamChannelTab> AddSharePointPageOrListTabAsync(string name);

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public ITeamChannelTab AddSharePointPageOrListTab(string name);

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public Task<ITeamChannelTab> AddSharePointPageOrListTabBatchAsync(Batch batch, string name);

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public ITeamChannelTab AddSharePointPageOrListTabBatch(Batch batch, string name);

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public Task<ITeamChannelTab> AddSharePointPageOrListTabBatchAsync(string name);

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public ITeamChannelTab AddSharePointPageOrListTabBatch(string name);
    }
}
