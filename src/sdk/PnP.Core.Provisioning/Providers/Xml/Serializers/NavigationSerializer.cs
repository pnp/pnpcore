using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Navigation settings
    /// </summary>
    [TemplateSchemaSerializer(SerializationSequence = 800, DeserializationSequence = 800,
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        Scope = SerializerScope.ProvisioningTemplate)]
    internal class NavigationSerializer : PnPBaseSchemaSerializer<Model.Navigation>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var navigation = persistence.GetPublicInstancePropertyValue("Navigation");

            if (navigation != null)
            {
                var expressions = new Dictionary<Expression<Func<Model.Navigation, Object>>, IResolver>
                {
                    { n => n.CurrentNavigation, new NavigationFromSchemaToModelTypeResolver("CurrentNavigation") },
                    { n => n.GlobalNavigation, new NavigationFromSchemaToModelTypeResolver("GlobalNavigation") }
                };

                template.Navigation = new Model.Navigation();
                PnPObjectsMapper.MapProperties(navigation, template.Navigation, expressions, true);
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.Navigation != null)
            {
                var navigationTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Navigation, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var navigationType = Type.GetType(navigationTypeName, true);
                var target = Activator.CreateInstance(navigationType, true);

                var resolvers = new Dictionary<String, IResolver>
                {
                    {
                        $"{navigationType}.GlobalNavigation",
                        new NavigationFromModelToSchemaTypeResolver("GlobalNavigation")
                    },
                    {
                        $"{navigationType}.CurrentNavigation",
                        new NavigationFromModelToSchemaTypeResolver("CurrentNavigation")
                    }
                };

                PnPObjectsMapper.MapProperties(template.Navigation, target, resolvers, recursive: true);

                persistence.GetPublicInstanceProperty("Navigation").SetValue(persistence, target);
            }
        }
    }
}
