using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.SharePoint.Utilities;

namespace System.Net
{
    /// <summary>
    /// HttpWebRequest extension methods
    /// </summary>
    public static class HttpWebRequestExtensions
    {
        /// <summary>
        /// Grabs authenticaiton data from the passed client context and attaches that to the http request
        /// </summary>
        /// <param name="httpWebRequest">http request to update</param>
        /// <param name="cc">ClientContext object to grab authentication data from</param>
        public static void AddAuthenticationData(this HttpWebRequest httpWebRequest, ClientContext cc)
        {
            if (cc.Credentials != null)
            {
                // Copy credentials if set
                httpWebRequest.Credentials = cc.Credentials;
            }
            else
            {
                // If authentication happened via a cookie based approach (e.g. ADFS) then get the cookies that are currently linked to the context and reuse them
                httpWebRequest.CookieContainer = new CookieManager().GetCookies(cc);
            }
        }
    }
}
