using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnP Context class...the glue between the model and the data stores
    /// </summary>
    public class PnPContext : IDisposable
    {
        private bool disposed = false;

        private readonly Lazy<IWeb> web = new Lazy<IWeb>(() =>
        {
            return new Web();
        }, true);

        private readonly Lazy<ISite> site = new Lazy<ISite>(() =>
        {
            return new Site();
        }, true);

        private readonly Lazy<ITeam> team = new Lazy<ITeam>(() =>
        {
            return new Team();
        }, true);

        private readonly ILogger log;
        private readonly IAuthenticationProvider authProvider;
        private readonly SharePointRestClient restClient;
        private readonly MicrosoftGraphClient graphClient;
        private readonly BatchClient batchClient;
        private Batch currentBatch;

        #region Constructors

        /// <summary>
        /// Public constructor for an SPO context based on target site URL
        /// </summary>
        /// <param name="url">The URL of the site as a string</param>
        /// <param name="authenticationProvider">The authentication provider to authenticate against the target site url</param>
        public PnPContext(string url, ILogger logger,
                                      IAuthenticationProvider authenticationProvider,
                                      SharePointRestClient sharePointRestClient,
                                      MicrosoftGraphClient microsoftGraphClient) : this(new Uri(url), logger, authenticationProvider, sharePointRestClient, microsoftGraphClient)
        {
        }

        /// <summary>
        /// Public constructor for an SPO context based on target site URL
        /// </summary>
        /// <param name="uri">The URI of the site as a URI</param>
        /// <param name="authenticationProvider">The authentication provider to authenticate against the target site url</param>
        public PnPContext(Uri uri, ILogger logger,
                                   IAuthenticationProvider authenticationProvider,
                                   SharePointRestClient sharePointRestClient,
                                   MicrosoftGraphClient microsoftGraphClient)
        {
            log = logger;
            Uri = uri;
#if DEBUG
            Mode = PnPContextMode.Default;
#endif
            authProvider = authenticationProvider;
            restClient = sharePointRestClient;
            graphClient = microsoftGraphClient;

            batchClient = new BatchClient(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Uri of the SharePoint site we're working against
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Connected logger
        /// </summary>
        public ILogger Logger
        {
            get
            {
                return log;
            }
        }

        /// <summary>
        /// Connected authentication provider
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider
        {
            get
            {
                return authProvider;
            }
        }

        /// <summary>
        /// Connected SharePoint REST client
        /// </summary>
        public SharePointRestClient RestClient
        {
            get
            {
                return restClient;
            }
        }

        /// <summary>
        /// Connected Microsoft Graph client
        /// </summary>
        public MicrosoftGraphClient GraphClient
        {
            get
            {
                return graphClient;
            }
        }

        /// <summary>
        /// Connected batch client
        /// </summary>
        internal BatchClient BatchClient
        {
            get
            {
                return batchClient;
            }
        }

#if DEBUG

#region Test related properties
        /// <summary>
        /// Mode this context operates in
        /// </summary>
        internal PnPContextMode Mode { get; private set; }

        /// <summary>
        /// Id associated to this context in a test case
        /// </summary>
        internal int TestId { get; private set; }

        /// <summary>
        /// Name of the test case using this context
        /// </summary>
        internal string TestName { get; private set; }

        /// <summary>
        /// Path of the test case
        /// </summary>
        internal string TestFilePath { get; private set; }

        /// <summary>
        /// Generate the .request and .debug files, can be handy for debugging
        /// </summary>
        internal bool GenerateTestMockingDebugFiles { get; private set; }

        /// <summary>
        /// Urls's used by the test cases
        /// </summary>
        internal Dictionary<string, Uri> TestUris { get; set; }

        #endregion

#endif

        #region Graph related properties
        
        /// <summary>
        /// Controls whether the library will try to use Microsoft Graph over REST whenever that's defined in the model
        /// </summary>
        public bool GraphFirst { get; set;  } = true;

        /// <summary>
        /// If true than all requests to Microsoft Graph use the beta endpoint
        /// </summary>
        public bool GraphAlwaysUseBeta { get; set; } = false;

        /// <summary>
        /// If true than the Graph beta endpoint is used when there's no other option, default approach stays using the v1 endpoint
        /// </summary>
        public bool GraphCanUseBeta { get; set; } = true;
        
        #endregion

        /// <summary>
        /// Current batch, used for implicit batching
        /// </summary>
        public Batch CurrentBatch
        {
            get
            {
                if (currentBatch == null || currentBatch.Executed)
                {
                    currentBatch = batchClient.EnsureBatch();
                }

                return currentBatch;
            }
        }

        /// <summary>
        /// Are there pending requests to execute (in the case of batching)
        /// </summary>
        public bool HasPendingRequests
        {
            get
            {
                return CurrentBatch.Requests.Count > 0;
            }
        }

        /// <summary>
        /// Entry point for the Web Object
        /// </summary>
        public IWeb Web
        {
            get
            {
                (web.Value as Web).PnPContext = this;
                return web.Value;
            }
        }

        /// <summary>
        /// Entry point for the Site Object
        /// </summary>
        public ISite Site
        {
            get
            {
                // Special case: PnPContext.Site.RootWeb = PnPContext.Web, 
                // only applies when the loaded Url is from a root web of the site
                if (!IsSubSite(Uri))
                {
                    (site.Value as Site).RootWeb = web.Value;
                }

                (site.Value as Site).PnPContext = this;
                return site.Value;
            }
        }

        /// <summary>
        /// Entry point for the Team Object
        /// </summary>
        public ITeam Team
        {
            get
            {
                (team.Value as Team).PnPContext = this;
                return team.Value;
            }
        }

        #endregion

        #region Public Methods   

        /// <summary>
        /// Creates a new batch
        /// </summary>
        /// <returns></returns>
        public Batch NewBatch()
        {
            return BatchClient.EnsureBatch();
        }

        /// <summary>
        /// Method to execute the current batch
        /// </summary>
        /// <returns>The asynchronous task that will be executed</returns>
        public async Task ExecuteAsync()
        {
            await batchClient.ExecuteBatch(CurrentBatch).ConfigureAwait(false);
        }

        /// <summary>
        /// Method to execute a given batch
        /// </summary>
        /// <param name="batch">Batch to execute</param>
        /// <returns>The asynchronous task that will be executed</returns>
        public async Task ExecuteAsync(Batch batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            await batchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }

        #endregion

#if DEBUG

        #region Internal methods to support unit testing

        internal void SetRecordingMode(int id, string testName, string sourceFilePath, bool generateTestMockingDebugFiles, Dictionary<string, Uri> testUris)
        {
            SetTestConfiguration(id, testName, sourceFilePath, generateTestMockingDebugFiles, testUris);
            Mode = PnPContextMode.Record;
        }

        internal void SetMockMode(int id, string testName, string sourceFilePath, bool generateTestMockingDebugFiles, Dictionary<string, Uri> testUris)
        {
            SetTestConfiguration(id, testName, sourceFilePath, generateTestMockingDebugFiles, testUris);
            Mode = PnPContextMode.Mock;
        }

        private void SetTestConfiguration(int id, string testName, string sourceFilePath, bool generateTestMockingDebugFiles, Dictionary<string, Uri> testUris)
        {
            TestId = id;
            TestName = testName;
            TestFilePath = sourceFilePath.Replace(".cs", "");
            GenerateTestMockingDebugFiles = generateTestMockingDebugFiles;
            TestUris = testUris;
        }

        #endregion

#endif

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
            }

            disposed = true;
        }

        #endregion

        #region Helper methods

        private static bool IsSubSite(Uri site)
        {
            string cleanedPath = site.AbsolutePath.ToLower().Replace("/teams/", "").Replace("/sites/", "").TrimEnd(new char[] { '/' });

            if (cleanedPath.Contains("/"))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
