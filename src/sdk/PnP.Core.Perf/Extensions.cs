using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Perf
{
    public static class Extensions
    {
        public const char NonWidthWhiteSpace = (char)0x200B; //Use zero-width space marker to capture empty string

        public static bool IsNullOrEmpty(this ReadOnlySpan<char> value) => value.IsEmpty || (value.Length == 1 && value[0] == NonWidthWhiteSpace);

        public static bool IsNullOrEmpty(this Span<char> value) => value.IsEmpty || (value.Length == 1 && value[0] == NonWidthWhiteSpace);

        public static ReadOnlySpan<char> RightPart(this ReadOnlySpan<char> strVal, char needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Slice(pos + 1);
        }

        public static ReadOnlySpan<char> LastRightPart(this ReadOnlySpan<char> strVal, char needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.LastIndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Slice(pos + 1);
        }
    }
}
