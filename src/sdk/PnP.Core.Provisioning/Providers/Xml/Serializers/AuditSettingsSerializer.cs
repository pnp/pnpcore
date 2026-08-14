using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Audit Settings
    /// </summary>
    [TemplateSchemaSerializer(
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        SerializationSequence = 600, DeserializationSequence = 600,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class AuditSettingsSerializer : PnPBaseSchemaSerializer<AuditSettings>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var auditSettings = persistence.GetPublicInstancePropertyValue("AuditSettings");

            if (auditSettings != null)
            {
                var expressions = new Dictionary<Expression<Func<AuditSettings, Object>>, IResolver>
                {
                    { a => a.AuditFlags, new FromArrayToAuditFlagsValueResolver() }
                };

                template.AuditSettings = new AuditSettings();
                PnPObjectsMapper.MapProperties(auditSettings, template.AuditSettings, expressions, true);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.AuditSettings != null)
            {
                var auditSettingsType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.AuditSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
                var target = Activator.CreateInstance(auditSettingsType, true);
                var expressions = new Dictionary<string, IResolver>
                {
                    { $"{auditSettingsType}.AuditLogTrimmingRetentionSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{auditSettingsType}.TrimAuditLogSpecified", new ExpressionValueResolver((s, p) => true) },
                    { $"{auditSettingsType}.Audit", new FromAuditFlagsToArrayValueResolver() }
                };

                PnPObjectsMapper.MapProperties(template.AuditSettings, target, expressions, recursive: true);

                persistence.GetPublicInstanceProperty("AuditSettings").SetValue(persistence, target);
            }
        }
    }
}
