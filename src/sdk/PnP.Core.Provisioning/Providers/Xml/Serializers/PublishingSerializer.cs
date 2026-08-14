using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Publishing settings
    /// </summary>
    [TemplateSchemaSerializer(SerializationSequence = 1900, DeserializationSequence = 1900,
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class PublishingSerializer : PnPBaseSchemaSerializer<Publishing>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var publishing = persistence.GetPublicInstancePropertyValue("Publishing");

            if (publishing != null)
            {
                template.Publishing = new Publishing();
                var expressions = new Dictionary<Expression<Func<Publishing, Object>>, IResolver>
                {
                    { p => p.DesignPackage, new PropertyObjectTypeResolver<Publishing>(p => p.DesignPackage) },
                    { p => p.DesignPackage.PackageGuid, new FromStringToGuidValueResolver() },
                    { p => p.PageLayouts, new PageLayoutsFromSchemaToModelTypeResolver() },
                    {
                        p => p.AvailableWebTemplates[0].LanguageCode,
                        new ExpressionValueResolver((s, v) => (bool)s.GetPublicInstancePropertyValue("LanguageCodeSpecified") ? v : 1033)
                    }
                };

                PnPObjectsMapper.MapProperties(publishing, template.Publishing, expressions, true);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.Publishing != null)
            {
                var publishingType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Publishing, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
                var designPackageType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.PublishingDesignPackage, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
                var webTemplateType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.PublishingWebTemplate, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);

                var target = Activator.CreateInstance(publishingType, true);
                var expressions = new Dictionary<string, IResolver>
                {
                    { $"{publishingType}.DesignPackage", new PropertyObjectTypeResolver(designPackageType, "DesignPackage") },
                    { $"{designPackageType}.MajorVersionSpecified", new ExpressionValueResolver(() => true) },
                    { $"{designPackageType}.MinorVersionSpecified", new ExpressionValueResolver(() => true) },
                    { $"{webTemplateType}.LanguageCodeSpecified", new ExpressionValueResolver(() => true) },
                    { $"{publishingType}.PageLayouts", new PageLayoutsFromModelToSchemaTypeResolver() }
                };

                PnPObjectsMapper.MapProperties(template.Publishing, target, expressions, recursive: true);

                persistence.GetPublicInstanceProperty("Publishing").SetValue(persistence, target);
            }
        }
    }
}
