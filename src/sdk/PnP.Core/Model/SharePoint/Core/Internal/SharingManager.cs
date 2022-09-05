using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal static class SharingManager
    {
        internal static IGraphPermissionCollection DeserializeGraphPermissionsResponse(string responseJson, PnPContext context, IDataModelParent parent)
        {
            var graphPermissions = new GraphPermissionCollection(context, parent);

            var json = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (json.TryGetProperty("value", out JsonElement dataRows))
            {
                if (dataRows.GetArrayLength() == 0)
                {
                    return graphPermissions;
                }

                foreach (var row in dataRows.EnumerateArray())
                {
                    graphPermissions.Add(DeserializeGraphPermission(row, context, parent));
                }
            }
            return graphPermissions;
        }

        internal static IGraphPermission DeserializeGraphPermission(JsonElement row, PnPContext context, IDataModelParent parent)
        {
            var returnPermission = new GraphPermission(context, parent);

            if (row.TryGetProperty("id", out JsonElement id))
            {
                returnPermission.Id = id.GetString();
            }

            if (row.TryGetProperty("roles", out JsonElement roles))
            {
                returnPermission.Roles = JsonSerializer.Deserialize<List<PermissionRole>>(roles.GetRawText(), PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);
            }

            if (row.TryGetProperty("hasPassword", out JsonElement hasPassword))
            {
                returnPermission.HasPassword = hasPassword.GetBoolean();
            }

            if (row.TryGetProperty("sharedId", out JsonElement sharedId))
            {
                returnPermission.ShareId = sharedId.GetString();
            }

            if (row.TryGetProperty("expirationDateTime", out JsonElement expirationDateTime))
            {
                returnPermission.ExpirationDateTime = expirationDateTime.GetDateTime();
            }

            if (row.TryGetProperty("grantedToV2", out JsonElement grantedToV2))
            {
                returnPermission.GrantedToV2 = DeserializeGrantedToV2(grantedToV2);
            }

            if (row.TryGetProperty("invitation", out JsonElement invitation))
            {
                returnPermission.Invitation = DeserializeInvitation(invitation);
            }

            if (row.TryGetProperty("grantedToIdentitiesV2", out JsonElement grantedToIdentitiesV2))
            {
                var identitySets = new List<ISharePointIdentitySet>();

                foreach (var grantedToIdentitiesV2Object in grantedToIdentitiesV2.EnumerateArray())
                {
                    identitySets.Add(DeserializeGrantedToV2(grantedToIdentitiesV2Object));
                }
                returnPermission.GrantedToIdentitiesV2 = identitySets;
            }

            if (row.TryGetProperty("link", out JsonElement link))
            {
                returnPermission.Link = DeserializeSharingLink(link);
            }

            return returnPermission;
        }

        private static ISharingInvitation DeserializeInvitation(JsonElement invitation)
        {
            var sharingInvitation = new SharingInvitation();

            if (invitation.TryGetProperty("signInRequired", out JsonElement signInRequired))
            {
                sharingInvitation.SignInRequired = signInRequired.GetBoolean();
            }
            if (invitation.TryGetProperty("email", out JsonElement email))
            {
                sharingInvitation.Email = email.GetString();
            }
            if (invitation.TryGetProperty("invitedBy", out JsonElement invitedBy))
            {
                sharingInvitation.InvitedBy = DeserializeInvitedBy(invitedBy);
            }
            return sharingInvitation;
        }

        private static IIdentitySet DeserializeInvitedBy(JsonElement invitedBy)
        {
            var identitySet = new IdentitySet();

            if (invitedBy.TryGetProperty("user", out JsonElement user))
            {
                identitySet.User = DeserializeIdentity(user);
            }

            if (invitedBy.TryGetProperty("application", out JsonElement application))
            {
                identitySet.Application = DeserializeIdentity(application);
            }

            if (invitedBy.TryGetProperty("device", out JsonElement device))
            {
                identitySet.Device = DeserializeIdentity(device);
            }

            return identitySet;
        }

        private static ISharingLink DeserializeSharingLink(JsonElement link)
        {
            var sharingLink = new SharingLink();

            if (link.TryGetProperty("scope", out JsonElement scope) && Enum.TryParse(scope.GetString(), true, out ShareScope shareScope))
            {
                sharingLink.Scope = shareScope;
            }
            if (link.TryGetProperty("type", out JsonElement type) && Enum.TryParse(type.GetString(), true, out ShareType shareType))
            {
                sharingLink.Type = shareType;
            }
            if (link.TryGetProperty("webUrl", out JsonElement webUrl))
            {
                sharingLink.WebUrl = webUrl.GetString();
            }
            if (link.TryGetProperty("webHtml", out JsonElement webHtml))
            {
                sharingLink.WebHtml = webHtml.GetString();
            }
            if (link.TryGetProperty("preventsDownload", out JsonElement preventsDownload))
            {
                sharingLink.PreventsDownload = preventsDownload.GetBoolean();
            }
            return sharingLink;
        }

        private static ISharePointIdentitySet DeserializeGrantedToV2(JsonElement grantedToV2)
        {
            var grantedV2Identity = new SharePointIdentitySet();

            if (grantedToV2.TryGetProperty("user", out JsonElement user))
            {
                grantedV2Identity.User = DeserializeIdentity(user);
            }

            if (grantedToV2.TryGetProperty("application", out JsonElement application))
            {
                grantedV2Identity.Application = DeserializeIdentity(application);
            }

            if (grantedToV2.TryGetProperty("device", out JsonElement device))
            {
                grantedV2Identity.Device = DeserializeIdentity(device);
            }

            if (grantedToV2.TryGetProperty("group", out JsonElement group))
            {
                grantedV2Identity.Group = DeserializeIdentity(group);
            }

            if (grantedToV2.TryGetProperty("siteUser", out JsonElement siteUser))
            {
                grantedV2Identity.SiteUser = DeserializeSharePointIdentity(siteUser);
            }

            if (grantedToV2.TryGetProperty("siteGroup", out JsonElement siteGroup))
            {
                grantedV2Identity.SiteGroup = DeserializeSharePointIdentity(siteGroup);
            }

            return grantedV2Identity;
        }

        private static ISharePointIdentity DeserializeSharePointIdentity(JsonElement row)
        {
            var sharePointIdentity = new SharePointIdentity();

            if (row.TryGetProperty("id", out JsonElement id))
            {
                sharePointIdentity.Id = id.GetString();
            }
            if (row.TryGetProperty("displayName", out JsonElement displayName))
            {
                sharePointIdentity.DisplayName = displayName.GetString();
            }
            if (row.TryGetProperty("loginName", out JsonElement loginName))
            {
                sharePointIdentity.LoginName = loginName.GetString();
            }
            if (row.TryGetProperty("email", out JsonElement email))
            {
                sharePointIdentity.Email = email.GetString();
            }
            return sharePointIdentity;
        }

        private static IIdentity DeserializeIdentity(JsonElement row)
        {
            var identity = new Identity();
            if (row.TryGetProperty("id", out JsonElement id))
            {
                identity.Id = id.GetString();
            }
            if (row.TryGetProperty("displayName", out JsonElement displayName))
            {
                identity.DisplayName = displayName.GetString();
            }
            if (row.TryGetProperty("email", out JsonElement email))
            {
                identity.Email = email.GetString();
            }
            return identity;
        }

    }
}
