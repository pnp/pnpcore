using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services;
using PnP.M365.DomainModelGenerator.Mappings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Implements the logic to process an EDMX metadata file
    /// </summary>
    internal class EdmxProcessor : IEdmxProcessor
    {
        private readonly EdmxProcessorOptions options;
        private readonly IAuthenticationProviderFactory authProviderFactory;
        private readonly ILogger log;

        // Define a global HttpClient object to download metadata files
        private readonly HttpClient httpClient = new HttpClient();

        public EdmxProcessor(
            IOptionsMonitor<EdmxProcessorOptions> options,
            IAuthenticationProviderFactory authenticationProviderFactory,
            ILogger<EdmxProcessor> logger,
            IServiceProvider serviceProvider)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (authenticationProviderFactory == null)
            {
                throw new ArgumentNullException(nameof(authenticationProviderFactory));
            }

            this.options = options.CurrentValue;
            this.authProviderFactory = authenticationProviderFactory;
            this.log = logger;
        }

        public async Task<Model> ProcessAsync()
        {
            Model result = null;

            // Load into memory the EDMX -> Domain Model mapping file
            var entityMappings = await LoadMappingFileAsync();

            // Process the EDMX metadata providers
            this.log.LogInformation($"Started processing of metadata providers.");

            var edmxDocuments = new Dictionary<string, XDocument>();

            foreach (var provider in this.options.EdmxProviders)
            {
                this.log.LogInformation($"Started processing of '{provider.EdmxProviderName}' metadata.");

                XDocument edmxDocument = await LoadMetadataFileAsync(new Uri(provider.MetadataUri), provider.AuthenticationProviderName);
                edmxDocuments.Add(provider.EdmxProviderName, edmxDocument);

                this.log.LogInformation($"Finished processing of '{provider.EdmxProviderName}' metadata.");
            }

            this.log.LogInformation($"Finished processing of metadata providers.");

            // Process the actual mapping
            this.log.LogInformation($"Started mapping entities.");

            result = ProcessMetadataFile(entityMappings, edmxDocuments);

            this.log.LogInformation($"Finished mapping entities.");

            return result;
        }

        private async Task<ModelMappings> LoadMappingFileAsync()
        {
            ModelMappings result = null;

            this.log.LogInformation($"Started loading mapping file from {this.options.MappingFilePath}.");

            var jsonMappingsString = await File.ReadAllTextAsync(this.options.MappingFilePath);

            result = JsonSerializer.Deserialize<ModelMappings>(jsonMappingsString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            this.log.LogInformation($"Finished loading mapping file from {this.options.MappingFilePath}.");

            return result;
        }

        private async Task<XDocument> LoadMetadataFileAsync(Uri edmxUri, string authenticationProviderName)
        {
            XDocument result = null;

            using (var request = new HttpRequestMessage(HttpMethod.Get, edmxUri))
            {
                // Hookup authentication, if any
                if (authenticationProviderName != null)
                {
                    var authenticationProvider = this.authProviderFactory.Create(authenticationProviderName);
                    await authenticationProvider.AuthenticateRequestAsync(edmxUri, request).ConfigureAwait(false);
                }
                HttpResponseMessage response = await this.httpClient.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    // Read the EDMX response
                    var edmx = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    result = XDocument.Parse(edmx);
                }
                else
                {
                    // Something went wrong...
                    throw new Exception(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
            }

            return result;
        }

        private Model ProcessMetadataFile(ModelMappings entityMappings, Dictionary<string, XDocument> edmxDocuments)
        {
            Model result = new Model();

            // Configure the EDMX namespaces for XML
            var edmxNamespaces = new Dictionary<string, XNamespace>();
            var schemaNamespaces = new Dictionary<string, XNamespace>();

            foreach (var p in entityMappings.Providers)
            {
                edmxNamespaces[p.Provider] = (XNamespace)p.EdmxNamespace;
                schemaNamespaces[p.Provider] = (XNamespace)p.SchemaNamespace;
            }

            // For every namespace
            foreach (var ns in entityMappings.Namespaces)
            {
                // And for every type in the namespace
                foreach (var t in ns.Types)
                {
                    ModelEntity entity = new ModelEntity();
                    entity.TypeName = t.Name;
                    entity.Namespace = ns.Name;

                    var providersProperties = new Dictionary<string, ModelProperty>();

                    // For every set of mappings
                    foreach (var m in t.Mappings)
                    {
                        // Get the REST URL of the provider for the current entity
                        entity.ProvidersUris[m.Provider] = m.RestUrl;

                        // Now process the properties from the EDMX providers
                        var providerSchema = edmxDocuments[m.Provider].Descendants(schemaNamespaces[m.Provider] + "Schema").FirstOrDefault(s => s.Attribute("Namespace")?.Value == m.ProviderEntity.Namespace);

                        if (providerSchema != null)
                        {
                            // First get the target entity
                            var providerEntity = providerSchema.Elements(schemaNamespaces[m.Provider] + "EntityType").FirstOrDefault(e => e.Attribute("Name")?.Value == m.ProviderEntity.Name);

                            if (providerEntity != null)
                            {
                                // Process simple properties
                                foreach (var property in providerEntity.Elements(schemaNamespaces[m.Provider] + "Property"))
                                {
                                    var propertyNameAttribute = property.Attribute("Name");
                                    var propertyTypeAttribute = property.Attribute("Type");
                                    if (propertyNameAttribute != null && propertyTypeAttribute != null)
                                    {
                                        this.log.LogInformation($"Processing property '{propertyNameAttribute.Value}' of type '{m.ProviderEntity.Namespace}.{m.ProviderEntity.Name}'");

                                        var propertyName = NormalizePropertyName(propertyNameAttribute.Value);
                                        var propertyType = ResolvePropertyType(propertyTypeAttribute.Value);
                                        if (propertyType != null)
                                        {
                                            // If the current property is not in the exclusion list
                                            if (m.ExcludedProperties != null &&
                                                !m.ExcludedProperties.Contains(propertyNameAttribute.Value))
                                            {
                                                // Prepare the variable to contain the property mapping information
                                                ModelProperty entityProperty = null;

                                                // Try to see if the property is already defined in the entity
                                                bool newProperty = false;

                                                // See if there is a mapping defined in the mappings model
                                                var mappingProperty = m.Properties.FirstOrDefault(p => p.ProviderName == propertyNameAttribute.Value);
                                                if (mappingProperty != null)
                                                {
                                                    // Search the property using the Domain Model name
                                                    entityProperty = entity.Properties.FirstOrDefault(p => p.Name == mappingProperty.Name);
                                                }
                                                else
                                                {
                                                    // Otherwise search the property using the provider's name
                                                    entityProperty = entity.Properties.FirstOrDefault(p => p.Name == propertyName);
                                                }

                                                // If it is a new property, simply create it
                                                if (entityProperty == null)
                                                {
                                                    entityProperty = new ModelProperty
                                                    {
                                                        // Use the Domain Model if any, or use the actual property name
                                                        Name = mappingProperty != null ? mappingProperty.Name : propertyName,
                                                        Type = propertyType,
                                                        Expandable = false,
                                                    };
                                                    newProperty = true;
                                                }

                                                // Then add the property mapping information
                                                entityProperty.ProvidersProperties[m.Provider] = propertyNameAttribute.Value;

                                                if (newProperty)
                                                {
                                                    entity.Properties.Add(entityProperty);
                                                }
                                            }
                                        }
                                    }
                                }

                                // Process navigation properties
                                foreach (var navigationProperty in providerEntity.Elements(schemaNamespaces[m.Provider] + "NavigationProperty"))
                                {
                                    var navigationPropertyNameAttribute = navigationProperty.Attribute("Name");
                                    var navigationPropertyRelationshipAttribute = navigationProperty.Attribute("Relationship");
                                    if (navigationPropertyNameAttribute != null && navigationPropertyRelationshipAttribute != null)
                                    {
                                        this.log.LogInformation($"Processing navigation property '{navigationPropertyNameAttribute.Value}' of type '{m.ProviderEntity.Namespace}.{m.ProviderEntity.Name}'");
                                    }
                                }
                            }
                        }
                    }

                    result.Entities.Add(entity);
                }
            }

            return result;
        }

        private string ResolvePropertyType(string propertyTypeName)
        {
            switch (propertyTypeName)
            {
                case "Edm.String":
                    return "string";
                case "Edm.Boolean":
                    return "bool";
                case "Edm.Guid":
                    return "System.Guid";
                case "Edm.Int32":
                    return "int";
                case "Edm.DateTime":
                    return "System.DateTime";
                default:
                    return null;
            }
        }

        private string NormalizePropertyName(string propertyName)
        {
            if (propertyName != null)
            {
                if (propertyName.Length > 1)
                {
                    return $"{propertyName.Substring(0, 1).ToUpper()}{propertyName.Substring(1, propertyName.Length - 1)}";
                }
                else
                {
                    return propertyName.ToUpper();
                }
            }
            else
            {
                return null;
            }
        }
    }
}
