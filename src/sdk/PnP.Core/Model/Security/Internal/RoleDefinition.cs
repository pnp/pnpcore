using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.RoleDefinition", Target = typeof(Web), Uri = "_api/Web/RoleDefinitions({Id})", Get = "_api/web/RoleDefinitions", LinqGet = "_api/web/RoleDefinitions")]
    [SharePointType("SP.RoleDefinition", Target = typeof(RoleAssignment), Uri = "_api/Web/RoleAssignments/GetByPrincipalId({Parent.Id})/RoleDefinitionBindings({Id})", Get = "_api/Web/RoleAssignments/GetByPrincipalId({Parent.Id})/RoleDefinitionBindings", LinqGet = "_api/Web/RoleAssignments/GetByPrincipalId({Parent.Id})/RoleDefinitionBindings")]
    internal sealed class RoleDefinition : BaseDataModel<IRoleDefinition>, IRoleDefinition
    {

        public RoleDefinition()
        {
            AddApiCallHandler = async (keyValuePairs) =>
            {
                return await Task.Run(() =>
                {
                    var parent2 = Parent?.Parent;
                    if (parent2 is Web)
                    {
                        var endpointUri = $"_api/web/roledefinitions";
                        var basePermissions = (BasePermissions)keyValuePairs["permissions"];
                        var body = new
                        {
                            __metadata = new { type = "SP.RoleDefinition" },
                            Name,
                            Description,
                            Hidden,
                            Order,
                            RoleTypeKind = (int)RoleTypeKind,
                            BasePermissions = new
                            {
                                __metadata = new { type = "SP.BasePermissions" },
                                High = basePermissions.High.ToString(),
                                Low = basePermissions.Low.ToString()
                            }
                        };
                        var jsonBody = JsonSerializer.Serialize(body);
                        return new ApiCall(endpointUri, ApiType.SPORest, jsonBody);
                    }
                    throw new System.Exception("You can only create new role definitions on root web level, PnPContext.Web.RoleDefinitions");
                }).ConfigureAwait(false);
            };

            UpdateApiCallOverrideHandler = async (ApiCallRequest input) =>
            {
                return await Task.Run(() =>
                {
                    var parent2 = Parent?.Parent;
                    if (parent2 is Web)
                    {
                        var endpointUri = $"_api/web/roledefinitions({Id})";
                        var body = new
                        {
                            __metadata = new { type = "SP.RoleDefinition" },
                            Name,
                            Description,
                            Hidden,
                            Order,
                            RoleTypeKind = (int)RoleTypeKind,
                            BasePermissions = new
                            {
                                __metadata = new { type = "SP.BasePermissions" },
                                High = BasePermissions.High.ToString(),
                                Low = BasePermissions.Low.ToString()
                            }
                        };
                        var jsonBody = JsonSerializer.Serialize(body);

                        return new ApiCallRequest(new ApiCall($"{PnPContext.Uri.AbsoluteUri}/{endpointUri}", ApiType.SPORest, jsonBody));
                    }
                    throw new System.Exception("You can only create new role definitions on root web level, PnPContext.Web.RoleDefinitions");
                }).ConfigureAwait(false);
            };

            DeleteApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
                {
                    return await Task.Run(() =>
                    {
                        var parent2 = Parent?.Parent;
                        if (!(parent2 is Web))
                        {
                            apiCallRequest.CancelRequest("You can only delete role definitions on root web level, PnPContext.Web.RoleDefinitions.");
                        }

                        return apiCallRequest;
                    }).ConfigureAwait(false);
                };
        }

        public IBasePermissions BasePermissions { get => GetModelValue<IBasePermissions>(); set => SetModelValue(value); }
        public string Description { get => GetValue<string>(); set => SetValue(value); }
        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }
        public string Name { get => GetValue<string>(); set => SetValue(value); }
        public int Order { get => GetValue<int>(); set => SetValue(value); }
        public RoleType RoleTypeKind { get => GetValue<RoleType>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = int.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

    }
}
