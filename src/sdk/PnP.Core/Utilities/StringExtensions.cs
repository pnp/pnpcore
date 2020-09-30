using System;
using System.IO;
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
    }
}
