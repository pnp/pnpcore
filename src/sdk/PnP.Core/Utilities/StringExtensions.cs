using System;
using System.IO;

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
            var newStream = new MemoryStream();
            using (var writer = new StreamWriter(newStream))
                writer.Write(source);
            return newStream;
        }
    }
}
