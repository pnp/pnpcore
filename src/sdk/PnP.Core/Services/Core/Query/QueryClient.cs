using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace PnP.Core.Services
{
    internal static class QueryClient
    {
        #region GET

        #region Model data get

        internal static Task<ApiCallRequest> BuildGetAPICallAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, ApiCall apiOverride, bool forceSPORest = false, bool useLinqGet = false, bool loadPages = false)
        {
            return BuildGetAPICallAsync(model, entity, new ODataQuery<TModel>(), apiOverride, forceSPORest, useLinqGet, loadPages);
        }

        internal static async Task<ApiCallRequest> BuildGetAPICallAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, ODataQuery<TModel> oDataQuery, ApiCall apiOverride, bool forceSPORest = false, bool useLinqGet = false, bool loadPages = false)
        {
            // Can we use Microsoft Graph for this GET request?
            bool useGraph = model.PnPContext.GraphFirst &&    // See if Graph First is enabled/configured
                !forceSPORest &&                        // and if we are not forced to use SPO REST
                entity.CanUseGraphGet;                  // and if the entity supports GET via Graph 

            // If entity cannot be surfaced with SharePoint Rest then force graph
            if (string.IsNullOrEmpty(entity.SharePointType))
            {
                useGraph = true;
            }
            // Else if we've overriden the query then simply take what was set in the query override
            else if (!apiOverride.Equals(default(ApiCall)))
            {
                useGraph = apiOverride.Type == ApiType.Graph;
            }
            else if (useGraph && useLinqGet)
            {
                // LINQ Get will based upon the defined LinqGet query + query arguments determined via entity and oDataQuery.
                // e.g. _api/web/lists or _api/web/webs
                // When there are no query arguments and when the model supports Graph (e.g. Web) then useGraph = true while
                // the queried collections (e.g. webs, lists) might not be decorated with a GraphType attribute

                if (string.IsNullOrEmpty(entity.GraphLinqGet) && !string.IsNullOrEmpty(entity.SharePointLinqGet))
                {
                    useGraph= false;
                }
            }

            if (useGraph)
            {
                return await BuildGetAPICallGraphAsync(model, entity, oDataQuery, apiOverride, useLinqGet, loadPages).ConfigureAwait(false);
            }
            else
            {
                return await BuildGetAPICallRestAsync(model, entity, oDataQuery, apiOverride, useLinqGet, loadPages).ConfigureAwait(false);
            }
        }

        private static async Task<ApiCallRequest> BuildGetAPICallRestAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, ODataQuery<TModel> oDataQuery, ApiCall apiOverride, bool useLinqGet, bool loadPages)
        {
            string getApi = useLinqGet ? entity.SharePointLinqGet : entity.SharePointGet;

            IEnumerable<EntityFieldInfo> fields = entity.Fields.Where(p => p.Load);

            Dictionary<string, string> urlParameters = new Dictionary<string, string>();

            StringBuilder sb = new StringBuilder();

            // Only add select statement whenever there was a filter specified
            if (entity.SharePointFieldsLoadedViaExpression)
            {
                // $select
                foreach (var field in fields)
                {
                    // If there was a selection on which fields to include in an expand (via the QueryProperties() option) then add those fields
                    if (field.SharePointExpandable && field.ExpandFieldInfo != null)
                    {
                        AddExpandableSelectRest(sb, field, null, "");
                    }
                    else
                    {
                        sb.Append($"{JsonMappingHelper.GetRestField(field)},");
                    }
                }

                urlParameters.Add("$select", sb.ToString().TrimEnd(new char[] { ',' }));
                sb.Clear();
            }

            // $expand
            foreach (var field in fields.Where(p => p.SharePointExpandable))
            {
                if (entity.SharePointFieldsLoadedViaExpression)
                {
                    sb.Append($"{JsonMappingHelper.GetRestField(field)},");

                    // If there was a selection on which fields to include in an expand (via the Include() option) and the included field was expandable itself then add it 
                    if (field.ExpandFieldInfo != null)
                    {
                        string path = "";
                        AddExpandableExpandRest(sb, field, null, path);
                    }
                }
                else
                {
                    if (field.ExpandableByDefault)
                    {
                        sb.Append($"{JsonMappingHelper.GetRestField(field)},");
                    }
                }
            }
            urlParameters.Add("$expand", sb.ToString().TrimEnd(new char[] { ',' }));

            oDataQuery.AddODataToUrlParameters(urlParameters, ODataTargetPlatform.SPORest);

            // REST apis do not apply a default top
            // In order to not receive all items in one request, we apply a default top
            // We don't change the original ODataQuery to avoid side effects
            if (useLinqGet && !urlParameters.ContainsKey(ODataQuery<TModel>.TopKey))
            {
                urlParameters.Add(ODataQuery<TModel>.TopKey, model.PnPContext.GlobalOptions.HttpSharePointRestDefaultPageSize.ToString());
            }

            sb.Clear();

            // Build the API call
            string baseApiCall = "";
            if (apiOverride.Equals(default(ApiCall)))
            {
                baseApiCall = $"{model.PnPContext.Uri.AbsoluteUri.TrimEnd(new char[] { '/' })}/{getApi}";
            }
            else
            {
                baseApiCall = $"{model.PnPContext.Uri.AbsoluteUri.TrimEnd(new char[] { '/' })}/{apiOverride.Request}";
            }

            // Parse tokens in the base api call
            baseApiCall = await ApiHelper.ParseApiCallAsync(model, baseApiCall).ConfigureAwait(false);

            sb.Append(baseApiCall);

            // Build the querystring parameters
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var urlParameter in urlParameters.Where(i => !string.IsNullOrEmpty(i.Value)))
            {
                // Add key and value, which will be automatically URL-encoded, if needed
                queryString.Add(urlParameter.Key, urlParameter.Value);
            }

            // Build the whole URL
            if (queryString.AllKeys.Length > 0)
            {
                // In .NET Framework to ToString() of a NameValueCollection will use HttpUtility.UrlEncodeUnicode under
                // the covers resulting in issues. So we decode and encode again as a workaround. This code produces the 
                // same result when used under .NET5/Core versus .NET Framework
                sb.Append($"?{queryString.ToEncodedString()}");
            }

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(sb.ToString(), ApiType.SPORest, loadPages: loadPages));
            if (model.GetApiCallOverrideHandler != null)
            {
                call = await model.GetApiCallOverrideHandler.Invoke(call).ConfigureAwait(false);
            }

            return call;
        }

        private static void AddExpandableExpandRest(StringBuilder sb, EntityFieldInfo field, EntityFieldExpandInfo expandFields, string path)
        {
            EntityInfo collectionEntityInfo = null;
            if (expandFields == null)
            {
                collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(field.ExpandFieldInfo.Type);
                expandFields = field.ExpandFieldInfo;
            }
            else
            {
                collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(expandFields.Type);
            }

            foreach (var expandableField in expandFields.Fields.OrderBy(p => p.Expandable))
            {
                var expandableFieldInfo = collectionEntityInfo.Fields.First(p => p.Name == expandableField.Name);

                if (expandableFieldInfo.SharePointExpandable)
                {
                    path = path + "/" + JsonMappingHelper.GetRestField(expandableFieldInfo);
                    sb.Append($"{JsonMappingHelper.GetRestField(field)}{path},");
                    if (expandableField.Fields.Any())
                    {
                        AddExpandableExpandRest(sb, field, expandableField, path);
                    }
                    path = "";
                }
            }
        }

        private static void AddExpandableSelectRest(StringBuilder sb, EntityFieldInfo field, EntityFieldExpandInfo expandFields, string path)
        {
            EntityInfo collectionEntityInfo = null;

            if (expandFields == null)
            {
                collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(field.ExpandFieldInfo.Type);
                expandFields = field.ExpandFieldInfo;
            }
            else
            {
                if (expandFields.Type != null)
                {
                    collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(expandFields.Type);
                }
            }

            if (collectionEntityInfo != null)
            {
                foreach (var expandableField in expandFields.Fields.OrderBy(p => p.Expandable))
                {
                    var expandableFieldInfo = collectionEntityInfo.Fields.First(p => p.Name == expandableField.Name);
                    if (!expandableFieldInfo.SharePointExpandable)
                    {
                        sb.Append($"{JsonMappingHelper.GetRestField(field)}{path}/{JsonMappingHelper.GetRestField(expandableFieldInfo)},");
                    }
                    else
                    {
                        path = path + "/" + JsonMappingHelper.GetRestField(expandableFieldInfo);
                        AddExpandableSelectRest(sb, field, expandableField, path);
                        path = "";
                    }
                }
            }
        }

        private static async Task<ApiCallRequest> BuildGetAPICallGraphAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, ODataQuery<TModel> oDataQuery, ApiCall apiOverride, bool useLinqGet, bool loadPages)
        {
            string getApi = useLinqGet ? entity.GraphLinqGet : entity.GraphGet;

            ApiType apiType = ApiType.Graph;

            if (entity.GraphBeta)
            {
                if (CanUseGraphBeta(model, entity))
                {
                    apiType = ApiType.GraphBeta;
                }
                else
                {
                    // we can't make this request
                    var cancelledApiCallRequest = new ApiCallRequest(default);
                    cancelledApiCallRequest.CancelRequest($"Getting {getApi} requires the Graph Beta endpoint which was not configured to be allowed");
                    return cancelledApiCallRequest;
                }
            }

            IEnumerable<EntityFieldInfo> fields = entity.Fields.Where(p => p.Load);

            Dictionary<string, string> urlParameters = new Dictionary<string, string>(2);

            StringBuilder sb = new StringBuilder();

            // Only add select statement whenever there was a filter specified
            if (entity.GraphFieldsLoadedViaExpression)
            {
                // $select
                bool graphIdFieldAdded = false;
                foreach (var field in fields.Where(p => p.ExpandFieldInfo == null))
                {
                    // Don't add the field in the select if it will be added as expandable field
                    if (!string.IsNullOrEmpty(field.GraphName))
                    {
                        bool addExpand = true;
                        if (field.GraphBeta)
                        {
                            if (CanUseGraphBeta(model, field))
                            {
                                apiType = ApiType.GraphBeta;
                            }
                            else
                            {
                                // Field will be skipped as we're forced to use v1
                                addExpand = false;
                            }
                        }

                        if (addExpand)
                        {
                            sb.Append($"{JsonMappingHelper.GetGraphField(field)},");
                        }
                    }

                    if (!graphIdFieldAdded && !string.IsNullOrEmpty(entity.GraphId))
                    {
                        if (JsonMappingHelper.GetGraphField(field) == entity.GraphId)
                        {
                            graphIdFieldAdded = true;
                        }
                    }
                }

                if (!graphIdFieldAdded && !string.IsNullOrEmpty(entity.GraphId))
                {
                    sb.Append($"{entity.GraphId},");
                }

                urlParameters.Add("$select", sb.ToString().TrimEnd(new char[] { ',' }));
                sb.Clear();
            }

            // $expand
            foreach (var field in fields.Where(p => p.GraphExpandable && string.IsNullOrEmpty(p.GraphGet)))
            {
                if (!string.IsNullOrEmpty(field.GraphName))
                {
                    if (entity.GraphFieldsLoadedViaExpression)
                    {
                        bool addExpand = true;

                        if (field.GraphBeta)
                        {
                            if (CanUseGraphBeta(model, field))
                            {
                                apiType = ApiType.GraphBeta;
                            }
                            else
                            {
                                // Expand will be skipped since we're bound to v1 graph
                                addExpand = false;
                            }
                        }

                        if (addExpand)
                        {
                            if (field.ExpandFieldInfo != null)
                            {
                                StringBuilder sbExpand = new StringBuilder();
                                AddExpandableSelectGraph(false, sbExpand, field, null, "");
                                sb.Append($"{JsonMappingHelper.GetGraphField(field)}{sbExpand}");
                            }
                            else
                            {
                                sb.Append($"{JsonMappingHelper.GetGraphField(field)},");
                            }
                        }
                    }
                    else
                    {
                        if (field.ExpandableByDefault)
                        {
                            bool addExpand = true;

                            if (field.GraphBeta)
                            {
                                if (CanUseGraphBeta(model, field))
                                {
                                    apiType = ApiType.GraphBeta;
                                }
                                else
                                {
                                    // Expand will be skipped since we're bound to v1 graph
                                    addExpand = false;
                                }
                            }

                            if (addExpand)
                            {
                                sb.Append($"{JsonMappingHelper.GetGraphField(field)},");
                            }
                        }
                    }
                }
            }
            urlParameters.Add("$expand", sb.ToString().TrimEnd(new char[] { ',' }));

            oDataQuery.AddODataToUrlParameters(urlParameters, ODataTargetPlatform.Graph);

            sb.Clear();

            // Build the API call
            string baseApiCall = "";
            if (apiOverride.Equals(default(ApiCall)))
            {
                if (string.IsNullOrEmpty(getApi))
                {
                    throw new ClientException(ErrorType.ModelMetadataIncorrect,
                        PnPCoreResources.Exception_ModelMetadataIncorrect_MissingGetMapping);
                }

                // Ensure tokens in the base url are replaced
                baseApiCall = await TokenHandler.ResolveTokensAsync(model, getApi, model.PnPContext).ConfigureAwait(false);
            }
            else
            {
                // Ensure tokens in the base url are replaced
                baseApiCall = await TokenHandler.ResolveTokensAsync(model, apiOverride.Request, model.PnPContext).ConfigureAwait(false);
            }

            // Parse tokens in the base api call
            baseApiCall = await ApiHelper.ParseApiCallAsync(model, baseApiCall).ConfigureAwait(false);

            sb.Append(baseApiCall);

            // Build the querystring parameters
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var urlParameter in urlParameters.Where(i => !string.IsNullOrEmpty(i.Value)))
            {
                // Add key and value, which will be automatically URL-encoded, if needed
                // as = already might have been encoded as %3D we need to undo that as otherwise we get a double encoding resulting in %253D
                queryString.Add(urlParameter.Key, urlParameter.Value.Replace("%3D", "="));
            }

            // Build the whole URL
            if (queryString.AllKeys.Length > 0)
            {
                sb.Append($"?{queryString.ToEncodedString()}");
            }

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(sb.ToString(), apiType, loadPages: loadPages));
            if (model.GetApiCallOverrideHandler != null)
            {
                call = await model.GetApiCallOverrideHandler.Invoke(call).ConfigureAwait(false);
            }

            return call;
        }

        internal static void AddExpandableSelectGraph(bool newQuery, StringBuilder sb, EntityFieldInfo field, EntityFieldExpandInfo expandFields, string path)
        {
            if (path == null)
            {
                path = "";
            }

            EntityInfo collectionEntityInfo = null;

            if (expandFields == null)
            {
                collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(field.ExpandFieldInfo.Type);
                expandFields = field.ExpandFieldInfo;
            }
            else
            {
                if (expandFields.Type != null)
                {
                    collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(expandFields.Type);
                }
            }

            if (collectionEntityInfo != null)
            {
                bool first = true;
                foreach (var expandableField in expandFields.Fields.OrderBy(p => p.Expandable))
                {
                    var expandableFieldInfo = collectionEntityInfo.Fields.First(p => p.Name == expandableField.Name);

                    if (!string.IsNullOrEmpty(expandableFieldInfo.GraphGet))
                    {
                        throw new ClientException(ErrorType.Unsupported,
                            string.Format(PnPCoreResources.Exception_Unsupported_ExtraGet,
                            expandableFieldInfo.Name,
                            expandableFieldInfo.GraphGet));
                    }

                    if (!expandableFieldInfo.GraphExpandable)
                    {
                        if (first)
                        {
                            sb.Append($"{path}{(newQuery ? "" : "(")}$select{(newQuery ? "=" : "%3D")}");
                        }

                        sb.Append($"{(first ? "" : ",")}{(!string.IsNullOrEmpty(expandableFieldInfo.GraphJsonPath) ? expandableFieldInfo.GraphJsonPath : JsonMappingHelper.GetGraphField(expandableFieldInfo))}");
                        first = false;
                        path = "";
                    }
                    else
                    {
                        path = $";$expand%3D{JsonMappingHelper.GetGraphField(expandableFieldInfo)}";
                        AddExpandableSelectGraph(false, sb, field, expandableField, path);
                    }
                }

                if (!newQuery)
                {
                    sb.Append(')');
                }
            }
        }

        #endregion

        #region API Calls for non expandable collections

        internal static async Task AddGraphBatchRequestsForNonExpandableCollectionsAsync<TModel>(BaseDataModel<TModel> model, Batch batch, EntityInfo entityInfo, Expression<Func<TModel, object>>[] expressions, Func<FromJson, object> fromJsonCasting, Action<string> postMappingJson)
        {
            ApiType apiType = ApiType.Graph;

            if (entityInfo.GraphBeta)
            {
                if (CanUseGraphBeta(model, entityInfo))
                {
                    apiType = ApiType.GraphBeta;
                }
            }

            var nonExpandableFields = entityInfo.GraphNonExpandableCollections;
            if (nonExpandableFields.Any())
            {
                foreach (var nonExpandableField in nonExpandableFields)
                {
                    // Was this non expandable field requested?
                    bool needed = nonExpandableField.ExpandableByDefault;

                    if (!needed)
                    {
                        // Check passed in expressions
                        needed = IsDefinedInExpression(expressions, nonExpandableField.Name) || nonExpandableField.ExpandFieldInfo != null;
                    }

                    if (needed)
                    {
                        bool addExpandableCollection = true;
                        if (nonExpandableField.GraphBeta)
                        {
                            if (CanUseGraphBeta(model, nonExpandableField))
                            {
                                apiType = ApiType.GraphBeta;
                            }
                            else
                            {
                                // Can't add this non expanable collection request since we're bound to Graph v1
                                addExpandableCollection = false;
                            }
                        }

                        if (addExpandableCollection)
                        {
                            string graphGet = nonExpandableField.GraphGet;
                            if (nonExpandableField.ExpandFieldInfo != null)
                            {
                                bool graphGetHasExpand = false;
                                var uriBuilder = new UriBuilder(graphGet);

                                // Check if the model we're expanding uses JsonPath
                                bool usesJsonPath = false;
                                var collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(nonExpandableField.ExpandFieldInfo.Type);
                                foreach (var fieldToExpand in nonExpandableField.ExpandFieldInfo.Fields)
                                {
                                    var expandableFieldInfo = collectionEntityInfo.Fields.First(p => p.Name == fieldToExpand.Name);

                                    if (!string.IsNullOrEmpty(expandableFieldInfo.GraphGet))
                                    {
                                        throw new ClientException(ErrorType.Unsupported,
                                            string.Format(PnPCoreResources.Exception_Unsupported_ExtraGet,
                                            expandableFieldInfo.Name,
                                            expandableFieldInfo.GraphGet));
                                    }

                                    if (!string.IsNullOrEmpty(expandableFieldInfo.GraphJsonPath))
                                    {
                                        usesJsonPath = true;
                                    }
                                }

                                NameValueCollection queryParameters = HttpUtility.ParseQueryString(uriBuilder.Query.ToLowerInvariant());
                                string value = queryParameters["$expand"];
                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    // we've an $expand url parameter in the GET query
                                    var currentExpandValues = value.Split(new char[] { ',' });

                                    if (usesJsonPath)
                                    {
                                        // JsonPath usage will be handled as an existing expand, so we append the extra $select via ($select%3D...)
                                        graphGetHasExpand = true;
                                    }
                                    else
                                    {
                                        // do we have an $expand for our property we're applying the expandable select on?
                                        foreach (var expandValue in currentExpandValues)
                                        {
                                            if (expandValue.Equals(nonExpandableField.GraphName, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                graphGetHasExpand = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                StringBuilder sb = new StringBuilder();

                                AddExpandableSelectGraph(!graphGetHasExpand, sb, nonExpandableField, null, "");

                                // Since the URL listed for a graph get can already have url parameters we need to "merge" them together
                                var urlComplement = sb.ToString();
                                if (graphGetHasExpand)
                                {
                                    // We append the extra $select via ($select%3D...)
                                    graphGet += urlComplement;
                                }
                                else
                                {
                                    // We combine an $select=id,field,... with the existing url
                                    graphGet = UrlUtility.CombineRelativeUrlWithUrlParameters(graphGet, urlComplement);
                                }
                            }

                            var parsedApiRequest = await ApiHelper.ParseApiRequestAsync(model, graphGet).ConfigureAwait(false);
                            ApiCall extraApiCall = new ApiCall(parsedApiRequest, apiType, receivingProperty: nonExpandableField.GraphName);

                            if (model.GetApiCallNonExpandableCollectionOverrideHandler != null)
                            {
                                var extraApiCallRequest = await model.GetApiCallNonExpandableCollectionOverrideHandler.Invoke(new ApiCallRequest(extraApiCall)).ConfigureAwait(false);
                                extraApiCall = extraApiCallRequest.ApiCall;
                            }

                            batch.Add(model, entityInfo, HttpMethod.Get, extraApiCall, default, fromJsonCasting, postMappingJson, "GetBatch");
                        }
                    }
                }
            }
        }

        private static bool IsDefinedInExpression<TModel>(Expression<Func<TModel, object>>[] expressions, string field)
        {
            if (expressions == null)
            {
                return false;
            }

            foreach (var expression in expressions)
            {
                if (expression.Body.NodeType == ExpressionType.Call && expression.Body is MethodCallExpression)
                {
                    // Future use? (includes)                    
                }
                else
                {
                    var body = expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression;
                    if (body != null && body.Member.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Paging 

        internal static Tuple<string, ApiType> BuildNextPageLink(IMetadataExtensible collection)
        {
            string nextLink;
            ApiType nextLinkApiType;

            // important: the skiptoken is case sensitive, so ensure to keep it the way is was provided to you by Graph/SharePoint (for listitem paging)
            if (collection.Metadata.ContainsKey(PnPConstants.GraphNextLink) && collection.Metadata[PnPConstants.GraphNextLink].Contains($"/{PnPConstants.GraphBetaEndpoint}/", StringComparison.InvariantCultureIgnoreCase))
            {
                string graphUrl;
                if ((collection as IDataModelWithContext).PnPContext.Environment == Microsoft365Environment.Custom)
                {
                    graphUrl = $"https://{(collection as IDataModelWithContext).PnPContext.MicrosoftGraphAuthority}/";
                }
                else
                {
                    graphUrl = $"https://{CloudManager.GetMicrosoftGraphAuthority((collection as IDataModelWithContext).PnPContext.Environment.Value)}/";
                }

                nextLink = collection.Metadata[PnPConstants.GraphNextLink].Replace($"{graphUrl}{PnPConstants.GraphBetaEndpoint}/", "");
                nextLinkApiType = ApiType.GraphBeta;
            }
            else if (collection.Metadata.ContainsKey(PnPConstants.GraphNextLink) && collection.Metadata[PnPConstants.GraphNextLink].Contains($"/{PnPConstants.GraphV1Endpoint}/", StringComparison.InvariantCultureIgnoreCase))
            {
                string graphUrl;
                if ((collection as IDataModelWithContext).PnPContext.Environment == Microsoft365Environment.Custom)
                {
                    graphUrl = $"https://{(collection as IDataModelWithContext).PnPContext.MicrosoftGraphAuthority}/";
                }
                else
                {
                    graphUrl = $"https://{CloudManager.GetMicrosoftGraphAuthority((collection as IDataModelWithContext).PnPContext.Environment.Value)}/";
                }

                nextLink = collection.Metadata[PnPConstants.GraphNextLink].Replace($"{graphUrl}{PnPConstants.GraphV1Endpoint}/", "");
                nextLinkApiType = ApiType.Graph;
            }
            else if (!string.IsNullOrEmpty(collection.Metadata[PnPConstants.SharePointRestListItemNextLink]))
            {
                nextLink = CleanUpUrlParameters(collection.Metadata[PnPConstants.SharePointRestListItemNextLink]);
                nextLinkApiType = ApiType.SPORest;
            }
            else
            {
                throw new ClientException(ErrorType.Unsupported,
                    PnPCoreResources.Exception_InvalidNextPage);
            }

            return new Tuple<string, ApiType>(nextLink, nextLinkApiType);
        }

        private static string CleanUpUrlParameters(string url)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                NameValueCollection queryString = HttpUtility.ParseQueryString(uri.Query);
                if (queryString["$skiptoken"] != null && queryString["$skip"] != null)
                {
                    // $skiptoken and $skip cannot be combined, removing $skip in this case
                    queryString.Remove("$skip");
                    return $"{uri.Scheme}://{uri.DnsSafeHost}{uri.AbsolutePath}?{queryString.ToEncodedString()}";
                }
            }

            return url;
        }

        #endregion

        #endregion

        #region UPDATE
        internal static async Task<ApiCallRequest> BuildUpdateAPICallAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, bool viaBatchMethod)
        {
            bool useGraph = false;

            // If entity cannot be surfaced with SharePoint Rest then force graph
            if (string.IsNullOrEmpty(entity.SharePointType))
            {
                useGraph = true;
            }

            if (useGraph)
            {
                return await BuildUpdateAPICallGraphAsync(model, entity, viaBatchMethod).ConfigureAwait(false);
            }
            else
            {
                return await BuildUpdateAPICallRestAsync(model, entity, viaBatchMethod).ConfigureAwait(false);
            }
        }

        private static async Task<ApiCallRequest> BuildUpdateAPICallGraphAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, bool viaBatchMethod)
        {
            ApiType apiType = ApiType.Graph;

            if (entity.GraphBeta)
            {
                if (CanUseGraphBeta(model, entity))
                {
                    apiType = ApiType.GraphBeta;
                }
                else
                {
                    // we can't make this request
                    var cancelledApiCallRequest = new ApiCallRequest(default);
                    cancelledApiCallRequest.CancelRequest($"Updating {entity.GraphUpdate} requires the Graph Beta endpoint which was not configured to be allowed");
                    return cancelledApiCallRequest;
                }
            }

            IEnumerable<EntityFieldInfo> fields = entity.Fields;

            // Define the JSON body of the update request based on the actual changes
            dynamic updateMessage = new ExpandoObject();

            foreach (PropertyDescriptor cp in model.ChangedProperties)
            {
                // Look for the corresponding property in the type
                var changedField = fields.FirstOrDefault(f => f.Name == cp.Name);

                // If we found a field 
                if (changedField != null)
                {
                    bool addField = true;
                    if (changedField.GraphBeta)
                    {
                        if (CanUseGraphBeta(model, changedField))
                        {
                            apiType = ApiType.GraphBeta;
                        }
                        else
                        {
                            addField = false;
                        }
                    }
                    else
                    {
                        // What if the complex type we're updating contains a beta property
                        if (!model.PnPContext.GraphCanUseBeta)
                        {
                            addField = false;
                        }
                    }

                    if (addField)
                    {
                        if (changedField.DataType.FullName == typeof(TransientDictionary).FullName)
                        {
                            // Get the changed properties in the dictionary
                            var dictionaryObject = (TransientDictionary)cp.GetValue(model);
                            foreach (KeyValuePair<string, object> changedProp in dictionaryObject.ChangedProperties)
                            {
                                // Let's set its value into the update message
                                ((ExpandoObject)updateMessage).SetProperty(changedProp.Key, changedProp.Value);
                            }
                        }
                        else if (JsonMappingHelper.IsTypeWithoutGet(changedField.PropertyInfo.PropertyType))
                        {
                            // Build a new dynamic object that will hold the changed properties of the complex type
                            dynamic updateMessageComplexType = new ExpandoObject();
                            var complexObject = model.GetValue(changedField.Name) as TransientObject;

                            // Get the properties that have changed in the complex type
                            foreach (PropertyDescriptor changedProp in complexObject.ChangedProperties)
                            {
                                ((ExpandoObject)updateMessageComplexType).SetProperty(changedProp.Name, complexObject.GetValue(changedProp.Name));
                            }
                            // Add this as value to the original changed property
                            ((ExpandoObject)updateMessage).SetProperty(changedField.GraphName, updateMessageComplexType as object);
                        }
                        else
                        {
                            // Let's set its value into the update message, assuming it's a not a model or model collection as those have to be updated by
                            // calling update on their respective models
                            if (!(model.GetValue(changedField.Name) is IDataModelWithContext))
                            {
                                // Let's set its value into the update message
                                ((ExpandoObject)updateMessage).SetProperty(changedField.GraphName, model.GetValue(changedField.Name));
                            }
                        }
                    }
                }
            }

            // If defined, call the Action to expand the update payload
            model?.ExpandUpdatePayLoad?.Invoke((ExpandoObject)updateMessage);

            // Get the corresponding JSON text content
            var jsonUpdateMessage = JsonSerializer.Serialize(updateMessage,
                typeof(ExpandoObject),
                PnPConstants.JsonSerializer_WriteIndentedFalse_CamelCase_JsonStringEnumConverter);

            // Prepare the variable to contain the target URL for the update operation
            var updateUrl = await ApiHelper.ParseApiCallAsync(model, entity.GraphUpdate).ConfigureAwait(false);

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(updateUrl, apiType, jsonUpdateMessage)
            {
                Commit = true,
                AddedViaBatchMethod = viaBatchMethod
            });
            if (model.UpdateApiCallOverrideHandler != null)
            {
                call = await model.UpdateApiCallOverrideHandler.Invoke(call).ConfigureAwait(false);
            }
            // If the call was cancelled by the override then bail out
            if (call.Cancelled)
            {
                return call;
            }

            // If a field validation prevented updating a field it might mean there's nothing to update, if so then don't do a server request
            if (string.IsNullOrEmpty(call.ApiCall.JsonBody) || call.ApiCall.JsonBody == "{}")
            {
                call.CancelRequest("No changes (empty body), so nothing to update");
            }

            return call;
        }

        private static async Task<ApiCallRequest> BuildUpdateAPICallRestAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, bool viaBatchMethod)
        {
            IEnumerable<EntityFieldInfo> fields = entity.Fields;

            // Define the JSON body of the update request based on the actual changes
            dynamic updateMessage = new ExpandoObject();

            // Configure the metadata section of the update request
            if (model.Metadata.ContainsKey(PnPConstants.MetaDataType))
            {
                updateMessage.__metadata = new
                {
                    type = model.Metadata[PnPConstants.MetaDataType]
                };
            }

            foreach (PropertyDescriptor cp in model.ChangedProperties)
            {
                // Look for the corresponding property in the type
                var changedField = fields.FirstOrDefault(f => f.Name == cp.Name);

                // If we found a field 
                if (changedField != null)
                {
                    if (changedField.DataType.FullName == typeof(TransientDictionary).FullName)
                    {
                        // Get the changed properties in the dictionary
                        var dictionaryObject = (TransientDictionary)cp.GetValue(model);
                        foreach (KeyValuePair<string, object> changedProp in dictionaryObject.ChangedProperties)
                        {
                            ((ExpandoObject)updateMessage).SetProperty(changedProp.Key, changedProp.Value);
                        }
                    }
                    else if (JsonMappingHelper.IsTypeWithoutGet(changedField.PropertyInfo.PropertyType))
                    {
                        // Build a new dynamic object that will hold the changed properties of the complex type
                        dynamic updateMessageComplexType = new ExpandoObject();
                        var complexObject = model.GetValue(changedField.Name) as TransientObject;

                        // Get the properties that have changed in the complex type
                        foreach (PropertyDescriptor changedProp in complexObject.ChangedProperties)
                        {
                            ((ExpandoObject)updateMessageComplexType).SetProperty(changedProp.Name, complexObject.GetValue(changedProp.Name));
                        }

                        // Add this as value to the original changed property
                        ((ExpandoObject)updateMessage).SetProperty(changedField.SharePointName, updateMessageComplexType as object);
                    }
                    else if (model.GetValue(changedField.Name) is List<string>)
                    {
                        // multi value field
                        ((ExpandoObject)updateMessage).SetProperty(changedField.SharePointName, FieldValueCollection.StringArrayToJson(model.GetValue(changedField.Name) as List<string>));
                    }
                    else if (model.GetValue(changedField.Name) is List<int>)
                    {
                        // multi value field
                        ((ExpandoObject)updateMessage).SetProperty(changedField.SharePointName, FieldValueCollection.IntArrayToJson(model.GetValue(changedField.Name) as List<int>));
                    }
                    else
                    {
                        // Let's set its value into the update message, assuming it's a not a model or model collection as those have to be updated by
                        // calling update on their respective models
                        if (!(model.GetValue(changedField.Name) is IDataModelWithContext))
                        {
                            ((ExpandoObject)updateMessage).SetProperty(changedField.SharePointName, model.GetValue(changedField.Name));
                        }
                    }
                }
            }

            // Get the corresponding JSON text content
            var jsonUpdateMessage = JsonSerializer.Serialize(updateMessage,
                typeof(ExpandoObject),
                PnPConstants.JsonSerializer_WriteIndentedTrue);

            // Prepare the variable to contain the target URL for the update operation
            var updateUrl = await ApiHelper.ParseApiCallAsync(model, $"{model.PnPContext.Uri.AbsoluteUri.ToString().TrimEnd(new char[] { '/' })}/{entity.SharePointUpdate}").ConfigureAwait(false);

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(updateUrl, ApiType.SPORest, jsonUpdateMessage)
            {
                Commit = true,
                AddedViaBatchMethod = viaBatchMethod
            });

            if (model.UpdateApiCallOverrideHandler != null)
            {
                call = await model.UpdateApiCallOverrideHandler.Invoke(call).ConfigureAwait(false);
            }

            // If the call was cancelled by the override then bail out
            if (call.Cancelled)
            {
                return call;
            }

            // If a field validation prevented updating a field it might mean there's nothing to update, if so then don't do a server request
            if (string.IsNullOrEmpty(call.ApiCall.JsonBody) || call.ApiCall.JsonBody == "{}")
            {
                call.CancelRequest("No changes (empty body), so nothing to update");
            }

            return call;
        }

        #endregion

        #region DELETE
        internal static async Task<ApiCallRequest> BuildDeleteAPICallAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, bool viaBatchMethod)
        {
            bool useGraph = false;

            // If entity cannot be surfaced with SharePoint Rest then force graph
            if (string.IsNullOrEmpty(entity.SharePointType))
            {
                useGraph = true;
            }

            if (useGraph)
            {
                return await BuildDeleteAPICallGraphAsync(model, entity, viaBatchMethod).ConfigureAwait(false);
            }
            else
            {
                return await BuildDeleteAPICallRestAsync(model, entity, viaBatchMethod).ConfigureAwait(false);
            }
        }

        private static async Task<ApiCallRequest> BuildDeleteAPICallGraphAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, bool viaBatchMethod)
        {
            ApiType apiType = ApiType.Graph;

            if (entity.GraphBeta)
            {
                if (CanUseGraphBeta(model, entity))
                {
                    apiType = ApiType.GraphBeta;
                }
                else
                {
                    // we can't make this request
                    var cancelledApiCallRequest = new ApiCallRequest(default);
                    cancelledApiCallRequest.CancelRequest($"Deleting {entity.GraphDelete} requires the Graph Beta endpoint which was not configured to be allowed");
                    return cancelledApiCallRequest;
                }
            }

            // Prepare the variable to contain the target URL for the delete operation
            var deleteUrl = await ApiHelper.ParseApiCallAsync(model, entity.GraphDelete).ConfigureAwait(false);

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(deleteUrl, apiType)
            {
                AddedViaBatchMethod = viaBatchMethod
            });
            if (model.DeleteApiCallOverrideHandler != null)
            {
                call = await model.DeleteApiCallOverrideHandler.Invoke(call).ConfigureAwait(false);
            }

            return call;
        }

        private static async Task<ApiCallRequest> BuildDeleteAPICallRestAsync<TModel>(BaseDataModel<TModel> model, EntityInfo entity, bool viaBatchMethod)
        {
            // Prepare the variable to contain the target URL for the delete operation
            var deleteUrl = await ApiHelper.ParseApiCallAsync(model, $"{model.PnPContext.Uri.AbsoluteUri.ToString().TrimEnd(new char[] { '/' })}/{entity.SharePointDelete}").ConfigureAwait(false);

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(deleteUrl, ApiType.SPORest)
            {
                AddedViaBatchMethod = viaBatchMethod
            });
            if (model.DeleteApiCallOverrideHandler != null)
            {
                call = await model.DeleteApiCallOverrideHandler.Invoke(call).ConfigureAwait(false);
            }

            return call;
        }

        #endregion

        #region Helper methods

        private static bool CanUseGraphBeta<TModel>(BaseDataModel<TModel> model, EntityFieldInfo field)
        {
            return model.PnPContext.GraphCanUseBeta && field.GraphBeta;
        }

        private static bool CanUseGraphBeta<TModel>(BaseDataModel<TModel> model, EntityInfo entity)
        {
            return model.PnPContext.GraphCanUseBeta && entity.GraphBeta;
        }

        #endregion

    }
}
