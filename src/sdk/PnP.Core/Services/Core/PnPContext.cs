using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Model.Me;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnP Context class...the glue between the model and the data stores
    /// </summary>
    public class PnPContext : IPnPContext
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

        private readonly Lazy<ISocial> social = new Lazy<ISocial>(() =>
        {
            return new Social();
        }, true);

        private readonly Lazy<IMe> me= new Lazy<IMe>(() =>
        {
            return new Me();
        }, true);

        private readonly Lazy<IContentTypeHub> contentTypeHub = new Lazy<IContentTypeHub>(() =>
        {
            return new ContentTypeHub();
        }, true);

        #endregion

        #region Internal properties

        internal readonly PnPGlobalSettingsOptions GlobalOptions;
        internal readonly PnPContextFactoryOptions ContextOptions;

        #endregion

        #region Private properties

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
            GlobalOptions = globalOptions;
            ContextOptions = contextOptions;
            telemetry = telemetryManager;

            if (ContextOptions != null)
            {
                GraphFirst = ContextOptions.GraphFirst;
                GraphAlwaysUseBeta = ContextOptions.GraphAlwaysUseBeta;
                GraphCanUseBeta = ContextOptions.GraphCanUseBeta;
            }

            if (globalOptions != null && globalOptions.Environment.HasValue)
            {
                Environment = globalOptions.Environment.Value;

                if (Environment.Value == Microsoft365Environment.Custom)
                {
                    MicrosoftGraphAuthority = globalOptions.MicrosoftGraphAuthority;
                    AzureADLoginAuthority = globalOptions.AzureADLoginAuthority;
                    
                    // Ensure the Microsoft Graph URL is set depending on the used cloud environment
                    GraphClient.UpdateBaseAddress(MicrosoftGraphAuthority);
                }
                else
                {
                    // Ensure the Microsoft Graph URL is set depending on the used cloud environment
                    GraphClient.UpdateBaseAddress(CloudManager.GetMicrosoftGraphAuthority(Environment.Value));
                }
            }

            BatchClient = new BatchClient(this, GlobalOptions, telemetryManager);
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
        /// Returns the used Microsoft 365 cloud environment
        /// </summary>
        public Microsoft365Environment? Environment { get; internal set; }

        /// <summary>
        /// Returns the Microsoft Graph authority (e.g. graph.microsoft.com) to use when <see cref="Environment"/> is set to <see cref="Microsoft365Environment.Custom"/>
        /// </summary>
        public string MicrosoftGraphAuthority { get; internal set; }

        /// <summary>
        /// Returns the Azure AD Login authority (e.g. login.microsoftonline.com) to use when <see cref="Environment"/> is set to <see cref="Microsoft365Environment.Custom"/>
        /// </summary>
        public string AzureADLoginAuthority { get; internal set; }

        /// <summary>
        /// Collection for custom properties that you want to attach to a <see cref="PnPContext"/>
        /// </summary>
        public IDictionary<string, object> Properties { get; internal set; } = new Dictionary<string, object>();

        /// <summary>
        /// Connected batch client
        /// </summary>
        internal BatchClient BatchClient { get; }

        /// <summary>
        /// Unique id for this <see cref="PnPContext"/>
        /// </summary>
        internal Guid Id { get; private set; }

        /// <summary>
        /// Optional options specified during context creation, needed for context cloning
        /// </summary>
        internal PnPContextOptions LocalContextOptions { get; set; }

        /// <summary>
        /// Modules to be added to the next request's execution pipeline
        /// </summary>
        internal List<IRequestModule> RequestModules { get; set; }

        /// <summary>
        /// The cancellation token to cancel operation
        /// </summary>
        internal CancellationToken CancellationToken { get; set; }

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

        /// <summary>
        /// Number of clones created from this context
        /// </summary>
        internal int CloneCount { get; set; } = 0;
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

        /// <summary>
        /// Entry point for the social-related APIs
        /// </summary>
        public ISocial Social
        {
            get
            {
                social.Value.PnPContext = this;
                return social.Value;
            }
        }

        /// <summary>
        /// Entry point for the Me object
        /// </summary>
        public IMe Me
        {
            get
            {
                (me.Value as Me).PnPContext = this;
                return me.Value;
            }
        }

        /// <summary>
        /// Entry point for the ContentTypeHub object
        /// </summary>
        public IContentTypeHub ContentTypeHub
        {
            get
            {
                (contentTypeHub.Value as ContentTypeHub).PnPContext = this;
                return contentTypeHub.Value;
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
        /// Gets an ongoing Graph long-running operation.
        /// </summary>
        /// <param name="location">The location of the operation</param>
        /// <returns>An <see cref="ILongRunningOperation"/> associated with the location</returns>
        public ILongRunningOperation GetLongRunningOperation(string location)
        {
            return new LongRunningOperation(location, this);
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
            return CloneAsync().GetAwaiter().GetResult();
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
            return CloneAsync(name).GetAwaiter().GetResult();
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

            var configuration = ContextOptions.Configurations.FirstOrDefault(c => c.Name == name);
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
            return CloneAsync(uri).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="uri">Uri of the other SharePoint site</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        public async Task<PnPContext> CloneAsync(Uri uri)
        {
            PnPContext clonedContext = CreateClonedContext(uri);

#if DEBUG
            if (Mode != TestMode.Default)
            {
                CloneCount++;
                clonedContext = await CloneForTestingAsync(this, uri, TestName, TestId + (100 * CloneCount)).ConfigureAwait(false);
            }
            else
            {
                await InitializeClonedContextAsync(uri, clonedContext).ConfigureAwait(false);
            }
#else
            await InitializeClonedContextAsync(uri, clonedContext).ConfigureAwait(false);
#endif
            return clonedContext;
        }

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="groupId">Id of the other Microsoft 365 group to create a <see cref="PnPContext"/> for</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        public PnPContext Clone(Guid groupId)
        { 
            return CloneAsync(groupId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Clones this context for another SharePoint site
        /// </summary>
        /// <param name="groupId">Id of the other Microsoft 365 group to create a <see cref="PnPContext"/> for</param>
        /// <returns>New <see cref="PnPContext"/></returns>
        public async Task<PnPContext> CloneAsync(Guid groupId)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException(nameof(groupId));
            }

            PnPContext clonedContext = PrepareClonedContext();
#if DEBUG
            if (Mode != TestMode.Default)
            {
                CloneCount++;
                clonedContext = await CloneForTestingAsync(this, groupId, TestId + (100 * CloneCount)).ConfigureAwait(false);
            }
            else
            {
                await InitializeClonedContextAsync(groupId, clonedContext).ConfigureAwait(false);
            }
#else
            await InitializeClonedContextAsync(groupId, clonedContext).ConfigureAwait(false);
#endif
            return clonedContext;
        }

        private async Task InitializeClonedContextAsync(Uri uri, PnPContext clonedContext)
        {
            if (!uri.Equals(Uri))
            {
                await PnPContextFactory.InitializeContextAsync(clonedContext, LocalContextOptions).ConfigureAwait(false);
            }
            else
            {
                await PnPContextFactory.CopyContextInitializationAsync(this, clonedContext).ConfigureAwait(false);
            }
        }

        private async Task InitializeClonedContextAsync(Guid groupId, PnPContext clonedContext)
        {
            if (!groupId.Equals(Site.GroupId))
            {
                await PnPContextFactory.ConfigureForGroup(clonedContext, groupId).ConfigureAwait(false);
                await PnPContextFactory.InitializeContextAsync(clonedContext, LocalContextOptions).ConfigureAwait(false);
            }
            else
            {
                // Ensure the context has an Url set
                clonedContext.Uri = Uri;
                await PnPContextFactory.CopyContextInitializationAsync(this, clonedContext).ConfigureAwait(false);
            }
        }

        private PnPContext CreateClonedContext(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            using (PnPContext clonedContext = PrepareClonedContext())
            {
                clonedContext.Uri = uri;

                return clonedContext;
            }
        }

        private PnPContext PrepareClonedContext()
        {
            using (PnPContext clonedContext = new PnPContext(Logger, AuthenticationProvider, RestClient, GraphClient, ContextOptions, GlobalOptions, telemetry)
            {
                // Take over graph settings
                GraphCanUseBeta = graphCanUseBeta,
                GraphAlwaysUseBeta = GraphAlwaysUseBeta,
                GraphFirst = GraphFirst,
                Environment = Environment,
                Properties = Properties,
            })
            {
                return clonedContext;
            }
        }
        #endregion

        #region Internal methods

        internal async Task<Guid> GetTenantIdAsync()
        {
            // in case telemetry is configured, the globaloptions already has a populated tenantid value
            if (GlobalOptions.AADTenantId == Guid.Empty)
            {
                var useOpenIdConfiguration = false;
#if NET5_0_OR_GREATER
                useOpenIdConfiguration = RuntimeInformation.RuntimeIdentifier == "browser-wasm";
#endif
                await SetAADTenantId(useOpenIdConfiguration).ConfigureAwait(false);
            }

            if (GlobalOptions.AADTenantId != Guid.Empty)
            {
                return GlobalOptions.AADTenantId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        internal async Task<bool> AccessTokenHasRoleAsync(string role)
        {
            return AnalyzeAccessToken(role, await AuthenticationProvider.GetAccessTokenAsync(Uri).ConfigureAwait(false), "roles");
        }

        internal static bool AccessTokenHasRole(string accessToken, string role)
        {
            return AnalyzeAccessToken(role, accessToken, "roles");
        }

        internal async Task<bool> AccessTokenHasScopeAsync(string scope)
        {
            return AnalyzeAccessToken(scope, await AuthenticationProvider.GetAccessTokenAsync(Uri).ConfigureAwait(false), "scp");
        }

        internal static bool AccessTokenHasScope(string accessToken, string scope)
        {
            return AnalyzeAccessToken(scope, accessToken, "scp");
        }

        internal async Task<bool> AccessTokenUsesApplicationPermissionsAsync()
        {
            return AccessTokenUsesRoles(await AuthenticationProvider.GetAccessTokenAsync(Uri).ConfigureAwait(false));
        }

        internal static bool AccessTokenUsesApplicationPermissions(string accessToken)
        {
            return AccessTokenUsesRoles(accessToken);
        }

        private static bool AnalyzeAccessToken(string role, string accessToken, string tokenPropertyToVerify)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(accessToken);

                if (token != null)
                {
                    if (token.Payload.ContainsKey(tokenPropertyToVerify))
                    {
                        if (token.Payload[tokenPropertyToVerify].ToString().Contains(role, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            return false;
        }

        private static bool AccessTokenUsesRoles(string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(accessToken);

                if (token != null)
                {
                    return token.Payload.ContainsKey("roles");
                }
            }

            return false;
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
                    var configuration = ContextOptions.Configurations.FirstOrDefault(c => c.Name == name);
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

            PnPContext clonedContext = CreateClonedContext(uri);
            clonedContext.CloneCount = source.CloneCount + 13;

            if (source.Mode == TestMode.Mock)
            {
                clonedContext.SetMockMode(id, source.TestName, source.TestFilePath, source.GenerateTestMockingDebugFiles, source.TestUris);
            }
            else
            {
                clonedContext.SetRecordingMode(id, source.TestName, source.TestFilePath, source.GenerateTestMockingDebugFiles, source.TestUris);
            }

            await InitializeClonedContextAsync(uri, clonedContext).ConfigureAwait(false);

            return clonedContext;
        }

        internal async Task<PnPContext> CloneForTestingAsync(PnPContext source, Guid groupId, int id)
        {

            PnPContext clonedContext = PrepareClonedContext();
            clonedContext.CloneCount = source.CloneCount + 13;

            if (source.Mode == TestMode.Mock)
            {
                clonedContext.SetMockMode(id, source.TestName, source.TestFilePath, source.GenerateTestMockingDebugFiles, source.TestUris);
            }
            else
            {
                clonedContext.SetRecordingMode(id, source.TestName, source.TestFilePath, source.GenerateTestMockingDebugFiles, source.TestUris);
            }

            await InitializeClonedContextAsync(groupId, clonedContext).ConfigureAwait(false);

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
                // Note: flushing telemetry does not belong here since apps potentially can create/dispose a lot of contexts
            }

            disposed = true;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Gets the Azure Active Directory tenant id. Using the client.svc endpoint approach as that one will also work with vanity SharePoint domains
        /// </summary>
        internal async Task SetAADTenantId(bool useOpenIdConfiguration = false)
        {
            if (GlobalOptions.AADTenantId == Guid.Empty && Uri != null)
            {
                if (useOpenIdConfiguration)
                {
                    // This approach is used when running from Blazor WASM as the other approach results in an OPTIONS call with a redirect,
                    // which is not allowed. Returned error: "Response to preflight request doesn't pass access control check: Redirect
                    // is not allowed for a preflight request"

                    string loginEndpoint = "login.microsoftonline.com";
                    if (Environment.HasValue)
                    {
                        if (Environment.Value == Microsoft365Environment.Custom)
                        {
                            loginEndpoint = AzureADLoginAuthority;
                        }
                        else
                        {
                            loginEndpoint = CloudManager.GetAzureADLoginAuthority(Environment.Value);
                        }
                    }

                    // Approach might not always work given the tenant name parsing, but at least works for 99%+ of the tenants
                    using (var request = new HttpRequestMessage(HttpMethod.Get, $"https://{loginEndpoint}/{GetTenantNameFromUrl(Uri.ToString())}.onmicrosoft.com/.well-known/openid-configuration"))
                    {
                        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
                        var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var json = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                        if (json.TryGetProperty("token_endpoint", out JsonElement tokenEndpoint))
                        {
                            string targetRealm = GetSubstringFromMiddle(tokenEndpoint.GetString(), $"https://{loginEndpoint}/", "/oauth2/");
                            if (Guid.TryParse(targetRealm, out Guid realmGuid))
                            {
                                GlobalOptions.AADTenantId = realmGuid;
                            }
                        }
                    }
                }
                else
                {
                    // Default approach for non Blazor WASM usage us

                    using (var request = new HttpRequestMessage(HttpMethod.Get, $"{Uri}/_vti_bin/client.svc"))
                    {
                        request.Headers.Add("Authorization", "Bearer");
                        request.Headers.Add("Access-Control-Allow-Origin", "*");
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
                                GlobalOptions.AADTenantId = realmGuid;
                            }
                        }
                    }
                }
            }
        }

        private static string GetTenantNameFromUrl(string tenantUrl)
        {
            if (tenantUrl.ToLower().Contains("-admin.sharepoint."))
            {
                return GetSubstringFromMiddle(tenantUrl, "https://", "-admin.sharepoint.");
            }
            else
            {
                return GetSubstringFromMiddle(tenantUrl, "https://", ".sharepoint.");
            }
        }

        private static string GetSubstringFromMiddle(string originalString, string prefix, string suffix)
        {
            var index = originalString.IndexOf(suffix, StringComparison.OrdinalIgnoreCase);
            return index != -1 ? originalString.Substring(prefix.Length, index - prefix.Length) : null;
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
