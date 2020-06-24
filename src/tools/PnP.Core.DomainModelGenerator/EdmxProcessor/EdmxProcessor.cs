using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services;
using PnP.M365.DomainModelGenerator.CodeAnalyzer;
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
        private readonly ICodeAnalyzer codeAnalyzer;

        // Define a global HttpClient object to download metadata files
        private readonly HttpClient httpClient = new HttpClient();

        public EdmxProcessor(
            IOptionsMonitor<EdmxProcessorOptions> options,
            IAuthenticationProviderFactory authenticationProviderFactory,
            ILogger<EdmxProcessor> logger,
            IServiceProvider serviceProvider,
            ICodeAnalyzer codeAnalyzer)
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
            this.codeAnalyzer = codeAnalyzer;
        }

        public async Task<Model> ProcessAsync()
        {
            // Load into memory the EDMX -> Domain Model mapping file
            var entityMappings = await LoadMappingFileAsync();

            var spExclusions = await LoadUnifiedMappingFileAsync(true);
            var graphExclusions = await LoadUnifiedMappingFileAsync(false);

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

            //result = ProcessMetadataFile(entityMappings, edmxDocuments);

            var preparedModelData = await ProcessUnifiedModelFiles(spExclusions, graphExclusions, edmxDocuments);
            Model result = new Model
            {
                SharePoint = preparedModelData.Item1,
                Graph = preparedModelData.Item2
            };

            this.log.LogInformation($"Finished mapping entities.");

            return result;
        }

        private async Task<UnifiedModelMapping> LoadUnifiedMappingFileAsync(bool loadSharePointExclusions)
        {
            string pathToLoad = loadSharePointExclusions ? this.options.SPMappingFilePath : this.options.GraphMappingFilePath;

            this.log.LogInformation($"Started loading SPO exclusion file from {pathToLoad}.");

            var jsonExclusionsString = await File.ReadAllTextAsync(pathToLoad);

            UnifiedModelMapping result = JsonSerializer.Deserialize<UnifiedModelMapping>(jsonExclusionsString,
                                                                        new JsonSerializerOptions
                                                                        {
                                                                            PropertyNameCaseInsensitive = true
                                                                        });
            this.log.LogInformation($"Finished loading exclusions file from {pathToLoad}.");

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

        private async Task<Tuple<UnifiedModel, UnifiedModel>> ProcessUnifiedModelFiles(UnifiedModelMapping spExclusions, UnifiedModelMapping graphExclusions, Dictionary<string, XDocument> edmxDocuments)
        { 
            // Combine settings+data from exclusion files with data coming from code analysis to build the model used for code generation
            if (codeAnalyzer == null)
            {
                throw new Exception("There was no code analyzer available");
            }

            // Process SPO
            var spoModel = await ProcessProvider(spExclusions, edmxDocuments.Where(p => p.Key == "SPO").First());

            // Process Graph
            //var graphModel = await ProcessProvider(graphExclusions, edmxDocuments.Where(p => p.Key == "MSGraph").First());

            return new Tuple<UnifiedModel, UnifiedModel>(spoModel, null);
        }

        private async Task<UnifiedModel> ProcessProvider(UnifiedModelMapping mapping, KeyValuePair<string, XDocument> edmxDocumentInfo)
        {
            UnifiedModel model = new UnifiedModel
            {
                Provider = edmxDocumentInfo.Key
            };

            Dictionary<string, AnalyzedModelType> analyzedModelTypes = new Dictionary<string, AnalyzedModelType>();
            var edmxDocument = edmxDocumentInfo.Value;

            foreach (var location in mapping.Locations)
            {
                // Combine code information with information from the metadata into a model that will be used to drive code generation
                var edmxNamespace = (XNamespace)mapping.EdmxNamespace;
                var schemaNamespace = (XNamespace)mapping.SchemaNamespace;

                // Get information about the current code implementation
                var analyzedTypes = await codeAnalyzer.ProcessNamespace(location);
                analyzedTypes.ToList().ForEach(x => 
                {
                    if (!analyzedModelTypes.ContainsKey(x.Key))
                    {
                        analyzedModelTypes.Add(x.Key, x.Value);
                    }
                });

                // Now process the properties from the EDMX providers
                var providerSchemaElements = edmxDocument.Descendants(schemaNamespace + "Schema").Where(e => e.Attribute("Namespace")?.Value == location.SchemaNamespace);
                foreach (var providerSchemaElement in providerSchemaElements)
                {
                    //var providerEntities = providerSchemaElement.Elements(schemaNamespace + "EntityType").Where(e => e.Attribute("Name")?.Value == "Web");
                    var providerEntities = providerSchemaElement.Elements(schemaNamespace + "EntityType");
                    foreach (var providerEntity in providerEntities)
                    {

                        UnifiedModelEntity entity = new UnifiedModelEntity
                        {
                            TypeName = providerEntity.Attribute("Name").Value,
                            Namespace = location.Namespace,
                            SchemaNamespace = location.SchemaNamespace,
                            Folder = location.Folder,
                            SPRestType = $"{providerSchemaElement.Attribute("Namespace").Value}.{providerEntity.Attribute("Name").Value}",
                            BaseType = providerEntity.Attribute("BaseType")?.Value
                        };

                        // Process simple properties
                        foreach (var property in providerEntity.Elements(schemaNamespace + "Property"))
                        {
                            var propertyNameAttribute = property.Attribute("Name");
                            var propertyTypeAttribute = property.Attribute("Type");
                            if (propertyNameAttribute != null && propertyTypeAttribute != null)
                            {
                                var propertyName = NormalizePropertyName(propertyNameAttribute.Value);
                                var propertyType = ResolvePropertyType(propertyTypeAttribute.Value);
                                if (propertyType != null)
                                {
                                    UnifiedModelProperty modelProp = new UnifiedModelProperty()
                                    {
                                        Name = propertyName,
                                        Type = propertyType,
                                        NavigationProperty = false
                                    };
                                    entity.Properties.Add(modelProp);
                                }
                            }
                        }

                        // Process navigation properties
                        foreach (var property in providerEntity.Elements(schemaNamespace + "NavigationProperty"))
                        {
                            //<NavigationProperty Name="Alerts" Relationship="SP.SP_Web_Alerts_SP_Alert_AlertsPartner" ToRole="Alerts" FromRole="AlertsPartner" />

                            var propertyNameAttribute = property.Attribute("Name");
                            var propertyRelationshipAttribute = property.Attribute("Relationship");
                            var propertyToRoleAttribute = property.Attribute("ToRole");

                            if (propertyNameAttribute != null && propertyRelationshipAttribute != null && propertyToRoleAttribute != null)
                            {
                                var propertyName = NormalizePropertyName(propertyNameAttribute.Value);
                                if (propertyName != null)
                                {
                                    // Find associationset:

                                    //<AssociationSet Name="SP_Web_Alerts_SP_Alert_AlertsPartnerSet" Association="SP.SP_Web_Alerts_SP_Alert_AlertsPartner">
                                    //    <End Role="AlertsPartner" EntitySet="Webs" />
                                    //    <End Role="Alerts" EntitySet="Alerts" />
                                    //</AssociationSet>

                                    var associatedSet = edmxDocument.Descendants(schemaNamespace + "AssociationSet").FirstOrDefault(e => e.Attribute("Association")?.Value == propertyRelationshipAttribute.Value);
                                    if (associatedSet != null)
                                    {
                                        // Find related association

                                        //<Association Name="SP_Web_Alerts_SP_Alert_AlertsPartner">
                                        //    <End Type="SP.Alert" Role="Alerts" Multiplicity="*" />
                                        //    <End Type="SP.Web" Role="AlertsPartner" Multiplicity="0..1" />
                                        //</Association>

                                        //Remove namespace from association : SP.SP_Web_Alerts_SP_Alert_AlertsPartner can be found as SP_Web_Alerts_SP_Alert_AlertsPartner
                                        var associationNameToFind = associatedSet.Attribute("Association").Value.Substring(associatedSet.Attribute("Association").Value.LastIndexOf(".") + 1);

                                        var association = edmxDocument.Descendants(schemaNamespace + "Association").FirstOrDefault(e => e.Attribute("Name")?.Value == associationNameToFind);

                                        if (association != null)
                                        {
                                            // Find the needed "End"
                                            var associatedEnd = association.Elements(schemaNamespace + "End").FirstOrDefault(e => e.Attribute("Role")?.Value == propertyToRoleAttribute.Value);
                                            if (associatedEnd != null)
                                            {
                                                UnifiedModelProperty modelProp = new UnifiedModelProperty()
                                                {
                                                    Name = propertyName,
                                                    Type = associatedEnd.Attribute("Type").Value,
                                                    // Multiplicity = * (collection) or 0..1 
                                                    NavigationPropertyIsCollection = associatedEnd.Attribute("Multiplicity").Value.Equals("*"),
                                                    NavigationProperty = true
                                                };
                                                entity.Properties.Add(modelProp);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        model.Entities.Add(entity);
                    }
                }

                // Combine with mapping file data
                if (mapping != null && mapping.ExcludedTypes != null)
                {
                    foreach (var entityFromMapping in mapping.ExcludedTypes)
                    {
                        var typeToExclude = model.Entities.FirstOrDefault(p => p.SPRestType == entityFromMapping.Type);
                        if (typeToExclude != null)
                        {
                            if (entityFromMapping.Properties.Any())
                            {
                                foreach (var property in entityFromMapping.Properties)
                                {
                                    var propertyToExclude = typeToExclude.Properties.FirstOrDefault(p => p.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));
                                    if (propertyToExclude != null)
                                    {
                                        propertyToExclude.Skip = true;
                                    }
                                }
                            }
                            else
                            {
                                // Exclude the complete type
                                typeToExclude.Skip = true;
                            }
                        }
                    }
                }

                // Hookup with the possibly available types in the PnP Core SDK
                foreach (var analyzedModelType in analyzedModelTypes)
                {
                    if (analyzedModelType.Value.SPRestTypes.Any())
                    {
                        foreach (var spRestType in analyzedModelType.Value.SPRestTypes)
                        {
                            var typeToUpdate = model.Entities.FirstOrDefault(p => p.SPRestType == spRestType); 
                            if (typeToUpdate != null)
                            {
                                typeToUpdate.AnalyzedModelType = analyzedModelType.Value;
                            }
                        }
                    }
                }
            }

            return model;
        }

        /*
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
        */

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
