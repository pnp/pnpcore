using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers.V201807
{
    internal class ClientSidePageSecurityFromModelToSchemaTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;

        public bool CustomCollectionResolver => false;


        public ClientSidePageSecurityFromModelToSchemaTypeResolver()
        {
        }

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            Object result = null;

            // Try with the tenant-wide AppCatalog
            var security = (source as Model.ClientSidePage)?.Security;

            if (security == null)
                security = (source as Model.TranslatedClientSidePage)?.Security;

            // If we have security settings
            if (null != security &&
                security.RoleAssignments != null &&
                security.RoleAssignments.Count > 0)
            {
                // Map them to the output
                var securityTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ObjectSecurity, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var securityType = Type.GetType(securityTypeName, true);
                result = Activator.CreateInstance(securityType);

                PnPObjectsMapper.MapProperties(security, result, resolvers, recursive);
            }

            return (result);
        }
    }
}
