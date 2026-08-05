using PnP.Core.Provisioning.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Tenant-wide settings
    /// </summary>
    [TemplateSchemaSerializer(
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201807,
        SerializationSequence = -1, DeserializationSequence = -1,
        Scope = SerializerScope.Tenant)]
    internal class ProvisioningHierarchySerializer : PnPBaseSchemaSerializer<ProvisioningHierarchy>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            if (persistence != null)
            {
                var expressions = new Dictionary<Expression<Func<ProvisioningHierarchy, Object>>, IResolver>();
                PnPObjectsMapper.MapProperties(persistence, template.ParentHierarchy, expressions, recursive: false);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            //if (template.ParentHierarchy != null)
            //{
            //    var resolvers = new Dictionary<String, IResolver>();
            //    PnPObjectsMapper.MapProperties(template.ParentHierarchy, persistence, resolvers, recursive: false);
            //}
        }
    }
}
