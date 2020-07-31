using PnP.Core.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using PnP.Core.QueryModel.OData.Enums;
using PnP.Core.QueryModel.OData.Model;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Internal class to define an OData query with an in-memory abstract model
    /// </summary>
    /// <typeparam name="TModel">Defines the main type targeted by the query</typeparam>
    /// <remarks>
    /// We support what is defined here: https://docs.microsoft.com/en-us/sharepoint/dev/sp-add-ins/use-odata-query-operations-in-sharepoint-rest-requests#odata-query-operators-supported-in-the-sharepoint-rest-service
    /// except the substrinof and startswith functions
    /// </remarks>
    internal class ODataQuery<TModel>
    {
        private const string EncodedSpace = "%20";
        private readonly CultureInfo FormatProvider = CultureInfo.InvariantCulture;

        /// <summary>
        /// Property corresponding to the $top OData query option
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// Property corresponding to the $skip OData query option
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Property corresponding to the $filter OData query option
        /// </summary>
        public List<ODataFilter> Filters { get; private set; } = new List<ODataFilter>();

        /// <summary>
        /// Property corresponding to the $orderby OData query option
        /// </summary>
        public List<OrderByItem> OrderBy { get; private set; } = new List<OrderByItem>();

        /// <summary>
        /// Property corresponding to the $select OData query option
        /// </summary>
        public List<string> Select { get; private set; } = new List<string>();

        /// <summary>
        /// Property corresponding to the $expand OData query option
        /// </summary>
        public List<string> Expand { get; private set; } = new List<string>();

        public override string ToString()
        {
            // By default provide a Graph query without URL encoding
            return ToQueryString(ODataTargetPlatform.Graph, false);
        }

        /// <summary>
        /// Converts the in-memory OData query representation into an actual set of querystring OData options
        /// </summary>
        /// <param name="targetPlatform">Defines the target platform for the OData query</param>
        /// <param name="urlEncode">Declares whether to encode URL strings or not</param>
        /// <returns>The OData querystring for the current query</returns>
        public string ToQueryString(ODataTargetPlatform targetPlatform, bool urlEncode = true)
        {
            var queryText = new StringBuilder();
            var spacer = urlEncode ? EncodedSpace : " ";
            // var argumentTrailer = targetPlatform == ODataTargetPlatform.SPORest ? "$" : string.Empty;
            var argumentTrailer = "$";

            // Process the $select items
            if (Select.Count > 0)
            {
                queryText.Append($"{argumentTrailer}select=");
                foreach (var s in Select)
                {
                    queryText.AppendFormat(
                        FormatProvider,
                        "{0},",
                        HttpUtility.UrlEncode(TranslateFieldName(s, targetPlatform)));
                }

                // Remove the last ,
                queryText.Remove(queryText.Length - 1, 1);
            }

            // Process the $filter items
            if (Filters.Count > 0)
            {
                EnsureQueryStringConcat(queryText);
                queryText.Append($"{argumentTrailer}filter=");
                ProcessFilters(Filters, queryText, targetPlatform, depth: 0, urlEncode);
            }

            // Process any $top restriction
            if (Top.HasValue)
            {
                EnsureQueryStringConcat(queryText);
                queryText.AppendFormat(
                        FormatProvider,
                        $"{argumentTrailer}top={Top.Value}");
            }

            // Process any $skip restriction
            if (Skip.HasValue)
            {
                EnsureQueryStringConcat(queryText);
                queryText.AppendFormat(
                        FormatProvider,
                        $"{argumentTrailer}skip={Skip.Value}");
            }

            // Process the $orderby items
            if (OrderBy.Count > 0)
            {
                EnsureQueryStringConcat(queryText);
                queryText.Append($"{argumentTrailer}orderby=");
                foreach (var o in OrderBy)
                {
                    queryText.AppendFormat(
                        FormatProvider, "{0}{1},",
                        HttpUtility.UrlEncode(TranslateFieldName(o.Field, targetPlatform)),
                        o.Direction == OrderByDirection.Desc ? $"{spacer}desc" : null
                        );
                }

                // Remove the last ,
                queryText.Remove(queryText.Length - 1, 1);
            }

            // Process the $expand items
            if (Expand.Count > 0)
            {
                EnsureQueryStringConcat(queryText);
                queryText.Append($"{argumentTrailer}expand=");
                foreach (var e in Expand)
                {
                    queryText.AppendFormat(
                        FormatProvider,
                        "{0},",
                        HttpUtility.UrlEncode(TranslateFieldName(e, targetPlatform)));
                }

                // Remove the last ,
                queryText.Remove(queryText.Length - 1, 1);
            }

            return queryText.ToString();
        }

        private void ProcessFilters(List<ODataFilter> filters, StringBuilder queryText, ODataTargetPlatform targetPlatform, int depth = 0, bool urlEncode = true)
        {
            var spacer = urlEncode ? EncodedSpace : " ";
            var isFirst = true;

            foreach (var f in filters)
            {
                switch (f)
                {
                    case FiltersGroup group:
                        if (!isFirst)
                        {
                            // Add the concat operator and open the parentheses
                            queryText.AppendFormat(
                                FormatProvider,
                                "{0}{1}{0}",
                                spacer,
                                group.ConcatOperator.ToString().ToLower(FormatProvider));
                        }
                        // Open the parentheses
                        queryText.Append("(");
                        // Process the group
                        ProcessFilters(group.Filters, queryText, targetPlatform, depth: depth + 1, urlEncode);
                        // Close the parentheses
                        queryText.Append(")");
                        break;
                    case FilterItem filter:

                        // Add a trailing space and the concat operator if this is not the first filter
                        if (!isFirst)
                        {
                            queryText.AppendFormat(
                                FormatProvider,
                                "{0}{1}{0}",
                                spacer,
                                filter.ConcatOperator.ToString().ToLower(FormatProvider));
                        }

                        // Process the filtering condition
                        queryText.AppendFormat(
                                FormatProvider,
                                "{0}{1}{2}{1}{3}",
                                urlEncode ? HttpUtility.UrlEncode(TranslateFieldName(filter.Field, targetPlatform)) : TranslateFieldName(filter.Field, targetPlatform),
                                spacer,
                                ConvertFilteringCriteria(filter.Criteria),
                                urlEncode ? HttpUtility.UrlEncode(ODataUtilities.ConvertToString(filter.Value)) : ODataUtilities.ConvertToString(filter.Value));

                        break;
                    default:
                        throw new InvalidCastException("Invalid type for the filter object");
                }

                // Disable the isFirst flag, because for sure we already processed the first filter
                isFirst = false;
            }
        }

        private static string ConvertFilteringCriteria(FilteringCriteria criteria)
        {
            switch (criteria)
            {
                case FilteringCriteria.Equal:
                    return "eq";
                case FilteringCriteria.NotEqual:
                    return "ne";
                case FilteringCriteria.GreaterThan:
                    return "gt";
                case FilteringCriteria.GreaterThanOrEqual:
                    return "ge";
                case FilteringCriteria.LessThan:
                    return "lt";
                case FilteringCriteria.LessThanOrEqual:
                    return "le";
                case FilteringCriteria.Not:
                    return "not";
                default:
                    throw new NotSupportedException($"Criteria {criteria} is not supported");
            }
        }

        private static void EnsureQueryStringConcat(StringBuilder queryText)
        {
            if (queryText.Length > 0)
            {
                queryText.Append("&");
            }
        }

        /// <summary>
        /// Private method to translate the query field toward the target platform
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="targetPlatform"></param>
        /// <returns></returns>
        private static string TranslateFieldName(string fieldName, ODataTargetPlatform targetPlatform)
        {
            // Retrieve information about the field
            var modelType = typeof(TModel);
            var entityInfo = EntityManager.Instance.GetStaticClassInfo(modelType);
            var entityField = entityInfo?.Fields.FirstOrDefault(f => f.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));

            // Configure the default result
            var result = fieldName;

            if (entityField != null)
            {
                switch (targetPlatform)
                {
                    case ODataTargetPlatform.Graph when !string.IsNullOrEmpty(entityField.GraphName):
                        result = entityField.GraphName;
                        break;
                    case ODataTargetPlatform.SPORest when !string.IsNullOrEmpty(entityField.SharePointName):
                        result = entityField.SharePointName;
                        break;
                }
            }

            return result;
        }
    }
}
