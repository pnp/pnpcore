using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Utilities
{
    /// <summary>
    /// Class MimeTypeMap.
    /// 
    /// Code copied from https://github.com/samuelneff/MimeTypeMap, created by @samuelneff 
    /// 
    /// </summary>
    internal static class MimeTypeMap
    {
        private const string dot = ".";
        private const string questionMark = "?";
        private static readonly Lazy<IDictionary<string, string>> mappings = new Lazy<IDictionary<string, string>>(BuildMappings);

        private static IDictionary<string, string> BuildMappings()
        {
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {

                // Mime list needed by PnP Core SDK
                // See https://github.com/samuelneff/MimeTypeMap/blob/master/MimeTypeMap.cs for the full list
                {".png", "image/png"},
                {".pnz", "image/png"},
                {".jpe", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".jpg", "image/jpeg"},
                {".gif", "image/gif"},
                {".bmp", "image/bmp"},
                {".dib", "image/bmp"},
                {".tif", "image/tiff"},
                {".tiff", "image/tiff"},
            };

            var cache = mappings.ToList(); // need ToList() to avoid modifying while still enumerating

            foreach (var mapping in cache)
            {
                if (!mappings.ContainsKey(mapping.Value))
                {
                    mappings.Add(mapping.Value, mapping.Key);
                }
            }

            return mappings;
        }

        /// <summary>
        /// Tries to get the type of the MIME from the provided string.
        /// </summary>
        /// <param name="str">The filename or extension.</param>
        /// <param name="mimeType">The variable to store the MIME type.</param>
        /// <returns>The MIME type.</returns>
        /// <exception cref="ArgumentNullException" />
        internal static bool TryGetMimeType(string str, out string mimeType)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            var indexQuestionMark = str.IndexOf(questionMark, StringComparison.Ordinal);
            if (indexQuestionMark != -1)
            {
                str = str.Remove(indexQuestionMark);
            }


            if (!str.StartsWith(dot))
            {
                var index = str.LastIndexOf(dot);
                if (index != -1 && str.Length > index + 1)
                {
                    str = str.Substring(index + 1);
                }

                str = dot + str;
            }

            return mappings.Value.TryGetValue(str, out mimeType);
        }

    }
}
