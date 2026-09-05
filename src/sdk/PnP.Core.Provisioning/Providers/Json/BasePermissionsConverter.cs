using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using BasePermissions = PnP.Core.Provisioning.Model.BasePermissions;

namespace PnP.Core.Provisioning.Providers.Json
{
    /// <summary>
    /// Serializes a <see cref="BasePermissions"/> as the comma separated list of permission names it
    /// contains, and reads it back.
    /// </summary>
    internal class BasePermissionsConverter : JsonConverter<BasePermissions>
    {
        public override BasePermissions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            BasePermissions result = new BasePermissions();

            string basePermissionString = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();

            if (!string.IsNullOrEmpty(basePermissionString))
            {
                if (int.TryParse(basePermissionString, out int permissionInt))
                {
                    result.Set((PermissionKind)permissionInt);
                }
                else
                {
                    foreach (string pk in basePermissionString.Split(new char[] { ',' }))
                    {
                        if (Enum.TryParse(pk.Trim(), out PermissionKind permissionKind))
                        {
                            result.Set(permissionKind);
                        }
                    }
                }
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, BasePermissions value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            List<string> permissions = new List<string>();
            foreach (PermissionKind pk in (PermissionKind[])Enum.GetValues(typeof(PermissionKind)))
            {
                if (value.Has(pk) && pk != PermissionKind.EmptyMask)
                {
                    permissions.Add(pk.ToString());
                }
            }

            writer.WriteStringValue(string.Join(",", permissions.ToArray()));
        }
    }
}
