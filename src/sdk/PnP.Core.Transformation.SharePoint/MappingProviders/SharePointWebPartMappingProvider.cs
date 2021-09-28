using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using PnP.Core.Transformation.SharePoint.MappingFiles;
using System.Xml.XPath;
using Microsoft.Extensions.Caching.Memory;
using PnP.Core.Transformation.Model;
using PnP.Core.Transformation.SharePoint.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Functions;
using Newtonsoft.Json.Linq;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IWebPartMappingProvider"/>
    /// </summary>
    public class SharePointWebPartMappingProvider : IWebPartMappingProvider
    {
        private ILogger<SharePointWebPartMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> spOptions;
        private readonly IServiceProvider serviceProvider;
        private readonly IMemoryCache memoryCache;
        private readonly FunctionProcessor functionProcessor;

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="spOptions">Configuration options</param>
        /// <param name="functionProcessor">The SharePoint Function processor</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointWebPartMappingProvider(ILogger<SharePointWebPartMappingProvider> logger, 
            IOptions<SharePointTransformationOptions> spOptions,
            FunctionProcessor functionProcessor,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.spOptions = spOptions ?? throw new ArgumentNullException(nameof(spOptions));
            this.functionProcessor = functionProcessor ?? throw new ArgumentNullException(nameof(functionProcessor));
            this.serviceProvider = serviceProvider;
            this.memoryCache = this.serviceProvider.GetService<IMemoryCache>();
        }

        /// <summary>
        /// Maps a classic Web Part into a modern Web Part
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use, if any</param>
        /// <returns>The output of the mapping activity</returns>
        public async Task<WebPartMappingProviderOutput> MapWebPartAsync(WebPartMappingProviderInput input, CancellationToken token = default)
        {
            // Check that we have input data
            if (input == null) throw new ArgumentNullException(nameof(input));

            // Try to convert the mapping provider input into a typed version
            var specializedInput = input as SharePointWebPartMappingProviderInput;
            if (specializedInput == null) throw new ArgumentException(SharePointTransformationResources.Error_InvalidWebPartMappingProviderInput);

            // Configure the SharePoint Function service instance
            this.functionProcessor.Init(specializedInput.Context, specializedInput.SourceContext);

            // Define the variable holding the input web part
            var webPart = input.WebPart;

            // Prepare the output object
            var result = new WebPartMappingProviderOutput();

            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapWebPartAsync");
            logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                SharePointTransformationResources.Info_ContentWebPartBeingTransformed,
                webPart.Title, webPart.TypeShort()));

            // Title bar will never be migrated
            if (input.WebPart.Type == WebParts.TitleBar)
            {
                logger.LogInformation(SharePointTransformationResources.Info_NotTransformingTitleBar);
                return result;
            }

            // Load the mapping configuration
            WebPartMapping mappingFile = LoadMappingFile(this.spOptions.Value.WebPartMappingFile);

            var sourceItem = input.Context.SourceItem as SharePointSourceItem;

            if (sourceItem == null)
            {
                throw new ApplicationException(SharePointTransformationResources.Error_MissiningSharePointInputItem);
            }

            // Find the default mapping, will be used for webparts for which the model does not contain a mapping
            var defaultMapping = mappingFile.BaseWebPart.Mappings.Mapping.FirstOrDefault(p => p.Default == true);
            if (defaultMapping == null)
            {
                logger.LogError(SharePointTransformationResources.Error_NoDefaultMappingFound);
                throw new Exception(SharePointTransformationResources.Error_NoDefaultMappingFound);
            }

            // Assign the default mapping, if we've a more specific mapping than that will overwrite this mapping
            Mapping mapping = defaultMapping;

            // Does the web part have a mapping defined?
            var webPartData = mappingFile.WebParts.FirstOrDefault(p => p.Type.GetTypeShort() == webPart.Type.GetTypeShort());

            // Check for cross site transfer support
            if (webPartData != null && input.IsCrossSiteTransformation)
            {
                if (!webPartData.CrossSiteTransformationSupported)
                {
                    logger.LogWarning(SharePointTransformationResources.Warning_CrossSiteNotSupported);
                    return result;
                }
            }

            var globalTokens = PrepareGlobalTokens();

            if (webPartData != null && webPartData.Mappings != null)
            {
                // Add site level (e.g. site) tokens to the web part properties and model so they can be used in the same manner as a web part property
                UpdateWebPartDataProperties(webPart, webPartData, mappingFile, globalTokens);

                string selectorResult = null;
                try
                {
                    // The mapping can have a selector function defined, if so it will be executed.
                    // If a selector was executed the selectorResult will contain the name of the mapping to use
                    logger.LogDebug(SharePointTransformationResources.Debug_ProcessingSelectorFunctions);
                    selectorResult = this.functionProcessor.Process(ref webPartData, webPart);
                }
                catch (Exception ex)
                {
                    // NotAvailableAtTargetException is used to "skip" a web part since it's not valid for the target site collection (only applies to cross site collection transfers)
                    if (ex.InnerException is NotAvailableAtTargetException)
                    {
                        logger.LogError(SharePointTransformationResources.Error_NotValidForTargetSiteCollection);
                    }

                    if (ex.InnerException is MediaWebpartConfigurationException)
                    {
                        logger.LogError(SharePointTransformationResources.Error_MediaWebpartConfiguration);
                    }

                    logger.LogError($"{SharePointTransformationResources.Error_AnErrorOccurredFunctions} - {ex.Message}");
                    throw;
                }

                Mapping webPartMapping = null;
                // Get the needed mapping:
                // - use the mapping returned by the selector
                // - if no selector then take the default mapping
                // - if no mapping found we'll fall back to the default web part mapping
                if (!string.IsNullOrEmpty(selectorResult))
                {
                    webPartMapping = webPartData.Mappings.Mapping.Where(p => p.Name.Equals(selectorResult, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                }
                else
                {
                    // If there's only one mapping let's take that one, even if not specified as default
                    if (webPartData.Mappings.Mapping.Length == 1)
                    {
                        webPartMapping = webPartData.Mappings.Mapping[0];
                    }
                    else
                    {
                        webPartMapping = webPartData.Mappings.Mapping.FirstOrDefault(p => p.Default == true);
                    }
                }

                if (webPartMapping != null)
                {
                    mapping = webPartMapping;
                }
                else
                {
                    logger.LogWarning($"{SharePointTransformationResources.Warning_ContentWebPartMappingNotFound}");
                }

                // Process mapping specific functions (if any)
                if (!String.IsNullOrEmpty(mapping.Functions))
                {
                    try
                    {
                        logger.LogInformation(SharePointTransformationResources.Info_ProcessingMappingFunctions);
                        functionProcessor.ProcessMappingFunctions(ref webPartData, webPart, mapping);
                    }
                    catch (Exception ex)
                    {
                        // NotAvailableAtTargetException is used to "skip" a web part since it's not valid for the target site collection (only applies to cross site collection transfers)
                        if (ex.InnerException is NotAvailableAtTargetException)
                        {
                            logger.LogError(SharePointTransformationResources.Error_NotValidForTargetSiteCollection);
                        }

                        logger.LogError($"{SharePointTransformationResources.Error_AnErrorOccurredFunctions} - {ex.Message}");
                        throw;
                    }
                }
            }

            return new SharePointWebPartMappingProviderOutput(mapping);
        }

        /// <summary>
        /// Prepares placeholders for global tokens
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> PrepareGlobalTokens()
        {
            Dictionary<string, string> globalTokens = new Dictionary<string, string>(5);

            // Add the fixed properties tokens
            globalTokens.Add("Host", "{Host}");
            globalTokens.Add("Web", "{Web}");
            globalTokens.Add("SiteCollection", "{SiteCollection}");
            globalTokens.Add("WebId", "{WebId}");
            globalTokens.Add("SiteId", "{SiteId}");

            return globalTokens;
        }

        internal WebPartMapping LoadMappingFile(string mappingFilePath = null)
        {
            // Prepare the result variable
            WebPartMapping result = null;

            if (string.IsNullOrEmpty(mappingFilePath))
            {
                mappingFilePath = "stream.webpartmapping.xml";
            }

            // Check if we already have the mapping file in the in-memory cache
            if (this.memoryCache.TryGetValue(mappingFilePath, out result))
            {
                return result;
            }

            // Create the xml mapping serializer
            XmlSerializer xmlMapping = new XmlSerializer(typeof(WebPartMapping));

            // If we don't have the mapping file as an input
            if (string.IsNullOrEmpty(mappingFilePath) ||
                !System.IO.File.Exists(mappingFilePath))
            {
                // We use the default one, without validation
                using (var mappingStream = this.GetType().Assembly.GetManifestResourceStream("PnP.Core.Transformation.SharePoint.MappingFiles.webpartmapping.xml"))
                {
                    using (var reader = XmlReader.Create(mappingStream))
                    {
                        result = (WebPartMapping)xmlMapping.Deserialize(reader);
                    }
                }
            }
            else
            {
                using (Stream schema = this.GetType().Assembly.GetManifestResourceStream("PnP.Core.Transformation.SharePoint.MappingFiles.webpartmapping.xsd"))
                {
                    using (var mappingStream = new FileStream(mappingFilePath, FileMode.Open))
                    {
                        // Ensure the provided file complies with the current schema
                        ValidateSchema(schema, mappingStream);

                        using (var reader = XmlReader.Create(mappingStream))
                        {
                            result = (WebPartMapping)xmlMapping.Deserialize(reader);
                        }
                    }
                }
            }

            // Cache the mapping file into the in-memory cache
            this.memoryCache.Set(mappingFilePath, result);
 
            return result;
        }

        private void ValidateSchema(Stream schema, Stream stream)
        {
            // Load the template into an XDocument
            XDocument xml = XDocument.Load(stream);

            using (var schemaReader = XmlReader.Create(schema))
            {
                // Prepare the XML Schema Set
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add(SharePointConstants.PageTransformationSchema, schemaReader);

                // Set stream back to start
                stream.Seek(0, SeekOrigin.Begin);

                xml.Validate(schemas, (o, e) =>
                {
                    var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Error_WebPartMappingSchemaValidation, e.Message);
                    this.logger.LogError(e.Exception, errorMessage);
                    throw new ApplicationException(errorMessage);
                });
            }
        }

        private static Dictionary<string, string> ExtractProperties(WebPartMappingProviderInput input, WebPartMapping mapping)
        {
            // Storage for properties to keep
            Dictionary<string, string> propertiesToKeep = new Dictionary<string, string>();

            // List of properties to retrieve
            List<Property> propertiesToRetrieve = mapping.BaseWebPart.Properties.ToList<Property>();

            // For older versions of SharePoint the type in the mapping would not match. Use the TypeShort Comparison. 
            var webPartProperties = mapping.WebParts.FirstOrDefault(p => p.Type.GetTypeShort().Equals(input.SourceComponentType.GetTypeShort(), StringComparison.InvariantCultureIgnoreCase));
            if (webPartProperties != null && webPartProperties.Properties != null)
            {
                foreach (var p in webPartProperties.Properties.ToList<Property>())
                {
                    if (!propertiesToRetrieve.Contains(p))
                    {
                        propertiesToRetrieve.Add(p);
                    }
                }
            }

            // If we don't have the raw content
            if (string.IsNullOrEmpty(input.SourceComponentRawContent))
            {
                if (input.SourceComponentType.GetTypeShort() == WebParts.Client.GetTypeShort())
                {
                    // Special case since we don't know upfront which properties are relevant here...so let's take them all
                    foreach (var p in input.SourceProperties)
                    {
                        if (!propertiesToKeep.ContainsKey(p.Key))
                        {
                            propertiesToKeep.Add(p.Key, p.Value != null ? p.Value.ToString() : string.Empty);
                        }
                    }
                }
                else
                {
                    // Special case where we did not have export rights for the web part XML, assume this is a V3 web part
                    foreach (var p in propertiesToRetrieve)
                    {
                        if (!string.IsNullOrEmpty(p.Name) &&
                            input.SourceProperties.ContainsKey(p.Name) &&
                            !propertiesToKeep.ContainsKey(p.Name))
                        {
                            propertiesToKeep.Add(p.Name, input.SourceProperties[p.Name] != null ?
                                input.SourceProperties[p.Name].ToString() : string.Empty);
                        }
                    }
                }
            }
            else
            {
                var xml = XElement.Parse(input.SourceComponentRawContent);
                var xmlns = xml.XPathSelectElement("*").GetDefaultNamespace();
                if (xmlns.NamespaceName.Equals("http://schemas.microsoft.com/WebPart/v3",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    if (input.SourceComponentType.GetTypeShort() == WebParts.Client.GetTypeShort())
                    {
                        // Special case since we don't know upfront which properties are relevant here...so let's take them all
                        foreach (var p in input.SourceProperties)
                        {
                            if (!propertiesToKeep.ContainsKey(p.Key))
                            {
                                propertiesToKeep.Add(p.Key, p.Value != null ? p.Value.ToString() : string.Empty);
                            }
                        }
                    }
                    else
                    {
                        // the retrieved properties are sufficient
                        foreach (var p in propertiesToRetrieve)
                        {
                            if (!string.IsNullOrEmpty(p.Name) &&
                                input.SourceProperties.ContainsKey(p.Name) &&
                                !propertiesToKeep.ContainsKey(p.Name))
                            {
                                propertiesToKeep.Add(p.Name, input.SourceProperties[p.Name] != null ?
                                    input.SourceProperties[p.Name].ToString() : string.Empty);
                            }
                        }
                    }
                }
                else if (xmlns.NamespaceName.Equals("http://schemas.microsoft.com/WebPart/v2",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var p in propertiesToRetrieve)
                    {
                        if (!string.IsNullOrEmpty(p.Name))
                        {
                            if (input.SourceProperties.ContainsKey(p.Name))
                            {
                                if (!propertiesToKeep.ContainsKey(p.Name))
                                {
                                    propertiesToKeep.Add(p.Name, input.SourceProperties[p.Name] != null ?
                                        input.SourceProperties[p.Name].ToString() : "");
                                }
                            }
                            else
                            {
                                // check XMl for property
                                var v2Element = xml.Descendants(xmlns + p.Name).FirstOrDefault();
                                if (v2Element != null)
                                {
                                    if (!propertiesToKeep.ContainsKey(p.Name))
                                    {
                                        propertiesToKeep.Add(p.Name, v2Element.Value);
                                    }
                                }

                                // Some properties do have their own namespace defined
                                if (input.SourceComponentType.GetTypeShort() == WebParts.SimpleForm.GetTypeShort() &&
                                    p.Name.Equals("Content", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Load using the http://schemas.microsoft.com/WebPart/v2/SimpleForm namespace
                                    XNamespace xmlcontentns = "http://schemas.microsoft.com/WebPart/v2/SimpleForm";
                                    v2Element = xml.Descendants(xmlcontentns + p.Name).FirstOrDefault();
                                    if (v2Element != null)
                                    {
                                        if (!propertiesToKeep.ContainsKey(p.Name))
                                        {
                                            propertiesToKeep.Add(p.Name, v2Element.Value);
                                        }
                                    }
                                }
                                else if (input.SourceComponentType.GetTypeShort() == WebParts.ContentEditor.GetTypeShort())
                                {
                                    if (p.Name.Equals("ContentLink", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("Content", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("PartStorage", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        XNamespace xmlcontentns = "http://schemas.microsoft.com/WebPart/v2/ContentEditor";
                                        v2Element = xml.Descendants(xmlcontentns + p.Name).FirstOrDefault();
                                        if (v2Element != null)
                                        {
                                            if (!propertiesToKeep.ContainsKey(p.Name))
                                            {
                                                propertiesToKeep.Add(p.Name, v2Element.Value);
                                            }
                                        }
                                    }
                                }
                                else if (input.SourceComponentType.GetTypeShort() == WebParts.Xml.GetTypeShort())
                                {
                                    if (p.Name.Equals("XMLLink", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("XML", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("XSLLink", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("XSL", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("PartStorage", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        XNamespace xmlcontentns = "http://schemas.microsoft.com/WebPart/v2/Xml";
                                        v2Element = xml.Descendants(xmlcontentns + p.Name).FirstOrDefault();
                                        if (v2Element != null)
                                        {
                                            if (!propertiesToKeep.ContainsKey(p.Name))
                                            {
                                                propertiesToKeep.Add(p.Name, v2Element.Value);
                                            }
                                        }
                                    }
                                }
                                else if (input.SourceComponentType.GetTypeShort() == WebParts.SiteDocuments.GetTypeShort())
                                {
                                    if (p.Name.Equals("UserControlledNavigation", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("ShowMemberships", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("UserTabs", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        XNamespace xmlcontentns = "urn:schemas-microsoft-com:sharepoint:portal:sitedocumentswebpart";
                                        v2Element = xml.Descendants(xmlcontentns + p.Name).FirstOrDefault();
                                        if (v2Element != null)
                                        {
                                            if (!propertiesToKeep.ContainsKey(p.Name))
                                            {
                                                propertiesToKeep.Add(p.Name, v2Element.Value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return propertiesToKeep;
        }

        private void UpdateWebPartDataProperties(WebPartEntity webPart, WebPart webPartData, WebPartMapping mapping, Dictionary<string, string> globalProperties)
        {
            List<Property> tempList = new List<Property>();
            if (webPartData.Properties != null)
            {
                tempList.AddRange(webPartData.Properties);
            }

            // Add properties listed on the Base web part
            var baseProperties = mapping.BaseWebPart.Properties;
            foreach (var baseProperty in baseProperties)
            {
                // Only add the base property once as the webPartData.Properties collection is reused across web parts and pages
                if (!tempList.Any(p => p.Name.Equals(baseProperty.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    // Add parameter to model
                    tempList.Add(new Property()
                    {
                        Functions = baseProperty.Functions,
                        Name = baseProperty.Name,
                        Type = PropertyType.@string
                    });
                }
            }

            // NOTE: We replaced Global Properties, with tokens

            // Add global properties
            foreach (var token in globalProperties)
            {
                // Add property to web part
                if (!webPart.Properties.ContainsKey(token.Key))
                {
                    webPart.Properties.Add(token.Key, token.Value);
                }

                // Only add the global property once as the webPartData.Properties collection is reused across web parts and pages
                var propAlreadyAdded = tempList.Where(p => p.Name.Equals(token.Key, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (propAlreadyAdded == null)
                {
                    // Add parameter to model
                    tempList.Add(new Property()
                    {
                        Functions = "",
                        Name = token.Key,
                        Type = PropertyType.@string
                    });
                }
            }

            webPartData.Properties = tempList.ToArray();
        }
    }
}
