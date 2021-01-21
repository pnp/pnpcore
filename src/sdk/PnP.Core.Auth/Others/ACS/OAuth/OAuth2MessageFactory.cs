namespace PnP.Core.Auth.ACS.OAuth
{
    public static class OAuth2MessageFactory
    {
        public static OAuth2Message CreateFromEncodedResponse(System.IO.StreamReader reader)
        {
            return OAuth2MessageFactory.CreateFromEncodedResponse(reader.ReadToEnd());
        }

        public static OAuth2Message CreateFromEncodedResponse(string responseString)
        {
            if (responseString.StartsWith("{\"error"))
            {
                return OAuth2ErrorResponse.CreateFromEncodedResponse(responseString);
            }
            return OAuth2AccessTokenResponse.Read(responseString);
        }

        public static OAuth2AccessTokenRequest CreateAccessTokenRequestWithClientCredentials(string clientId, string clientSecret, string scope)
        {
            return new OAuth2AccessTokenRequest
            {
                GrantType = "client_credentials",
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scope
            };
        }
    }
}
