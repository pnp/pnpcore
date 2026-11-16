using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PnP.Core.Modernization.Publishing
{
    internal sealed class EnterpriseWikiRatings
    {
        private static readonly string[] FieldNames = { "RatedBy", "Ratings", "RatingCount", "AverageRating" };

        internal FieldUserValue[] Voters { get; private set; }
        internal string Ratings { get; private set; }
        internal int[] Scores { get; private set; }
        internal int Count { get; private set; }
        internal double Average { get; private set; }

        internal static bool UsesSameNameMapping(PageLayout layout)
        {
            return layout?.MetaData?.Field != null && FieldNames.All(name =>
                layout.MetaData.Field.Count(f => f.Name == name && f.TargetFieldName == name &&
                    string.IsNullOrEmpty(f.Functions)) == 1);
        }

        internal static bool IsRatingField(string name) => FieldNames.Contains(name);

        internal static EnterpriseWikiRatings Read(ListItem source)
        {
            if (FieldNames.Any(name => !source.FieldValues.ContainsKey(name)))
            {
                // Refresh the complete group so unloaded fields cannot become an empty history.
                source.Context.Load(source, item => item["RatedBy"], item => item["Ratings"],
                    item => item["RatingCount"], item => item["AverageRating"]);
                source.Context.ExecuteQueryRetry();
            }
            return Read(source.FieldValues);
        }

        internal static EnterpriseWikiRatings Read(IDictionary<string, object> fields)
        {
            var missing = FieldNames.Where(name => !fields.ContainsKey(name)).ToArray();
            if (missing.Length > 0)
            {
                throw new InvalidOperationException(
                    $"Enterprise Wiki rating history cannot be migrated because rating fields are missing or not loaded: {string.Join(", ", missing)}.");
            }
            var votersValue = fields["RatedBy"];
            if (votersValue != null && !(votersValue is FieldUserValue[]))
            {
                throw InvalidHistory("RatedBy is not a multi-user value.");
            }
            var voters = (FieldUserValue[])votersValue ?? Array.Empty<FieldUserValue>();
            var ratingsValue = fields["Ratings"];
            var ratings = Convert.ToString(ratingsValue, CultureInfo.InvariantCulture);
            var parts = string.IsNullOrWhiteSpace(ratings) ? Array.Empty<string>() : ratings.Trim().Split(',');
            if (parts.Length > 0 && parts[parts.Length - 1].Length == 0)
            {
                parts = parts.Take(parts.Length - 1).ToArray();
            }
            var scores = new int[parts.Length];
            for (var index = 0; index < parts.Length; index++)
            {
                if (!int.TryParse(parts[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out scores[index]) ||
                    scores[index] < 1 || scores[index] > 5)
                {
                    throw InvalidHistory("Ratings must contain comma-separated scores from 1 through 5.");
                }
            }
            var count = ReadNumber(fields, "RatingCount");
            var average = ReadNumber(fields, "AverageRating");
            if (count != scores.Length || voters.Length != scores.Length ||
                voters.Any(v => v == null || v.LookupId <= 0) ||
                voters.Select(v => v.LookupId).Distinct().Count() != voters.Length)
            {
                throw InvalidHistory("RatedBy, Ratings and RatingCount must describe the same uniquely identified voters, in order.");
            }
            var expectedAverage = scores.Length == 0 ? 0 : scores.Average();
            if (Math.Abs(average - expectedAverage) > 0.0001)
            {
                throw InvalidHistory("AverageRating does not match the individual scores.");
            }
            return new EnterpriseWikiRatings
            {
                Voters = voters.ToArray(),
                Ratings = ratings,
                Scores = scores,
                Count = scores.Length,
                Average = average
            };
        }

        internal EnterpriseWikiRatings MapVoters(Func<FieldUserValue, FieldUserValue> resolve)
        {
            var mapped = Voters.Select(resolve).ToArray();
            if (mapped.Any(v => v == null || v.LookupId <= 0) ||
                mapped.Select(v => v.LookupId).Distinct().Count() != mapped.Length)
            {
                throw InvalidHistory("Every voter must resolve to a distinct destination user; votes cannot be dropped or merged.");
            }
            return new EnterpriseWikiRatings { Voters = mapped, Ratings = Ratings, Scores = Scores, Count = Count, Average = Average };
        }

        internal void CopyTo(ListItem target)
        {
            // Persist the user/score pairs and aggregates together, never as independent field updates.
            target["RatedBy"] = Voters.Length == 0 ? null : Voters;
            target["Ratings"] = string.IsNullOrEmpty(Ratings) ? null : Ratings;
            target["RatingCount"] = Count;
            target["AverageRating"] = Average;
            target.SystemUpdate();
            target.Context.Load(target);
            target.Context.ExecuteQueryRetry();

            var saved = Read(target.FieldValues);
            if (!saved.Voters.Select(v => v.LookupId).SequenceEqual(Voters.Select(v => v.LookupId)) ||
                !saved.Scores.SequenceEqual(Scores) || Math.Abs(saved.Average - Average) > 0.0001)
            {
                throw InvalidHistory("Destination readback did not preserve the voter/score pairs.");
            }
        }

        private static double ReadNumber(IDictionary<string, object> fields, string name)
        {
            var value = fields[name];
            if (value == null)
            {
                return 0;
            }
            if (!double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Float,
                CultureInfo.InvariantCulture, out var number) || double.IsNaN(number) || double.IsInfinity(number))
            {
                throw InvalidHistory($"{name} is not a finite number.");
            }
            return number;
        }

        private static InvalidOperationException InvalidHistory(string reason)
        {
            return new InvalidOperationException(
                $"Enterprise Wiki rating history cannot be migrated: {reason} Resolve incomplete or legacy Social-store history before retrying.");
        }
    }
}
