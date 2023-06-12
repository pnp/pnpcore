using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// This class contains helpers for creating new graph events and setting properties
    /// </summary>
    internal static class GraphEventHelper
    {
        /// <summary>
        ///  Checks for the create options
        /// </summary>
        /// <param name="options">Options for the new event</param>
        /// <exception cref="ArgumentNullException">Triggers when argument is null</exception>
        /// <exception cref="ArgumentException">Triggers when option has wrong value</exception>
        internal static void CheckCreateOptions(EventCreateOptions options)
        {
            if (string.IsNullOrEmpty(options.Subject))
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.Subject)}");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            #region Recurrence pattern check
            if (options.Recurrence != null && options.Recurrence.Pattern != null)
            {

                if (options.Recurrence.Pattern.Type == EventRecurrenceType.Daily)
                {
                    if (options.Recurrence.Pattern.Interval < 1)
                    {
                        throw new ArgumentException("We need atleast an interval value of one day when creating a pattern with type 'Daily'");
                    }
                }
                if (options.Recurrence.Pattern.Type == EventRecurrenceType.AbsoluteMonthly || options.Recurrence.Pattern.Type == EventRecurrenceType.AbsoluteYearly)
                {
                    if (options.Recurrence.Pattern.DayOfMonth < 1)
                    {
                        throw new ArgumentException("Day of month is required and has to be greater than 0 when recurrence is of type 'AbsoluteYearly' or 'AbsoluteMonthly'.");
                    }
                }
                if (options.Recurrence.Pattern.Type == EventRecurrenceType.Weekly)
                {
                    if (options.Recurrence.Pattern.DaysOfWeek == null || options.Recurrence.Pattern.DaysOfWeek.Count == 0)
                    {
                        throw new ArgumentException("We need atleast one dayofweek to be set when recurrence is of type 'RelativeMonthly', 'RelativeYearly' or 'Weekly'.");
                    }
                    if (options.Recurrence.Pattern.Interval < 1)
                    {
                        throw new ArgumentException("We need atleast an interval value of one when creating a pattern with type 'Weekly'.");
                    }
                }
                if (options.Recurrence.Pattern.Type == EventRecurrenceType.AbsoluteMonthly)
                {
                    if (options.Recurrence.Pattern.Interval < 1)
                    {
                        throw new ArgumentException("We need atleast an interval value of one when creating a pattern with type 'AbsoluteMonthly'.");
                    }
                    if (options.Recurrence.Pattern.DayOfMonth < 1)
                    {
                        throw new ArgumentException("We need atleast a day of month to be set when creating a pattern with type 'AbsoluteMonthly'.");
                    }
                }
                if (options.Recurrence.Pattern.Type == EventRecurrenceType.RelativeMonthly)
                {
                    if (options.Recurrence.Pattern.Interval < 1)
                    {
                        throw new ArgumentException("We need atleast an interval value of one when creating a pattern with type 'RelativeMonthly'.");
                    }
                    if (options.Recurrence.Pattern.DaysOfWeek == null || options.Recurrence.Pattern.DaysOfWeek.Count == 0)
                    {
                        throw new ArgumentException("We need atleast a day of week to be set when creating a pattern with type 'RelativeMonthly'.");
                    }
                }
                if (options.Recurrence.Pattern.Type == EventRecurrenceType.AbsoluteYearly)
                {
                    if (options.Recurrence.Pattern.Interval < 1)
                    {
                        throw new ArgumentException("We need atleast an interval value of one when creating a pattern with type 'AbsoluteYearly'.");
                    }
                    if (options.Recurrence.Pattern.DayOfMonth < 1)
                    {
                        throw new ArgumentException("We need atleast a day of month to be set when creating a pattern with type 'AbsoluteYearly'.");
                    }
                    if (options.Recurrence.Pattern.Month < 1)
                    {
                        throw new ArgumentException("We need atleast a month value between 1 and 12 when creating a pattern with type 'AbsoluteYearly'.");
                    }
                }
                if (options.Recurrence.Pattern.Type == EventRecurrenceType.RelativeYearly)
                {
                    if (options.Recurrence.Pattern.Interval < 1)
                    {
                        throw new ArgumentException("We need atleast an interval value of one when creating a pattern with type 'RelativeYearly'.");
                    }
                    if (options.Recurrence.Pattern.DaysOfWeek == null || options.Recurrence.Pattern.DaysOfWeek.Count == 0)
                    {
                        throw new ArgumentException("We need atleast a day of week to be set when creating a pattern with type 'RelativeYearly'.");
                    }
                    if (options.Recurrence.Pattern.Month < 1)
                    {
                        throw new ArgumentException("We need atleast a month value between 1 and 12 when creating a pattern with type 'RelativeYearly'.");
                    }
                }

            }
            #endregion

            #region Recurrence range check
            if (options.Recurrence != null && options.Recurrence.Range != null)
            {
                if (options.Recurrence.Range.StartDate == DateTime.MinValue)
                {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                    throw new ArgumentNullException("options.Recurrence.Range.StartDate");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
                }

                if (options.Recurrence.Range.Type == EventRecurrenceRangeType.EndDate)
                {
                    if (options.Recurrence.Range.EndDate == DateTime.MinValue)
                    {
                        throw new ArgumentException("EndDate has to be set when Recurrence range type is of type 'EndDate'");
                    }
                }

                if (options.Recurrence.Range.Type == EventRecurrenceRangeType.Numbered)
                {
                    if (options.Recurrence.Range.NumberOfOccurences < 0)
                    {
                        throw new ArgumentException("Number of occurences has to be larger than 0 when range type is of type 'Numbered'");
                    }
                }
            }
            #endregion
        }

        internal static string ConvertDateTimeToRegularString(DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss.0000000", CultureInfo.InvariantCulture);
        }

        internal static string ConvertDateTimeToAllDayString(DateTime date)
        {
            return date.ToString("yyyy-MM-ddT00:00:00.0000000", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the enum member value for a timezone
        /// </summary>
        /// <param name="value">timezone to retrieve</param>
        /// <returns>enum member value</returns>
        internal static string GetEnumMemberValue(EventTimeZone value)
        {
            return typeof(EventTimeZone)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }
    }
}
