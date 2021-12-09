using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    internal abstract class SocialBase<TModel> : BaseDataModel<TModel>
    {
        protected string GetPersonPropertiesSelectQuery(Expression<Func<IPersonProperties, object>>[] selectors)
        {
            var fields = new List<string>();
            foreach (var expression in selectors)
            {
                if (!(expression.Body is MemberExpression))
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_Selector);
                }

                fields.Add(((MemberExpression)expression.Body).Member.Name);
            }

            if (!fields.Contains(nameof(PersonProperties.AccountName)))
            {
                fields.Add(nameof(PersonProperties.AccountName));
            }

            return string.Join(",", fields.ToArray());
        }

        protected async Task<IList<IPersonProperties>> GetGenericPeopleManagerResultsAsync(string baseUrl, params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            var result = await ExecuteWithSelectAsync(baseUrl, selectors).ConfigureAwait(false);

            return (await ParseResultsAsync<PersonProperties>(result.Json, root => root.Get("d")?.Get("results")).ConfigureAwait(false)).Cast<IPersonProperties>().ToList();
        }

        protected async Task<IPersonProperties> GetGenericPeopleManagerResultAsync(string baseUrl, params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            var result = await ExecuteWithSelectAsync(baseUrl, selectors).ConfigureAwait(false);

            return await ParsePersonPropertiesResultAsync(result).ConfigureAwait(false);
        }

        protected static async Task<IList<T>> ParseResultsAsync<T>(string response, Func<JsonElement, JsonElement?> rootResultsSelector) where T : TransientObject, new()
        {
            var personProps = new List<T>();
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            var results = rootResultsSelector(json);

            if (results == null)
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Json_Unexpected);
            }

            var dataRows = results.Value;
            if (dataRows.GetArrayLength() == 0)
            {
                return personProps;
            }

            var entityInfo = EntityManager.Instance.GetStaticClassInfo(typeof(TModel));

            foreach (var row in dataRows.EnumerateArray())
            {
                var prop = new T();
                await JsonMappingHelper.FromJson(prop, entityInfo, new ApiResponse(new ApiCall(String.Empty, ApiType.SPORest), row, Guid.Empty)).ConfigureAwait(false);
                personProps.Add(prop);
            }

            return personProps;
        }

        protected async Task<IPersonProperties> ParsePersonPropertiesResultAsync(ApiCallResponse response)
        {
            var results = new List<IPersonProperties>();
            var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

            if (json.TryGetProperty("d", out JsonElement dRoot))
            {
                var entityInfo = EntityManager.Instance.GetStaticClassInfo(typeof(PersonProperties));

                var prop = new PersonProperties();
                await JsonMappingHelper.FromJson(prop, entityInfo, new ApiResponse(new ApiCall(String.Empty, ApiType.SPORest), dRoot, Guid.Empty)).ConfigureAwait(false);

                return prop;
            }

            return null;
        }

        protected async Task<ApiCallResponse> ExecuteWithSelectAsync(string baseUrl, params Expression<Func<IPersonProperties, object>>[] selectors)
        {
            if (selectors.Any())
            {
                baseUrl = $"{baseUrl}?$select={GetPersonPropertiesSelectQuery(selectors)}";
            }

            var apiCall = new ApiCall(baseUrl, ApiType.SPORest);

            return await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        protected async Task<ApiCallResponse> ExecuteAsync(string baseUrl)
        {
            var apiCall = new ApiCall(baseUrl, ApiType.SPORest);

            return await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        protected async Task<string> GetSingleResult(string baseUrl, string rootPropertyName)
        {
            var response = await ExecuteAsync(baseUrl).ConfigureAwait(false);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

            var result = json.Get("d")?.Get(rootPropertyName);

            if (result == null)
            {
                throw new ClientException(PnPCoreResources.Exception_Json_Unexpected);
            }

            return result.Value.ToString();
        }
    }
}
