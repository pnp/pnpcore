using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers.V201909
{
    /// <summary>
    /// Class to serialize/deserialize the client side pages
    /// </summary>
    [TemplateSchemaSerializer(
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201909,
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
                        stringDictionaryType, stringDictionaryKeySelector, stringDictionaryValueSelector, "FieldValues"));

                expressions.Add(cp => cp.Properties,
                    new FromArrayToDictionaryValueResolver<String, String>(
                        stringDictionaryType, stringDictionaryKeySelector, stringDictionaryValueSelector, "Properties"));

                expressions.Add(cp => cp.Sections[0].Controls[0].Type,
                    new ExpressionValueResolver(
                        (s, p) => (Model.WebPartType)Enum.Parse(typeof(Model.WebPartType), s.GetPublicInstancePropertyValue("WebPartType").ToString())
                        ));

                expressions.Add(cp => cp.Sections[0].Controls[0].ControlId,
                    new FromStringToGuidValueResolver());

                expressions.Add(cp => cp.Header,
                    new Resolvers.V201909.ClientSidePageHeaderFromSchemaToModelTypeResolver());

                expressions.Add(cp => cp.Security, new PropertyObjectTypeResolver<File>(fl => fl.Security,
                    fl => fl.GetPublicInstancePropertyValue("Security")?.GetPublicInstancePropertyValue("BreakRoleInheritance")));
                expressions.Add(cp => cp.Security.RoleAssignments, new RoleAssigmentsFromSchemaToModelTypeResolver());

                template.ClientSidePages.AddRange(
                    PnPObjectsMapper.MapObjects(clientSidePages,
                            new CollectionFromSchemaToModelTypeResolver(typeof(ClientSidePage)),
                            expressions,
                            recursive: true)
                        as IEnumerable<ClientSidePage>);

                foreach (var page in template.ClientSidePages)
                {
                    if (page.Sections != null && page.Sections.Count > 0)
                    {
                        foreach (var section in page.Sections.Where(s=>s.Type == CanvasSectionType.OneColumn || s.Type == CanvasSectionType.OneColumnVerticalSection))
                        {
                            if (section.Controls != null && section.Controls.Any(c => !string.IsNullOrWhiteSpace(c.JsonControlData) && c.JsonControlData.Contains("\"sectionFactor\":100")))
                            {
                                section.Type = section.Type == CanvasSectionType.OneColumn ? CanvasSectionType.FlexibleLayoutSection : CanvasSectionType.FlexibleLayoutVerticalSection;
                            }
                        }
                    }
                }
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.ClientSidePages != null && template.ClientSidePages.Count > 0)
            {
                var baseClientSidePageTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.BaseClientSidePage, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var baseClientSidePageType = Type.GetType(baseClientSidePageTypeName, true);
                var clientSidePageTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ClientSidePage, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var clientSidePageType = Type.GetType(clientSidePageTypeName, true);
                var canvasSectionTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CanvasSection, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var canvasSectionType = Type.GetType(canvasSectionTypeName, true);
                var canvasControlTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CanvasControl, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var canvasControlType = Type.GetType(canvasControlTypeName, true);
                var canvasControlWebPartTypeTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CanvasControlWebPartType, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var canvasControlWebPartTypeType = Type.GetType(canvasControlWebPartTypeTypeName, true);
                var objectSecurityTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ObjectSecurity, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var objectSecurityType = Type.GetType(objectSecurityTypeName, true);

                var expressions = new Dictionary<string, IResolver>
                {

                    { $"{baseClientSidePageType}.PromoteAsNewsArticleSpecified", new ExpressionValueResolver((s, p) => true) },

                    { $"{baseClientSidePageType}.OverwriteSpecified", new ExpressionValueResolver((s, p) => true) },

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

                expressions.Add($"{baseClientSidePageType}.FieldValues", new FromDictionaryToArrayValueResolver<string, string>(dictionaryItemType, dictionaryItemKeySelector, dictionaryItemValueSelector));

                expressions.Add($"{baseClientSidePageType}.Properties", new FromDictionaryToArrayValueResolver<string, string>(dictionaryItemType, dictionaryItemKeySelector, dictionaryItemValueSelector));

                var clientSidePageHeaderType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.BaseClientSidePageHeader, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", false);

                if (null != clientSidePageHeaderType)
                {
                    expressions.Add($"{baseClientSidePageType}.Header", new Resolvers.V201909.ClientSidePageHeaderFromModelToSchemaTypeResolver());
                    expressions.Add($"{clientSidePageHeaderType}.TranslateX", new FromNullableToSpecifiedValueResolver<double>("TranslateXSpecified"));
                    expressions.Add($"{clientSidePageHeaderType}.TranslateY", new FromNullableToSpecifiedValueResolver<double>("TranslateYSpecified"));
                }

                expressions.Add($"{baseClientSidePageType}.Security", new Resolvers.V201807.ClientSidePageSecurityFromModelToSchemaTypeResolver());
                expressions.Add($"{objectSecurityType}.BreakRoleInheritance", new RoleAssignmentsFromModelToSchemaTypeResolver());

                expressions.Add($"{clientSidePageType}.LCIDSpecified", new ExpressionValueResolver(((s, p) =>
                {
                    var csp = s as ClientSidePage;
                    if (csp != null)
                    {
                        return (csp.LCID > 0);
                    }
                    else
                    {
                        return (false);
                    }
                })));

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
