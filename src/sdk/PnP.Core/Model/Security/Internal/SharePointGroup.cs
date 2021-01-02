using PnP.Core.Services;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.Group", Uri = "_api/Web/sitegroups/getbyid({Id})", LinqGet = "_api/Web/SiteGroups")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class SharePointGroup : BaseDataModel<ISharePointGroup>, ISharePointGroup
    {
        #region Construction
        public SharePointGroup()
        {
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

        public bool RequestToJoinLeaveEmailSetting { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = int.Parse(value.ToString()); }

        #endregion

        public ISharePointUserCollection Users { get => GetModelCollectionValue<ISharePointUserCollection>(); }

        public void AddUser(string loginName)
        {
            AddUserAsync(loginName).GetAwaiter().GetResult();
        }

        public async Task AddUserAsync(string loginName)
        {
            var apiCall = GetAddUserApiCall(loginName);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
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
            var apiCall = GetAddUserApiCall(loginName);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private ApiCall GetAddUserApiCall(string loginName)
        {
            // Given this method can apply to multiple parents we're getting the entity info which will 
            // automatically provide the correct 'parent'
            // entity.SharePointGet contains the correct endpoint (e.g. _api/web or _api/web/sitegroups(id) )
            EntityInfo entity = EntityManager.GetClassInfo(typeof(SharePointGroup), this);
            string endpointUrl = $"{entity.SharePointGet}/Users";

            var parameters = new
            {
                __metadata = new { type = "SP.User" },
                LoginName = loginName
            };

            string json = JsonSerializer.Serialize(parameters);
            return new ApiCall(endpointUrl, ApiType.SPORest, json);
        }

        public void RemoveUser(int userId)
        {
            RemoveUserAsync(userId).GetAwaiter().GetResult();
        }

        public async Task RemoveUserAsync(int userId)
        {
            var apiCall = GetRemoveUserApiCall(userId);
            await RawRequestAsync(apiCall, HttpMethod.Delete).ConfigureAwait(false);
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
            var apiCall = GetRemoveUserApiCall(userId);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Delete).ConfigureAwait(false);
        }

        private ApiCall GetRemoveUserApiCall(int userId)
        {
            // Given this method can apply to multiple parents we're getting the entity info which will 
            // automatically provide the correct 'parent'
            // entity.SharePointGet contains the correct endpoint (e.g. _api/web or _api/web/sitegroups(id) )
            EntityInfo entity = EntityManager.GetClassInfo(typeof(SharePointGroup), this);
            string endpointUrl = $"{entity.SharePointGet}/Users/GetById({userId})";
            return new ApiCall(endpointUrl, ApiType.SPORest);
        }
    }
}