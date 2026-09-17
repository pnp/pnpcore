using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a collection type from Domain Model to Schema
    /// </summary>
    internal class RoleAssignmentsFromModelToSchemaTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;
        public bool CustomCollectionResolver => false;

        public RoleAssignmentsFromModelToSchemaTypeResolver()
        {
        }

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            var baseNamespace = PnPSerializationScope.Current?.BaseSchemaNamespace;
            var breakRoleInheritanceTypeName = $"{baseNamespace}.ObjectSecurityBreakRoleInheritance, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var breakRoleInheritanceType = Type.GetType(breakRoleInheritanceTypeName, true);
            var roleAssignmentTypeName = $"{baseNamespace}.RoleAssignment, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var roleAssignmentType = Type.GetType(roleAssignmentTypeName, true);

            var security = (ObjectSecurity)source;

            if (security.ClearSubscopes || security.CopyRoleAssignments ||
                (security.RoleAssignments != null && security.RoleAssignments.Count > 0))
            {
                var breakRoleInheritance = Activator.CreateInstance(breakRoleInheritanceType, true);

                PnPObjectsMapper.MapProperties(source, breakRoleInheritance, recursive: true);

                if (security.RoleAssignments != null)
                {
                    var roleAssignment = PnPObjectsMapper.MapObjects(security.RoleAssignments,
                        new CollectionFromModelToSchemaTypeResolver(roleAssignmentType), null, true);
                    breakRoleInheritance.GetPublicInstanceProperty("RoleAssignment").SetValue(breakRoleInheritance, roleAssignment);
                }

                return breakRoleInheritance;
            }
            else
            {
                return null;
            }
        }
    }
}
