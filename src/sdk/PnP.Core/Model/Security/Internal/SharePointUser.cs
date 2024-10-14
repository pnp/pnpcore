using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.User", Target = typeof(SharePointGroup), Uri = "_api/Web/sitegroups/getbyid({Parent.Id})/users/getbyid({Id})", Get = "_api/Web/sitegroups/getbyid({Parent.Id})/users", LinqGet = "_api/Web/sitegroups/getbyid({Parent.Id})/users")]
    [SharePointType("SP.User", Target = typeof(Web), Uri = "_api/Web/GetUserById({Id})", LinqGet = "_api/Web/SiteUsers")]
    internal sealed class SharePointUser : BaseDataModel<ISharePointUser>, ISharePointUser
    {
        #region Construction
        public SharePointUser()
        {
            AddApiCallHandler = async (keyValuePairs) =>
            {
                return await Task.Run(() =>
                {
                    var entity = EntityManager.GetClassInfo(GetType(), this);

                    var body = new
                    {
                        __metadata = new { type = "SP.User" },
                        LoginName
                    };
                    var jsonBody = JsonSerializer.Serialize(body);
                    return new ApiCall("_api/Web/SiteUsers", ApiType.SPORest, jsonBody);

                }).ConfigureAwait(false);
            };
        }
        #endregion

        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("AadObjectId", JsonPath = "NameId")]
        public string AadObjectId { get => GetValue<string>(); set => SetValue(value); }

        public bool IsHiddenInUI { get => GetValue<bool>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public PrincipalType PrincipalType { get => GetValue<PrincipalType>(); set => SetValue(value); }

        public string Department { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Email")]
        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        public string Expiration { get => GetValue<string>(); set => SetValue(value); }

        public bool IsEmailAuthenticationGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsShareByEmailGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAdmin { get => GetValue<bool>(); set => SetValue(value); }

        public ISharePointGroupCollection Groups { get => GetModelCollectionValue<ISharePointGroupCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = int.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion

        #region Extension methods
        public async Task<IGraphUser> AsGraphUserAsync()
        {
            if (!IsPropertyAvailable(p => p.PrincipalType) || (PrincipalType != PrincipalType.User && PrincipalType != PrincipalType.SecurityGroup))
            {
                throw new ClientException(ErrorType.Unsupported,
                    PnPCoreResources.Exception_Unsupported_GraphUserOnSharePoint);
            }

            if (!IsPropertyAvailable(p => p.AadObjectId))
            {
                // Try loading the current user again with the aadobjectid property also loaded next all other user properties
                var apiCall = new ApiCall($"_api/Web/GetUserById({Id})?$select=id,ishiddeninUI,loginname,title,principaltype,aadobjectid,email,expiration,IsEmailAuthenticationGuestUser,IsShareByEmailGuestUser,userprincipalname,issiteadmin,userid", ApiType.SPORest);
                await RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
            }

            // Check again for principal type
            if (PrincipalType != PrincipalType.User && PrincipalType != PrincipalType.SecurityGroup)
            {
                throw new ClientException(ErrorType.Unsupported,
                    PnPCoreResources.Exception_Unsupported_GraphUserOnSharePoint);
            }

            if (string.IsNullOrEmpty(AadObjectId))
            {
                throw new ClientException(ErrorType.Unsupported,
                    PnPCoreResources.Exception_Unsupported_GraphUserOnSharePointNoGraphId);
            }

            GraphUser graphUser = new GraphUser
            {
                PnPContext = PnPContext,
                Id = AadObjectId,
                UserPrincipalName = UserPrincipalName,
                Mail = Mail,
            };

            // Populate the graph metadata
            graphUser.AddMetadata(PnPConstants.MetaDataGraphType, "#microsoft.graph.user");
            graphUser.AddMetadata(PnPConstants.MetaDataGraphId, AadObjectId);

            return graphUser;
        }

        public IGraphUser AsGraphUser()
        {
            return AsGraphUserAsync().GetAwaiter().GetResult();
        }

        public IRoleDefinitionCollection GetRoleDefinitions()
        {
            return GetRoleDefinitionsAsync().GetAwaiter().GetResult();
        }

        public async Task<IRoleDefinitionCollection> GetRoleDefinitionsAsync()
        {
            var roleAssignment = await PnPContext.Web.RoleAssignments
                .QueryProperties(r => r.RoleDefinitions)
                .FirstOrDefaultAsync(p => p.PrincipalId == Id).ConfigureAwait(false);
            return roleAssignment?.RoleDefinitions;
        }

        public bool AddRoleDefinitions(params string[] names)
        {
            return AddRoleDefinitionsAsync(names).GetAwaiter().GetResult();
        }

        public async Task<bool> AddRoleDefinitionsAsync(params string[] names)
        {
            var result = false;
            foreach (var name in names)
            {
                var roleDefinition = await PnPContext.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == name).ConfigureAwait(false);
                if (roleDefinition != null)
                {
                    var apiCall = new ApiCall($"_api/web/roleassignments/addroleassignment(principalid={Id},roledefid={roleDefinition.Id})", ApiType.SPORest);
                    var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                    result = response.StatusCode == System.Net.HttpStatusCode.OK;
                }
                else
                {
                    throw new ArgumentException($"Role definition '{name}' not found.");
                }
            }
            return result;
        }

        public bool RemoveRoleDefinitions(params string[] names)
        {
            return RemoveRoleDefinitionsAsync(names).GetAwaiter().GetResult();
        }

        public async Task<bool> RemoveRoleDefinitionsAsync(params string[] names)
        {
            var result = false;
            foreach (var name in names)
            {
                var roleDefinitions = await GetRoleDefinitionsAsync().ConfigureAwait(false);

                var roleDefinition = roleDefinitions.AsRequested().FirstOrDefault(r => r.Name == name);
                if (roleDefinition != null)
                {
                    var apiCall = new ApiCall($"_api/web/roleassignments/removeroleassignment(principalid={Id},roledefid={roleDefinition.Id})", ApiType.SPORest);
                    var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                    result = response.StatusCode == System.Net.HttpStatusCode.OK;
                }
                else
                {
                    throw new ArgumentException($"Role definition '{name}' not found for this user.");
                }
            }
            return result;
        }
        #endregion
    }
}
