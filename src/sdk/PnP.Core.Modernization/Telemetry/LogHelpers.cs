using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Modernization.Telemetry
{
    /// <summary>
    /// Class with extension methods that are used to help with logging
    /// </summary>
    public static class LogHelpers
    {
        /// <summary>
        /// Converts boolean value to Yes/No string
        /// </summary>
        /// <param name="value">Boolean value</param>
        /// <returns>Yes or No</returns>
        public static string ToYesNoString(this bool value)
        {
            return value ? "Yes" : "No";
        }

        /// <summary>
        /// Formats a string that has the format ThisIsAClassName and formats in a friendly way
        /// </summary>
        /// <param name="value">string value</param>
        /// <returns>Friendly string value</returns>
        public static string FormatAsFriendlyTitle(this string value)
        {
            var charArr = value.ToCharArray();
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

        /// <summary>
        /// Use reflection to read the object properties and detail the values
        /// </summary>
        /// <param name="pti">PageTransformationInformation object</param>
        /// <param name="version"></param>
        /// <returns>List of log records</returns>
        public static List<LogEntry> DetailSettingsAsLogEntries(this PageTransformationInformation pti, string version)
        {
            List<LogEntry> logs = new List<LogEntry>();

            try
            {
                // Add version 
                logs.Add(new LogEntry()
                {
                    Heading = LogStrings.Heading_PageTransformationInfomation,
                    Message = $"Engine version {LogStrings.KeyValueSeperatorToken} {version ?? "Not Specified"}"
                });

                var properties = pti.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(String) ||
                        property.PropertyType == typeof(bool))
                    {
                        var propVal = property.GetValue(pti);
                        logs.Add(new LogEntry() { Heading = LogStrings.Heading_PageTransformationInfomation,
                            Message = $"{property.Name.FormatAsFriendlyTitle()} {LogStrings.KeyValueSeperatorToken} {propVal ?? "Not Specified"}" });
                    }
                }
            }
            catch (Exception ex)
            {
                logs.Add(new LogEntry() { Message = "Failed to convert object properties for reporting", Exception = ex, Heading = LogStrings.Heading_PageTransformationInfomation });
            }
            
            return logs;

        }

        /// <summary>
        /// Display version for SharePoint
        /// </summary>
        /// <param name="version">SharePoint version</param>
        /// <returns>SharePoint version in string format</returns>
        public static string DisplaySharePointVersion(this SPVersion version)
        {
            switch (version)
            {
                case SPVersion.SP2010:
                    return "2010";
                case SPVersion.SP2013:
                case SPVersion.SP2013Legacy:
                    return "2013";
                case SPVersion.SP2016:
                case SPVersion.SP2016Legacy:
                    return "2016";
                case SPVersion.SP2019:
                    return "2019";
                case SPVersion.SPO:
                    return "Online";
                default:
                    return string.Empty;
            }
        }
    }
}
