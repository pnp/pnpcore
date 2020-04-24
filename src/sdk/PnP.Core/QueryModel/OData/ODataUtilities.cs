using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PnP.Core.QueryModel.OData
{
    /// <summary>
    /// Utility class
    /// </summary>
    internal static class ODataUtilities
    {
        /// <summary>
        /// Convert a value to a valid string formatted for OData. String values are quoted and escaped
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertToString(object value)
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
                    return $"guid('{g.ToString()}')";
                default:
                    // Convert to invariant string
                    return String.Format(CultureInfo.InvariantCulture, "{0}", value);
            }
        }
    }
}
