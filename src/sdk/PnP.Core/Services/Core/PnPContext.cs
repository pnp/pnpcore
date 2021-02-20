using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnP Context class...the glue between the model and the data stores
    /// </summary>
    public class PnPContext : IDisposable
    {
        #region Private fields

        private bool graphCanUseBeta = true;
        private static readonly HttpClient httpClient = new HttpClient();

        #endregion

        #region Lazy properties for fluent API

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

        private readonly Lazy<IGraphGroup> group = new Lazy<IGraphGroup>(() =>
        {
            return new GraphGroup();
        }, true);

        private readonly Lazy<ITermStore> termStore = new Lazy<ITermStore>(() =>
        {
            return new TermStore();
        }, true);
        #endregion

        #region Private properties

        private readonly PnPGlobalSettingsOptions globalOptions;
        private readonly PnPContextFactoryOptions contextOptions;
        private readonly TelemetryManager telemetry;
        private Batch currentBatch;

        #endregion

        #region Constructor
        internal PnPContext(ILogger logger,
                           IAuthenticationProvider authenticationProvider,
                           SharePointRestClient sharePointRestClient,
                           MicrosoftGraphClient microsoftGraphClient,
                           IOptions<PnPContextFactoryOptions> contextOptions,
                           IOptions<PnPGlobalSettingsOptions> globalOptions,
                           TelemetryManager telemetryManager) :
            this(logger,
                authenticationProvider,
                sharePointRestClient,
                microsoftGraphClient,
                contextOptions?.Value,
                globalOptions?.Value,
                telemetryManager)
        {
        }

        internal PnPContext(ILogger logger,
                           IAuthenticationProvider authenticationProvider,
                           SharePointRestClient sharePointRestClient,
                           MicrosoftGraphClient microsoftGraphClient,
                           PnPContextFactoryOptions contextOptions,
                           PnPGlobalSettingsOptions globalOptions,
                           TelemetryManager telemetryManager)
        {
            Id = Guid.NewGuid();
            Logger = logger;

#if DEBUG
            Mode = TestMode.Default;
#endif
            AuthenticationProvider = authenticationProvider;
            RestClient = sharePointRestClient;
            GraphClient = microsoftGraphClient;
            this.globalOptions = globalOptions;
            this.contextOptions = contextOptions;
            telemetry = telemetryManager;

            if (this.contextOptions != null)
            {
                GraphFirst = this.contextOptions.GraphFirst;
                GraphAlwaysUseBeta = this.contextOptions.GraphAlwaysUseBeta;
                GraphCanUseBeta = this.contextOptions.GraphCanUseBeta;
            }

            BatchClient = new BatchClient(this, this.globalOptions, telemetryManager);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Uri of the SharePoint site we're working against
        /// </summary>
        public Uri Uri { get; internal set; }

        /// <summary>
        /// Connected logger
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Connected authentication provider
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; }

        /// <summary>
        /// Connected SharePoint REST client
        /// </summary>
        public SharePointRestClient RestClient { get; }

        /// <summary>
        /// Connected Microsoft Graph client
        /// </summary>
        public MicrosoftGraphClient GraphClient { get; }

        /// <summary>
        /// Connected batch client
        /// </summary>
        internal BatchClient BatchClient { get; }

        /// <summary>
        /// Unique id for this <see cref="PnPContext"/>
        /// </summary>
        internal Guid Id { get; private set; }

#if DEBUG

        #region Test related properties
        /// <summary>
        /// Mode this context operates in
        /// </summary>
        internal TestMode Mode { get; private set; }

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
        public bool GraphFirst { get; set; } = true;

        /// <summary>
        /// If true than all requests to Microsoft Graph use the beta endpoint
        /// </summary>
        public bool GraphAlwaysUseBeta { get; set; }

        /// <summary>
        /// If true than the Graph beta endpoint is used when there's no other option, default approach stays using the v1 endpoint
        /// </summary>
        public bool GraphCanUseBeta
        {
            get
            {
                if (GraphAlwaysUseBeta)
                {
                    return true;
                }
                else
                {
                    return graphCanUseBeta;
                }
            }

            set
            {
                if (GraphAlwaysUseBeta && value == false)
                {
                    throw new Exception(PnPCoreResources.Exception_ConfigurationError_CannotSetCanUseBeta);
                }

                graphCanUseBeta = value;
            }
        }
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
                    currentBatch = BatchClient.EnsureBatch();
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

        /// <summary>
        /// Entry point for the Microsoft 365 Group Object
        /// </summary>
        public IGraphGroup Group
        {
            get
            {
                (group.Value as GraphGroup).PnPContext = this;
                return group.Value;
            }
        }

        /// <summary>
        /// Entry point for the Microsoft 365 TermStore
        /// </summary>
        public ITermStore TermStore
        {
            get
            {
                (termStore.Value as TermStore).PnPContext = this;
                return termStore.Value;
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
        /// <param name="throwOnError">Throw an exception on the first encountered error in the batch</param>
        /// <returns>The asynchronous task that will be executed</returns>
        public async Task<List<BatchResult>> ExecuteAsync(bool throwOnError = true)
        {
            CurrentBatch.ThrowOnError = throwOnError;
            return await BatchClient.ExecuteBatch(CurrentBatch).ConfigureAwait(false);
        }

        /// <summary>
        /// Method to execute a given batch
        /// </summary>
        /// <param name="batch">Batch to execute</param>
        /// <param name="throwOnError">Throw an exception on the first encountered error in the batch</param>
        /// <returns>The asynchronous task that will be executed</returns>
        public async Task<List<BatchResult>> ExecuteAsync(Batch batch, bool throwOnError = true)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            batch.ThrowOnError = throwOnError;
            return await BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }

        /// <summary>
        /// Method to execute the current batch
        /// </summary>
        /// <param name="throwOnError">Throw an exception on the first encountered error in the batch</param>
        /// <returns>The asynchronous task that will be executed</returns>
        public List<BatchResult> Execute(bool throwOnError = true)
        {
            return ExecuteAsync(throwOnError).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Method to execute a given batch
        /// </summary>
        /// <param name="batch">Batch to execute</param>
        /// <param name="throwOnError">Throw an exception on the first encountered error in the batch</param>
        /// <returns>The asynchronous task that will be executed</returns>
        public List<BatchResult> Execute(Batch batch, bool throwOnError = true)
        {
            return ExecuteAsync(batch, throwOnError).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Clones this context into a new context for the same SharePoint site
        /// </summary>
        /// <returns>New <see cref="PnPContext"/></returns>
        public PnPContext Clone()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            return CloneAsync().GetAwaiter().GetResult();
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        /// <summary>
        /// Clones this context into a new context for the same SharePoint site
        /// </summary>
        /// <returns>New <see cref="PnPContext"/></returns>
        public async Task<PnPContext> CloneAsync()
        {
            return await CloneAsync(Uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Clones this context for another SharePoint site provided as configuration
        /// </summary>
        /// <param name="name">The name of the SPOContext configuration to use</param>
        /// <returns>New <see cref="PnPContext"/> for the request config</returns>
        public PnPContext Clone(string name)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            return CloneAsync(name).GetAwaiter().GetResult();
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        /// <summary>
        /// Clones this context for another SharePoint site provided as configuration
        /// </summary>
        /// <param name="name">The name of the SPOContext configuration to use</param>
        /// <returns>New <see cref="PnPContext"/> for the request config</returns>
        public async Task<PnPContext> CloneAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(string.Format(PnPCoreResources.Exception_PnPContext_EmptyConfiguration, nameof(name)));
            }

            var configuration = contextOptions.Configurations.FirstOrDefault(c => c.Name == name);
            if (configuration == null)
            {
                throw new ArgumentException(string.Format(PnPCoreResources.Exception_PnPContext_InvalidConfiguration, name));
            }

            return await CloneAsync(configuration.SiteUrl).ConfigureAwait(false);
        }

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="uri">Uri of the other SharePoint site</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        public PnPContext Clone(Uri uri)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            return CloneAsync(uri).GetAwaiter().GetResult();
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="uri">Uri of the other SharePoint site</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        public async Task<PnPContext> CloneAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            PnPContext clonedContext = new PnPContext(Logger, AuthenticationProvider, RestClient, GraphClient, contextOptions, globalOptions, telemetry)
            {                
                // Take over graph settings
                GraphCanUseBeta = graphCanUseBeta,
                GraphAlwaysUseBeta = GraphAlwaysUseBeta,
                GraphFirst = GraphFirst,
                // Set the Uri for which this context was cloned
                Uri = uri
            };

            if (!uri.Equals(Uri))
            {
                await PnPContextFactory.InitializeContextAsync(clonedContext).ConfigureAwait(false);
            }
            else
            {
                await PnPContextFactory.CopyContextInitializationAsync(this, clonedContext).ConfigureAwait(false);
            }

            return clonedContext;
        }

        #endregion

#if DEBUG

        #region Internal methods to support unit testing

        internal async Task<PnPContext> CloneForTestingAsync(PnPContext source, Uri uri, string name, int id)
        {
            if (uri == null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new ArgumentException(string.Format(PnPCoreResources.Exception_PnPContext_EmptyConfiguration, nameof(name)));
                    }

                    var configuration = contextOptions.Configurations.FirstOrDefault(c => c.Name == name);
                    if (configuration == null)
                    {
                        throw new ArgumentException(string.Format(PnPCoreResources.Exception_PnPContext_InvalidConfiguration, name));
                    }
                    uri = configuration.SiteUrl;
                }
                else if (uri == null)
                {
                    uri = Uri;
                }
            }

            PnPContext clonedContext = new PnPContext(Logger, AuthenticationProvider, RestClient, GraphClient, contextOptions, globalOptions, telemetry)
            {
                // Take over graph settings
                GraphCanUseBeta = graphCanUseBeta,
                GraphAlwaysUseBeta = GraphAlwaysUseBeta,
                GraphFirst = GraphFirst,
                // Set the Uri for which this context was cloned
                Uri = uri,               
            };

            if (source.Mode == TestMode.Mock)
            {
                clonedContext.SetMockMode(id, source.TestName, source.TestFilePath, source.GenerateTestMockingDebugFiles, source.TestUris);
            }
            else
            {
                clonedContext.SetRecordingMode(id, source.TestName, source.TestFilePath, source.GenerateTestMockingDebugFiles, source.TestUris);
            }

            if (!uri.Equals(Uri))
            {
                await PnPContextFactory.InitializeContextAsync(clonedContext).ConfigureAwait(false);
            }
            else
            {
                await PnPContextFactory.CopyContextInitializationAsync(this, clonedContext).ConfigureAwait(false);
            }

            return clonedContext;
        }

        internal void SetRecordingMode(int id, string testName, string sourceFilePath, bool generateTestMockingDebugFiles, Dictionary<string, Uri> testUris)
        {
            SetTestConfiguration(id, testName, sourceFilePath, generateTestMockingDebugFiles, testUris);
            Mode = TestMode.Record;
        }

        internal void SetMockMode(int id, string testName, string sourceFilePath, bool generateTestMockingDebugFiles, Dictionary<string, Uri> testUris)
        {
            SetTestConfiguration(id, testName, sourceFilePath, generateTestMockingDebugFiles, testUris);
            Mode = TestMode.Mock;
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

        private bool disposed;

        /// <summary>
        /// Disposes this <see cref="PnPContext"/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this <see cref="PnPContext"/>
        /// </summary>
        /// <param name="disposing">Do we need to dispose resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Future, lightweight, custom logic comes here
                // Note: flushing telemetry does not belong here since apps potentially can create/dispose a log of contexts
            }

            disposed = true;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Gets the Azure Active Directory tenant id. Using the client.svc endpoint approach as that one will also work with vanity SharePoint domains
        /// </summary>
        internal async Task SetAADTenantId()
        {
            if (globalOptions.AADTenantId == Guid.Empty && Uri != null)
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{Uri}/_vti_bin/client.svc"))
                {
                    request.Headers.Add("Authorization", "Bearer");
                    HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);

                    // Grab the tenant id from the wwwauthenticate header. 
                    var bearerResponseHeader = response.Headers.WwwAuthenticate.ToString();
                    const string bearer = "Bearer realm=\"";
                    var bearerIndex = bearerResponseHeader.IndexOf(bearer, StringComparison.Ordinal);

                    var realmIndex = bearerIndex + bearer.Length;

                    if (bearerResponseHeader.Length >= realmIndex + 36)
                    {
                        var targetRealm = bearerResponseHeader.Substring(realmIndex, 36);

                        if (Guid.TryParse(targetRealm, out Guid realmGuid))
                        {
                            globalOptions.AADTenantId = realmGuid;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Simple is sub site check based upon the url pattern
        /// </summary>
        /// <param name="site">Uri to check</param>
        /// <returns>True if sub site, false otherwise</returns>
        private static bool IsSubSite(Uri site)
        {
            string cleanedPath = site.AbsolutePath.ToLower().Replace("/teams/", "").Replace("/sites/", "").TrimEnd(new char[] { '/' });

            if (cleanedPath.Contains("/"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Builds and executes provided requests
        /// </summary>
        /// <param name="requests">Collection of CSOM requests You want to execute.</param>
        /// <returns>CSOM ApiCall</returns>
        internal ApiCall GetCSOMCallForRequests(List<IRequest<object>> requests)
        {
            foreach (IRequest<object> request in requests)
            {
                CSOMApiBuilder.AddRequest(request);
            }

            return CSOMApiBuilder.BuildApiCall();
        }

        #endregion

        #region Internal dependencies
        /// <summary>
        /// Encapsulates logic to build CSOM Api call
        /// </summary>
        internal CSOMApiCallBuilder CSOMApiBuilder { get; set; } = new CSOMApiCallBuilder();

        #endregion
    }
}
