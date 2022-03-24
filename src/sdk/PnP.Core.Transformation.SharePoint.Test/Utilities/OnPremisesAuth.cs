using Microsoft.Extensions.DependencyInjection;
using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.SharePoint.Test.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace PnP.Core.Transformation.SharePoint.Test.Utilities
{
    internal class OnPremisesAuth : IDisposable
    {

        /// <summary>
        /// Gets an On-Premises connection
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="credentials">Connection credentials</param>
        /// <returns></returns>
        public ClientContext GetOnPremisesContext(string siteUrl, ICredentials credentials)
        {
            ClientContext clientContext = new ClientContext(siteUrl)
            {
                DisableReturnValueCache = true,
                Credentials = credentials
            };

            ConfigureOnPremisesContext(siteUrl, clientContext);

            return clientContext;
        }

        /// <summary>
        /// Gets an On-Premises connection
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        public ClientContext GetOnPremisesContext(string siteUrl)
        {
            ClientContext clientContext = new ClientContext(siteUrl)
            {
                DisableReturnValueCache = true,
                Credentials = CredentialCache.DefaultNetworkCredentials
            };

            ConfigureOnPremisesContext(siteUrl, clientContext);

            return clientContext;
        }


        /*
         *  This code is taken from PnP.Framework.AuthenticationManager to allow establishing connections to 
         *  on-premises servers.
         */

        internal void ConfigureOnPremisesContext(string siteUrl, ClientContext clientContext)
        {
            clientContext.ExecutingWebRequest += (sender, webRequestEventArgs) =>
            {
                // CSOM for .NET Standard 2.0 is not sending along credentials for an on-premises request, so ensure 
                // credentials and request digest are in place. This will make CSOM for .NET Standard work for 
                // SharePoint 2013, 2016 and 2019. For SharePoint 2010 this does not work as the generated CSOM request
                // contains references to version 15 while 2010 expects version 14.
                //
                // Note: the "onpremises" part of AuthenticationManager internal by design as it's only intended to be
                //       used by transformation tech that needs to get data from on-premises. PnP Framework, nor PnP 
                //       PowerShell do support SharePoint on-premises.
                webRequestEventArgs.WebRequestExecutor.WebRequest.Credentials = (sender as ClientContext).Credentials;
                // CSOM for .NET Standard does not handle request digest management, a POST to client.svc requires a digest, so ensuring that
                webRequestEventArgs.WebRequestExecutor.WebRequest.Headers.Add("X-RequestDigest", (sender as ClientContext).GetOnPremisesRequestDigestAsync().GetAwaiter().GetResult());
                // Add Request Header to force Windows Authentication which avoids an issue if multiple authentication providers are enabled on a webapplication
                webRequestEventArgs.WebRequestExecutor.RequestHeaders["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";
            };

            ClientContextSettings clientContextSettings = new ClientContextSettings()
            {
                Type = ClientContextType.OnPremises,
                SiteUrl = siteUrl,
                AuthenticationManager = null
            };

            clientContext.AddContextSettings(clientContextSettings);
        }


        ///// <summary>
        ///// Returns the request digest from the current session/site
        ///// </summary>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //private static async Task<(string digestToken, DateTime expiresOn)> GetRequestDigestInfoAsync(ClientContext context)
        //{
        //    await new SynchronizationContextRemover();

        //    string responseString = string.Empty;
        //    var accessToken = context.GetAccessToken();

        //    context.Web.EnsureProperty(w => w.Url);

        //    var httpClient = PnPHttpClient.Instance.GetHttpClient();

        //    string requestUrl = String.Format("{0}/_api/contextinfo", context.Url);
        //    using (var request = new HttpRequestMessage(HttpMethod.Post, requestUrl))
        //    {
        //        request.Headers.Add("accept", "application/json;odata=nometadata");
        //        if (!string.IsNullOrEmpty(accessToken))
        //        {
        //            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        //        }

        //        HttpResponseMessage response = await httpClient.SendAsync(request);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            responseString = await response.Content.ReadAsStringAsync();
        //        }
        //        else
        //        {
        //            var errorSb = new System.Text.StringBuilder();

        //            errorSb.AppendLine(await response.Content.ReadAsStringAsync());
        //            if (response.Headers.Contains("SPRequestGuid"))
        //            {
        //                var values = response.Headers.GetValues("SPRequestGuid");
        //                if (values != null)
        //                {
        //                    var spRequestGuid = values.FirstOrDefault();
        //                    errorSb.AppendLine($"ServerErrorTraceCorrelationId: {spRequestGuid}");
        //                }
        //            }

        //            throw new Exception(errorSb.ToString());
        //        }
        //    }
        //    var contextInformation = JsonSerializer.Deserialize<JsonElement>(responseString);

        //    string formDigestValue = contextInformation.GetProperty("FormDigestValue").GetString();
        //    int expiresIn = contextInformation.GetProperty("FormDigestTimeoutSeconds").GetInt32();
        //    return (formDigestValue, DateTime.Now.AddSeconds(expiresIn - 30));
        //}


        /// <summary>
        /// called when disposing the object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // For backwards compatibility
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    internal static class OnPremisesAuthExtensions
    {
        private static ConcurrentDictionary<string, (string requestDigest, DateTime expiresOn)> requestDigestInfos = new ConcurrentDictionary<string, (string requestDigest, DateTime expiresOn)>();

        internal static async Task<string> GetOnPremisesRequestDigestAsync(this ClientContext context)
        {
            var hostUrl = context.Url;
            if (requestDigestInfos.TryGetValue(hostUrl, out (string digestToken, DateTime expiresOn) requestDigestInfo))
            {
                // We only have to add a request digest when running in dotnet core
                if (DateTime.Now > requestDigestInfo.expiresOn)
                {
                    requestDigestInfo = await GetOnPremisesRequestDigestInfoAsync(context);
                    requestDigestInfos.AddOrUpdate(hostUrl, requestDigestInfo, (key, oldValue) => requestDigestInfo);
                }
            }
            else
            {
                // admin url maybe?
                requestDigestInfo = await GetOnPremisesRequestDigestInfoAsync(context);
                requestDigestInfos.AddOrUpdate(hostUrl, requestDigestInfo, (key, oldValue) => requestDigestInfo);
            }
            return requestDigestInfo.digestToken;
        }

        private static async Task<(string digestToken, DateTime expiresOn)> GetOnPremisesRequestDigestInfoAsync(ClientContext context)
        {
            await new SynchronizationContextRemover();

            string responseString = string.Empty;

            string requestUrl = $"{context.Url}/_vti_bin/sites.asmx";

            StringContent content = new StringContent("<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><GetUpdatedFormDigestInformation xmlns=\"http://schemas.microsoft.com/sharepoint/soap/\" /></soap:Body></soap:Envelope>");
            // Remove the default Content-Type content header
            if (content.Headers.Contains("Content-Type"))
            {
                content.Headers.Remove("Content-Type");
            }
            // Add the batch Content-Type header
            content.Headers.Add($"Content-Type", "text/xml");
            content.Headers.Add("SOAPAction", "http://schemas.microsoft.com/sharepoint/soap/GetUpdatedFormDigestInformation");
            content.Headers.Add("X-RequestForceAuthentication", "true");

            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUrl))
            {
                request.Content = content;

#pragma warning disable CA2000 // Dispose objects before losing scope
                var httpClient = PnPHttpClient.Instance.GetHttpClient(context);
#pragma warning restore CA2000 // Dispose objects before losing scope

                //Note: no credentials are passed here because the returned http context uses an already correctly configured handler

                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    responseString = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var errorSb = new System.Text.StringBuilder();

                    errorSb.AppendLine(await response.Content.ReadAsStringAsync());
                    if (response.Headers.Contains("SPRequestGuid"))
                    {
                        var values = response.Headers.GetValues("SPRequestGuid");
                        if (values != null)
                        {
                            var spRequestGuid = values.FirstOrDefault();
                            errorSb.AppendLine($"ServerErrorTraceCorrelationId: {spRequestGuid}");
                        }
                    }

                    throw new Exception(errorSb.ToString());
                }
            }

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(responseString);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xd.NameTable);
            nsmgr.AddNamespace("soap", "http://schemas.microsoft.com/sharepoint/soap/");
            XmlNode digestNode = xd.SelectSingleNode("//soap:DigestValue", nsmgr);
            if (digestNode != null)
            {
                XmlNode timeOutNode = xd.SelectSingleNode("//soap:TimeoutSeconds", nsmgr);
                int expiresIn = int.Parse(timeOutNode.InnerText);
                return (digestNode.InnerText, DateTime.Now.AddSeconds(expiresIn - 30));
            }
            else
            {
                throw new Exception("No digest found!");
            }
        }


        /// <summary>
        /// Create a new HTTP request by copying previous HTTP request's headers and properties from response's request message.
        /// Copied from: https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/blob/dev/src/Microsoft.Graph.Core/Extensions/HttpRequestMessageExtensions.cs
        /// </summary>
        /// <param name="originalRequest">The previous <see cref="HttpRequestMessage"/> needs to be copy.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>
        /// Re-issue a new HTTP request with the previous request's headers and properities
        /// </remarks>
        internal static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage originalRequest)
        {
            var newRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri);

            // Copy request headers.
            foreach (var header in originalRequest.Headers)
                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);

            // Copy request properties.
