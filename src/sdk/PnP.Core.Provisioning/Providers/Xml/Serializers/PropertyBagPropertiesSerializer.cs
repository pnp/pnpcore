using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Property Bag Properties
    /// </summary>
    [TemplateSchemaSerializer(SerializationSequence = 200, DeserializationSequence = 200,
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class PropertyBagPropertiesSerializer : PnPBaseSchemaSerializer<PropertyBagEntry>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var properties = persistence.GetPublicInstancePropertyValue("PropertyBagEntries");

            if (properties != null)
            {
                template.PropertyBagEntries.AddRange(
                    PnPObjectsMapper.MapObjects(properties,
                            new CollectionFromSchemaToModelTypeResolver(typeof(PropertyBagEntry)))
                            as IEnumerable<PropertyBagEntry>);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.PropertyBagEntries != null && template.PropertyBagEntries.Count > 0)
            {
                var propertyBagTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.PropertyBagEntry, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var propertyBagType = Type.GetType(propertyBagTypeName, true);


                var expressions = new Dictionary<string, IResolver>
                {
                    { $"{propertyBagType}.OverwriteSpecified", new ExpressionValueResolver(() => true) },
                    { $"{propertyBagType}.IndexedSpecified", new ExpressionValueResolver(() => true) }
                };

                persistence.GetPublicInstanceProperty("PropertyBagEntries")
                    .SetValue(
                        persistence,
                        PnPObjectsMapper.MapObjects(template.PropertyBagEntries,
                        new CollectionFromModelToSchemaTypeResolver(propertyBagType),
                        expressions));
            }
        }
    }
}
