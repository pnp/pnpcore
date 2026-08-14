using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Site Webhooks
    /// </summary>
    [TemplateSchemaSerializer(
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201705,
        SerializationSequence = 2200, DeserializationSequence = 2200,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class SiteWebhooksSerializer : PnPBaseSchemaSerializer<SiteWebhook>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var siteWebhooks = persistence.GetPublicInstancePropertyValue("SiteWebhooks");

            if (siteWebhooks != null)
            {
                template.SiteWebhooks.AddRange(
                    PnPObjectsMapper.MapObjects(siteWebhooks,
                            new CollectionFromSchemaToModelTypeResolver(typeof(SiteWebhook)))
                            as IEnumerable<SiteWebhook>);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.SiteWebhooks != null && template.SiteWebhooks.Count > 0)
            {
                var siteWebhookTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.SiteWebhook, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var siteWebhookType = Type.GetType(siteWebhookTypeName, true);

                var expressions = new Dictionary<string, IResolver>
                {

                    { $"{siteWebhookType}.SiteWebhookTypeSpecified", new ExpressionValueResolver((s, p) => true) }
                };

                persistence.GetPublicInstanceProperty("SiteWebhooks")
                    .SetValue(
                        persistence,
                        PnPObjectsMapper.MapObjects(template.SiteWebhooks,
                            new CollectionFromModelToSchemaTypeResolver(siteWebhookType),
                            expressions,
                            recursive: true));
            }
        }
    }
}
