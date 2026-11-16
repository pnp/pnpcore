using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PnP.Core.Modernization.Transform;
using System;
using System.Net;

namespace PnP.Core.Modernization.Functions
{
    /// <summary>
    /// Base class for all function libraries
    /// </summary>
    public abstract class FunctionsBase: BaseTransform
    {
        protected ClientContext clientContext;

        #region Construction
        /// <summary>
        /// Instantiates a function library class
        /// </summary>
        /// <param name="clientContext">ClientContext object for the site holding the page being transformed</param>
        public FunctionsBase(ClientContext clientContext)
        {
            this.clientContext = clientContext;
        }
        #endregion

        protected static string EncodeWebPartMappingJson(JObject data)
        {
            // Protect literal tokens and HTML entities until ContentTransformator's final HTML decode.
            return WebUtility.HtmlEncode(data.ToString(Formatting.None)).Replace("{", "&#123;").Replace("}", "&#125;");
        }

        protected static bool IsHttpUrl(Uri uri)
        {
            return uri.IsAbsoluteUri && !string.IsNullOrEmpty(uri.Host) &&
                string.IsNullOrEmpty(uri.UserInfo) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
