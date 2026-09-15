using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using PnP.Core.Provisioning.Providers.Xml.Resolvers.V201805;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the client side pages
    /// </summary>
    [TemplateSchemaSerializer(
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201705,
        SerializationSequence = 2300, DeserializationSequence = 2300,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class ClientSidePagesSerializer : PnPBaseSchemaSerializer<ClientSidePage>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var clientSidePages = persistence.GetPublicInstancePropertyValue("ClientSidePages");

            if (clientSidePages != null)
            {
                var expressions = new Dictionary<Expression<Func<ClientSidePage, Object>>, IResolver>();

                var stringDictionaryTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.StringDictionaryItem, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var stringDictionaryType = Type.GetType(stringDictionaryTypeName, true);
                var stringDictionaryKeySelector = CreateSelectorLambda(stringDictionaryType, "Key");
                var stringDictionaryValueSelector = CreateSelectorLambda(stringDictionaryType, "Value");

                expressions.Add(cp => cp.Sections[0].Controls[0].ControlProperties,
                    new FromArrayToDictionaryValueResolver<String, String>(
                        stringDictionaryType, stringDictionaryKeySelector, stringDictionaryValueSelector));

                expressions.Add(cp => cp.FieldValues,
                    new FromArrayToDictionaryValueResolver<String, String>(
                        stringDictionaryType, stringDictionaryKeySelector, stringDictionaryValueSelector));

                expressions.Add(cp => cp.Sections[0].Controls[0].Type,
                    new ExpressionValueResolver(
                        (s, p) => (Model.WebPartType)Enum.Parse(typeof(Model.WebPartType), s.GetPublicInstancePropertyValue("WebPartType").ToString())
                        ));

                expressions.Add(cp => cp.Sections[0].Controls[0].ControlId,
                    new FromStringToGuidValueResolver());

                expressions.Add(cp => cp.Header, new ClientSidePageHeaderFromSchemaToModelTypeResolver());

                template.ClientSidePages.AddRange(
                    PnPObjectsMapper.MapObjects(clientSidePages,
                            new CollectionFromSchemaToModelTypeResolver(typeof(ClientSidePage)),
                            expressions,
                            recursive: true)
                        as IEnumerable<ClientSidePage>);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.ClientSidePages != null && template.ClientSidePages.Count > 0)
            {
                var clientSidePageTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ClientSidePage, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var clientSidePageType = Type.GetType(clientSidePageTypeName, true);
                var canvasSectionTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CanvasSection, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var canvasSectionType = Type.GetType(canvasSectionTypeName, true);
                var canvasControlTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CanvasControl, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var canvasControlType = Type.GetType(canvasControlTypeName, true);
                var canvasControlWebPartTypeTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CanvasControlWebPartType, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var canvasControlWebPartTypeType = Type.GetType(canvasControlWebPartTypeTypeName, true);

                var expressions = new Dictionary<string, IResolver>
                {

                    { $"{clientSidePageType}.PromoteAsNewsArticleSpecified", new ExpressionValueResolver((s, p) => true) },

                    { $"{clientSidePageType}.OverwriteSpecified", new ExpressionValueResolver((s, p) => true) },

                    { $"{canvasSectionType}.OrderSpecified", new ExpressionValueResolver((s, p) => true) },

                    { $"{canvasSectionType}.TypeSpecified", new ExpressionValueResolver((s, p) => true) },

                    {
                        $"{canvasControlType}.WebPartType",
                        new ExpressionValueResolver(
                        (s, p) => Enum.Parse(canvasControlWebPartTypeType, s.GetPublicInstancePropertyValue("Type").ToString()))
                    }
                };

                var dictionaryItemTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.StringDictionaryItem, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var dictionaryItemType = Type.GetType(dictionaryItemTypeName, true);
                var dictionaryItemKeySelector = CreateSelectorLambda(dictionaryItemType, "Key");
                var dictionaryItemValueSelector = CreateSelectorLambda(dictionaryItemType, "Value");

                expressions.Add($"{canvasControlType}.CanvasControlProperties",
                    new FromDictionaryToArrayValueResolver<string, string>(
                        dictionaryItemType, dictionaryItemKeySelector, dictionaryItemValueSelector, "ControlProperties"));

                expressions.Add($"{clientSidePageType}.FieldValues", new FromDictionaryToArrayValueResolver<string, string>(dictionaryItemType, dictionaryItemKeySelector, dictionaryItemValueSelector));

                var clientSidePageHeaderType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ClientSidePageHeader, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", false);

                if (null != clientSidePageHeaderType)
                {
                    expressions.Add($"{clientSidePageType}.Header", new ClientSidePageHeaderFromModelToSchemaTypeResolver());
                    expressions.Add($"{clientSidePageHeaderType}.TranslateX", new FromNullableToSpecifiedValueResolver<double>("TranslateXSpecified"));
                    expressions.Add($"{clientSidePageHeaderType}.TranslateY", new FromNullableToSpecifiedValueResolver<double>("TranslateYSpecified"));
                }

                persistence.GetPublicInstanceProperty("ClientSidePages")
                    .SetValue(
                        persistence,
                        PnPObjectsMapper.MapObjects(template.ClientSidePages,
                            new CollectionFromModelToSchemaTypeResolver(clientSidePageType),
                            expressions,
                            recursive: true));
            }
        }
    }
}
