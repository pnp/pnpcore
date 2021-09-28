using PnP.Core.Transformation.SharePoint.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Formats a string that has the format ThisIsAClassName and formats in a friendly way
        /// </summary>
        /// <param name="value">string value</param>
        /// <returns>Friendly string value</returns>
        internal static string FormatAsFriendlyTitle(this SourcePageType value)
        {
            var charArr = value.ToString().ToCharArray();
            var result = new StringBuilder();
            for (var i = 0; i < charArr.Length; i++)
            {
                if (char.IsUpper(charArr[i]))
                {
                    result.Append($" {charArr[i]}");
                }
                else
                {
                    result.Append(charArr[i]);
                }
            }

            // Convert to string and remove space at start
            return result.ToString().TrimStart(' ');
        }
    }
}
