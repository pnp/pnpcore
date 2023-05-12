using PnP.Core.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;

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
    internal sealed class ODataQuery<TModel>
    {
        private const string EncodedSpace = "%20";
        private readonly CultureInfo FormatProvider = CultureInfo.InvariantCulture;

        internal const string TopKey = "$top";
        internal const string FilterKey = "$filter";
        internal const string SkipKey = "$skip";
        internal const string OrderByKey = "$orderby";

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
        public List<ODataFilter> Filters { get; } = new List<ODataFilter>();

        /// <summary>
        /// Property corresponding to the $orderby OData query option
        /// </summary>
        public List<OrderByItem> OrderBy { get; } = new List<OrderByItem>();

        /// <summary>
        /// Returns the list of fields to load
        /// </summary>
        public List<Expression<Func<TModel, object>>> Fields { get; } = new List<Expression<Func<TModel, object>>>();

        public override string ToString()
        {
            // By default provide a Graph query without URL encoding
            return ToQueryString(ODataTargetPlatform.Graph);
        }

        /// <summary>
        /// Converts the in-memory OData query representation into an actual set of querystring OData options
        /// </summary>
        /// <param name="targetPlatform">Defines the target platform for the OData query</param>
        /// <returns>The OData querystring for the current query</returns>
        public string ToQueryString(ODataTargetPlatform targetPlatform)
        {
            var urlParameters = new Dictionary<string, string>();
            AddODataToUrlParameters(urlParameters, targetPlatform);

            // Exclude empty items
            IEnumerable<string> items = urlParameters
                .Where(i => !string.IsNullOrEmpty(i.Value))
                .Select(p => $"{p.Key}={p.Value}");
            return String.Join("&", items);
        }

        /// <summary>
        /// Returns the OData $orderby clause
        /// </summary>
        /// <param name="targetPlatform"></param>
        /// <param name="urlEncode"></param>
        /// <returns></returns>
        internal string GetOrderBy(ODataTargetPlatform targetPlatform, bool urlEncode = true)
        {
            var spacer = urlEncode ? EncodedSpace : " ";

            var sb = new StringBuilder();
            foreach (var o in OrderBy)
            {
                sb.AppendFormat(
                    FormatProvider, "{0}{1},",
                    HttpUtility.UrlEncode(TranslateFieldName(o.Field, targetPlatform)),
                    o.Direction == OrderByDirection.Desc ? $"{spacer}desc" : null
                );
            }

            // Remove the last ,
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        /// <summary>
        /// Returns the OData $filter clause
        /// </summary>
        /// <param name="targetPlatform"></param>
        /// <param name="urlEncode"></param>
        /// <returns></returns>
        internal string GetFilters(ODataTargetPlatform targetPlatform, bool urlEncode = true)
        {
            var sb = new StringBuilder();
            ProcessFilters(Filters, sb, targetPlatform, 0, urlEncode);
            return sb.ToString();
        }

        internal void AddODataToUrlParameters(Dictionary<string, string> urlParameters, ODataTargetPlatform targetPlatform)
        {
            // Process the $filter items
            if (Filters.Count > 0)
            {
                urlParameters.Add(FilterKey, GetFilters(targetPlatform, false));
            }

            // Process any $top restriction if and only if the target platform is not Graph
            // or if the target platform is Graph, but there are no filters
            if (Top.HasValue && (Filters.Count == 0 || targetPlatform != ODataTargetPlatform.Graph))
            {
                urlParameters.Add(TopKey, Top.ToString());
            }

            // Process any $skip restriction if and only if the target platform is not Graph
            // or if the target platform is Graph, but there are no filters
            if (Skip.HasValue && (Filters.Count == 0 || targetPlatform != ODataTargetPlatform.Graph))
            {
                urlParameters.Add(SkipKey, Skip.ToString());
            }

            // Process the $orderby items
            if (OrderBy.Count > 0)
            {
                urlParameters.Add(OrderByKey, GetOrderBy(targetPlatform, false));
            }
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
                        queryText.Append('(');
                        // Process the group
                        ProcessFilters(group.Filters, queryText, targetPlatform, depth: depth + 1, urlEncode);
                        // Close the parentheses
                        queryText.Append(')');
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
                                urlEncode ? HttpUtility.UrlEncode(ODataUtilities.ConvertToString(filter.Value, targetPlatform)) : ODataUtilities.ConvertToString(filter.Value, targetPlatform));

                        break;
                    default:
                        throw new InvalidCastException(PnPCoreResources.Exception_InvalidTypeForFilter);
                }

                // Disable the isFirst flag, because for sure we already processed the first filter
                isFirst = false;
            }
        }

        private static string ConvertFilteringCriteria(FilteringCriteria criteria)
        {
            return criteria switch
            {
                FilteringCriteria.Equal => "eq",
                FilteringCriteria.NotEqual => "ne",
                FilteringCriteria.GreaterThan => "gt",
                FilteringCriteria.GreaterThanOrEqual => "ge",
                FilteringCriteria.LessThan => "lt",
                FilteringCriteria.LessThanOrEqual => "le",
                FilteringCriteria.Not => "not",
                _ => throw new NotSupportedException(string.Format(PnPCoreResources.Exception_Unsupported_Criteria, criteria)),
            };
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
