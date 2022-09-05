using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    internal sealed class TeamChannelTabCollection : QueryableDataModelCollection<ITeamChannelTab>, ITeamChannelTabCollection
    {
        public TeamChannelTabCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

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
        /// <param name="name">Display name of the channel tab</param>
        /// <returns>Newly added channel tab</returns>
        public ITeamChannelTab AddWikiTab(string name)
        {
            return AddWikiTabAsync(name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the channel tab</param>
        /// <returns>Newly added channel tab</returns>
        public async Task<ITeamChannelTab> AddWikiTabBatchAsync(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelWikiTab(name);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the channel tab</param>
        /// <returns>Newly added channel tab</returns>
        public ITeamChannelTab AddWikiTabBatch(Batch batch, string name)
        {
            return AddWikiTabBatchAsync(batch, name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel tab
        /// </summary>
        /// <param name="name">Display name of the channel tab</param>
        /// <returns>Newly added channel tab</returns>
        public async Task<ITeamChannelTab> AddWikiTabBatchAsync(string name)
        {
            return await AddWikiTabBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel tab
        /// </summary>
        /// <param name="name">Display name of the channel tab</param>
        /// <returns>Newly added channel tab</returns>
        public ITeamChannelTab AddWikiTabBatch(string name)
        {
            return AddWikiTabBatchAsync(name).GetAwaiter().GetResult();
        }
        #endregion

        #region Website tab

        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public async Task<ITeamChannelTab> AddWebsiteTabAsync(string name, Uri websiteUri)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (websiteUri == null)
            {
                throw new ArgumentNullException(nameof(websiteUri));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelWebsiteTab(name, websiteUri);
            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public ITeamChannelTab AddWebsiteTab(string name, Uri websiteUri)
        {
            return AddWebsiteTabAsync(name, websiteUri).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public async Task<ITeamChannelTab> AddWebsiteTabBatchAsync(Batch batch, string name, Uri websiteUri)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (websiteUri == null)
            {
                throw new ArgumentNullException(nameof(websiteUri));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelWebsiteTab(name, websiteUri);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new DocumentLibrary channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public ITeamChannelTab AddWebsiteTabBatch(Batch batch, string name, Uri websiteUri)
        {
            return AddWebsiteTabBatchAsync(batch, name, websiteUri).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the Website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public async Task<ITeamChannelTab> AddWebsiteTabBatchAsync(string name, Uri websiteUri)
        {
            return await AddWebsiteTabBatchAsync(PnPContext.CurrentBatch, name, websiteUri).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new Website channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="websiteUri">Uri to the Website that needs to be added as tab</param>
        /// <returns>Newly added Website channel tab</returns>
        public ITeamChannelTab AddWebsiteTabBatch(string name, Uri websiteUri)
        {
            return AddWebsiteTabBatchAsync(name, websiteUri).GetAwaiter().GetResult();
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

            if (documentLibraryUri == null)
            {
                throw new ArgumentNullException(nameof(documentLibraryUri));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelDocumentLibraryTab(name, documentLibraryUri);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new DocumentLibrary tab
        /// </summary>
        /// <param name="name">Display name of the  DocumentLibrary tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary tab</returns>
        public ITeamChannelTab AddDocumentLibraryTab(string name, Uri documentLibraryUri)
        {
            return AddDocumentLibraryTabAsync(name, documentLibraryUri).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new DocumentLibrary tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the DocumentLibrary tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary tab</returns>
        public async Task<ITeamChannelTab> AddDocumentLibraryTabBatchAsync(Batch batch, string name, Uri documentLibraryUri)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (documentLibraryUri == null)
            {
                throw new ArgumentNullException(nameof(documentLibraryUri));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelDocumentLibraryTab(name, documentLibraryUri);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new DocumentLibrary tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the DocumentLibrary tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary tab</returns>
        public ITeamChannelTab AddDocumentLibraryTabBatch(Batch batch, string name, Uri documentLibraryUri)
        {
            return AddDocumentLibraryTabBatchAsync(batch, name, documentLibraryUri).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new DocumentLibrary tab
        /// </summary>
        /// <param name="name">Display name of the DocumentLibrary tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary tab</returns>
        public async Task<ITeamChannelTab> AddDocumentLibraryTabBatchAsync(string name, Uri documentLibraryUri)
        {
            return await AddDocumentLibraryTabBatchAsync(PnPContext.CurrentBatch, name, documentLibraryUri).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new DocumentLibrary tab
        /// </summary>
        /// <param name="name">Display name of the DocumentLibrary tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Newly added DocumentLibrary tab</returns>
        public ITeamChannelTab AddDocumentLibraryTabBatch(string name, Uri documentLibraryUri)
        {
            return AddDocumentLibraryTabBatchAsync(name, documentLibraryUri).GetAwaiter().GetResult();
        }
        #endregion

        #region Word tab
        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="fileUri">Uri to the file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Word channel tab</returns>
        public async Task<ITeamChannelTab> AddWordTabAsync(string name, Uri fileUri, Guid fileId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (fileUri == null)
            {
                throw new ArgumentNullException(nameof(fileUri));
            }

            if ( fileId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(fileId));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelWordTab(name, fileUri, fileId);
            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="fileUri">Uri to the website that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Word channel tab</returns>
        public ITeamChannelTab AddWordTab(string name, Uri fileUri, Guid fileId)
        {
            return AddWordTabAsync(name, fileUri, fileId).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Word channel tab</returns>
        public async Task<ITeamChannelTab> AddWordTabBatchAsync(Batch batch, string name, Uri fileUri, Guid fileId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (fileUri== null)
            {
                throw new ArgumentNullException(nameof(fileUri));
            }

            if (fileId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(fileId));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelWordTab(name, fileUri, fileId);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param> 
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Word channel tab</returns>
        public ITeamChannelTab AddWordTabBatch(Batch batch, string name, Uri fileUri, Guid fileId)
        {
            return AddWordTabBatchAsync(batch, name, fileUri, fileId).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param>        
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Word channel tab</returns>
        public async Task<ITeamChannelTab> AddWordTabBatchAsync(string name, Uri fileUri, Guid fileId)
        {
            return await AddWordTabBatchAsync(PnPContext.CurrentBatch, name, fileUri, fileId).ConfigureAwait(false);
        }
        /// <summary>
        /// Adds a new Word channel tab
        /// </summary>
        /// <param name="name">Display name of the Website channel tab</param>
        /// <param name="fileUri">Uri to the Word file that needs to be added as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Word channel tab</returns>
        public ITeamChannelTab AddWordTabBatch(string name, Uri fileUri, Guid fileId)
        {
            return AddWordTabBatchAsync(PnPContext.CurrentBatch, name, fileUri, fileId).GetAwaiter().GetResult();
        }

        #endregion

        #region Excel tab

        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to the file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Excel channel tab</returns>
        public async Task<ITeamChannelTab> AddExcelTabAsync(string name, Uri fileUri, Guid fileId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (fileUri == null)
            {
                throw new ArgumentNullException(nameof(fileUri));
            }

            if (fileId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(fileId));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelExcelTab(name, fileUri, fileId);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to the website that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Excel channel tab</returns>
        public ITeamChannelTab AddExcelTab(string name, Uri fileUri, Guid fileId)
        {
            return AddExcelTabAsync(name, fileUri, fileId).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Excel channel tab</returns>
        public async Task<ITeamChannelTab> AddExcelTabBatchAsync(Batch batch, string name, Uri fileUri, Guid fileId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (fileUri == null)
            {
                throw new ArgumentNullException(nameof(fileUri));
            }

            if (fileId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(fileId));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelExcelTab(name, fileUri, fileId);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param> 
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Excel channel tab</returns>
        public ITeamChannelTab AddExcelTabBatch(Batch batch, string name, Uri fileUri, Guid fileId)
        {
            return AddExcelTabBatchAsync(batch, name, fileUri, fileId).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param>        
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Excel channel tab</returns>
        public async Task<ITeamChannelTab> AddExcelTabBatchAsync(string name, Uri fileUri, Guid fileId)
        {
            return await AddExcelTabBatchAsync(PnPContext.CurrentBatch, name, fileUri, fileId).ConfigureAwait(false);
        }
        /// <summary>
        /// Adds a new Excel channel tab
        /// </summary>
        /// <param name="name">Display name of the Excel channel tab</param>
        /// <param name="fileUri">Uri to the Word file that needs to be added as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Excel channel tab</returns>
        public ITeamChannelTab AddExcelTabBatch(string name, Uri fileUri, Guid fileId)
        {
            return AddExcelTabBatchAsync(PnPContext.CurrentBatch, name, fileUri, fileId).GetAwaiter().GetResult();
        }

        #endregion

        #region Pdf tab

        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to the file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Pdf channel tab</returns>
        public async Task<ITeamChannelTab> AddPdfTabAsync(string name, Uri fileUri, Guid fileId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (fileUri == null)
            {
                throw new ArgumentNullException(nameof(fileUri));
            }

            if (fileId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(fileId));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelPdfTab(name, fileUri, fileId);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to the website that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Pdf channel tab</returns>
        public ITeamChannelTab AddPdfTab(string name, Uri fileUri, Guid fileId)
        {
            return AddPdfTabAsync(name, fileUri, fileId).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Pdf channel tab</returns>
        public async Task<ITeamChannelTab> AddPdfTabBatchAsync(Batch batch, string name, Uri fileUri, Guid fileId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (fileUri == null)
            {
                throw new ArgumentNullException(nameof(fileUri));
            }

            if (fileId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(fileId));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelPdfTab(name, fileUri, fileId);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param> 
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Pdf channel tab</returns>
        public ITeamChannelTab AddPdfTabBatch(Batch batch, string name, Uri fileUri, Guid fileId)
        {
            return AddPdfTabBatchAsync(batch, name, fileUri, fileId).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to word file that needs to be displayed as tab</param>        
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Pdf channel tab</returns>
        public async Task<ITeamChannelTab> AddPdfTabBatchAsync(string name, Uri fileUri, Guid fileId)
        {
            return await AddPdfTabBatchAsync(PnPContext.CurrentBatch, name, fileUri, fileId).ConfigureAwait(false);
        }
        /// <summary>
        /// Adds a new Pdf channel tab
        /// </summary>
        /// <param name="name">Display name of the Pdf channel tab</param>
        /// <param name="fileUri">Uri to the Word file that needs to be added as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Pdf channel tab</returns>
        public ITeamChannelTab AddPdfTabBatch(string name, Uri fileUri, Guid fileId)
        {
            return AddPdfTabBatchAsync(PnPContext.CurrentBatch, name, fileUri, fileId).GetAwaiter().GetResult();
        }

        #endregion

        #region Ppt tab
        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to the file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Ppt channel tab</returns>
        public async Task<ITeamChannelTab> AddPptTabAsync(string name, Uri fileUri, Guid fileId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (fileUri == null)
            {
                throw new ArgumentNullException(nameof(fileUri));
            }

            if (fileId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(fileId));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelPptTab(name, fileUri, fileId);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to the Ppt that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Ppt channel tab</returns>
        public ITeamChannelTab AddPptTab(string name, Uri fileUri, Guid fileId)
        {
            return AddPptTabAsync(name, fileUri, fileId).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to Ppt file that needs to be displayed as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Ppt channel tab</returns>
        public async Task<ITeamChannelTab> AddPptTabBatchAsync(Batch batch, string name, Uri fileUri, Guid fileId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (fileUri == null)
            {
                throw new ArgumentNullException(nameof(fileUri));
            }

            if (fileId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(fileId));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelPptTab(name, fileUri, fileId);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to Ppt file that needs to be displayed as tab</param> 
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Ppt channel tab</returns>
        public ITeamChannelTab AddPptTabBatch(Batch batch, string name, Uri fileUri, Guid fileId)
        {
            return AddPptTabBatchAsync(batch, name, fileUri, fileId).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to Ppt file that needs to be displayed as tab</param>        
        /// <param name="fileId">The sourceDoc ID of the file</param>      
        /// <returns>Newly added Ppt channel tab</returns>
        public async Task<ITeamChannelTab> AddPptTabBatchAsync(string name, Uri fileUri, Guid fileId)
        {
            return await AddPptTabBatchAsync(PnPContext.CurrentBatch, name, fileUri, fileId).ConfigureAwait(false);
        }
        /// <summary>
        /// Adds a new Ppt channel tab
        /// </summary>
        /// <param name="name">Display name of the Ppt channel tab</param>
        /// <param name="fileUri">Uri to the Ppt file that needs to be added as tab</param>
        /// <param name="fileId">The sourceDoc ID of the file</param>
        /// <returns>Newly added Ppt channel tab</returns>
        public ITeamChannelTab AddPptTabBatch(string name, Uri fileUri, Guid fileId)
        {
            return AddPptTabBatchAsync(PnPContext.CurrentBatch, name, fileUri, fileId).GetAwaiter().GetResult();
        }

        #endregion

        #region Planner tab
        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public async Task<ITeamChannelTab> AddPlannerTabAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelPlannerTab(name);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public ITeamChannelTab AddPlannerTab(string name)
        {
            return AddPlannerTabAsync(name).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public async Task<ITeamChannelTab> AddPlannerTabBatchAsync(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelPlannerTab(name);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }
        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public ITeamChannelTab AddPlannerTabBatch(Batch batch, string name)
        {
            return AddPlannerTabBatchAsync(batch, name).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public async Task<ITeamChannelTab> AddPlannerTabBatchAsync(string name)
        {
            return await AddPlannerTabBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }
        /// <summary>
        /// Adds a new planner channel tab
        /// </summary>
        /// <param name="name">Display name of the planner channel tab</param>
        /// <returns>Newly added planner channel tab</returns>
        public ITeamChannelTab AddPlannerTabBatch(string name)
        {
            return AddPlannerTabBatchAsync(PnPContext.CurrentBatch, name).GetAwaiter().GetResult();
        }
        #endregion

        #region Stream tab

        /// <summary>
        /// Adds a new Streams channel tab
        /// </summary>
        /// <param name="name">Display name of the Streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public async Task<ITeamChannelTab> AddStreamsTabAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelStreamsTab(name);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new Streams channel tab
        /// </summary>
        /// <param name="name">Display name of the Streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public ITeamChannelTab AddStreamsTab(string name)
        {
            return AddStreamsTabAsync(name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new streams channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public async Task<ITeamChannelTab> AddStreamsTabBatchAsync(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelStreamsTab(name);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new streams channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public ITeamChannelTab AddStreamsTabBatch(Batch batch, string name)
        {
            return AddStreamsTabBatchAsync(batch, name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new streams channel tab
        /// </summary>
        /// <param name="name">Display name of the streams channel tab</param>
        /// <returns>Newly added streams channel tab</returns>
        public async Task<ITeamChannelTab> AddStreamsTabBatchAsync(string name)
        {
            return await AddStreamsTabBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new Streams channel tab
        /// </summary>
        /// <param name="name">Display name of the Streams channel tab</param>
        /// <returns>Newly added Streams channel tab</returns>
        public ITeamChannelTab AddStreamsTabBatch(string name)
        {
            return AddStreamsTabBatchAsync(PnPContext.CurrentBatch, name).GetAwaiter().GetResult();
        }

        #endregion

        #region Forms tab

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public async Task<ITeamChannelTab> AddFormsTabAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelFormsTab(name);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public ITeamChannelTab AddFormsTab(string name)
        {
            return AddFormsTabAsync(name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public async Task<ITeamChannelTab> AddFormsTabBatchAsync(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelFormsTab(name);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public ITeamChannelTab AddFormsTabBatch(Batch batch, string name)
        {
            return AddFormsTabBatchAsync(batch, name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public async Task<ITeamChannelTab> AddFormsTabBatchAsync(string name)
        {
            return await AddFormsTabBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new Forms channel tab
        /// </summary>
        /// <param name="name">Display name of the Forms channel tab</param>
        /// <returns>Newly added Forms channel tab</returns>
        public ITeamChannelTab AddFormsTabBatch(string name)
        {
            return AddFormsTabBatchAsync(PnPContext.CurrentBatch, name).GetAwaiter().GetResult();
        }

        #endregion

        #region OneNote tab

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public async Task<ITeamChannelTab> AddOneNoteTabAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelOneNoteTab(name);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public ITeamChannelTab AddOneNoteTab(string name)
        {
            return AddOneNoteTabAsync(name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public async Task<ITeamChannelTab> AddOneNoteTabBatchAsync(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelOneNoteTab(name);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public ITeamChannelTab AddOneNoteTabBatch(Batch batch, string name)
        {
            return AddOneNoteTabBatchAsync(batch, name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added planner OneNote tab</returns>
        public async Task<ITeamChannelTab> AddOneNoteTabBatchAsync(string name)
        {
            return await AddOneNoteTabBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new OneNote channel tab
        /// </summary>
        /// <param name="name">Display name of the OneNote channel tab</param>
        /// <returns>Newly added OneNote channel tab</returns>
        public ITeamChannelTab AddOneNoteTabBatch(string name)
        {
            return AddOneNoteTabBatchAsync(PnPContext.CurrentBatch, name).GetAwaiter().GetResult();
        }

        #endregion

        #region PowerBi tab

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public async Task<ITeamChannelTab> AddPowerBiTabAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelPowerBiTab(name);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public ITeamChannelTab AddPowerBiTab(string name)
        {
            return AddPowerBiTabAsync(name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public async Task<ITeamChannelTab> AddPowerBiTabBatchAsync(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelPowerBiTab(name);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public ITeamChannelTab AddPowerBiTabBatch(Batch batch, string name)
        {
            return AddPowerBiTabBatchAsync(batch, name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public async Task<ITeamChannelTab> AddPowerBiTabBatchAsync(string name)
        {
            return await AddPowerBiTabBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new PowerBi channel tab
        /// </summary>
        /// <param name="name">Display name of the PowerBi channel tab</param>
        /// <returns>Newly added PowerBi channel tab</returns>
        public ITeamChannelTab AddPowerBiTabBatch(string name)
        {
            return AddPowerBiTabBatchAsync(PnPContext.CurrentBatch, name).GetAwaiter().GetResult();
        }

        #endregion

        #region SharePoint Page Or List tab

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public async Task<ITeamChannelTab> AddSharePointPageOrListTabAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelSharePointPageOrListTab(name);

            return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public ITeamChannelTab AddSharePointPageOrListTab(string name)
        {
            return AddSharePointPageOrListTabAsync(name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public async Task<ITeamChannelTab> AddSharePointPageOrListTabBatchAsync(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelSharePointPageOrListTab(name);

            return await newTab.AddBatchAsync(batch, keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
        }

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public ITeamChannelTab AddSharePointPageOrListTabBatch(Batch batch, string name)
        {
            return AddSharePointPageOrListTabBatchAsync(batch, name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public async Task<ITeamChannelTab> AddSharePointPageOrListTabBatchAsync(string name)
        {
            return await AddSharePointPageOrListTabBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new sharepoint or page channel tab
        /// </summary>
        /// <param name="name">Display name of the sharepoint or page channel tab</param>
        /// <returns>Newly added sharepoint or page channel tab</returns>
        public ITeamChannelTab AddSharePointPageOrListTabBatch(string name)
        {
            return AddSharePointPageOrListTabBatchAsync(PnPContext.CurrentBatch, name).GetAwaiter().GetResult();
        }

        #endregion

        /// <summary>
        /// Creates a wiki <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <param name="documentLibraryUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelDocumentLibraryTab(string displayName, Uri documentLibraryUri)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.DocumentLibraryAppId },
            };

            newTab.Configuration = new TeamChannelTabConfiguration
            {
                PnPContext = PnPContext,
                Parent = this,
                EntityId = "",
                ContentUrl = documentLibraryUri.ToString()
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a website <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <param name="websiteUri">Uri to the document library that needs to be added as tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelWebsiteTab(string displayName, Uri websiteUri)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.WebAppId },
            };

            newTab.Configuration = new TeamChannelTabConfiguration
            {
                PnPContext = PnPContext,
                Parent = this,
                EntityId = "",
                ContentUrl = websiteUri.ToString(),
                WebsiteUrl = websiteUri.ToString(),
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a word <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="name">Name of the tab</param>
        /// <param name="fileUri">Uri to the Word document that needs to be added as tab</param>
        /// <param name="fileId">Unique ID of the Word document</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelWordTab(string name, Uri fileUri, Guid fileId)
        {
            var newTab = CreateTeamChannelTab(name);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.WordAppId },
            };

            newTab.Configuration = new TeamChannelTabConfiguration
            {
                PnPContext = PnPContext,
                Parent = this,
                EntityId = fileId.ToString(),
                ContentUrl = fileUri.ToString(),
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates an Excel <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="name">Name of the tab</param>
        /// <param name="fileUri">Uri to the Excel document that needs to be added as tab</param>
        /// <param name="fileId">Unique ID of the Excel document</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelExcelTab(string name, Uri fileUri, Guid fileId)
        {
            var newTab = CreateTeamChannelTab(name);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.ExcelAppId },
            };

            newTab.Configuration = new TeamChannelTabConfiguration
            {
                PnPContext = PnPContext,
                Parent = this,
                EntityId = fileId.ToString(),
                ContentUrl = fileUri.ToString(),
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a ppt <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="name">Name of the tab</param>
        /// <param name="fileUri">Uri to the Ppt document that needs to be added as tab</param>
        /// <param name="fileId">Unique ID of the Ppt document</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelPptTab(string name, Uri fileUri, Guid fileId)
        {
            var newTab = CreateTeamChannelTab(name);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.PptAppId },
            };

            newTab.Configuration = new TeamChannelTabConfiguration
            {
                PnPContext = PnPContext,
                Parent = this,
                EntityId = fileId.ToString(),
                ContentUrl = fileUri.ToString(),
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a pdf <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="name">Name of the tab</param>
        /// <param name="fileUri">Uri to the Pdf document that needs to be added as tab</param>
        /// <param name="fileId">Unique ID of the Pdfdocument</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelPdfTab(string name, Uri fileUri, Guid fileId)
        {
            var newTab = CreateTeamChannelTab(name);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.PdfAppId },
            };

            newTab.Configuration = new TeamChannelTabConfiguration
            {
                PnPContext = PnPContext,
                Parent = this,
                EntityId = fileId.ToString(),
                ContentUrl = fileUri.ToString(),
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
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.WikiAppId }
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a planner <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelPlannerTab(string displayName)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.PlannerAppId }
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a streams <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelStreamsTab(string displayName)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.StreamsAppId }
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a OneNote <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelOneNoteTab(string displayName)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.OneNoteAppId }
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a Forms <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelFormsTab(string displayName)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.FormsAppId }
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a PowerBi <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelPowerBiTab(string displayName)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.PowerBiAppId }
            };

            return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
        }

        /// <summary>
        /// Creates a SharePoint Page or List <see cref="TeamChannelTab"/>
        /// </summary>
        /// <param name="displayName">Name of the tab</param>
        /// <returns>Wiki <see cref="TeamChannelTab"/></returns>
        private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelSharePointPageOrListTab(string displayName)
        {
            var newTab = CreateTeamChannelTab(displayName);

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { TeamChannelTabConstants.TeamsAppId, TeamChannelTabConstants.SharePointPageOrListAppId }
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
