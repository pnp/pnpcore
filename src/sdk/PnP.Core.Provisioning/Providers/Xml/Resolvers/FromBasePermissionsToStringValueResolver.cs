using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using System;
using System.Collections.Generic;
using BasePermissions = PnP.Core.Provisioning.Model.BasePermissions;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a Decimal value into a Double
    /// </summary>
    internal class FromBasePermissionsToStringValueResolver : IValueResolver
    {
        public string Name => this.GetType().Name;

        public object Resolve(object source, object destination, object sourceValue)
        {
            string res = null;
            if (sourceValue != null)
            {
                var basePermissions = (BasePermissions)sourceValue;
                List<string> permissions = new List<string>();
                foreach (var pk in (PermissionKind[])Enum.GetValues(typeof(PermissionKind)))
                {
                    if (basePermissions.Has(pk) && pk != PermissionKind.EmptyMask)
                    {
                        permissions.Add(pk.ToString());
                    }
                }
                res = string.Join(",", permissions.ToArray());
            }
            return res;
        }
    }
}
