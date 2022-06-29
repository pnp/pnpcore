using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.Group", Uri = "_api/Web/sitegroups/getbyid({Id})", LinqGet = "_api/Web/SiteGroups", Delete = "_api/Web/SiteGroups/RemoveById({Id})")]
    internal sealed class SharePointGroup : BaseDataModel<ISharePointGroup>, ISharePointGroup
    {
        #region Construction
        public SharePointGroup()
        {
            AddApiCallHandler = async (keyValuePairs) =>
            {
                return await Task.Run(() =>
                {
                    var endPointUri = $"_api/web/sitegroups";

                    var body = new
                    {
                        __metadata = new { type = "SP.Group" },
                        Title,
                    };
                    var jsonBody = JsonSerializer.Serialize(body);
                    return new ApiCall(endPointUri, ApiType.SPORest, jsonBody);

                }).ConfigureAwait(false);
            };

            ValidateUpdateHandler = (PropertyUpdateRequest propertyUpdateRequest) =>
            {
                if (propertyUpdateRequest.PropertyName == nameof(Description))
                {
                    propertyUpdateRequest.Value = HtmlToText.ConvertSimpleHtmlToText(propertyUpdateRequest.Value.ToString(), 511);
                }
            };
        }
        #endregion

        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsHiddenInUI { get => GetValue<bool>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public PrincipalType PrincipalType { get => GetValue<PrincipalType>(); set => SetValue(value); }

        public bool AllowMembersEditMembership { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowRequestToJoinLeave { get => GetValue<bool>(); set => SetValue(value); }

        public bool AutoAcceptRequestToJoinLeave { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserEditMembership { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserManageGroup { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserViewMembership { get => GetValue<bool>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public bool OnlyAllowMembersViewMembership { get => GetValue<bool>(); set => SetValue(value); }

        public string OwnerTitle { get => GetValue<string>(); set => SetValue(value); }

        public string RequestToJoinLeaveEmailSetting { get => GetValue<string>(); set => SetValue(value); }

        public ISharePointUserCollection Users { get => GetModelCollectionValue<ISharePointUserCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = int.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion

        #region Methods

        public void AddUser(string loginName)
        {
            AddUserAsync(loginName).GetAwaiter().GetResult();
        }

        public async Task AddUserAsync(string loginName)
        {
            await Users.AddAsync(loginName).ConfigureAwait(false);
        }

        public void AddUserBatch(string loginName)
        {
            AddUserBatchAsync(PnPContext.CurrentBatch, loginName).GetAwaiter().GetResult();
        }

        public void AddUserBatch(Batch batch, string loginName)
        {
            AddUserBatchAsync(batch, loginName).GetAwaiter().GetResult();
        }

        public async Task AddUserBatchAsync(string loginName)
        {
            await AddUserBatchAsync(PnPContext.CurrentBatch, loginName).ConfigureAwait(false);
        }

        public async Task AddUserBatchAsync(Batch batch, string loginName)
        {
            await Users.AddBatchAsync(batch, loginName).ConfigureAwait(false);
        }

        public void RemoveUser(int userId)
        {
            RemoveUserAsync(userId).GetAwaiter().GetResult();
        }

        public async Task RemoveUserAsync(int userId)
        {
            await EnsurePropertiesAsync(p => p.Users).ConfigureAwait(false);

            var userToDelete = Users.AsRequested().FirstOrDefault(p=>p.Id == userId);
            if (userToDelete != null)
            {
                await userToDelete.DeleteAsync().ConfigureAwait(false);
            }
            else
            {
                throw new ClientException(ErrorType.InvalidParameters, $"User with id {userId} does not exist in this group");
            }
        }

        public void RemoveUserBatch(int userId)
        {
            RemoveUserBatchAsync(PnPContext.CurrentBatch, userId).GetAwaiter().GetResult();
        }

        public async Task RemoveUserBatchAsync(int userId)
        {
            await RemoveUserBatchAsync(PnPContext.CurrentBatch, userId).ConfigureAwait(false);
        }

        public void RemoveUserBatch(Batch batch, int userId)
        {
            RemoveUserBatchAsync(batch, userId).GetAwaiter().GetResult();
        }

        public async Task RemoveUserBatchAsync(Batch batch, int userId)
        {
            await EnsurePropertiesAsync(p => p.Users).ConfigureAwait(false);

            var userToDelete = Users.AsRequested().FirstOrDefault(p => p.Id == userId);
            if (userToDelete != null)
            {
                await userToDelete.DeleteBatchAsync(batch).ConfigureAwait(false);
            }
            else
            {
                throw new ClientException(ErrorType.InvalidParameters, $"User with id {userId} does not exist in this group");
            }
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
                    throw new ArgumentException($"Role definition '{name}' not found for this group.");
                }
            }
            return result;
        }
        
        public async Task SetUserAsOwnerAsync(int userId)
        {
            await RawRequestAsync(GetSetUserAsOwnerApiCall(userId), HttpMethod.Post).ConfigureAwait(false);
        }

        public void SetUserAsOwner(int userId)
        {
            SetUserAsOwnerAsync(userId).GetAwaiter().GetResult();
        }

        public void SetUserAsOwnerBatch(int userId)
        {
            SetUserAsOwnerBatchAsync(PnPContext.CurrentBatch, userId).GetAwaiter().GetResult();
        }

        public async Task SetUserAsOwnerBatchAsync(int userId)
        {
            await SetUserAsOwnerBatchAsync(PnPContext.CurrentBatch, userId).ConfigureAwait(false);
        }

        public void SetUserAsOwnerBatch(Batch batch, int userId)
        {
            SetUserAsOwnerBatchAsync(batch, userId).GetAwaiter().GetResult();
        }

        public async Task SetUserAsOwnerBatchAsync(Batch batch, int userId)
        {
            await RawRequestBatchAsync(batch, GetSetUserAsOwnerApiCall(userId), HttpMethod.Post).ConfigureAwait(false);
        }

        private ApiCall GetSetUserAsOwnerApiCall(int userId)
        {
            return new ApiCall($"_api/web/sitegroups({Id})/setuserasowner({userId})", ApiType.SPORest);
        }

        #endregion
    }
}