using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.SharePoint.Model;
using PnP.Core.Transformation.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.SharePoint.Extensions
{
    internal static class WebExtensions
    {
        internal static string GetLocalizedListName(this Web web, ListType listType, string defaultName)
        {
            var context = web.Context;

            int lcid = (int)web.EnsureProperty(w => w.Language);

            // TODO: Consider using caching

            string resourceName = null;
            string assemblyName = "core";
            switch (listType)
            {
                case ListType.Blogs:
                    resourceName = "$Resources:blogpost_Folder";
                    break;
                case ListType.SitePages:
                    resourceName = "$Resources:pages_Folder";
                    break;
                case ListType.PublishingPages:
                    resourceName = "$Resources:List_Pages_UrlName";
                    assemblyName = "osrvcore";
                    break;
                default:
                    break;
            }

            ClientResult<string> result = Microsoft.SharePoint.Client.Utilities.Utility.GetLocalizedString(context, resourceName, assemblyName, lcid);
            context.ExecuteQueryRetry();
            var listName = new Regex(@"['´`]").Replace(result.Value, "");

            return (string.IsNullOrEmpty(listName) ? defaultName : listName).ToLowerInvariant();
        }

        /// <summary>
        /// Checks if the current web is a sub site or not
        /// </summary>
        /// <param name="web">Web to check</param>
        /// <returns>True is sub site, false otherwise</returns>
        public static bool IsSubSite(this Web web)
        {
            if (web == null) throw new ArgumentNullException(nameof(web));

            var site = (web.Context as ClientContext).Site;
            var rootWeb = site.EnsureProperty(s => s.RootWeb);

            web.EnsureProperty(w => w.Id);
            rootWeb.EnsureProperty(w => w.Id);

            if (rootWeb.Id != web.Id)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a file as string
        /// </summary>
        /// <param name="web">The Web to process</param>
        /// <param name="serverRelativeUrl">The server relative URL to the file</param>
        /// <returns>The file contents as a string</returns>
        /// <remarks>#
        /// 
        ///     Based on https://github.com/SharePoint/PnP-Sites-Core/blob/master/Core/OfficeDevPnP.Core/Extensions/FileFolderExtensions.cs
        ///     Modified to force onpremises support
        ///     
        /// </remarks>
        public static string GetFileByServerRelativeUrlAsString(this Web web, string serverRelativeUrl)
        {
            var file = web.GetFileByServerRelativeUrl(serverRelativeUrl);
            var context = web.Context;
            context.Load(file);
            context.ExecuteQueryRetry();

            Stream sourceStream;
            ClientResult<Stream> stream = file.OpenBinaryStream();
            web.Context.ExecuteQueryRetry();
            sourceStream = stream.Value;

            string returnString = string.Empty;

            using (Stream memStream = new MemoryStream())
            {
                sourceStream.CopyTo(memStream);
                memStream.Position = 0;

                using (var reader = new StreamReader(memStream))
                {
                    returnString = reader.ReadToEnd();
                }
            }

            return returnString;
        }

        /// <summary>
        /// Returns a file as string
        /// </summary>
        /// <param name="web">The Web to process</param>
        /// <param name="serverRelativeUrl">The server relative URL to the file</param>
        /// <returns>The file contents as a string</returns>
        public static string GetFileAsString(this Web web, string serverRelativeUrl)
        {
            return Task.Run(() => web.GetFileAsStringImplementation(serverRelativeUrl)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Returns a file as string
        /// </summary>
        /// <param name="web">The Web to process</param>
        /// <param name="serverRelativeUrl">The server relative URL to the file</param>
        /// <returns>The file contents as a string</returns>
        public static async Task<string> GetFileAsStringAsync(this Web web, string serverRelativeUrl)
        {
            return await web.GetFileAsStringImplementation(serverRelativeUrl).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a file as string
        /// </summary>
        /// <param name="web">The Web to process</param>
        /// <param name="serverRelativeUrl">The server relative URL to the file</param>
        /// <returns>The file contents as a string</returns>
        private static async Task<string> GetFileAsStringImplementation(this Web web, string serverRelativeUrl)
        {
            var file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(serverRelativeUrl));
            web.Context.Load(file);
            await web.Context.ExecuteQueryRetryAsync().ConfigureAwait(false);
            ClientResult<Stream> stream = file.OpenBinaryStream();
            await web.Context.ExecuteQueryRetryAsync().ConfigureAwait(false);

            string returnString = string.Empty;
            using (Stream memStream = new MemoryStream())
            {
                stream.Value.CopyTo(memStream);
                memStream.Position = 0;
                using (var reader = new StreamReader(memStream))
                {
                    returnString = reader.ReadToEnd();
                }
            }

            return returnString;
        }
    }

    /// <summary>
    /// Defines the supported types of lists and libraries
    /// </summary>
    internal enum ListType
    {
        /// <summary>
        /// List of blogs
        /// </summary>
        Blogs,
        /// <summary>
        /// List of site pages
        /// </summary>
        SitePages,
        /// <summary>
        /// List of pages
        /// </summary>
        PublishingPages,
    }
}
