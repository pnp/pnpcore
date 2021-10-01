using PnP.Core.Transformation.SharePoint;
using PnP.Core.Transformation.SharePoint.Model;
using PnP.Core.Transformation.SharePoint.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using Microsoft.Extensions.DependencyInjection;

namespace PnP.Core.Transformation.SharePoint
{
    /// <summary>
    /// Class that deals with cloning client context object, getting access token and validates server version
    /// </summary>
    public static partial class ClientContextExtensions
    {
        private const string PnPSettingsKey = "SharePointPnP$Settings$ContextCloning";

        private static ILoggerFactory loggerFactory;
        private static ILogger log;
        private static readonly string userAgentFromConfig;

#pragma warning disable CS0169
        private static ConcurrentDictionary<string, (string requestDigest, DateTime expiresOn)> requestDigestInfos = new ConcurrentDictionary<string, (string requestDigest, DateTime expiresOn)>();
#pragma warning restore CS0169

        //private static bool hasAuthCookies;

        /// <summary>
        /// Static constructor, only executed once per class load
        /// </summary>
#pragma warning disable CA1810
        static ClientContextExtensions()
        {
            try
            {
                ClientContextExtensions.userAgentFromConfig = ConfigurationManager.AppSettings["SharePointPnPUserAgent"];
            }
            catch // throws exception if being called from a .NET Standard 2.0 application
            {

            }
            if (string.IsNullOrWhiteSpace(ClientContextExtensions.userAgentFromConfig))
            {
                ClientContextExtensions.userAgentFromConfig = Environment.GetEnvironmentVariable("SharePointPnPUserAgent", EnvironmentVariableTarget.Process);
            }

#if NET5_0
            loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
#else
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());
            loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
#endif
            log = new Logger<ClientContext>(loggerFactory);
        }
#pragma warning restore CA1810

        /// <summary>
        /// Executes the current set of data retrieval queries and method invocations and retries it if needed using the Task Library.
        /// </summary>
        /// <param name="clientContext">clientContext to operate on</param>
        /// <param name="retryCount">Number of times to retry the request</param>
        /// <param name="userAgent">UserAgent string value to insert for this request. You can define this value in your app's config file using key="SharePointPnPUserAgent" value="PnPRocks"></param>
        public static Task ExecuteQueryRetryAsync(this ClientRuntimeContext clientContext, int retryCount = 10, string userAgent = null)
        {
            return ExecuteQueryImplementation(clientContext, retryCount, userAgent);
        }


        /// <summary>
        /// Executes the current set of data retrieval queries and method invocations and retries it if needed.
        /// </summary>
        /// <param name="clientContext">clientContext to operate on</param>
        /// <param name="retryCount">Number of times to retry the request</param>
        /// <param name="userAgent">UserAgent string value to insert for this request. You can define this value in your app's config file using key="SharePointPnPUserAgent" value="PnPRocks"></param>
        public static void ExecuteQueryRetry(this ClientRuntimeContext clientContext, int retryCount = 10, string userAgent = null)
        {
            Task.Run(() => ExecuteQueryImplementation(clientContext, retryCount, userAgent)).GetAwaiter().GetResult();
        }

