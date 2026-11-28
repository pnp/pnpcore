

using System;
using System.IO;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Extensions
{
    /// <summary>
    /// File-content helpers for SharePoint Online modernization.
    /// </summary>
    public static class FileFolderExtensions
    {
        /// <summary>
        /// Reads a SharePoint file as text, detecting a byte-order mark when present.
        /// </summary>
        public static string GetFileByServerRelativeUrlAsString(this IWeb web, string serverRelativeUrl) =>
            GetFileByServerRelativeUrlAsStringAsync(web, serverRelativeUrl).GetAwaiter().GetResult();

        /// <summary>
        /// Reads a SharePoint file through the Core REST pipeline.
        /// </summary>
        public static async Task<string> GetFileByServerRelativeUrlAsStringAsync(this IWeb web, string serverRelativeUrl)
        {
            if (web == null)
            {
                throw new ArgumentNullException(nameof(web));
            }
            SharePointOnlineSupport.Validate(web.PnPContext);
            var file = await web.GetFileByServerRelativeUrlAsync(serverRelativeUrl).ConfigureAwait(false);
            using (var stream = await file.GetContentAsync().ConfigureAwait(false))
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}
