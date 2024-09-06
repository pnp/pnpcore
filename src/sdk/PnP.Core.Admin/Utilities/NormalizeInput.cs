using System.Text.RegularExpressions;

namespace PnP.Core.Admin.Utilities
{
    internal static class NormalizeInput
    {
        internal static string RemoveUnallowedCharacters(string str)
        {
            const string unallowedCharacters = "[&,!@;:#¤`´~¨='%<>/\\\\\"\\$\\*\\^\\+\\|\\{\\}\\[\\]\\(\\)\\?\\s]";
            var regex = new Regex(unallowedCharacters);
            return regex.Replace(str, "");
        }

        internal static string ReplaceAccentedCharactersWithLatin(string str)
        {
            const string a = "[äåàáâãæ]";
            var regex = new Regex(a, RegexOptions.IgnoreCase);
            str = regex.Replace(str, "a");

            const string e = "[èéêëēĕėęě]";
            regex = new Regex(e, RegexOptions.IgnoreCase);
            str = regex.Replace(str, "e");

            const string i = "[ìíîïĩīĭįı]";
            regex = new Regex(i, RegexOptions.IgnoreCase);
            str = regex.Replace(str, "i");

            const string o = "[öòóôõø]";
            regex = new Regex(o, RegexOptions.IgnoreCase);
            str = regex.Replace(str, "o");

            const string u = "[üùúû]";
            regex = new Regex(u, RegexOptions.IgnoreCase);
            str = regex.Replace(str, "u");

            const string c = "[çċčćĉ]";
            regex = new Regex(c, RegexOptions.IgnoreCase);
            str = regex.Replace(str, "c");

            const string d = "[ðďđđ]";
            regex = new Regex(d, RegexOptions.IgnoreCase);
            str = regex.Replace(str, "d");

            return str;
        }
    }
}
