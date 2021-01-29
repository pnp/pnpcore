using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PnP.Core
{
    internal static class StringExtensions
    {
        internal static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        internal static Stream AsStream(this string source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(source));
        }

        /// <summary>
        /// Used to alter a SharePoint eTag for Graph
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static string AsGraphEtag(this string source)
        {
            return source?.Replace("{", "").Replace("}", "").Replace("\"", "").Split(',').FirstOrDefault();
        }

    }
}
