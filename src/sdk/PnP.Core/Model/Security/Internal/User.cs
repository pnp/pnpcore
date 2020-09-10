using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [GraphType(Get = "users/{GraphId}", Id = "userPrincipalName")]
    [SharePointType("SP.User", Uri = "_api/Web/GetUserById({Id})", LinqGet = "_api/Web/SiteUsers")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class User
    {
        public User()
        {
            PostMappingHandler = (json) =>
            {
                if (HasValue(nameof(SharePointId)))
                {
                    AddMetadata(PnPConstants.MetaDataRestId, $"{SharePointId}");
                }
                if (HasValue(nameof(GraphId)))
                {
                    AddMetadata(PnPConstants.MetaDataGraphId, GraphId);
                }
                else if (HasValue(nameof(UserPrincipalName)))
                {
                    // NOTE Will not work for external users that contain "#EXT#" in their UPN, it won't be allowed as a safe URL part when calling Graph
                    // TODO Come up with some workaround, to query filter instead of get by Id for external users
                    AddMetadata(PnPConstants.MetaDataGraphId, UserPrincipalName);
                }
            };

            MappingHandler = (FromJson input) =>
            {
                // implement custom mapping logic
                switch (input.TargetType.Name)
                {
                    case nameof(PrincipalType): return JsonMappingHelper.ToEnum<PrincipalType>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal override async Task RestToGraphMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (HasValue(nameof(UserPrincipalName)))
            {
                AddMetadata(PnPConstants.MetaDataGraphId, UserPrincipalName);
            }
        }

        #region Extension methods
        public async Task LoadSharePointPropertiesAsync(bool ensureUser = true)
        {
            if (!HasValue(nameof(UserPrincipalName)))
                throw new ClientException(ErrorType.PropertyNotLoaded, "The UserPrincipalName has not been initialized. Loading the user SharePoint properties is not possible.");

            if (ensureUser)
            {
                var parameters = new
                {
                    logonName = $"i:0#.f|membership|{UserPrincipalName}"
                }.AsExpando();
                string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));

                var apiCall = new ApiCall("_api/Web/EnsureUser", ApiType.SPORest, body);

                await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                try
                {
                    // If not using ensure user, we need to query the users filtering on loginName
                    var apiCall = new ApiCall($"_api/Web/SiteUsers?$filter=UserPrincipalName eq '{UserPrincipalName}'", ApiType.SPORest);
                    ApiCallResponse apiCallResponse = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
                    var entityInfo = EntityManager.Instance.GetClassInfo(GetType(), this);
                    await JsonMappingHelper.MapJsonToModel(this, entityInfo, apiCallResponse, dataRootElementPath: "results[0]").ConfigureAwait(false);
                }
                catch (ClientException ex)
                {
                    throw new SharePointRestServiceException("An error occured while trying to load SharePoint properties of the user", ex);
                }
            }
        }
        #endregion
    }
}
