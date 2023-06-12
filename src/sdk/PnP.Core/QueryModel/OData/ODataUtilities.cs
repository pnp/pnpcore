using System;
using System.Globalization;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Utility class
    /// </summary>
    internal static class ODataUtilities
    {
        /// <summary>
        /// Convert a value to a valid string formatted for OData. String values are quoted and escaped
        /// </summary>
        /// <param name="value">Value to translate into an OData filter string</param>
        /// <returns></returns>
        public static string ConvertToString(object value)
        {
            return ConvertToString(value, ODataTargetPlatform.SPORest);
        }

        /// <summary>
        /// Convert a value to a valid string formatted for OData. String values are quoted and escaped
        /// </summary>
        /// <param name="value">Value to translate into an OData filter string</param>
        /// <param name="targetPlatform">Indicates whether we're building a SPORest or Graph query</param>
        /// <returns></returns>
        public static string ConvertToString(object value, ODataTargetPlatform targetPlatform)
        {
            switch (value)
            {
                case string s:
                    // Escape with double quote
                    s = s.Replace("'", "''");
                    return $"'{s}'";
                case bool b:
                    return b ? "true" : "false";
                case Guid g:
                    return targetPlatform == ODataTargetPlatform.SPORest ? $"(guid'{g}')" : $"{g}";
                case DateTime dt:
                    return targetPlatform == ODataTargetPlatform.SPORest ? $"datetime'{dt.ToUniversalTime():yyyy-MM-ddThh:mm:ssZ}'" : $"{dt.ToUniversalTime():yyyy-MM-ddThh:mm:ssZ}";
                case DateTimeOffset dto:
                    return targetPlatform == ODataTargetPlatform.SPORest ? $"datetime'{dto.UtcDateTime:yyyy-MM-ddThh:mm:ssZ}'" : $"{dto.UtcDateTime:yyyy-MM-ddThh:mm:ssZ}";
                default:
                    // Convert to invariant string
                    return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            }
        }
    }
}
