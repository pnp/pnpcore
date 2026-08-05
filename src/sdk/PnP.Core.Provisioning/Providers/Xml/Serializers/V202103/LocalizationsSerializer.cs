using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Serializers.V202103
{
    /// <summary>
    /// Class to serialize/deserialize the Localization Settings
    /// </summary>
    [TemplateSchemaSerializer(
        MinimalSupportedSchemaVersion = XMLPnPSchemaVersion.V202103,
        SerializationSequence = 200, DeserializationSequence = 200,
        Scope = SerializerScope.Provisioning)]
    internal class LocalizationsSerializer : PnPBaseSchemaSerializer<Localization>
    {
        public override void Deserialize(object persistence, ProvisioningTemplate template)
        {
            var localizationsContainer = persistence.GetPublicInstancePropertyValue("Localizations");

            if (localizationsContainer != null)
            {
                // Try to get the default LCID or use the default one 1033
                var defaultLcidValue = localizationsContainer.GetPublicInstancePropertyValue("DefaultLCID");
                template.Localizations.DefaultLCID = defaultLcidValue != null ? (int)defaultLcidValue : 1033;

                // Process the localization items
                var localizations = localizationsContainer.GetPublicInstancePropertyValue("Localization");

                if (localizations != null)
                {
                    template.Localizations.AddRange(
                        PnPObjectsMapper.MapObjects(localizations,
                                new CollectionFromSchemaToModelTypeResolver(typeof(Localization)))
                                as IEnumerable<Localization>);
                }
            }
        }

        public override void Serialize(ProvisioningTemplate template, object persistence)
        {
            if (template.Localizations != null && template.Localizations.Count > 0)
            {
                var localizationsTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Localizations, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var localizationsType = Type.GetType(localizationsTypeName, true);

                var localizationTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.LocalizationsLocalization, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                var localizationType = Type.GetType(localizationTypeName, true);

                // Create an instance of the localization type
                persistence.SetPublicInstancePropertyValue("Localizations", Activator.CreateInstance(localizationsType));

                // Configure the collection of localizations
                var localizations = persistence.GetPublicInstancePropertyValue("Localizations");
                localizations.GetPublicInstanceProperty("Localization")
                    .SetValue(
                        localizations,
                        PnPObjectsMapper.MapObjects(template.Localizations,
                            new CollectionFromModelToSchemaTypeResolver(localizationType)));

                // Set the Default LCID, if not default
                if (template.Localizations.DefaultLCID != 1033)
                {
                    localizations.SetPublicInstancePropertyValue("DefaultLCID", template.Localizations.DefaultLCID);
                }
            }
        }
    }
}
