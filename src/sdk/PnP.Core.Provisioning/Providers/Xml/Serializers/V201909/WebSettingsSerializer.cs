using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using PnP.Core.Provisioning.Providers.Xml.Resolvers.V201909;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers.V201909
{
    /// <summary>
    /// Class to serialize/deserialize the Web Settings
    /// </summary>
    [TemplateSchemaSerializer(SerializationSequence = 300, DeserializationSequence = 300,
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201909,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class WebSettingsSerializer : PnPBaseSchemaSerializer<WebSettings>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var webSettings = persistence.GetPublicInstancePropertyValue("WebSettings");

            if (webSettings != null)
            {
                var expressions = new Dictionary<Expression<Func<WebSettings, Object>>, IResolver>
                {
                    {
                        s => s.AlternateUICultures,
                        new AlternateUICultureFromSchemaToModelTypeResolver()
                    }
                };

                template.WebSettings = new WebSettings();
                PnPObjectsMapper.MapProperties(webSettings, template.WebSettings, expressions, true);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.WebSettings != null)
            {
                var webSettingsType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.WebSettings, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
                var target = Activator.CreateInstance(webSettingsType, true);
                var expressions = new Dictionary<string, IResolver>
                {
                    { $"{webSettingsType}.NoCrawlSpecified", new ExpressionValueResolver(() => true) },
                    { $"{webSettingsType}.QuickLaunchEnabledSpecified", new ExpressionValueResolver(() => true) }
                };

                PnPObjectsMapper.MapProperties(template.WebSettings, target, expressions, recursive: true);

                persistence.GetPublicInstanceProperty("WebSettings").SetValue(persistence, target);
            }
        }
    }
}
