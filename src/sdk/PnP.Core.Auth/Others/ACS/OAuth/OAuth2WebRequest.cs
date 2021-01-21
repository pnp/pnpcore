using System.Threading.Tasks;

namespace PnP.Core.Auth.ACS.OAuth
{
    public class OAuth2WebRequest : System.Net.WebRequest
    {
        private static readonly System.TimeSpan DefaultTimeout = System.TimeSpan.FromMinutes(10.0);

        private readonly System.Net.WebRequest _innerRequest;

        private readonly OAuth2AccessTokenRequest _request;

        public OAuth2WebRequest(string requestUriString, OAuth2AccessTokenRequest request)
        {
            this._innerRequest = System.Net.WebRequest.Create(requestUriString);
            this._request = request;
        }

        public override async Task<System.Net.WebResponse> GetResponseAsync()
        {
            string text = this._request.ToString();
            this._innerRequest.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
            this._innerRequest.ContentLength = text.Length;
            this._innerRequest.ContentType = "application/x-www-form-urlencoded";
            this._innerRequest.Method = "POST";
            this._innerRequest.Timeout = (int)OAuth2WebRequest.DefaultTimeout.TotalMilliseconds;
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(this._innerRequest.GetRequestStream(), System.Text.Encoding.ASCII);
            streamWriter.Write(text);
            streamWriter.Close();
            return await _innerRequest.GetResponseAsync().ConfigureAwait(false);
        }
    }
}