        private static async Task ExecuteQueryImplementation(ClientRuntimeContext clientContext, int retryCount = 10, string userAgent = null)
        {
            // Temporarly remove the SynchronizationContext
            await new SynchronizationContextRemover();

            // Set the TLS preference. Needed on some server os's to work when Office 365 removes support for TLS 1.0
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var clientTag = string.Empty;
            int backoffInterval = 500;
            int retryAttempts = 0;
            int retryAfterInterval = 0;
            bool retry = false;
            ClientRequestWrapper wrapper = null;

            if (retryCount <= 0)
                throw new ArgumentException("Provide a retry count greater than zero.");

            // Do while retry attempt is less than retry count
            while (retryAttempts < retryCount)
            {
                try
                {
                    clientContext.ClientTag = SetClientTag(clientTag);

                    // Make CSOM request more reliable by disabling the return value cache. Given we 
                    // often clone context objects and the default value is
                    clientContext.DisableReturnValueCache = true;
                    // Add event handler to "insert" app decoration header to mark the PnP Sites Core library as a known application
                    EventHandler<WebRequestEventArgs> appDecorationHandler = AttachRequestUserAgent(userAgent);

                    clientContext.ExecutingWebRequest += appDecorationHandler;

                    // DO NOT CHANGE THIS TO EXECUTEQUERYRETRY
                    if (!retry)
                    {
                        await clientContext.ExecuteQueryAsync();
                    }
                    else
                    {
                        if (wrapper != null && wrapper.Value != null)
                        {
                            await clientContext.RetryQueryAsync(wrapper.Value);
                        }
                    }

                    // Remove the app decoration event handler after the executequery
                    clientContext.ExecutingWebRequest -= appDecorationHandler;

                    return;
                }
                catch (WebException wex)
                {
                    var response = wex.Response as HttpWebResponse;
                    // Check if request was throttled - http status code 429
                    // Check is request failed due to server unavailable - http status code 503
                    if ((response != null &&
                        (response.StatusCode == (HttpStatusCode)429
                        || response.StatusCode == (HttpStatusCode)503
                        // || response.StatusCode == (HttpStatusCode)500
                        ))
                        || wex.Status == WebExceptionStatus.Timeout)
                    {
                        wrapper = (ClientRequestWrapper)wex.Data["ClientRequest"];
                        retry = true;
                        retryAfterInterval = 0;

                        //Add delay for retry, retry-after header is specified in seconds
                        if (response != null && response.Headers["Retry-After"] != null)
                        {
                            if (int.TryParse(response.Headers["Retry-After"], out int retryAfterHeaderValue))
                            {
                                retryAfterInterval = retryAfterHeaderValue * 1000;
                            }
                        }
                        else
                        {
                            retryAfterInterval = backoffInterval;
                            backoffInterval *= 2;
                        }

                        if (wex.Status == WebExceptionStatus.Timeout)
                        {
                            log.LogWarning(string.Format(SharePointTransformationResources.Warning_CSOMRequestTimeout, retryAttempts + 1, retryAfterInterval));
                        }
                        else
                        {
                            log.LogWarning(string.Format(SharePointTransformationResources.Warning_CSOMRequestFrequencyExceeded, retryAttempts + 1, retryAfterInterval));
                        }

                        await Task.Delay(retryAfterInterval);

                        //Add to retry count and increase delay.
                        retryAttempts++;
                    }
                    else
                    {
                        var errorSb = new System.Text.StringBuilder();

                        errorSb.AppendLine(wex.ToString());
                        errorSb.AppendLine($"TraceCorrelationId: {clientContext.TraceCorrelationId}");
                        errorSb.AppendLine($"Url: {clientContext.Url}");

                        //find innermost Error and check if it is a SocketException
                        Exception innermostEx = wex;
                        while (innermostEx.InnerException != null) innermostEx = innermostEx.InnerException;
                        var socketEx = innermostEx as System.Net.Sockets.SocketException;
                        if (socketEx != null)
                        {
                            errorSb.AppendLine($"ErrorCode: {socketEx.ErrorCode}"); //10054
                            errorSb.AppendLine($"SocketErrorCode: {socketEx.SocketErrorCode}"); //ConnectionReset
                            errorSb.AppendLine($"Message: {socketEx.Message}"); //An existing connection was forcibly closed by the remote host

                            log.LogError(string.Format(SharePointTransformationResources.Error_ClientContextExtensions_ExecuteQueryRetryException, errorSb));

                            //retry
                            wrapper = (ClientRequestWrapper)wex.Data["ClientRequest"];
                            retry = true;
                            retryAfterInterval = 0;

                            //Add delay for retry, retry-after header is specified in seconds
                            if (response != null && response.Headers["Retry-After"] != null)
                            {
                                if (int.TryParse(response.Headers["Retry-After"], out int retryAfterHeaderValue))
                                {
                                    retryAfterInterval = retryAfterHeaderValue * 1000;
                                }
                            }
                            else
                            {
                                retryAfterInterval = backoffInterval;
                                backoffInterval *= 2;
                            }

                            log.LogWarning(string.Format(SharePointTransformationResources.Error_CSOMRequestSocketException, retryAttempts + 1, retryAfterInterval));

                            await Task.Delay(retryAfterInterval);

                            //Add to retry count and increase delay.
                            retryAttempts++;
                        }
                        else
                        {
                            if (response != null)
                            {
                                //if(response.Headers["SPRequestGuid"] != null) 
                                if (response.Headers.AllKeys.Any(k => string.Equals(k, "SPRequestGuid", StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    var spRequestGuid = response.Headers["SPRequestGuid"];
                                    errorSb.AppendLine($"ServerErrorTraceCorrelationId: {spRequestGuid}");
                                }
                            }

                            log.LogError(string.Format(SharePointTransformationResources.Error_ClientContextExtensions_ExecuteQueryRetryException, errorSb));
                            throw;
                        }
                    }
                }
                catch (ServerException serverEx)
                {
                    var errorSb = new System.Text.StringBuilder();

                    errorSb.AppendLine(serverEx.ToString());
                    errorSb.AppendLine($"ServerErrorCode: {serverEx.ServerErrorCode}");
                    errorSb.AppendLine($"ServerErrorTypeName: {serverEx.ServerErrorTypeName}");
                    errorSb.AppendLine($"ServerErrorTraceCorrelationId: {serverEx.ServerErrorTraceCorrelationId}");
                    errorSb.AppendLine($"ServerErrorValue: {serverEx.ServerErrorValue}");
                    errorSb.AppendLine($"ServerErrorDetails: {serverEx.ServerErrorDetails}");

                    log.LogError(string.Format(SharePointTransformationResources.Error_ClientContextExtensions_ExecuteQueryRetryException, errorSb));

                    throw;
                }
            }

            throw new MaximumRetryAttemptedException($"Maximum retry attempts {retryCount}, has be attempted.");
        }

        /// <summary>
        /// Attaches either a passed user agent, or one defined in the App.config file, to the WebRequstExecutor UserAgent property.
        /// </summary>
        /// <param name="customUserAgent">a custom user agent to override any defined in App.config</param>
        /// <returns>An EventHandler of WebRequestEventArgs.</returns>
        private static EventHandler<WebRequestEventArgs> AttachRequestUserAgent(string customUserAgent)
        {
            return (s, e) =>
            {
                bool overrideUserAgent = true;
                var existingUserAgent = e.WebRequestExecutor.WebRequest.UserAgent;
                if (!string.IsNullOrEmpty(existingUserAgent) && existingUserAgent.StartsWith("NONISV|SharePointPnP|PnPPS/"))
                {
                    overrideUserAgent = false;
                }
                if (overrideUserAgent)
                {
                    e.WebRequestExecutor.WebRequest.UserAgent = string.IsNullOrEmpty(customUserAgent) ? SharePointConstants.TransformationUserAgent : customUserAgent;
                }
            };
        }

        /// <summary>
        /// Sets the client context client tag on outgoing CSOM requests.
        /// </summary>
        /// <param name="clientTag">An optional client tag to set on client context requests.</param>
        /// <returns></returns>
        private static string SetClientTag(string clientTag = "")
        {
            // ClientTag property is limited to 32 chars
            if (string.IsNullOrEmpty(clientTag))
            {
                clientTag = $"{SharePointConstants.TransformationClientTag}:{GetCallingPnPMethod()}";
            }
            if (clientTag.Length > 32)
            {
                clientTag = clientTag.Substring(0, 32);
            }

            return clientTag;
        }

#pragma warning disable CA1034,CA2229,CA1032
        /// <summary>
        /// Defines a Maximum Retry Attemped Exception
        /// </summary>
        [Serializable]
        public class MaximumRetryAttemptedException : Exception
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="message"></param>
            public MaximumRetryAttemptedException(string message)
                : base(message)
            {

            }
        }
#pragma warning restore CA1034,CA2229,CA1032

        /// <summary>
        /// Checks the server library version of the context for a minimally required version
        /// </summary>
        /// <param name="clientContext">clientContext to operate on</param>
        /// <param name="minimallyRequiredVersion">provide version to validate</param>
        /// <returns>True if it has minimal required version, false otherwise</returns>
        public static bool HasMinimalServerLibraryVersion(this ClientRuntimeContext clientContext, string minimallyRequiredVersion)
        {
            return HasMinimalServerLibraryVersion(clientContext, new Version(minimallyRequiredVersion));
        }

        /// <summary>
        /// Checks the server library version of the context for a minimally required version
        /// </summary>
        /// <param name="clientContext">clientContext to operate on</param>
        /// <param name="minimallyRequiredVersion">provide version to validate</param>
        /// <returns>True if it has minimal required version, false otherwise</returns>
        public static bool HasMinimalServerLibraryVersion(this ClientRuntimeContext clientContext, Version minimallyRequiredVersion)
        {
            bool hasMinimalVersion = false;
            try
            {
                clientContext.ExecuteQueryRetry();
                hasMinimalVersion = clientContext.ServerLibraryVersion.CompareTo(minimallyRequiredVersion) >= 0;
            }
            catch (PropertyOrFieldNotInitializedException)
            {
                // swallow the exception.
            }

            return hasMinimalVersion;
        }

        /// <summary>
        /// Returns the name of the method calling ExecuteQueryRetry and ExecuteQueryRetryAsync
        /// </summary>
        /// <returns>A string with the method name</returns>
        private static string GetCallingPnPMethod()
        {
            StackTrace t = new StackTrace();

            string pnpMethod = "";
            try
            {
                for (int i = 0; i < t.FrameCount; i++)
                {
                    var frame = t.GetFrame(i);
                    var frameName = frame.GetMethod().Name;
                    if (frameName.Equals("ExecuteQueryRetry") || frameName.Equals("ExecuteQueryRetryAsync"))
                    {
                        var method = t.GetFrame(i + 1).GetMethod();

                        // Only return the calling method in case ExecuteQueryRetry was called from inside the PnP core library
                        if (method.Module.Name.Equals("PnP.Framework.dll", StringComparison.InvariantCultureIgnoreCase))
                        {
                            pnpMethod = method.Name;
                        }
                        break;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return pnpMethod;
        }

        /// <summary>
        /// Gets the version of SharePoint
        /// </summary>
        /// <param name="clientContext"></param>
        /// <returns>A tuple with the SharePoint version and the SharePoint exact version</returns>
        public static (SPVersion, string) GetVersions(this ClientRuntimeContext clientContext)
        {
            Uri urlUri = new Uri(clientContext.Url);

            // TODO: Consider adding a global caching provider via DI
            var spVersionFromCache = SPVersion.Unknown;
            string version = null;
            if (spVersionFromCache != SPVersion.Unknown)
            {
                return (spVersionFromCache, version);
            }
            else
            {
                try
                {

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{urlUri.Scheme}://{urlUri.DnsSafeHost}:{urlUri.Port}/_vti_pvt/service.cnf");
                    //request.Credentials = clientContext.Credentials;
                    request.AddAuthenticationData(clientContext as ClientContext);

                    var response = request.GetResponse();

                    using (var dataStream = response.GetResponseStream())
                    {
                        // Open the stream using a StreamReader for easy access.
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(dataStream))
                        {
                            // Read the content. Will be in this format
                            // SPO:
                            // vti_encoding: SR | utf8 - nl
                            // vti_extenderversion: SR | 16.0.0.8929
                            // SP2019:
                            // vti_encoding:SR|utf8-nl
                            // vti_extenderversion:SR|16.0.0.10340
                            // SP2016:
                            // vti_encoding: SR | utf8 - nl
                            // vti_extenderversion: SR | 16.0.0.4732
                            // SP2013:
                            // vti_encoding:SR|utf8-nl
                            // vti_extenderversion: SR | 15.0.0.4505
                            // Version numbers from https://buildnumbers.wordpress.com/sharepoint/

                            // Microsoft Developer Blog - 
                            //      https://developer.microsoft.com/en-us/sharepoint/blogs/updated-versions-of-the-sharepoint-on-premises-csom-nuget-packages/
                            // Todd Klindt's Blog - 
                            //      http://www.toddklindt.com/sp2010builds
                            //      http://www.toddklindt.com/sp2013builds
                            //      http://www.toddklindt.com/sp2016builds
                            //      http://www.toddklindt.com/sp2019builds


                            version = reader.ReadToEnd().Split('|')[2].Trim();
                            
                            // TODO: Consider using global caching to store versions

                            if (Version.TryParse(version, out Version v))
                            {
                                if (v.Major == 14)
                                {
                                    // SP2010 is not supported anymore
                                    spVersionFromCache = SPVersion.Unsupported;
                                }
                                else if (v.Major == 15)
                                {
                                    // You can change the output to SP2013 to use standard CSOM calls.
                                    spVersionFromCache = SPVersion.SP2013Legacy;
                                }
                                else if (v.Major == 16)
                                {
                                    if (v.MinorRevision < 6000)
                                    {
                                        spVersionFromCache = SPVersion.SP2016Legacy;
                                    }
                                    // Set to 12000 because some SPO reports as 12012 and SP2019 build numbers are increasing very slowly
                                    else if (v.MinorRevision > 10300 && v.MinorRevision < 12000)
                                    {
                                        spVersionFromCache = SPVersion.SP2019;
                                    }
                                    else
                                    {
                                        spVersionFromCache = SPVersion.SPO;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (WebException)
                {
                    // todo
                }
            }

            // TODO: Consider using global caching to store versions
            return (spVersionFromCache, version);
        }

        /// <summary>
        /// Clones a ClientContext object while "taking over" the security context of the existing ClientContext instance
        /// </summary>
        /// <param name="clientContext">ClientContext to be cloned</param>
        /// <param name="siteUrl">Site URL to be used for cloned ClientContext</param>
        /// <returns>A ClientContext object created for the passed site URL</returns>
        public static ClientContext Clone(this ClientRuntimeContext clientContext, string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                throw new ArgumentException(SharePointTransformationResources.Error_Clone_Context_Url_Required, nameof(siteUrl));
            }

            return clientContext.Clone(new Uri(siteUrl));
        }

        /// <summary>
        /// Clones a ClientContext object while "taking over" the security context of the existing ClientContext instance
        /// </summary>
        /// <param name="clientContext">ClientContext to be cloned</param>
        /// <param name="siteUrl">Site URL to be used for cloned ClientContext</param>
        /// <returns>A ClientContext object created for the passed site URL</returns>
        public static ClientContext Clone(this ClientRuntimeContext clientContext, Uri siteUrl)
        {
            return Clone(clientContext, new ClientContext(siteUrl), siteUrl);
        }

        /// <summary>
        /// Clones a ClientContext object while "taking over" the security context of the existing ClientContext instance
        /// </summary>
        /// <param name="clientContext">ClientContext to be cloned</param>
        /// <param name="targetContext">CientContext stub to be used for cloning</param>
        /// <param name="siteUrl">Site URL to be used for cloned ClientContext</param>
        /// <returns>A ClientContext object created for the passed site URL</returns>
        internal static ClientContext Clone(this ClientRuntimeContext clientContext, ClientContext targetContext, Uri siteUrl)
        {
            if (siteUrl == null)
            {
                throw new ArgumentException(SharePointTransformationResources.Error_Clone_Context_Url_Required, nameof(siteUrl));
            }

            ClientContext clonedClientContext = targetContext;
            clonedClientContext.ClientTag = clientContext.ClientTag;
            clonedClientContext.DisableReturnValueCache = clientContext.DisableReturnValueCache;
            clonedClientContext.WebRequestExecutorFactory = clientContext.WebRequestExecutorFactory;

            //Take over the form digest handling setting

            var originalUri = new Uri(clientContext.Url);

            clonedClientContext.ExecutingWebRequest += (sender, webRequestEventArgs) =>
            {
                // Call the ExecutingWebRequest delegate method from the original ClientContext object, but pass along the webRequestEventArgs of 
                // the new delegate method
                MethodInfo methodInfo = clientContext.GetType().GetMethod("OnExecutingWebRequest", BindingFlags.Instance | BindingFlags.NonPublic);
                object[] parametersArray = new object[] { webRequestEventArgs };
                methodInfo.Invoke(clientContext, parametersArray);
            };

            return clonedClientContext;
        }
    }
}
