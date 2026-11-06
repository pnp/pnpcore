using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using PnP.Core.Provisioning.Providers.Xml.Resolvers.V201705;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers.V201909
{
    /// <summary>
    /// Class to serialize/deserialize the List Instances
    /// </summary>
    [TemplateSchemaSerializer(SerializationSequence = 1100, DeserializationSequence = 1100,
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201909,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class ListInstancesSerializer : PnPBaseSchemaSerializer<ListInstance>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var lists = persistence.GetPublicInstancePropertyValue("Lists");

            if (lists != null)
            {
                var expressions = new Dictionary<Expression<Func<ListInstance, Object>>, IResolver>
                {

                    { l => l.FieldRefs[0].Id, new FromStringToGuidValueResolver() },
                    { l => l.TemplateFeatureID, new FromStringToGuidValueResolver() },

                    {
                        l => l.DataRows,
                        new ListInstanceDataRowsFromSchemaToModelTypeResolver()
                    },
                    {
                        l => l.DataRows.KeyColumn,
                        new ExpressionValueResolver((s, p) => s.GetPublicInstancePropertyValue("DataRows")?.GetPublicInstancePropertyValue("KeyColumn"))
                    },
                    {
                        l => l.DataRows.UpdateBehavior,
                        new ExpressionValueResolver((s, p) =>
                            (Model.UpdateBehavior)Enum.Parse(typeof(Model.UpdateBehavior),
                                s.GetPublicInstancePropertyValue("DataRows")?
                                .GetPublicInstancePropertyValue("UpdateBehavior")?
                                .ToString()))
                    }
                };

                var fieldDefaultTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.FieldDefault, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var fieldDefaultType = Type.GetType(fieldDefaultTypeName, true);
                var fieldDefaultKeySelector = CreateSelectorLambda(fieldDefaultType, "FieldName");
                var fieldDefaultValueSelector = CreateSelectorLambda(fieldDefaultType, "Value");
                expressions.Add(l => l.FieldDefaults,
                    new FromArrayToDictionaryValueResolver<String, String>(
                        fieldDefaultType, fieldDefaultKeySelector, fieldDefaultValueSelector));

                expressions.Add(l => l.Security, new SecurityFromSchemaToModelTypeResolver());

                expressions.Add(l => l.UserCustomActions[0].CommandUIExtension, new XmlAnyFromSchemaToModelValueResolver("CommandUIExtension"));
                expressions.Add(l => l.UserCustomActions[0].RegistrationType, new FromStringToEnumValueResolver(typeof(UserCustomActionRegistrationType)));
                expressions.Add(l => l.UserCustomActions[0].Rights, new FromStringToBasePermissionsValueResolver());
                expressions.Add(l => l.UserCustomActions[0].ClientSideComponentId, new FromStringToGuidValueResolver());

                expressions.Add(l => l.Views,
                    new ListViewsFromSchemaToModelTypeResolver());
                expressions.Add(l => l.RemoveExistingViews,
                    new RemoveExistingViewsFromSchemaToModelValueResolver());

                expressions.Add(l => l.Folders,
                   new FoldersFromSchemaToModelTypeResolver());

                expressions.Add(l => l.Fields, new ExpressionValueResolver((s, v) =>
                {
                    var fields = new Model.FieldCollection(template);
                    var sourceFields = s.GetPublicInstancePropertyValue("Fields")?.GetPublicInstancePropertyValue("Any") as System.Xml.XmlElement[];
                    if (sourceFields != null)
                    {
                        foreach (var f in sourceFields)
                        {
                            fields.Add(new Model.Field { SchemaXml = f.OuterXml });
                        }
                    }
                    return fields;
                }));

                expressions.Add(l => l.IRMSettings, new IRMSettingsFromSchemaToModelTypeResolver());

                var dataSourceItemTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.StringDictionaryItem, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var dataSourceItemType = Type.GetType(dataSourceItemTypeName, true);
                var dataSourceItemKeySelector = CreateSelectorLambda(dataSourceItemType, "Key");
                var dataSourceItemValueSelector = CreateSelectorLambda(dataSourceItemType, "Value");
                expressions.Add(l => l.DataSource, new FromArrayToDictionaryValueResolver<string, string>(dataSourceItemType, dataSourceItemKeySelector, dataSourceItemValueSelector));

                template.Lists.AddRange(
                    PnPObjectsMapper.MapObjects<ListInstance>(lists,
                            new CollectionFromSchemaToModelTypeResolver(typeof(ListInstance)),
                            expressions,
                            recursive: true)
                            as IEnumerable<ListInstance>);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.Lists != null && template.Lists.Count > 0)
            {
                var listInstanceTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ListInstance, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var listInstanceType = Type.GetType(listInstanceTypeName, true);

                var resolvers = new Dictionary<String, IResolver>
                {

                    { $"{listInstanceType}.DataRows", new ListInstanceDataRowsFromModelToSchemaTypeResolver() }
                };

                var fieldDefaultTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.FieldDefault, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var fieldDefaultType = Type.GetType(fieldDefaultTypeName, true);
                var fieldDefaultKeySelector = CreateSelectorLambda(fieldDefaultType, "FieldName");
                var fieldDefaultValueSelector = CreateSelectorLambda(fieldDefaultType, "Value");

                resolvers.Add($"{listInstanceType}.FieldDefaults", new FromDictionaryToArrayValueResolver<string, string>(fieldDefaultType, fieldDefaultKeySelector, fieldDefaultValueSelector));

                resolvers.Add($"{listInstanceType}.Security", new SecurityFromModelToSchemaTypeResolver());

                var customActionTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CustomAction, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var customActionType = Type.GetType(customActionTypeName, true);
                var commandUIExtensionTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CustomActionCommandUIExtension";
                var commandUIExtensionType = Type.GetType(commandUIExtensionTypeName, true);
                var registrationTypeTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.RegistrationType";
                var registrationTypeType = Type.GetType(registrationTypeTypeName, true);
                resolvers.Add($"{customActionType}.CommandUIExtension", new XmlAnyFromModelToSchemalValueResolver(commandUIExtensionType));
                resolvers.Add($"{customActionType}.Rights", new FromBasePermissionsToStringValueResolver());
                resolvers.Add($"{customActionType}.RegistrationType", new FromStringToEnumValueResolver(registrationTypeType));
                resolvers.Add($"{customActionType}.RegistrationTypeSpecified", new ExpressionValueResolver(() => true));
                resolvers.Add($"{customActionType}.SequenceSpecified", new ExpressionValueResolver(() => true));


                var listInstanceViewsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ListInstanceViews, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var listInstanceViewsType = Type.GetType(listInstanceViewsTypeName, true);

                resolvers.Add($"{listInstanceType}.Views",
                    new ListViewsFromModelToSchemaTypeResolver());
                resolvers.Add($"{listInstanceViewsType}.RemoveExistingViews",
                    new ExpressionValueResolver((s, v) => (Boolean)s.GetPublicInstancePropertyValue("RemoveExistingViews")));

                resolvers.Add($"{listInstanceType}.Folders", new FoldersFromModelToSchemaTypeResolver());

                var fieldsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.ListInstanceFields, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var fieldsType = Type.GetType(fieldsTypeName, true);
                resolvers.Add($"{listInstanceType}.Fields", new ExpressionValueResolver<ListInstance>((s, v) =>
                {
                    if (s.Fields != null && s.Fields.Count > 0)
                    {
                        var fields = Activator.CreateInstance(fieldsType);
                        var xmlFields = from f in s.Fields
                                        select XElement.Parse(f.SchemaXml).ToXmlElement();

                        fields.SetPublicInstancePropertyValue("Any", xmlFields.ToArray());
                        return fields;
                    }
                    else
                    {
                        return null;
                    }
                }));

                resolvers.Add($"{listInstanceType}.DraftVersionVisibilitySpecified", new ExpressionValueResolver(() => true));
                resolvers.Add($"{listInstanceType}.MaxVersionLimitSpecified", new ExpressionValueResolver(() => true));
                resolvers.Add($"{listInstanceType}.MinorVersionLimitSpecified", new ExpressionValueResolver(() => true));
                resolvers.Add($"{listInstanceType}.ReadSecuritySpecified", new ExpressionValueResolver((s, v) =>
                {
                    var value = (Int32)s.GetPublicInstancePropertyValue("ReadSecurity");
                    return (value == 1 || value == 2);
                }
                ));

                resolvers.Add($"{listInstanceType}.IsApplicationListSpecified", new ExpressionValueResolver(() => true));

                resolvers.Add($"{listInstanceType}.IRMSettings", new IRMSettingsFromModelToSchemaTypeResolver());

                var dataSourceItemTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.StringDictionaryItem, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var dataSourceItemType = Type.GetType(dataSourceItemTypeName, true);
                var dataSourceItemKeySelector = CreateSelectorLambda(dataSourceItemType, "Key");
                var dataSourceItemValueSelector = CreateSelectorLambda(dataSourceItemType, "Value");

                resolvers.Add($"{listInstanceType}.DataSource", new FromDictionaryToArrayValueResolver<string, string>(dataSourceItemType, dataSourceItemKeySelector, dataSourceItemValueSelector));

                resolvers.Add($"{listInstanceType}.TemplateFeatureID", new ExpressionValueResolver((s, v) =>
                {
                    var value = (Guid)s.GetPublicInstancePropertyValue("TemplateFeatureID");
                    if (value == Guid.Empty)
                    {
                        return (null);
                    }
                    else
                    {
                        return (value.ToString());
                    }
                }));

                persistence.GetPublicInstanceProperty("Lists")
                    .SetValue(
                        persistence,
                        PnPObjectsMapper.MapObjects(template.Lists,
                            new CollectionFromModelToSchemaTypeResolver(listInstanceType), resolvers, recursive: true));
            }
        }
    }
}
