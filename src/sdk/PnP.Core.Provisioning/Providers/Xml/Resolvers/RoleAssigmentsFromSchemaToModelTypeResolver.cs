using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a collection type from Domain Model to Schema
    /// </summary>
    internal class RoleAssigmentsFromSchemaToModelTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;
        public bool CustomCollectionResolver => false;

        public RoleAssigmentsFromSchemaToModelTypeResolver()
        {
        }

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            List<RoleAssignment> res = new List<RoleAssignment>();
            var sourceValue = source.GetPublicInstancePropertyValue("RoleAssignment");
            if (sourceValue != null)
            {
                res = PnPObjectsMapper.MapObjects(sourceValue, new CollectionFromSchemaToModelTypeResolver(typeof(RoleAssignment)), null, true) as List<RoleAssignment>;
            }
            return res;
        }
    }
}
