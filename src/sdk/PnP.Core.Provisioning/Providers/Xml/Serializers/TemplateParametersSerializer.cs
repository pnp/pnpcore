using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers
{
    /// <summary>
    /// Class to serialize/deserialize the Parameters of the Template
    /// </summary>
    [TemplateSchemaSerializer(
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V201605,
        SerializationSequence = 100, DeserializationSequence = 100,
        Scope = SerializerScope.Provisioning)]
    internal class TemplateParametersSerializer : PnPBaseSchemaSerializer<ProvisioningTemplate>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var preferences = persistence.GetPublicInstancePropertyValue("Preferences");

            if (preferences != null)
            {
                var parameters = preferences.GetPublicInstancePropertyValue("Parameters");

                if (parameters != null)
                {
                    template.GetPublicInstanceProperty("Parameters")
                        .SetValue(template, PnPObjectsMapper.MapObjects(parameters,
                                new TemplateParameterFromSchemaToModelTypeResolver())
                                as Dictionary<String, String>);
                }
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.Parameters != null && template.Parameters.Count > 0)
            {
                var preferences = persistence.GetPublicInstancePropertyValue("Preferences");

                if (preferences != null)
                {
                    var parametersTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.PreferencesParameter, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var parametersType = Type.GetType(parametersTypeName, true);

                    preferences.GetPublicInstanceProperty("Parameters")
                        .SetValue(
                            preferences,
                            PnPObjectsMapper.MapObjects(template.Parameters,
                                new TemplateParameterFromModelToSchemaTypeResolver(parametersType)));

                    var parameters = preferences.GetPublicInstancePropertyValue("Parameters");
                    if (parameters != null && ((Array)parameters).Length == 0)
                    {
                        preferences.GetPublicInstanceProperty("Parameters").SetValue(preferences, null);
                    }
                }
            }
        }
    }
}
