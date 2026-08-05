using System;
using System.IO;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.Providers.Xml
{
    /// <summary>
    /// Helper class that abstracts from any specific version of XMLPnPSchemaFormatter
    /// </summary>
    public class XMLPnPSchemaFormatter : ITemplateFormatterWithValidation
    {
        private TemplateProviderBase _provider;

        public void Initialize(TemplateProviderBase provider)
        {
            this._provider = provider;
        }

        #region Static methods and properties

        /// <summary>
        /// Static property to retrieve an instance of the latest XMLPnPSchemaFormatter
        /// </summary>
        public static ITemplateFormatter LatestFormatter
        {
            get
            {
                return (new XMLPnPSchemaV202209Serializer());
            }
        }

        /// <summary>
        /// Static method to retrieve a specific XMLPnPSchemaFormatter instance
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static ITemplateFormatter GetSpecificFormatter(XMLPnPSchemaVersion version)
        {
            switch (version)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                case XMLPnPSchemaVersion.V201903:
                    return (new XMLPnPSchemaV201903Serializer());
#pragma warning restore CS0618 // Type or member is obsolete
                case XMLPnPSchemaVersion.V201909:
                    return (new XMLPnPSchemaV201909Serializer());
                case XMLPnPSchemaVersion.V202002:
                    return (new XMLPnPSchemaV202002Serializer());
                case XMLPnPSchemaVersion.V202103:
                    return (new XMLPnPSchemaV202103Serializer());
                case XMLPnPSchemaVersion.V202209:
                default:
                    return (new XMLPnPSchemaV202209Serializer());
            }
        }

        /// <summary>
        /// Static method to retrieve a specific XMLPnPSchemaFormatter instance
        /// </summary>
        /// <param name="namespaceUri"></param>
        /// <returns></returns>
        public static ITemplateFormatter GetSpecificFormatter(string namespaceUri)
        {
            switch (namespaceUri)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                case XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2019_03:
                    return new XMLPnPSchemaV201903Serializer();
#pragma warning restore CS0618 // Type or member is obsolete
                case XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2019_09:
                    return new XMLPnPSchemaV201909Serializer();
                case XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2020_02:
                    return new XMLPnPSchemaV202002Serializer();
                case XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2021_03:
                    return new XMLPnPSchemaV202103Serializer();
                case XMLConstants.PROVISIONING_SCHEMA_NAMESPACE_2022_09:
                default:
                    return new XMLPnPSchemaV202209Serializer();
            }
        }

        #endregion

        #region Abstract methods implementation

        public bool IsValid(System.IO.Stream template)
        {
            ITemplateFormatter formatter = this.GetSpecificFormatterInternal(ref template);
            formatter.Initialize(this._provider);
            return (formatter.IsValid(template));
        }

        public ValidationResult GetValidationResults(Stream template)
        {
            var formatter = this.GetSpecificFormatterInternal(ref template);
            formatter.Initialize(this._provider);
            if (formatter is ITemplateFormatterWithValidation)
            {
                return ((ITemplateFormatterWithValidation)formatter).GetValidationResults(template);
            }
            return null;
        }


        public System.IO.Stream ToFormattedTemplate(Model.ProvisioningTemplate template)
        {
            ITemplateFormatter formatter = XMLPnPSchemaFormatter.LatestFormatter;
            formatter.Initialize(this._provider);
            return (formatter.ToFormattedTemplate(template));
        }

        public Model.ProvisioningTemplate ToProvisioningTemplate(System.IO.Stream template)
        {
            return (this.ToProvisioningTemplate(template, null));
        }

        public Model.ProvisioningTemplate ToProvisioningTemplate(System.IO.Stream template, String identifier)
        {
            ITemplateFormatter formatter = this.GetSpecificFormatterInternal(ref template);
            formatter.Initialize(this._provider);
            return (formatter.ToProvisioningTemplate(template, identifier));
        }

        #endregion

        #region Helper Methods

        internal ITemplateFormatter GetSpecificFormatterInternal(ref System.IO.Stream template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            // Create a copy of the source stream
            MemoryStream sourceStream = new MemoryStream();
            template.Position = 0;
            template.CopyTo(sourceStream);
            sourceStream.Position = 0;
            template = sourceStream;

            XDocument xml = XDocument.Load(template);
            template.Position = 0;

            String targetNamespaceUri = xml.Root.Name.NamespaceName;

            if (!String.IsNullOrEmpty(targetNamespaceUri))
            {
                return (XMLPnPSchemaFormatter.GetSpecificFormatter(targetNamespaceUri));
            }
            else
            {
                return (XMLPnPSchemaFormatter.LatestFormatter);
            }
        }
        #endregion
    }
}

