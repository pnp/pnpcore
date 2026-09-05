using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace PnP.Core.Provisioning.Providers.Xml
{
    /// <summary>
    /// Base class for serialization/deserialization of provisioning templates
    /// with the new schema serializer
    /// </summary>
    /// <typeparam name="TSchemaTemplate"></typeparam>
    internal abstract class XmlPnPSchemaBaseSerializer<TSchemaTemplate> : IXMLSchemaFormatter, ITemplateFormatterWithValidation, IProvisioningHierarchyFormatter
        where TSchemaTemplate : new()
    {
        private TemplateProviderBase _provider;
        private readonly Stream _referenceSchema;

        protected TemplateProviderBase Provider => _provider;

        public XmlPnPSchemaBaseSerializer(Stream referenceSchema)
        {
            this._referenceSchema = referenceSchema ??
                throw new ArgumentNullException(nameof(referenceSchema));
        }

        public abstract string NamespacePrefix { get; }
        public abstract string NamespaceUri { get; }

        public void Initialize(TemplateProviderBase provider)
        {
            this._provider = provider;
        }

        /// <summary>
        /// Checks if the provided source Stream (the XML) is valid against the current XSD schema
        /// </summary>
        /// <param name="template">The source Stream (the XML)</param>
        /// <returns>Whether the XML template is valid or not</returns>
        public bool IsValid(Stream template)
        {
            return GetValidationResults(template).IsValid;
        }

        /// <summary>
        /// Checks if the provided source Stream (the XML) is valid against the current XSD schema
        /// </summary>
        /// <param name="template">The source Stream (the XML)</param>
        /// <returns>Whether the XML template is valid or not</returns>
        public ValidationResult GetValidationResults(Stream template)
        {
            var exceptions = new List<Exception>();
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            XDocument xml = XDocument.Load(template);

            XmlSchemaSet schemas = new XmlSchemaSet();
            this._referenceSchema.Seek(0, SeekOrigin.Begin);
            schemas.Add(((IXMLSchemaFormatter)this).NamespaceUri,
                new XmlTextReader(this._referenceSchema));

            Boolean result = true;
            xml.Validate(schemas, (o, e) =>
            {
                exceptions.Add(e.Exception);
                result = false;
            });

            return new ValidationResult { IsValid = result, Exceptions = exceptions };
        }




        /// <summary>
        /// Converts a Stream of bytes (the XML) into a XML-based object created using XmlSerializer
        /// </summary>
        /// <param name="template">The source Stream of bytes (the XML)</param>
        /// <param name="identifier">An optional identifier for the template to extract from the XML</param>
        /// <param name="result">A reference ProvisioningTemplate object</param>
        /// <returns>The resulting XML-based object extracted from the Stream</returns>
        protected Object ProcessInputStream(Stream template, string identifier, ProvisioningTemplate result)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            MemoryStream sourceStream = new MemoryStream();
            template.CopyTo(sourceStream);
            sourceStream.Position = 0;


            var validationResult = this.GetValidationResults(sourceStream);
            if (!validationResult.IsValid)
            {
                throw new ApplicationException("Template is not valid", new AggregateException(validationResult.Exceptions));
            }

            sourceStream.Position = 0;
            XDocument xml = XDocument.Load(sourceStream);
            XNamespace pnp = this.NamespaceUri;

            TSchemaTemplate source = default(TSchemaTemplate);

            if (xml.Root.Name == pnp + "Provisioning")
            {
                Object wrapper = null;
                var wrapperType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Provisioning, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
                XmlSerializer xmlSerializer = new XmlSerializer(wrapperType);
                using (var reader = xml.Root.CreateReader())
                {
                    wrapper = xmlSerializer.Deserialize(reader);
                }

                var serializers = GetSerializersForCurrentContext(SerializerScope.Provisioning, a => a?.DeserializationSequence);

                InvokeSerializers(result, wrapper, serializers, SerializationAction.Deserialize);

                var wrapperTemplates = wrapper.GetPublicInstancePropertyValue("Templates");

                if (wrapperTemplates != null)
                {
                    foreach (var templates in (IEnumerable)wrapperTemplates)
                    {
                        var provisioningTemplates = templates.GetPublicInstancePropertyValue("ProvisioningTemplate");

                        if (provisioningTemplates != null)
                        {
                            foreach (var t in (IEnumerable)provisioningTemplates)
                            {
                                var templateId = t.GetPublicInstancePropertyValue("ID") as String;

                                if ((templateId != null && templateId == identifier) || String.IsNullOrEmpty(identifier))
                                {
                                    source = (TSchemaTemplate)t;
                                }
                            }

                            if (source == null)
                            {
                                var provisioningTemplateFiles = templates.GetPublicInstancePropertyValue("ProvisioningTemplateFile");

                                if (source == null && provisioningTemplateFiles != null)
                                {
                                    foreach (var f in (IEnumerable)provisioningTemplateFiles)
                                    {
                                        var templateId = f.GetPublicInstancePropertyValue("ID") as String;

                                        if ((templateId != null && templateId == identifier) || String.IsNullOrEmpty(identifier))
                                        {
                                            var externalFile = f.GetPublicInstancePropertyValue("File") as String;

                                            if (!String.IsNullOrEmpty(externalFile))
                                            {
                                                Stream externalFileStream = this.Provider.Connector.GetFileStream(externalFile);
                                                xml = XDocument.Load(externalFileStream);

                                                if (xml.Root.Name != pnp + "ProvisioningTemplate")
                                                {
                                                    throw new ApplicationException("Invalid external file format. Expected a ProvisioningTemplate file!");
                                                }
                                                else
                                                {
                                                    source = XMLSerializer.Deserialize<TSchemaTemplate>(xml);
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (source != null)
                        {
                            break;
                        }
                    }
                }
            }
            else if (xml.Root.Name == pnp + "ProvisioningTemplate")
            {
                var IdAttribute = xml.Root.Attribute("ID");

                if (!String.IsNullOrEmpty(identifier) &&
                    IdAttribute != null &&
                    IdAttribute.Value != identifier)
                {
                    throw new ApplicationException("The provided template identifier is not available!");
                }
                else
                {
                    source = XMLSerializer.Deserialize<TSchemaTemplate>(xml);
                }
            }

            return (source);
        }

        /// <summary>
        /// Deserializes a Stream of bytes (the XML) into a Provisioning Template
        /// </summary>
        /// <param name="template">The source Stream of bytes (the XML)</param>
        /// <returns>The deserialized Provisioning Template</returns>
        public ProvisioningTemplate ToProvisioningTemplate(Stream template)
        {
            return (this.ToProvisioningTemplate(template, null));
        }

        /// <summary>
        /// Deserializes a Stream of bytes (the XML) into a Provisioning Template, based on an optional identifier
        /// </summary>
        /// <param name="template">The source Stream of bytes (the XML)</param>
        /// <param name="identifier">An optional identifier for the template to deserialize</param>
        /// <returns>The deserialized Provisioning Template</returns>
        public ProvisioningTemplate ToProvisioningTemplate(Stream template, string identifier)
        {
            using (var scope = new PnPSerializationScope(typeof(TSchemaTemplate)))
            {
                var result = new ProvisioningTemplate();

                var source = ProcessInputStream(template, identifier, result);

                DeserializeTemplate(source, result);

                return (result);
            }
        }

        /// <summary>
        /// This method deserializes an XML-based object, created with XmlSerializer, into a Provisioning Template
        /// </summary>
        /// <param name="persistenceTemplate">The XML-based object</param>
        /// <param name="template">The resulting template</param>
        protected virtual void DeserializeTemplate(Object persistenceTemplate, ProvisioningTemplate template)
        {
            var serializers = GetSerializersForCurrentContext(SerializerScope.ProvisioningTemplate, a => a?.DeserializationSequence);

            InvokeSerializers(template, persistenceTemplate, serializers, SerializationAction.Deserialize);
        }

        /// <summary>
        /// Serializes an in-memory ProvisioningTemplate into a Stream (the XML)
        /// </summary>
        /// <param name="template">The ProvisioningTemplate to serialize</param>
        /// <returns>The resulting Stream (the XML)</returns>
        public Stream ToFormattedTemplate(ProvisioningTemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            using (var scope = new PnPSerializationScope(typeof(TSchemaTemplate)))
            {
                var result = new TSchemaTemplate();
                Stream output = null;

                output = ProcessOutputStream(template, result);

                return (output);
            }
        }

        /// <summary>
        /// Serializes an in-memory ProvisioningTemplate into a Stream (the XML)
        /// </summary>
        /// <param name="template">The ProvisioningTemplate to serialize</param>
        /// <param name="result">The typed XML-based object defined using XmlSerializer</param>
        /// <returns>The resulting Stream (the XML)</returns>
        protected Stream ProcessOutputStream(ProvisioningTemplate template, TSchemaTemplate result)
        {
            Type wrapperType;
            object wrapper, templatesItem;
            Array templates;

            ProcessOutputHierarchy(template, out wrapperType, out wrapper, out templates, out templatesItem);

            var provisioningTemplates = Array.CreateInstance(typeof(TSchemaTemplate), 1);
            provisioningTemplates.SetValue(result, 0);

            templatesItem.SetPublicInstancePropertyValue("ProvisioningTemplate", provisioningTemplates);

            templates.SetValue(templatesItem, 0);

            wrapper.SetPublicInstancePropertyValue("Templates", templates);

            SerializeTemplate(template, result);

            XmlSerializerNamespaces ns =
                new XmlSerializerNamespaces();
            ns.Add(((IXMLSchemaFormatter)this).NamespacePrefix,
                ((IXMLSchemaFormatter)this).NamespaceUri);

            MemoryStream output = new MemoryStream();
            XmlSerializer xmlSerializer = new XmlSerializer(wrapperType);
            if (ns != null)
            {
                xmlSerializer.Serialize(output, wrapper, ns);
            }
            else
            {
                xmlSerializer.Serialize(output, wrapper);
            }

            output.Position = 0;
            return (output);
        }

        /// <summary>
        /// Prepares a ProvisioningTemplate to be wrapped into the Hierarchy container object
        /// </summary>
        /// <param name="template">The ProvisioningTemplate to wrap</param>
        /// <param name="wrapperType">The Type of the wrapper</param>
        /// <param name="wrapper">The wrapper</param>
        /// <param name="templates">The collection of template within the wrapper</param>
        /// <param name="templatesItem">The template to add</param>
        private void ProcessOutputHierarchy(ProvisioningTemplate template, out Type wrapperType, out object wrapper, out Array templates, out object templatesItem)
        {
            wrapperType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Provisioning, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
            wrapper = Activator.CreateInstance(wrapperType);

            var preferencesType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Preferences, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
            Object preferences = Activator.CreateInstance(preferencesType);

            wrapper.SetPublicInstancePropertyValue("Preferences", preferences);

            var serializers = GetSerializersForCurrentContext(SerializerScope.Provisioning, a => a?.SerializationSequence);

            InvokeSerializers(template, wrapper, serializers, SerializationAction.Serialize);

            serializers = GetSerializersForCurrentContext(SerializerScope.Tenant, a => a?.SerializationSequence);

            InvokeSerializers(template, wrapper, serializers, SerializationAction.Serialize);

            if (template.ParentHierarchy != null)
            {
                wrapper.SetPublicInstancePropertyValue("Author", template.ParentHierarchy.Author);
                wrapper.SetPublicInstancePropertyValue("DisplayName", template.ParentHierarchy.DisplayName);
                wrapper.SetPublicInstancePropertyValue("Description", template.ParentHierarchy.Description);
                wrapper.SetPublicInstancePropertyValue("ImagePreviewUrl", template.ParentHierarchy.ImagePreviewUrl);
                wrapper.SetPublicInstancePropertyValue("Generator", template.ParentHierarchy.Generator);
                wrapper.SetPublicInstancePropertyValue("Version", (Decimal)template.ParentHierarchy.Version);
            }

            preferences.SetPublicInstancePropertyValue("Generator", this.GetType().Assembly.FullName);

            var templatesType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Templates, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
            templates = Array.CreateInstance(templatesType, 1);
            templatesItem = Activator.CreateInstance(templatesType);
            templatesItem.SetPublicInstancePropertyValue("ID", $"CONTAINER-{template.Id}");
        }

        /// <summary>
        /// Serializes a ProvisioningTemplate into a XML-based object generated with XmlSerializer
        /// </summary>
        /// <param name="template">The ProvisioningTemplate to serialize</param>
        /// <param name="persistenceTemplate">The XML-based object to serialize the template into</param>
        protected virtual void SerializeTemplate(ProvisioningTemplate template, Object persistenceTemplate)
        {
            var serializers = GetSerializersForCurrentContext(SerializerScope.ProvisioningTemplate, a => a?.SerializationSequence);

            InvokeSerializers(template, persistenceTemplate, serializers, SerializationAction.Serialize);
        }

        /// <summary>
        /// Allows to retrieve the current XML Schema version
        /// </summary>
        /// <returns>The current XML schema version</returns>
        private static XMLPnPSchemaVersion GetCurrentSchemaVersion()
        {
            var currentSchemaTemplateNamespace = typeof(TSchemaTemplate).Namespace;
            var currentSchemaVersionString = $"V{currentSchemaTemplateNamespace.Substring(currentSchemaTemplateNamespace.IndexOf(".Xml.") + 6)}";
            var currentSchemaVersion = (XMLPnPSchemaVersion)Enum.Parse(typeof(XMLPnPSchemaVersion), currentSchemaVersionString);
            return currentSchemaVersion;
        }

        /// <summary>
        /// Serializes a ProvisioningHierarchy into a Stream (the XML)
        /// </summary>
        /// <param name="hierarchy">The ProvisioningHierarchy to serialize</param>
        /// <returns>The resulting Stream (the XML)</returns>
        public Stream ToFormattedHierarchy(ProvisioningHierarchy hierarchy)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            using (var scope = new PnPSerializationScope(typeof(TSchemaTemplate)))
            {
                var dummyTemplate = new ProvisioningTemplate
                {
                    Id = $"DUMMY-{Guid.NewGuid()}"
                };
                hierarchy.Templates.Add(dummyTemplate);

                Type wrapperType;
                object wrapper, templatesItem;
                Array templates;

                ProcessOutputHierarchy(dummyTemplate, out wrapperType, out wrapper, out templates, out templatesItem);

                var serializers = GetSerializersForCurrentContext(SerializerScope.ProvisioningHierarchy, a => a?.SerializationSequence);

                InvokeSerializers(dummyTemplate, wrapper, serializers, SerializationAction.Serialize);

                hierarchy.Templates.Remove(dummyTemplate);

                var provisioningTemplates = Array.CreateInstance(typeof(TSchemaTemplate), hierarchy.Templates.Count);
                for (int c = 0; c < hierarchy.Templates.Count; c++)
                {
                    var outputTemplate = new TSchemaTemplate();

                    SerializeTemplate(hierarchy.Templates[c], outputTemplate);

                    provisioningTemplates.SetValue(outputTemplate, c);
                }

                templatesItem.SetPublicInstancePropertyValue("ProvisioningTemplate", provisioningTemplates);

                templates.SetValue(templatesItem, 0);

                if (provisioningTemplates.Length > 0)
                {
                    wrapper.SetPublicInstancePropertyValue("Templates", templates);
                }

                XmlSerializerNamespaces ns =
                    new XmlSerializerNamespaces();
                ns.Add(((IXMLSchemaFormatter)this).NamespacePrefix,
                    ((IXMLSchemaFormatter)this).NamespaceUri);

                MemoryStream output = new MemoryStream();
                XmlSerializer xmlSerializer = new XmlSerializer(wrapperType);
                if (ns != null)
                {
                    xmlSerializer.Serialize(output, wrapper, ns);
                }
                else
                {
                    xmlSerializer.Serialize(output, wrapper);
                }

                output.Position = 0;
                return (output);
            }
        }

        /// <summary>
        /// Deserializes a source Stream (the XML) into a ProvisioningHierarchy 
        /// </summary>
        /// <param name="hierarchy">The source Stream (the XML)</param>
        /// <returns>The resulting ProvisioningHierarchy object</returns>
        public ProvisioningHierarchy ToProvisioningHierarchy(Stream hierarchy)
        {
            MemoryStream sourceStream = new MemoryStream();
            hierarchy.Position = 0;
            hierarchy.CopyTo(sourceStream);
            sourceStream.Position = 0;

            var validationResult = this.GetValidationResults(sourceStream);
            if (!validationResult.IsValid)
            {
                throw new ApplicationException("Template is not valid", new AggregateException(validationResult.Exceptions));
            }

            ProvisioningHierarchy resultHierarchy = new ProvisioningHierarchy();

            sourceStream.Position = 0;
            XDocument xml = XDocument.Load(sourceStream);
            if (xml.Root.Name.LocalName != "Provisioning")
            {
                throw new ApplicationException("The provided provisioning file is not a Hierarchy!");
            }

            var innerFormatter = XMLPnPSchemaFormatter.GetSpecificFormatter(
                xml.Root.Name.NamespaceName);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(new System.Xml.NameTable());
            nsManager.AddNamespace("pnp", xml.Root.Name.NamespaceName);

            var templates = xml.XPathSelectElements("/pnp:Provisioning/pnp:Templates/pnp:ProvisioningTemplate", nsManager).ToList();

            foreach (var template in templates)
            {
                MemoryStream templateStream = new MemoryStream();
                template.Save(templateStream);
                templateStream.Position = 0;

                var provisioningTemplate = innerFormatter.ToProvisioningTemplate(templateStream);

                resultHierarchy.Templates.Add(provisioningTemplate);
            }

            var templateFiles = xml.XPathSelectElements("/pnp:Provisioning/pnp:Templates/pnp:ProvisioningTemplateFile", nsManager).ToList();

            foreach (var template in templateFiles)
            {
                var templateID = template.Attribute("ID")?.Value;
                var templateFile = template.Attribute("File")?.Value;
                if (!String.IsNullOrEmpty(templateFile) && !String.IsNullOrEmpty(templateID))
                {
                    var provisioningTemplate = this._provider.GetTemplate(templateFile);
                    provisioningTemplate.Id = templateID;

                    resultHierarchy.Templates.Add(provisioningTemplate);
                }
            }


            using (var scope = new PnPSerializationScope(typeof(TSchemaTemplate)))
            {
                var dummyTemplate = new ProvisioningTemplate
                {
                    Id = $"DUMMY-{Guid.NewGuid()}"
                };
                resultHierarchy.Templates.Add(dummyTemplate);

                Object wrapper = null;
                var wrapperType = Type.GetType($"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Provisioning, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}", true);
                XmlSerializer xmlSerializer = new XmlSerializer(wrapperType);
                using (var reader = xml.Root.CreateReader())
                {
                    wrapper = xmlSerializer.Deserialize(reader);
                }

                #region Process Provisioning level serializers

                var serializers = GetSerializersForCurrentContext(SerializerScope.Provisioning, a => a?.DeserializationSequence);

                InvokeSerializers(dummyTemplate, wrapper, serializers, SerializationAction.Deserialize);

                #endregion

                #region Process Tenant level serializers

                serializers = GetSerializersForCurrentContext(SerializerScope.Tenant, a => a?.DeserializationSequence);

                InvokeSerializers(dummyTemplate, wrapper, serializers, SerializationAction.Deserialize);

                #endregion

                #region Process ProvisioningHierarchy level serializers

                serializers = GetSerializersForCurrentContext(SerializerScope.ProvisioningHierarchy, a => a?.DeserializationSequence);

                InvokeSerializers(dummyTemplate, wrapper, serializers, SerializationAction.Deserialize);

                #endregion

                resultHierarchy.Templates.Remove(dummyTemplate);
            }

            return (resultHierarchy);
        }

        private IOrderedEnumerable<IGrouping<string, Type>> GetSerializersForCurrentContext(SerializerScope scope,
            Func<TemplateSchemaSerializerAttribute, Int32?> sortingSelector)
        {
            var currentAssembly = this.GetType().Assembly;

            XMLPnPSchemaVersion currentSchemaVersion = GetCurrentSchemaVersion();

            var serializers = currentAssembly.GetTypes()
                .Where(t => t.GetInterface(typeof(IPnPSchemaSerializer).FullName) != null
                       && t.BaseType.Name == typeof(Xml.PnPBaseSchemaSerializer<>).Name)
                .Where(t =>
                {
                    var a = t.GetCustomAttributes<TemplateSchemaSerializerAttribute>(false).FirstOrDefault();
                    return (a.MinimalSupportedSchemaVersion <= currentSchemaVersion && a.Scope == scope);
                })
                .OrderByDescending(s =>
                {
                    var a = s.GetCustomAttributes<TemplateSchemaSerializerAttribute>(false).FirstOrDefault();
                    return (a.MinimalSupportedSchemaVersion);
                }
                )
                .GroupBy(t => t.BaseType.GenericTypeArguments.FirstOrDefault()?.FullName)
                .OrderBy(g =>
                {
                    var maxInGroup = g.OrderByDescending(s =>
                    {
                        var a = s.GetCustomAttributes<TemplateSchemaSerializerAttribute>(false).FirstOrDefault();
                        return (a.MinimalSupportedSchemaVersion);
                    }
                    ).FirstOrDefault();
                    return sortingSelector(maxInGroup.GetCustomAttributes<TemplateSchemaSerializerAttribute>(false).FirstOrDefault());
                });
            return serializers;
        }

        private static void InvokeSerializers(ProvisioningTemplate template, object persistenceTemplate,
            IOrderedEnumerable<IGrouping<string, Type>> serializers, SerializationAction action)
        {
            foreach (var group in serializers)
            {
                var serializerType = group.FirstOrDefault();
                if (serializerType != null)
                {
                    var serializer = Activator.CreateInstance(serializerType) as IPnPSchemaSerializer;
                    if (serializer != null)
                    {
                        if (action == SerializationAction.Serialize)
                        {
                            serializer.Serialize(template, persistenceTemplate);
                        }
                        else
                        {
                            serializer.Deserialize(persistenceTemplate, template);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Defines the action to execute with a pool of serializers
    /// </summary>
    internal enum SerializationAction
    {
        /// <summary>
        /// Will serialize content
        /// </summary>
        Serialize,
        /// <summary>
        /// Will deserialize content
        /// </summary>
        Deserialize
    }
}