#pragma warning disable CS0618 // Type or member is obsolete
            foreach (var property in originalRequest.Properties)
            {
                newRequest.Properties.Add(property);
            }
#pragma warning restore CS0618 // Type or member is obsolete

            // Set Content if previous request had one.
            if (originalRequest.Content != null)
            {
                // HttpClient doesn't rewind streams and we have to explicitly do so.
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
                await originalRequest.Content.ReadAsStreamAsync().ContinueWith(t =>
                {
                    if (t.Result.CanSeek)
                    {
                        t.Result.Seek(0, SeekOrigin.Begin);
                    }

                    newRequest.Content = new StreamContent(t.Result);
                }).ConfigureAwait(false);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler

                // Copy content headers.
                if (originalRequest.Content.Headers != null)
                    foreach (var contentHeader in originalRequest.Content.Headers)
                        newRequest.Content.Headers.TryAddWithoutValidation(contentHeader.Key, contentHeader.Value);
            }

            return newRequest;
        }
    }

    public class ClientContextSettings
    {
        #region properties
        // Generic
        public ClientContextType Type { get; set; }
        public string SiteUrl { get; set; }
        public AuthenticationManager AuthenticationManager { get; set; }

        // User name + password flows
        internal string UserName { get; set; }
        internal string Password { get; set; }

        // App Only flows
        internal string ClientId { get; set; }
        internal string ClientSecret { get; set; }
        internal string Realm { get; set; }
        internal string AcsHostUrl { get; set; }
        internal string GlobalEndPointPrefix { get; set; }
        internal string Tenant { get; set; }
        #endregion

        #region methods
        internal bool UsesDifferentAudience(string newSiteUrl)
        {
            Uri newAudience = new Uri(newSiteUrl);
            Uri currentAudience = new Uri(this.SiteUrl);

            if (newAudience.Host != currentAudience.Host)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion


    }

    public class PnPHttpClient
    {
        //private static Configuration configuration;
        private const string PnPHttpClientName = "PnPHttpClient";
        private static readonly Lazy<PnPHttpClient> _lazyInstance = new Lazy<PnPHttpClient>(() => new PnPHttpClient(), true);
        private ServiceProvider serviceProvider;
        private static readonly ConcurrentDictionary<string, HttpClientHandler> credentialsHttpClients = new ConcurrentDictionary<string, HttpClientHandler>();

        private PnPHttpClient()
        {
            BuildServiceFactory();
        }

        public static PnPHttpClient Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        public HttpClient GetHttpClient(ClientContext context)
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            if (context.Credentials is NetworkCredential networkCredential)
            {
                string cacheKey = networkCredential.UserName;

                if (string.IsNullOrEmpty(cacheKey))
                {
                    cacheKey = CredentialCache.DefaultNetworkCredentials.UserName;
                }

                // The HttpClientHandler is the one managing the network connections and holds the resources and as
                // such we're caching this one for on-prem usage scenarions (for page transformation)
                if (credentialsHttpClients.TryGetValue(cacheKey, out HttpClientHandler cachedHttpHandler))
                {
                    // No need to dispose HttpClient, the IDisposable is purely there to trigger the 
                    // dispose of the created HttpClientHandler
                    return new HttpClient(cachedHttpHandler);
                }
                else
                {
                    // Create a new handler, do not dispose it since we're caching it
                    var handler = new HttpClientHandler
                    {
                        Credentials = context.Credentials
                    };

                    credentialsHttpClients.TryAdd(cacheKey, handler);

                    // No need to dispose HttpClient, the IDisposable is purely there to trigger the 
                    // dispose of the created HttpClientHandler
                    return new HttpClient(handler);
                }
            }
            else
            {
                // Let the HttpClientFactory handle things
                return factory.CreateClient(PnPHttpClientName);
            }
        }

        public HttpClient GetHttpClient()
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient(PnPHttpClientName);
        }

        //public static async Task AuthenticateRequestAsync(HttpRequestMessage request, ClientContext context)
        //{
        //    var accessToken = context.GetAccessToken();

        //    if (!string.IsNullOrEmpty(accessToken))
        //    {
        //        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        //    }
        //    else
        //    {
        //        var cookieContainer = context.GetAuthenticationCookies();
        //        if (cookieContainer != null)
        //        {
        //            request.Headers.Add("Cookie", cookieContainer.GetCookieHeader(new Uri(context.Url)));
        //            if (request.Method != HttpMethod.Get)
        //            {
        //                request.Headers.Add("X-RequestDigest", await context.GetRequestDigestAsync(cookieContainer).ConfigureAwait(false));
        //            }
        //        }
        //        else if (context.Credentials is NetworkCredential)
        //        {
        //            // No need to add credentials as these are already provided via the selected HttpClient/HttpClientHandler
        //            if (request.Method != HttpMethod.Get)
        //            {
        //                request.Headers.Add("X-RequestDigest", await context.GetOnPremisesRequestDigestAsync().ConfigureAwait(false));
        //            }
        //        }
        //    }
        //}

        public static void AuthenticateRequest(HttpRequestMessage request, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        private void BuildServiceFactory()
        {
            // Use TLS 1.2 as default connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            // Create container
            var serviceCollection = new ServiceCollection();

            // Add http handlers
            AddHttpHandlers(serviceCollection);

            // get User Agent String
            string userAgentFromConfig = null;
            try
            {
                userAgentFromConfig = "PnP Testing Transformation";
            }
            catch // throws exception if being called from a .NET Standard 2.0 application
            {

            }
            if (string.IsNullOrWhiteSpace(userAgentFromConfig))
            {
                userAgentFromConfig = Environment.GetEnvironmentVariable("SharePointPnPUserAgent", EnvironmentVariableTarget.Process);
            }

            // Add http clients
            AddHttpClients(serviceCollection, userAgentFromConfig);

            // Build the container
            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static TimeSpan GetHttpTimeout()
        {
            // get User Agent String
            string httpTimeOutValue = null;
           
            if (string.IsNullOrWhiteSpace(httpTimeOutValue))
            {
                httpTimeOutValue = Environment.GetEnvironmentVariable("SharePointPnPHttpTimeout", EnvironmentVariableTarget.Process);
            }

            if (int.TryParse(httpTimeOutValue, out int httpTimeout))
            {
                if (httpTimeout == -1)
                {
                    return Timeout.InfiniteTimeSpan;
                }
                else
                {
                    return new TimeSpan(0, 0, httpTimeout);
                }
            }

            // Return default value of 100 seconds
            return new TimeSpan(0, 0, 100);
        }

        private static IServiceCollection AddHttpClients(IServiceCollection collection, string UserAgent = null)
        {
            collection.AddHttpClient(PnPHttpClientName, config =>
            {
                config.Timeout = GetHttpTimeout();

                if (string.IsNullOrWhiteSpace(UserAgent))
                {
                    config.DefaultRequestHeaders.UserAgent.TryParseAdd("PnP Testing Transformation");
                }
                else
                {
                    config.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
                }
            })
            .AddHttpMessageHandler<RetryHandler>()
            // We use cookies by adding them to the header which works great when used from Core framework,
            // however when running the .NET Standard 2.0 version from .NET Framework we explicetely have to
            // tell the http client to not use the default (empty) cookie container
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            {
                UseCookies = false
            });

            return collection;
        }

        private static IServiceCollection AddHttpHandlers(IServiceCollection collection)
        {
            // Use transient for the DelegatingHandlers
            // https://stackoverflow.com/questions/53223411/httpclient-delegatinghandler-unexpected-life-cycle
            collection.AddTransient<RetryHandler, RetryHandler>();

            return collection;
        }
    }

    public enum ClientContextType
    {
        SharePointACSAppOnly = 0,
        AzureADCredentials = 1,
        AzureADCertificate = 2,
        Cookie = 3,
        AzureADInteractive = 4,
        AzureOnBehalfOf = 5,
        DeviceLogin = 6,
        OnPremises = 7,
        AccessToken = 8,
        PnPCoreSdk = 9
    }

    /// <summary>
    /// Retry handler for http requests
    /// Based upon: https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/blob/dev/src/Microsoft.Graph.Core/Extensions/HttpRequestMessageExtensions.cs
    /// </summary>
    internal class RetryHandler : DelegatingHandler
    {
        private const string RETRY_AFTER = "Retry-After";
        private const string RETRY_ATTEMPT = "Retry-Attempt";
        internal const int MAXDELAY = 300;

        #region Construction
        public RetryHandler()
        {
        }
        #endregion

        internal bool UseRetryAfterHeader { get; set; } = true;
        internal int MaxRetries { get; set; } = 10;
        internal int DelayInSeconds { get; set; } = 1;
        internal bool IncrementalDelay { get; set; } = true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposing will prevent cloning of the request needed for the retry")]
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int retryCount = 0;

            while (true)
            {
                HttpResponseMessage response = null;

                try
                {
                    response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (!ShouldRetry(response.StatusCode))
                    {
                        return response;
                    }

                    if (retryCount >= MaxRetries)
                    {
                        // Drain response content to free connections. Need to perform this
                        // before retry attempt and before the TooManyRetries ServiceException.
                        if (response.Content != null)
                        {
#if NET5_0_OR_GREATER
                            await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
                            await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
                        }

                        throw new Exception($"Too many http request retries: {retryCount}");
                    }
                }
                catch (Exception ex)
                {
                    // Find innermost exception and check if it is a SocketException
                    Exception innermostEx = ex;

                    while (innermostEx.InnerException != null) innermostEx = innermostEx.InnerException;
                    if (!(innermostEx is SocketException))
                    {
                        throw;
                    }

                    if (retryCount >= MaxRetries)
                    {
                        throw;
                    }
                }

                // Drain response content to free connections. Need to perform this
                // before retry attempt and before the TooManyRetries ServiceException.
                if (response?.Content != null)
                {
#if NET5_0_OR_GREATER
                    await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
                    await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
                }

                // Call Delay method to get delay time from response's Retry-After header or by exponential backoff 
                Task delay = Delay(response, retryCount, DelayInSeconds, cancellationToken);

                // general clone request with internal CloneAsync (see CloneAsync for details) extension method 
                // do not dispose this request as that breaks the request cloning
                request = await request.CloneAsync().ConfigureAwait(false);
                // Increase retryCount and then update Retry-Attempt in request header if needed
                retryCount++;
                AddOrUpdateRetryAttempt(request, retryCount);

                // Delay time
                await delay.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Update Retry-Attempt header in the HTTP request
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <param name="retryCount">Retry times</param>
        private void AddOrUpdateRetryAttempt(HttpRequestMessage request, int retryCount)
        {
            if (UseRetryAfterHeader)
            {
                if (request.Headers.Contains(RETRY_ATTEMPT))
                {
                    request.Headers.Remove(RETRY_ATTEMPT);
                }
                request.Headers.Add(RETRY_ATTEMPT, retryCount.ToString());
            }
        }

        private Task Delay(HttpResponseMessage response, int retryCount, int delay, CancellationToken cancellationToken)
        {
            double delayInSeconds = delay;

            if (UseRetryAfterHeader && response != null && response.Headers.TryGetValues(RETRY_AFTER, out IEnumerable<string> values))
            {
                // Can we use the provided retry-after header?
                string retryAfter = values.First();
                if (Int32.TryParse(retryAfter, out int delaySeconds))
                {
                    delayInSeconds = delaySeconds;
                }
            }
            else
            {
                // Custom delay
                if (IncrementalDelay)
                {
                    // Incremental delay, the wait time between each delay exponentially gets bigger
                    double power = Math.Pow(2, retryCount);
                    delayInSeconds = power * delay;
                }
                else
                {
                    // Linear delay
                    delayInSeconds = delay;
                }
            }

            // If the delay goes beyond our max wait time for a delay then cap it
            TimeSpan delayTimeSpan = TimeSpan.FromSeconds(Math.Min(delayInSeconds, RetryHandler.MAXDELAY));

            return Task.Delay(delayTimeSpan, cancellationToken);
        }

        internal static bool ShouldRetry(HttpStatusCode statusCode)
        {
            return (statusCode == HttpStatusCode.ServiceUnavailable ||
                    statusCode == HttpStatusCode.GatewayTimeout ||
                    statusCode == (HttpStatusCode)429);
        }
    }

    /// <summary>
    /// Based upon https://blogs.msdn.microsoft.com/benwilli/2017/02/09/an-alternative-to-configureawaitfalse-everywhere/
    /// </summary>
    public struct SynchronizationContextRemover : INotifyCompletion
    {
        public bool IsCompleted
        {
            get { return SynchronizationContext.Current == null; }
        }

        public void OnCompleted(Action continuation)
        {
            var prevContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                continuation();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevContext);
            }
        }

        public SynchronizationContextRemover GetAwaiter()
        {
            return this;
        }

        public void GetResult()
        {
        }
    }
}


namespace Microsoft.SharePoint.Client
{
    /// <summary>
    /// Class that holds the extension methods used to "tag" a client context for cloning support
    /// </summary>
    public static partial class InternalClientContextExtensions
    {
        private const string PnPSettingsKey = "SharePointPnP$Settings$ContextCloning";

        public static void AddContextSettings(this ClientRuntimeContext clientContext, ClientContextSettings contextData)
        {
            clientContext.StaticObjects[PnPSettingsKey] = contextData;
        }

        public static ClientContextSettings GetContextSettings(this ClientRuntimeContext clientContext)
        {
            if (!clientContext.StaticObjects.TryGetValue(PnPSettingsKey, out object settingsObject))
            {
                return null;
            }

            return (ClientContextSettings)settingsObject;
        }
    }
}