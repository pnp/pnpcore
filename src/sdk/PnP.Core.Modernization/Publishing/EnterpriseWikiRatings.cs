using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace PnP.Core.Modernization.Publishing
{
    internal sealed class EnterpriseWikiRatings
    {
        private static readonly string[] FieldNames = { "RatedBy", "Ratings", "RatingCount", "AverageRating" };

        internal IFieldUserValue[] Voters { get; private set; }
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

        internal static EnterpriseWikiRatings Read(IListItem source)
        {
            if (FieldNames.Any(name => !source.Values.ContainsKey(name)))
            {
                source = Reload(source);
            }
            return Read(source.Values);
        }

        private static IListItem Reload(IListItem item)
        {
            // Core merges returned fields, so stale values must not mask fields omitted by the server.
            foreach (var name in FieldNames)
            {
                item.Values.Remove(name);
            }
            item.LoadBatch(value => value["RatedBy"], value => value["Ratings"],
                value => value["RatingCount"], value => value["AverageRating"]);
            item.PnPContext.Execute();
            if (FieldNames.All(item.Values.ContainsKey))
            {
                return item;
            }

            // REST can omit native hidden rating fields even when explicitly requested.
            item.EnsureProperties(value => value.ParentList.QueryProperties(list => list.Id));
            var view = new XElement("View", new XAttribute("Scope", "RecursiveAll"),
                new XElement("Query", new XElement("Where", new XElement("Eq",
                    new XElement("FieldRef", new XAttribute("Name", "ID")),
                    new XElement("Value", new XAttribute("Type", "Counter"), item.Id)))),
                new XElement("ViewFields", FieldNames.Select(name => new XElement("FieldRef", new XAttribute("Name", name)))),
                new XElement("RowLimit", 1));
            return item.ParentList.LoadItemsByCamlQueryViaCsom(new CamlQueryOptions
            {
                ViewXml = view.ToString(SaveOptions.DisableFormatting), DatesInUtc = true
            }).Items.Single();
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
            // Core represents a null CAML user value as a single lookup with the empty-value sentinel.
            if (votersValue is IFieldUserValue emptyUser && emptyUser.LookupId == -1)
            {
                votersValue = null;
            }
            if (votersValue is IFieldValueCollection collection)
            {
                if (collection.Values.Any(v => !(v is IFieldUserValue)))
                {
                    throw InvalidHistory("RatedBy contains a non-user value.");
                }
                votersValue = collection.Values.Cast<IFieldUserValue>().ToArray();
            }
            if (votersValue != null && !(votersValue is IFieldUserValue[]))
            {
                throw InvalidHistory("RatedBy is not a multi-user value.");
            }
            var voters = (IFieldUserValue[])votersValue ?? Array.Empty<IFieldUserValue>();
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

        internal EnterpriseWikiRatings MapVoters(Func<IFieldUserValue, IFieldUserValue> resolve)
        {
            var mapped = Voters.Select(resolve).ToArray();
            if (mapped.Any(v => v == null || v.LookupId <= 0) ||
                mapped.Select(v => v.LookupId).Distinct().Count() != mapped.Length)
            {
                throw InvalidHistory("Every voter must resolve to a distinct destination user; votes cannot be dropped or merged.");
            }
            return new EnterpriseWikiRatings { Voters = mapped, Ratings = Ratings, Scores = Scores, Count = Count, Average = Average };
        }

        internal void CopyTo(IListItem target)
        {
            // Persist the user/score pairs and aggregates together, never as independent field updates.
            var ratedBy = target.ParentList.Fields.GetByInternalNameOrTitle("RatedBy");
            target["RatedBy"] = Voters.Length == 0 ? null :
                ratedBy.NewFieldValueCollection(Voters.Select(voter => ratedBy.NewFieldUserValue(voter.LookupId)));
            target["Ratings"] = string.IsNullOrEmpty(Ratings) ? null : Ratings;
            target["RatingCount"] = Count;
            target["AverageRating"] = Average;
            target.SystemUpdate();
            var saved = Read(Reload(target).Values);
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
