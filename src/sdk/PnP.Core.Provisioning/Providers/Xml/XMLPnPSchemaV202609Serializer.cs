namespace PnP.Core.Provisioning.Providers.Xml
{
    /// <summary>
    /// Implements the logic to serialize a schema of version 202609
    /// </summary>
    internal class XMLPnPSchemaV202609Serializer : XmlPnPSchemaBaseSerializer<V202609.ProvisioningTemplate>
    {
        public XMLPnPSchemaV202609Serializer() :
            base(typeof(XMLConstants)
                .Assembly
                .GetManifestResourceStream("PnP.Core.Provisioning.Providers.Xml.ProvisioningSchema-2026-09.xsd"))
        {
        }

        public override string NamespaceUri
        {
            get { return (XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2026_09); }
        }

        public override string NamespacePrefix
        {
            get { return (XMLConstants.PROVISIONING_SCHEMA_PREFIX); }
        }

        protected override void DeserializeTemplate(object persistenceTemplate, Model.ProvisioningTemplate template)
        {
            base.DeserializeTemplate(persistenceTemplate, template);
        }

        protected override void SerializeTemplate(Model.ProvisioningTemplate template, object persistenceTemplate)
        {
            base.SerializeTemplate(template, persistenceTemplate);
        }
    }
}

