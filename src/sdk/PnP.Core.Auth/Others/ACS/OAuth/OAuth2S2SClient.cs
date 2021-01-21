using System.Threading.Tasks;

namespace PnP.Core.Auth.ACS.OAuth
{
    public class OAuth2S2SClient
    {
        public async Task<OAuth2Message> Issue(string securityTokenServiceUrl, OAuth2AccessTokenRequest oauth2Request)
        {
            var oAuth2WebRequest = new OAuth2WebRequest(securityTokenServiceUrl, oauth2Request);
            try
            {
                System.Net.WebResponse response = await oAuth2WebRequest.GetResponseAsync().ConfigureAwait(false);
                using var rs = new System.IO.StreamReader(response.GetResponseStream());
                return OAuth2MessageFactory.CreateFromEncodedResponse(rs);
            }
            catch (System.Exception innerException)
            {
                throw new System.Exception("Token request failed.", innerException);
            }
        }
    }
}
