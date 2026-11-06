using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Custom Actions
    /// </summary>
    [TemplateSchemaSerializer(SerializationSequence = 1300, DeserializationSequence = 1300,
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class CustomActionsSerializer : PnPBaseSchemaSerializer<CustomActions>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var customActions = persistence.GetPublicInstancePropertyValue("CustomActions");

            if (customActions != null)
            {
                var expressions = new Dictionary<Expression<Func<CustomActions, Object>>, IResolver>
                {
                    { c => c.SiteCustomActions[0].CommandUIExtension, new XmlAnyFromSchemaToModelValueResolver("CommandUIExtension") },
                    { c => c.SiteCustomActions[0].RegistrationType, new FromStringToEnumValueResolver(typeof(UserCustomActionRegistrationType)) },
                    { c => c.SiteCustomActions[0].Rights, new FromStringToBasePermissionsValueResolver() },
                    { c => c.SiteCustomActions[0].ClientSideComponentId, new FromStringToGuidValueResolver() }
                };

                PnPObjectsMapper.MapProperties(customActions, template.CustomActions, expressions, true);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.CustomActions != null && (template.CustomActions.WebCustomActions.Count > 0 || template.CustomActions.SiteCustomActions.Count > 0))
            {
                var customActionsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CustomActions, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var customActionsType = Type.GetType(customActionsTypeName, true);
                var customActionTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CustomAction, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var customActionType = Type.GetType(customActionTypeName, true);
                var registrationTypeTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.RegistrationType";
                var registrationTypeType = Type.GetType(registrationTypeTypeName, true);
                var commandUIExtensionTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.CustomActionCommandUIExtension";
                var commandUIExtensionType = Type.GetType(commandUIExtensionTypeName, true);

                var target = Activator.CreateInstance(customActionsType, true);

                var expressions = new Dictionary<string, IResolver>
                {
                    { $"{customActionType}.Rights", new FromBasePermissionsToStringValueResolver() },
                    { $"{customActionType}.RegistrationType", new FromStringToEnumValueResolver(registrationTypeType) },
                    { $"{customActionType}.RegistrationTypeSpecified", new ExpressionValueResolver(() => true) },
                    { $"{customActionType}.SequenceSpecified", new ExpressionValueResolver(() => true) },
                    { $"{customActionType}.CommandUIExtension", new XmlAnyFromModelToSchemalValueResolver(commandUIExtensionType) },
                    { $"{customActionType}.ClientSideComponentId", new ExpressionValueResolver((s, v) => v != null ? v.ToString() : s?.ToString()) }
                };

                PnPObjectsMapper.MapProperties(template.CustomActions, target, expressions, recursive: true);

                if (target != null &&
                    ((target.GetPublicInstancePropertyValue("SiteCustomActions") != null && ((Array)target.GetPublicInstancePropertyValue("SiteCustomActions")).Length > 0) ||
                    (target.GetPublicInstancePropertyValue("WebCustomActions") != null && ((Array)target.GetPublicInstancePropertyValue("WebCustomActions")).Length > 0)))
                {
                    persistence.GetPublicInstanceProperty("CustomActions").SetValue(persistence, target);
                }
            }
        }
    }
}
