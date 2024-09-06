using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PnP.Core
{
    internal static class StringExtensions
    {
        internal const char NonWidthWhiteSpace = (char)0x200B; //Use zero-width space marker to capture empty string

        internal static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        internal static Stream AsStream(this string source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(source));
        }

#if NET5_0_OR_GREATER        
        internal static string FirstCharToLower(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToLower(), input.AsSpan(1))
            };
#else
        internal static string FirstCharToLower(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input[0].ToString().ToLower() + input.Substring(1)
            };
#endif

        /// <summary>
        /// Used to alter a SharePoint eTag for Graph
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static string AsGraphEtag(this string source)
        {
            return source?.Replace("{", "").Replace("}", "").Replace("\"", "").Split(',').FirstOrDefault();
        }

        #region Code copied from https://github.com/ServiceStack/ServiceStack.Text/blob/master/src/ServiceStack.Text/StringSpanExtensions.cs
        internal static bool IsNullOrEmpty(this ReadOnlySpan<char> value) => value.IsEmpty || (value.Length == 1 && value[0] == NonWidthWhiteSpace);

        internal static ReadOnlySpan<char> LeftPart(this ReadOnlySpan<char> strVal, char needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Slice(0, pos);
        }

        internal static ReadOnlySpan<char> RightPart(this ReadOnlySpan<char> strVal, char needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Slice(pos + 1);
        }
        #endregion

        #region Code copied from https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm

        internal static LineSplitEnumerator SplitLines(this string str)
        {
            // LineSplitEnumerator is a struct so there is no allocation here
            return new LineSplitEnumerator(str.AsSpan());
        }

        // Must be a ref struct as it contains a ReadOnlySpan<char>
        internal ref struct LineSplitEnumerator
        {
            private ReadOnlySpan<char> _str;

            public LineSplitEnumerator(ReadOnlySpan<char> str)
            {
                _str = str;
                Current = default;
            }

            // Needed to be compatible with the foreach operator
            public LineSplitEnumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                var span = _str;
                if (span.Length == 0) // Reach the end of the string
                    return false;

                var index = span.IndexOfAny('\r', '\n');
                if (index == -1) // The string is composed of only one line
                {
                    _str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                    Current = new LineSplitEntry(span, ReadOnlySpan<char>.Empty);
                    return true;
                }

                if (index < span.Length - 1 && span[index] == '\r')
                {
                    // Try to consume the '\n' associated to the '\r'
                    var next = span[index + 1];
                    if (next == '\n')
                    {
                        Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 2));
                        _str = span.Slice(index + 2);
                        return true;
                    }
                }

                Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 1));
                _str = span.Slice(index + 1);
                return true;
            }

            public LineSplitEntry Current { get; private set; }
        }

        internal readonly ref struct LineSplitEntry
        {
            public LineSplitEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
            {
                Line = line;
                Separator = separator;
            }

            public ReadOnlySpan<char> Line { get; }
            public ReadOnlySpan<char> Separator { get; }

            // This method allow to deconstruct the type, so you can write any of the following code
            // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
            // foreach (var (line, endOfLine) in str.SplitLines()) { _ = line; }
            // https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
            public void Deconstruct(out ReadOnlySpan<char> line, out ReadOnlySpan<char> separator)
            {
                line = Line;
                separator = Separator;
            }

            // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
            // foreach (ReadOnlySpan<char> entry in str.SplitLines())
            public static implicit operator ReadOnlySpan<char>(LineSplitEntry entry) => entry.Line;
        }

        #endregion
    }
}
