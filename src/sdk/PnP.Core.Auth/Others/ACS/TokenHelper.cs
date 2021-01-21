using PnP.Core.Auth.ACS.OAuth;
using PnP.Core.Auth.Services.Builder.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Auth.ACS
{
    internal class TokenHelper
    {
        #region public fields

        /// <summary>
        /// SharePoint principal.
        /// </summary>
        public const string SharePointPrincipal = "00000003-0000-0ff1-ce00-000000000000";

        #endregion public fields

        #region private fields

        //
        // Configuration Constants
        //

        private const string AcsMetadataEndPointRelativeUrl = "metadata/json/1";
        private const string S2SProtocol = "OAuth2";
        private PnPCoreAuthenticationACSOptions Options;
        private string ClientId { get; }

        public TokenHelper(string clientId, PnPCoreAuthenticationACSOptions options)
        {
            this.Options = options;
            this.ClientId = clientId;
        }

        #endregion

        public async static Task<string> GetRealmFromTargetUrl(Uri targetApplicationUri)
        {
            WebRequest request = WebRequest.Create(targetApplicationUri.ToString().TrimEnd(new[] { '/' }) + "/_vti_bin/client.svc");
            request.Headers.Add("Authorization: Bearer ");

            try
            {
                using (await request.GetResponseAsync().ConfigureAwait(false))
                {
                }
            }
            catch (WebException e)
            {
                if (e.Response == null)
                {
                    return null;
                }

                string bearerResponseHeader = e.Response.Headers["WWW-Authenticate"];
                if (string.IsNullOrEmpty(bearerResponseHeader))
                {
                    return null;
                }

                const string bearer = "Bearer realm=\"";
                int bearerIndex = bearerResponseHeader.IndexOf(bearer, StringComparison.Ordinal);
                if (bearerIndex < 0)
                {
                    return null;
                }

                int realmIndex = bearerIndex + bearer.Length;

                if (bearerResponseHeader.Length >= realmIndex + 36)
                {
                    string targetRealm = bearerResponseHeader.Substring(realmIndex, 36);

                    Guid realmGuid;

                    if (Guid.TryParse(targetRealm, out realmGuid))
                    {
                        return targetRealm;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves an app-only access token from ACS to call the specified principal
        /// at the specified targetHost. The targetHost must be registered for target principal.  If specified realm is
        /// null, the "Realm" setting in web.config will be used instead.
        /// </summary>
        /// <param name="targetPrincipalName">Name of the target principal to retrieve an access token for</param>
        /// <param name="targetHost">Url authority of the target principal</param>
        /// <param name="targetRealm">Realm to use for the access token's nameid and audience</param>
        /// <returns>An access token with an audience of the target principal</returns>
        public async Task<OAuth2AccessTokenResponse> GetAppOnlyAccessToken(
            string targetPrincipalName,
            string targetHost,
            string targetRealm = null)
        {
            targetRealm ??= this.Options.Realm;

            string resource = GetFormattedPrincipal(targetPrincipalName, targetHost, targetRealm);
            string clientIdPrincipal = GetFormattedPrincipal(this.ClientId, this.Options.HostedAppHostName, targetRealm);

            var oauth2Request = OAuth2MessageFactory.CreateAccessTokenRequestWithClientCredentials(clientIdPrincipal, this.Options.ClientSecret, resource);
            oauth2Request.Resource = resource;

            try
            {
                // Get token
                var client = new OAuth2S2SClient();
                return await
                    client.Issue(GetStsUrl(targetRealm), oauth2Request).ConfigureAwait(false) as OAuth2AccessTokenResponse;
            }
            catch (WebException wex) when (wex.Response != null)
            {
                using (var sr = new StreamReader(wex.Response.GetResponseStream()))
                {
                    string responseText = sr.ReadToEnd();
                    throw new WebException(wex.Message + " - " + responseText, wex);
                }
            }
        }

        #region AcsMetadataParser

        private JsonMetadataDocument GetMetadataDocument(string realm)
        {
            string acsMetadataEndpointUrlWithRealm = String.Format(CultureInfo.InvariantCulture, "{0}?realm={1}",
                                                                   GetAcsMetadataEndpointUrl(),
                                                                   realm);
            byte[] acsMetadata;
            using (WebClient webClient = new WebClient())
            {

                acsMetadata = webClient.DownloadData(acsMetadataEndpointUrlWithRealm);
            }
            string jsonResponseString = Encoding.UTF8.GetString(acsMetadata);

            JsonMetadataDocument document = JsonSerializer.Deserialize<JsonMetadataDocument>(jsonResponseString);

            if (null == document)
            {
                throw new Exception("No metadata document found at the global endpoint " + acsMetadataEndpointUrlWithRealm);
            }

            return document;
        }

        public string GetStsUrl(string realm)
        {
            JsonMetadataDocument document = GetMetadataDocument(realm);

            JsonEndpoint s2sEndpoint = document.endpoints.SingleOrDefault(e => e.protocol == S2SProtocol);

            if (null != s2sEndpoint)
            {
                return s2sEndpoint.location;
            }

            throw new Exception("Metadata document does not contain STS endpoint URL");
        }

        private class JsonMetadataDocument
        {
            public string serviceName { get; set; }
            public List<JsonEndpoint> endpoints { get; set; }
            public List<JsonKey> keys { get; set; }
        }

        private class JsonEndpoint
        {
            public string location { get; set; }
            public string protocol { get; set; }
            public string usage { get; set; }
        }

        private class JsonKeyValue
        {
            public string type { get; set; }
            public string value { get; set; }
        }

        private class JsonKey
        {
            public string usage { get; set; }
            public JsonKeyValue keyValue { get; set; }
        }
        #endregion


        #region private methods
        private string GetAcsMetadataEndpointUrl()
        {
            return Path.Combine(GetAcsGlobalEndpointUrl(), AcsMetadataEndPointRelativeUrl);
        }

        private string GetFormattedPrincipal(string principalName, string hostName, string realm)
        {
            if (!String.IsNullOrEmpty(hostName))
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}/{1}@{2}", principalName, hostName, realm);
            }

            return String.Format(CultureInfo.InvariantCulture, "{0}@{1}", principalName, realm);
        }


        private string GetAcsGlobalEndpointUrl()
        {
            if (this.Options.GlobalEndPointPrefix.Length == 0)
            {
                return String.Format(CultureInfo.InvariantCulture, "https://{0}/", this.Options.AcsHostUrl);
            }
            else
            {
                return String.Format(CultureInfo.InvariantCulture, "https://{0}.{1}/", this.Options.GlobalEndPointPrefix, this.Options.AcsHostUrl);
            }
        }
        #endregion
    }
}
