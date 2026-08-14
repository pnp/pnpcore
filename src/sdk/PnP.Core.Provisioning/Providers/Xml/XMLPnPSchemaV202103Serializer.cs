namespace PnP.Core.Provisioning.Providers.Xml
{
    /// <summary>
    /// Implements the logic to serialize a schema of version 202103
    /// </summary>
    internal class XMLPnPSchemaV202103Serializer : XmlPnPSchemaBaseSerializer<V202103.ProvisioningTemplate>
    {
        public XMLPnPSchemaV202103Serializer() :
            base(typeof(XMLConstants)
                .Assembly
                .GetManifestResourceStream("PnP.Core.Provisioning.Providers.Xml.ProvisioningSchema-2021-03.xsd"))
        {
        }

        public override string NamespaceUri
        {
            get { return (XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2021_03); }
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

