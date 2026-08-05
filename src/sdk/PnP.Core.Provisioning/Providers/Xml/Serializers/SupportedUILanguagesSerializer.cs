using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Supported UI Languages
    /// </summary>
    [TemplateSchemaSerializer(
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        SerializationSequence = 500, DeserializationSequence = 500,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class SupportedUILanguagesSerializer : PnPBaseSchemaSerializer<SupportedUILanguage>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var supportedUILanguages = persistence.GetPublicInstancePropertyValue("SupportedUILanguages");

            if (supportedUILanguages != null)
            {
                template.SupportedUILanguages.AddRange(
                    PnPObjectsMapper.MapObjects(supportedUILanguages,
                            new CollectionFromSchemaToModelTypeResolver(typeof(SupportedUILanguage)))
                            as IEnumerable<SupportedUILanguage>);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.SupportedUILanguages != null && template.SupportedUILanguages.Count > 0)
            {
                var supportedUILanguageTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.SupportedUILanguagesSupportedUILanguage, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var supportedUILanguageType = Type.GetType(supportedUILanguageTypeName, true);

                persistence.GetPublicInstanceProperty("SupportedUILanguages")
                    .SetValue(
                        persistence,
                        PnPObjectsMapper.MapObjects(template.SupportedUILanguages,
                            new CollectionFromModelToSchemaTypeResolver(supportedUILanguageType)));
            }
        }
    }
}
