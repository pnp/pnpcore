using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace PnP.Core
{
    /// <summary>
    /// ToString() on NameValueCollection is differently implemented for .NET Framework causing encoding issues. To work around
    /// this is a custom ToString() method which ensures the .NET Core/.NET 5 logic is always followed
    /// See https://github.com/aspnet/AspNetWebStack/blob/42991b3d2537b702736463f76a10a4fcf2ea44c9/src/System.Net.Http.Formatting/Internal/HttpValueCollection.cs for
    /// the original source code
    /// </summary>
    internal static class NameValueCollectionExtensions
    {
        internal static string ToEncodedString(this NameValueCollection collection)
        {
            return ToString(collection, true);
        }

        private static string ToString(NameValueCollection collection, bool urlEncode)
        {
            if (collection.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            bool first = true;
            foreach (string name in collection)
            {
                string[] values = GetValues(collection, name);
                if (values == null || values.Length == 0)
                {
                    first = AppendNameValuePair(builder, first, urlEncode, name, string.Empty);
                }
                else
                {
                    foreach (string value in values)
                    {
                        first = AppendNameValuePair(builder, first, urlEncode, name, value);
                    }
                }
            }

            return builder.ToString();
        }

        internal static string[] GetValues(NameValueCollection collection, string name)
        {
            name ??= string.Empty;

            if (!collection.AllKeys.Contains(name))
            {
                return null;
            }

            return GetValuesInternal(collection, name).ToArray();
        }

        private static List<string> GetValuesInternal(NameValueCollection collection, string name)
        {
            List<string> values = new List<string>();

            for (int i = 0; i < collection.Count; i++)
            {
                if (string.Equals(collection.GetKey(i), name, StringComparison.OrdinalIgnoreCase))
                {
                    values.AddRange(collection.GetValues(i));
                }
            }

            return values;
        }

        private static bool AppendNameValuePair(StringBuilder builder, bool first, bool urlEncode, string name, string value)
        {
            string effectiveName = name ?? string.Empty;
            string encodedName = urlEncode ? HttpUtility.UrlEncode(effectiveName) : effectiveName;

            string effectiveValue = value ?? string.Empty;
            string encodedValue = urlEncode ? HttpUtility.UrlEncode(effectiveValue) : effectiveValue;

            if (first)
            {
                first = false;
            }
            else
            {
                _ = builder.Append('&');
            }

            _ = builder.Append(encodedName.Replace("%24", "$"));
            if (!string.IsNullOrEmpty(encodedValue))
            {
                _ = builder.Append('=');
                _ = builder.Append(encodedValue);
            }
            return first;
        }

    }
}
