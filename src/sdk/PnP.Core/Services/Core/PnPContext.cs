using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using PnP.Core.Model.AzureActiveDirectory;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;
using System;
using System.Collections.Generic;
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

        private readonly Lazy<IGroup> group = new Lazy<IGroup>(() =>
        {
            return new Group();
        }, true);

        #endregion

        #region Private properties

        private readonly ISettings settings;
        private readonly TelemetryClient telemetry;
        private Batch currentBatch;

        #endregion

        #region Constructor
        internal PnPContext(ILogger logger,
                           IAuthenticationProvider authenticationProvider,
                           SharePointRestClient sharePointRestClient,
                           MicrosoftGraphClient microsoftGraphClient,
                           ISettings settingsClient,
                           TelemetryClient telemetryClient)
        {
            Id = Guid.NewGuid();
            Logger = logger;

#if DEBUG
            Mode = TestMode.Default;
#endif
            AuthenticationProvider = authenticationProvider;
            RestClient = sharePointRestClient;
            GraphClient = microsoftGraphClient;
            settings = settingsClient;
            telemetry = telemetryClient;

            if (settingsClient != null)
            {
                GraphFirst = settingsClient.GraphFirst;
                GraphAlwaysUseBeta = settingsClient.GraphAlwaysUseBeta;
                GraphCanUseBeta = settingsClient.GraphCanUseBeta;
            }

            BatchClient = new BatchClient(this, settingsClient, telemetryClient);
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
        public bool GraphFirst { get; set;  } = true;

        /// <summary>
        /// If true than all requests to Microsoft Graph use the beta endpoint
        /// </summary>
        public bool GraphAlwaysUseBeta { get; set; } = false;

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
                    throw new Exception("The GraphAlwaysUseBeta is set to true, you can't turn off the 'on-demand' beta support. First set GraphAlwaysUseBeta to false before turning of GraphCanUseBeta");
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
        public IGroup Group
        {
            get
            {
                (group.Value as Group).PnPContext = this;
                return group.Value;
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
            await BatchClient.ExecuteBatch(CurrentBatch).ConfigureAwait(false);
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

            await BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets an access token to be used for this context. Only works when the current configuration is using the <see cref="OAuthAccessTokenConfiguration"/>
        /// </summary>
        /// <param name="accessToken">Access token to set</param>
        public void SetAccessToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (!(AuthenticationProvider.Configuration is OAuthAccessTokenConfiguration))
            {
                throw new ClientException(ErrorType.ConfigurationError, "You can only set the access token when you've configured this context to use the OAuthAccessTokenConfiguration");
            }

            (AuthenticationProvider.Configuration as OAuthAccessTokenConfiguration).AccessToken = accessToken;
        }

        /// <summary>
        /// Clones this context into a new context for the same SharePoint site
        /// </summary>
        /// <returns>New <see cref="PnPContext"/></returns>
        public PnPContext Clone()
        {
            return Clone(null);
        }

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="uri">Uri of the other SharePoint site</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        public PnPContext Clone(Uri uri)
        {
            PnPContext clonedContext = new PnPContext(Logger, AuthenticationProvider, RestClient, GraphClient, settings, telemetry)
            {
                // Take over graph settings
                GraphCanUseBeta = graphCanUseBeta,
                GraphAlwaysUseBeta = GraphAlwaysUseBeta,
                GraphFirst = GraphFirst
            };

            if (uri != null)
            {
                clonedContext.Uri = uri;
            }
            else
            {
                if (Uri != null)
                {
                    clonedContext.Uri = Uri;
                }
            }

            return clonedContext;
        }

        #endregion

#if DEBUG

        #region Internal methods to support unit testing

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

        private bool disposed = false;

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
                // Flush telemetry
                telemetry.Flush();                
            }

            disposed = true;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Gets the Azure Active Directory tenant id. Using the client.svc endpoint approach as that one will also work with vanity SharePoint domains
        /// </summary>
        internal void SetAADTenantId()
        {
            if (settings.AADTenantId == Guid.Empty && Uri != null)
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{Uri}/_vti_bin/client.svc"))
                {
                    request.Headers.Add("Authorization", "Bearer");
                    HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult();

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
                            settings.AADTenantId = realmGuid;
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

        #endregion
    }
}
