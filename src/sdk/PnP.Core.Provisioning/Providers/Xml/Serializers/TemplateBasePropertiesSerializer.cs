using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Base Properties of a Template
    /// </summary>
    [TemplateSchemaSerializer(SerializationSequence = 100, DeserializationSequence = 100,
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class TemplateBasePropertiesSerializer : PnPBaseSchemaSerializer<ProvisioningTemplate>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var propertiesTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.StringDictionaryItem, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var propertiesType = Type.GetType(propertiesTypeName, true);
            var propertiesKeySelector = CreateSelectorLambda(propertiesType, "Key");
            var propertiesValueSelector = CreateSelectorLambda(propertiesType, "Value");

            var expressions = new Dictionary<Expression<Func<ProvisioningTemplate, Object>>, IResolver>
            {
                { t => t.Version, new FromDecimalToDoubleValueResolver() },
                {
                    t => t.Properties,
                    new FromArrayToDictionaryValueResolver<String, String>(
                    propertiesType, propertiesKeySelector, propertiesValueSelector)
                },

                {
                    t => t.SiteSearchSettings,
                    new ExpressionValueResolver((s, v) =>
                    s.GetPublicInstancePropertyValue("SearchSettings")?
                    .GetPublicInstancePropertyValue("SiteSearchSettings")?
                    .GetPublicInstancePropertyValue("OuterXml"))
                },
                {
                    t => t.WebSearchSettings,
                    new ExpressionValueResolver((s, v) =>
                    s.GetPublicInstancePropertyValue("SearchSettings")?
                    .GetPublicInstancePropertyValue("WebSearchSettings")?
                    .GetPublicInstancePropertyValue("OuterXml"))
                },

                {
                    t => t.Scope,
                    new FromStringToEnumValueResolver(typeof(Model.ProvisioningTemplateScope))
                }
            };

            PnPObjectsMapper.MapProperties(persistence, template, expressions);
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            var propertiesTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.StringDictionaryItem, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
            var propertiesType = Type.GetType(propertiesTypeName, true);
            var templateType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ProvisioningTemplate, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);

            var keySelector = CreateSelectorLambda(propertiesType, "Key");
            var valueSelector = CreateSelectorLambda(propertiesType, "Value");

            var expressions = new Dictionary<string, IResolver>
            {
                { $"{templateType}.Version", new FromDoubleToDecimalValueResolver() },
                { $"{templateType}.VersionSpecified", new ExpressionValueResolver(() => true) },
                {
                    $"{templateType}.Properties",
                    new FromDictionaryToArrayValueResolver<String, String>(
                    propertiesType, keySelector, valueSelector)
                }
            };

            if (PnPSerializationScope.Current?.BaseSchemaNamespace != null &&
                !PnPSerializationScope.Current.BaseSchemaNamespace.EndsWith("201605"))
            {
                var templateScopeType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ProvisioningTemplateScope, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
                expressions.Add($"{templateType}.Scope", new ExpressionValueResolver(() => Enum.Parse(templateScopeType, template.Scope.ToString())));
                expressions.Add($"{templateType}.ScopeSpecified", new ExpressionValueResolver(() => true));
            }

            PnPObjectsMapper.MapProperties(template, persistence, expressions, true);

            if (!string.IsNullOrEmpty(template.SiteSearchSettings) || !string.IsNullOrEmpty(template.WebSearchSettings))
            {
                var searchSettingType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ProvisioningTemplateSearchSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
                var searchSettings = Activator.CreateInstance(searchSettingType, true);
                if (!string.IsNullOrEmpty(template.SiteSearchSettings))
                {
                    searchSettings.GetPublicInstanceProperty("SiteSearchSettings").SetValue(searchSettings, XElement.Parse(template.SiteSearchSettings).ToXmlElement());
                }
                if (!string.IsNullOrEmpty(template.WebSearchSettings))
                {
                    searchSettings.GetPublicInstanceProperty("WebSearchSettings").SetValue(searchSettings, XElement.Parse(template.WebSearchSettings).ToXmlElement());
                }
                persistence.GetPublicInstanceProperty("SearchSettings").SetValue(persistence, searchSettings);
            }
        }
    }
}
