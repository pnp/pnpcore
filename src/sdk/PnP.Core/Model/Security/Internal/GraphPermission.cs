using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class GraphPermission : BaseDataModel<IGraphPermission>, IGraphPermission
    {
        #region Constructor
        public GraphPermission(PnPContext context, IDataModelParent parent)
        {
            PnPContext = context;
            Parent = parent;
        }
        #endregion

        #region Properties

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public ISharePointIdentitySet GrantedToV2 { get => GetModelValue<ISharePointIdentitySet>(); set => SetModelValue(value); }

        public List<ISharePointIdentitySet> GrantedToIdentitiesV2 { get => GetValue<List<ISharePointIdentitySet>>(); set => SetModelValue(value); }

        public ISharingInvitation Invitation { get => GetModelValue<ISharingInvitation>(); set => SetModelValue(value); }

        public ISharingLink Link { get => GetModelValue<ISharingLink>(); set => SetModelValue(value); }

        public List<PermissionRole> Roles { get => GetValue<List<PermissionRole>>(); set => SetModelValue(value); }

        public string ShareId { get => GetValue<string>(); set => SetValue(value); }

        public DateTime ExpirationDateTime { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool HasPassword { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion

        #region Methods

        public async Task<IGraphPermission> GrantUserPermissionsAsync(List<IDriveRecipient> recipients)
        {
            if (Link.Scope != ShareScope.Users) 
            {
                throw new Exception("Granting permisisons to an existing permission is only possible on a link of scope 'Users'");
            }

            string base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Link.WebUrl));
            string encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');

            var driveId = string.Empty;
            var driveItemId = string.Empty;

            if (Parent.GetType() == typeof(File))
            {
                var parent = (IFile)Parent;
                driveId = parent.VroomDriveID;
                driveItemId = parent.VroomItemID;
            }
            else if (Parent.GetType() == typeof(Folder))
            {
                var parent = (IFolder)Parent;
                (driveId, driveItemId) = await (parent as Folder).GetGraphIdsAsync().ConfigureAwait(false);
            }
            else if (Parent.GetType() == typeof(ListItem))
            {
                 throw new ClientException(ErrorType.Unsupported, "Granting list item permissions is not supported");
            }

            dynamic body = new ExpandoObject();
            body.recipients = recipients.ToArray();
            var apiCall = new ApiCall($"shares/{encodedUrl}/permission/grant", ApiType.GraphBeta, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase));
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // refresh permission because apicall will not return permission object
                apiCall = new ApiCall($"sites/{PnPContext.Site.Id}/drives/{driveId}/items/{driveItemId}/permissions/{Id}", ApiType.GraphBeta);
                response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);
                return SharingManager.DeserializeGraphPermission(json, PnPContext, this);
            }
            else
            {
                throw new Exception("Error occured when revoking the permissions for the users");
            }
        }

        public IGraphPermission GrantUserPermissions(List<IDriveRecipient> recipients)
        {
            return GrantUserPermissionsAsync(recipients).GetAwaiter().GetResult();
        }

        public async Task<IGraphPermission> RemoveUserPermissionsAsync(List<IDriveRecipient> recipients)
        {
            if (Link.Scope != ShareScope.Users)
            {
                throw new Exception("Revoking permisisons to an existing permission is only possible on a link of scope 'Users'");
            }

            var driveId = string.Empty;
            var driveItemId = string.Empty;

            if (Parent.GetType() == typeof(File))
            {
                var parent = (IFile)Parent;
                driveId = parent.VroomDriveID;
                driveItemId = parent.VroomItemID;
            }
            else if (Parent.GetType() == typeof(Folder))
            {
                var parent = (IFolder)Parent;
                (driveId, driveItemId) = await (parent as Folder).GetGraphIdsAsync().ConfigureAwait(false);
            }
            else if (Parent.GetType() == typeof(ListItem))
            {
                throw new ClientException(ErrorType.Unsupported, "Revoking list item permissions is not supported");
            }

            dynamic body = new ExpandoObject();
            body.grantees = recipients.ToArray();
            var apiCall = new ApiCall($"sites/{PnPContext.Site.Id}/drives/{driveId}/items/{driveItemId}/permissions/{Id}/revokeGrants", ApiType.GraphBeta, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase));
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);
                return SharingManager.DeserializeGraphPermission(json, PnPContext, this);
            }
            else
            {
                throw new Exception("Error occured when revoking the permissions for the users");
            }
        }

        public IGraphPermission RemoveUserPermissions(List<IDriveRecipient> recipients)
        {
            return RemoveUserPermissionsAsync(recipients).GetAwaiter().GetResult();
        }

        public async Task DeletePermissionAsync()
        {
            if (Parent.GetType() == typeof(File))
            {
                var parent = (IFile)Parent;
                var apiCall = new ApiCall($"sites/{PnPContext.Site.Id}/drives/{parent.VroomDriveID}/items/{parent.VroomItemID}/permissions/{Id}", ApiType.GraphBeta);
                var response = await RawRequestAsync(apiCall, HttpMethod.Delete).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new Exception("Error occured");
                }
            }
            else if (Parent.GetType() == typeof(Folder))
            {
                var parent = (IFolder)Parent;
                var (driveId, driveItemId) = await (parent as Folder).GetGraphIdsAsync().ConfigureAwait(false);

                var apiCall = new ApiCall($"sites/{PnPContext.Site.Id}/drives/{driveId}/items/{driveItemId}/permissions/{Id}", ApiType.GraphBeta);
                var response = await RawRequestAsync(apiCall, HttpMethod.Delete).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new Exception("Error occured");
                }
            }
            else if (Parent.GetType() == typeof(ListItem))
            {
                throw new ClientException(ErrorType.Unsupported, "Deleting list item permissions is not supported");

                // For now this is not yet supported via Graph APIs
                //var parent = (IListItem)Parent;
                //var listId = await (parent as ListItem).GetListIdAsync().ConfigureAwait(false);

                //var apiCall = new ApiCall($"sites/{PnPContext.Site.Id}/lists/{listId}/items/{(parent as ListItem).Id}/permissions/{Id}", ApiType.GraphBeta);
                //var response = await RawRequestAsync(apiCall, HttpMethod.Delete).ConfigureAwait(false);
                //if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                //{
                //    throw new Exception("Error occured");
                //}
            }

        }

        public void DeletePermission()
        {
            DeletePermissionAsync().GetAwaiter().GetResult();
        }

        #endregion
    }
}
