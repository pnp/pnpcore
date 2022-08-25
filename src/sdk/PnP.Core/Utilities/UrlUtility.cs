using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace PnP.Core
{
    // Taken and slightly adapted from https://raw.githubusercontent.com/pnp/PnP-Sites-Core/master/Core/OfficeDevPnP.Core/Utilities/UrlUtility.cs
    /// <summary>
    /// Static methods to modify URL paths.
    /// </summary>
    internal static class UrlUtility
    {
        const char PATH_DELIMITER = '/';

        /// <summary>
        /// Combines a path and a relative path.
        /// </summary>
        /// <param name="path">A SharePoint URL</param>
        /// <param name="relativePaths">SharePoint relative URLs</param>
        /// <returns>Returns combined path with a relative paths</returns>
        internal static Uri Combine(Uri path, params string[] relativePaths)
        {
            return Combine(path?.ToString(), relativePaths);
        }

        /// <summary>
        /// Combines a path and a relative path.
        /// </summary>
        /// <param name="path">A SharePoint URL</param>
        /// <param name="relativePaths">SharePoint relative URLs</param>
        /// <returns>Returns combined path with a relative paths</returns>
        internal static Uri Combine(string path, params string[] relativePaths)
        {
            string pathBuilder = path ?? string.Empty;

            if (relativePaths == null)
                return new Uri(pathBuilder);

            foreach (string relPath in relativePaths)
            {
                pathBuilder = CombineInternal(pathBuilder, relPath);
            }
            return new Uri(pathBuilder);
        }

        /// <summary>
        /// Combines a path and a relative path.
        /// </summary>
        /// <param name="path">A SharePoint URL</param>
        /// <param name="relative">SharePoint relative URL</param>
        /// <returns>Returns comibed path with a relative path</returns>
        internal static Uri Combine(string path, string relative)
        {
            return new Uri(CombineInternal(path, relative));
        }

        /// <summary>
        /// Combines a path and a relative path.
        /// </summary>
        /// <param name="path">A SharePoint URL</param>
        /// <param name="relative">SharePoint relative URL</param>
        /// <returns>Returns combined path with a relative path</returns>
        internal static string CombineAsString(string path, string relative)
        {
            return CombineInternal(path, relative);
        }

        private static string CombineInternal(string path, string relative)
        {
            if (relative == null)
                relative = string.Empty;

            if (path == null)
                path = string.Empty;

            if (relative.Length == 0 && path.Length == 0)
                return string.Empty;

            if (relative.Length == 0)
                return path;

            if (path.Length == 0)
                return relative;

            path = path.Replace('\\', PATH_DELIMITER);
            relative = relative.Replace('\\', PATH_DELIMITER);

            return path.TrimEnd(PATH_DELIMITER) + PATH_DELIMITER + relative.TrimStart(PATH_DELIMITER);
        }

        /// <summary>
        /// Returns absolute URL of a resource located in a SharePoint site.
        /// </summary>
        /// <param name="webUrl">The URL of a SharePoint site (Web).</param>
        /// <param name="serverRelativeUrl">Any server relative URL of a resource.</param>
        /// <returns></returns>
        internal static Uri MakeAbsoluteUrl(Uri webUrl, string serverRelativeUrl)
        {
            if (null == webUrl) return null;

            string serverUrl = $"{webUrl.Scheme}://{webUrl.Authority}";
            return new Uri(CombineInternal(serverUrl, serverRelativeUrl));
        }

        /// <summary>
        /// Ensure the absolute URL from a specified resource URL
        /// </summary>
        /// <param name="webUrl">The URL of a SharePoint site (Web).</param>
        /// <param name="resourceUrl">The absolute or server relative URL of a resource.</param>
        /// <param name="checkIfWebContainedResource">Indicates if the resource URL must belong to the specified web (default = false)</param>
        /// <returns>The absolute URL of the specified resource.</returns>
        internal static Uri EnsureAbsoluteUrl(Uri webUrl, string resourceUrl, bool checkIfWebContainedResource = false)
        {
            if (null == resourceUrl) throw new ArgumentNullException(nameof(resourceUrl));
            if (null == webUrl) throw new ArgumentNullException(nameof(webUrl));

            if (resourceUrl.StartsWith("https://"))
            {
                if (checkIfWebContainedResource && !resourceUrl.StartsWith(webUrl.ToString()))
                    throw new ArgumentException(string.Format(PnPCoreResources.Exception_InvalidSPOResource, nameof(resourceUrl), webUrl));

                return new Uri(resourceUrl);
            }

            return MakeAbsoluteUrl(webUrl, resourceUrl);
        }

        /// <summary>
        /// Checks wether the resource absolute or relative URL is located in specified site (Web).
        /// </summary>
        /// <param name="webUrl">The URL of the SharePoint site (Web).</param>
        /// <param name="resourceUrl">The absolute or relative URL of a resource.</param>
        /// <returns><c>true</c> if the resource is in the same site, <c>false</c> otherwise</returns>
        internal static bool IsSameSite(Uri webUrl, string resourceUrl)
        {
            if (null == webUrl) throw new ArgumentNullException(nameof(webUrl));
            if (string.IsNullOrEmpty(resourceUrl)) throw new ArgumentNullException(nameof(resourceUrl));

            string resourceAbsoluteUrl = EnsureAbsoluteUrl(webUrl, resourceUrl).ToString();
            return resourceAbsoluteUrl.ToLower().StartsWith(webUrl.ToString().ToLower());
        }


        /// <summary>
        /// Ensures that there is a trailing slash at the end of the URL.
        /// </summary>
        /// <param name="urlToProcess">The URL to ensure to have a trailing slash.</param>
        /// <returns>The ensured trailing slash URL.</returns>
        internal static string EnsureTrailingSlash(string urlToProcess)
        {
            if (null != urlToProcess && !urlToProcess.EndsWith("/"))
            {
                return urlToProcess + "/";
            }

            return urlToProcess;
        }

        /// <summary>
        /// Ensures that there is a trailing slash at the end of the URL.
        /// </summary>
        /// <param name="uri">The URL to ensure to have a trailing slash.</param>
        /// <returns>The ensured trailing slash URI.</returns>
        internal static Uri EnsureTrailingSlash(this Uri uri)
        {
            return new Uri(EnsureTrailingSlash(uri?.ToString()));
        }

        /// <summary>
        /// Combines provided url parameters with an url that has url parameters as wel
        /// </summary>
        /// <param name="baseRelativeUri">Base relative url (with url parameters)</param>
        /// <param name="urlParameters">Url parameters to combine</param>
        /// <returns>Relative url with combined url parameters</returns>
        internal static string CombineRelativeUrlWithUrlParameters(string baseRelativeUri, string urlParameters)
        {
            var uriBuilder = new UriBuilder(baseRelativeUri);
            // Input url parameters
            NameValueCollection main = HttpUtility.ParseQueryString(uriBuilder.Query.ToLowerInvariant());
            NameValueCollection parameters = HttpUtility.ParseQueryString(urlParameters);
            // Collection of merged parameters
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

            List<string> processedParameters = new List<string>();

            // Iterate the parameters on the base uri
            foreach (string mainKey in main.Keys)
            {
                if (string.IsNullOrWhiteSpace(mainKey)) continue;

                string[] originalValues = main.GetValues(mainKey)[0].Split(new char[] { ',' });
                string[] newValues = null;
                if (originalValues == null) continue;

                processedParameters.Add(mainKey);

                // do we have the same url parameter in provided url params?
                string value = parameters[mainKey];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    newValues = value.Split(new char[] { ',' });
                }

                string[] combinedValues;
                if (newValues != null)
                {
                    combinedValues = originalValues.Union(newValues, StringComparer.OrdinalIgnoreCase).ToArray();
                }
                else
                {
                    combinedValues = originalValues;
                }

                queryString.Add(mainKey, string.Join(",", combinedValues));
            }

            // Process the provided parameters which are not present in the base uri parameters
            foreach (string parameterKey in parameters.Keys)
            {
                if (string.IsNullOrWhiteSpace(parameterKey)) continue;

                string[] originalValues = parameters.GetValues(parameterKey);
                if (originalValues == null) continue;

                if (processedParameters.FindIndex(x => x.Equals(parameterKey, StringComparison.OrdinalIgnoreCase)) != -1) continue;

                queryString.Add(parameterKey, string.Join(",", originalValues));
            }

            return $"{uriBuilder.Host}{(uriBuilder.Path.Replace("%7B", "{").Replace("%7D", "}"))}{(queryString.Count > 0 ? "?" : "")}{queryString.ToEncodedString().Replace("%2c", ",")}";
        }

        #region NOT USED NOW
        // NOT Used for now, not covered by unit tests
        //const string INVALID_CHARS_REGEX = @"[\\#%*/:<>?+|\""]";
        //const string REGEX_INVALID_FILEFOLDER_NAME_CHARS = @"[""#%*:<>?/\|\t\r\n]";
        ///// <summary>
        ///// Returns relative URL of given URL
        ///// </summary>
        ///// <param name="urlToProcess">SharePoint URL to process</param>
        ///// <returns>Returns realtive URL of given URL</returns>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:Uri return values should not be strings", Justification = "<Pending>")]
        //public static string MakeRelativeUrl(string urlToProcess)
        //{
        //    Uri uri = new Uri(urlToProcess);
        //    return uri.AbsolutePath;
        //}

        ///// <summary>
        ///// Adds query string parameters to the end of a querystring and guarantees the proper concatenation with <b>?</b> and <b>&amp;.</b>
        ///// </summary>
        ///// <param name="path">A SharePoint URL</param>
        ///// <param name="queryString">Query string value that need to append to the URL</param>
        ///// <returns>Returns URL along with appended query string</returns>
        //public static string AppendQueryString(string path, string queryString)
        //{
        //    string url = path;

        //    if (null != path && queryString != null && queryString.Length > 0)
        //    {
        //        char startChar = (path.IndexOf("?") > 0) ? '&' : '?';
        //        url = string.Concat(path, startChar, queryString.TrimStart('?'));
        //    }
        //    return url;
        //}

        ///// <summary>
        ///// Checks if URL contains invalid characters or not
        ///// </summary>
        ///// <param name="content">Url value</param>
        ///// <returns>Returns true if URL contains invalid characters. Otherwise returns false.</returns>
        //public static bool ContainsInvalidUrlChars(this string content)
        //{
        //    return Regex.IsMatch(content, INVALID_CHARS_REGEX);
        //}

        ///// <summary>
        ///// Checks if file or folder contains invalid characters or not
        ///// </summary>
        ///// <param name="content">File or folder name to check</param>
        ///// <returns>True if contains invalid chars, false otherwise</returns>
        //public static bool ContainsInvalidFileFolderChars(this string content)
        //{
        //    return Regex.IsMatch(content, REGEX_INVALID_FILEFOLDER_NAME_CHARS);
        //}

        ///// <summary>
        ///// Removes invalid characters
        ///// </summary>
        ///// <param name="content">Url value</param>
        ///// <returns>Returns URL without invalid characters</returns>
        //public static Uri StripInvalidUrlChars(this string content)
        //{
        //    return ReplaceInvalidUrlChars(content, "");
        //}

        ///// <summary>
        ///// Replaces invalid charcters with other characters
        ///// </summary>
        ///// <param name="content">Url value</param>
        ///// <param name="replacer">string need to replace with invalid characters</param>
        ///// <returns>Returns replaced invalid charcters from URL</returns>
        //public static Uri ReplaceInvalidUrlChars(this string content, string replacer)
        //{
        //    return new Uri(new Regex(INVALID_CHARS_REGEX).Replace(content, replacer));
        //}
        #endregion
    }
}
