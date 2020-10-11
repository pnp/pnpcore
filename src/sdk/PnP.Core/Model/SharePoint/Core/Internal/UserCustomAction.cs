using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// UserCustomAction class, write your custom code here
    /// </summary>
    [SharePointType("SP.UserCustomAction", Target = typeof(Web), Uri = "_api/Web/UserCustomActions('{Id}')", Get = "_api/Web/UserCustomActions", LinqGet = "_api/Web/UserCustomActions")]
    [SharePointType("SP.UserCustomAction", Target = typeof(Site), Uri = "_api/Site/UserCustomActions('{Id}')", Get = "_api/Site/UserCustomActions", LinqGet = "_api/Site/UserCustomActions")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class UserCustomAction
    {
        internal const string AddUserCustomActionOptionsAdditionalInformationKey = "AddOptions";

        public UserCustomAction()
        {
            MappingHandler = (FromJson input) =>
            {
                // implement custom mapping logic
                switch (input.TargetType.Name)
                {
                    case "UserCustomActionScope": return JsonMappingHelper.ToEnum<UserCustomActionScope>(input.JsonElement);
                    case "UserCustomActionRegistrationType": return JsonMappingHelper.ToEnum<UserCustomActionRegistrationType>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var addOptions = (AddUserCustomActionOptions)additionalInformation[AddUserCustomActionOptionsAdditionalInformationKey];
                var entity = EntityManager.GetClassInfo(GetType(), this);
                string endpointUrl = entity.SharePointGet;

                ExpandoObject baseAddPayload = new
                {
                    __metadata = new { type = entity.SharePointType }
                }.AsExpando();

                dynamic addParameters = baseAddPayload.MergeWith(addOptions.AsExpando(ignoreNullValues:true));
                string json = JsonSerializer.Serialize(addParameters, typeof(ExpandoObject),
                    new JsonSerializerOptions()
                    {
                        IgnoreNullValues = true
                    });

                return new ApiCall(endpointUrl, ApiType.SPORest, json);
            };
        }
    }
}
