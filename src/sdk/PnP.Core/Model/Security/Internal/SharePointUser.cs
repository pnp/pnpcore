using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
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
                    
                    if (entity.Target == typeof(Web))
                    {
                        return new ApiCall("_api/Web/SiteUsers", ApiType.SPORest, jsonBody);
                    }
                    else
                    {
                        return new ApiCall(entity.SharePointGet, ApiType.SPORest, jsonBody);
                    }
                    

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

            await EnsureIdentityPropertiesAsync().ConfigureAwait(false);

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

        public async Task<IList<ISharePointGroup>> GetTransitiveGroupsAsync()
        {
            // One request for all site groups and their members: SharePoint groups cannot be nested, so every
            // indirect membership has to come from an Entra ID or Microsoft 365 group claim inside one of them
            var siteGroups = await PnPContext.Web.SiteGroups
                .QueryProperties(g => g.All,
                                 g => g.Users.QueryProperties(u => u.Id, u => u.LoginName, u => u.PrincipalType, u => u.AadObjectId))
                .ToListAsync().ConfigureAwait(false);

            var groupMembers = siteGroups
                .Select(group => (Group: group, Members: group.Users.AsRequested()
                    .Select(member =>
                    {
                        // Special claims such as "Everyone except external users" do not return an object id at all
                        var aadObjectId = member.IsPropertyAvailable(m => m.AadObjectId) ? member.AadObjectId : null;
                        return (member.Id, Claim: SharePointGroupMembership.Classify(member.PrincipalType, member.LoginName, aadObjectId), AadObjectId: aadObjectId);
                    })
                    .ToList()))
                .ToList();

            var memberOfGroupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ownedGroupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await EnsureIdentityPropertiesAsync().ConfigureAwait(false);

            // Principals without an Entra ID object (e.g. system accounts) can only be direct members
            if (Guid.TryParse(AadObjectId, out _))
            {
                var claims = groupMembers.SelectMany(g => g.Members).ToList();

                var memberClaimIds = DistinctGroupIds(claims, SharePointGroupMembership.GroupClaim.Members);
                if (memberClaimIds.Count > 0)
                {
                    memberOfGroupIds.UnionWith(await CheckMemberGroupsAsync(memberClaimIds).ConfigureAwait(false));
                }

                // Ownership is only looked up when the site actually uses a Microsoft 365 group owners claim,
                // and only users can own a Microsoft 365 group
                var ownerClaimIds = DistinctGroupIds(claims, SharePointGroupMembership.GroupClaim.Owners);
                if (PrincipalType == PrincipalType.User && ownerClaimIds.Count > 0)
                {
                    ownedGroupIds.UnionWith(await GetOwnedGroupIdsAsync(ownerClaimIds).ConfigureAwait(false));
                }
            }

            return groupMembers
                .Where(g => SharePointGroupMembership.GrantsMembership(Id, g.Members, memberOfGroupIds, ownedGroupIds))
                .Select(g => g.Group)
                .ToList();
        }

        public IList<ISharePointGroup> GetTransitiveGroups()
        {
            return GetTransitiveGroupsAsync().GetAwaiter().GetResult();
        }

        private static List<string> DistinctGroupIds(IEnumerable<(int Id, SharePointGroupMembership.GroupClaim Claim, string AadObjectId)> claims, SharePointGroupMembership.GroupClaim claim)
        {
            return claims
                .Where(c => c.Claim == claim)
                .Select(c => c.AadObjectId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<IEnumerable<string>> CheckMemberGroupsAsync(List<string> groupIds)
        {
            // The resource specific endpoint is used rather than /directoryObjects, because the latter requires
            // Directory.Read.All while these only need GroupMember.Read.All next to reading the principal itself
            var resource = PrincipalType == PrincipalType.SecurityGroup ? "groups" : "users";

            // checkMemberGroups is transitive, so a group nested inside another group is covered as well
            var requests = new List<BatchRequest>();
            var batch = PnPContext.NewBatch();
            foreach (var chunk in SharePointGroupMembership.ToBatches(groupIds, SharePointGroupMembership.CheckMemberGroupsBatchSize))
            {
                var body = JsonSerializer.Serialize(new { groupIds = chunk });
                requests.Add(await RawRequestBatchAsync(batch, new ApiCall($"{resource}/{AadObjectId}/checkMemberGroups", ApiType.Graph, body), HttpMethod.Post, "CheckMemberGroups").ConfigureAwait(false));
            }
            await PnPContext.ExecuteAsync(batch).ConfigureAwait(false);

            return requests.SelectMany(request => ReadStringValues(request.ResponseJson)).ToList();
        }

        private async Task<IEnumerable<string>> GetOwnedGroupIdsAsync(List<string> groupIds)
        {
            // Reading the owners of each group rather than the user's ownedObjects, as the latter does not
            // support application permissions
            var ownedGroupIds = new List<string>();
            foreach (var groupId in groupIds)
            {
                if (await IsOwnerAsync(groupId).ConfigureAwait(false))
                {
                    ownedGroupIds.Add(groupId);
                }
            }
            return ownedGroupIds;
        }

        private async Task<bool> IsOwnerAsync(string groupId)
        {
            var graphRoot = $"{CloudManager.GetGraphBaseUrl(PnPContext)}{PnPConstants.GraphV1Endpoint}/";
            string request = $"groups/{groupId}/owners?$select=id";

            while (!string.IsNullOrEmpty(request))
            {
                var response = await RawRequestAsync(new ApiCall(request, ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

                if (json.TryGetProperty("value", out JsonElement value)
                    && value.EnumerateArray().Any(owner => owner.TryGetProperty("id", out JsonElement id)
                                                           && id.GetString().Equals(AadObjectId, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                // The next link is absolute, while ApiCall expects a url relative to the Graph version root
                request = json.TryGetProperty(PnPConstants.GraphNextLink, out JsonElement nextLink)
                    ? nextLink.GetString().Replace(graphRoot, "")
                    : null;
            }

            return false;
        }

        private static IEnumerable<string> ReadStringValues(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return Enumerable.Empty<string>();
            }

            var parsed = JsonSerializer.Deserialize<JsonElement>(json);
            return parsed.TryGetProperty("value", out JsonElement value)
                ? value.EnumerateArray().Select(id => id.GetString()).ToList()
                : Enumerable.Empty<string>();
        }

        private async Task EnsureIdentityPropertiesAsync()
        {
            // Callers may have loaded the user with a partial property set, e.g. only UserPrincipalName and AadObjectId
            if (!IsPropertyAvailable(p => p.AadObjectId) || !IsPropertyAvailable(p => p.PrincipalType))
            {
                // Try loading the current user again with the aadobjectid property also loaded next all other user properties
                var apiCall = new ApiCall($"_api/Web/GetUserById({Id})?$select=id,ishiddeninUI,loginname,title,principaltype,aadobjectid,email,expiration,IsEmailAuthenticationGuestUser,IsShareByEmailGuestUser,userprincipalname,issiteadmin,userid", ApiType.SPORest);
                await RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
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
                var roleDefinition = await PnP.Core.QueryModel.QueryableExtensions.FirstOrDefaultAsync(
                    PnPContext.Web.RoleDefinitions, d => d.Name == name).ConfigureAwait(false);
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
