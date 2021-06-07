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
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Builder.Configuration;
using PnP.Core.Transformation.SharePoint.MappingFiles;
using System.Xml.XPath;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IWebPartMappingProvider"/>
    /// </summary>
    public class SharePointWebPartMappingProvider : IWebPartMappingProvider
    {
        private ILogger<SharePointWebPartMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly IServiceProvider serviceProvider;

        #region Local in memory cache for the web part mapping file

        private ConcurrentDictionary<string, WebPartMapping> mappingsCache = new ConcurrentDictionary<string, WebPartMapping>();

        #endregion

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="options">Configuration options</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointWebPartMappingProvider(ILogger<SharePointWebPartMappingProvider> logger, 
            IOptions<SharePointTransformationOptions> options, 
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Maps a classic Web Part into a modern Web Part
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use, if any</param>
        /// <returns>The output of the mapping activity</returns>
        public Task<WebPartMappingProviderOutput> MapWebPartAsync(WebPartMappingProviderInput input, CancellationToken token = default)
        {
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapWebPartAsync");

            // Check that we have input data
            if (input == null) throw new ArgumentNullException(nameof(input));

            // Load the mapping configuration
            WebPartMapping mapping = LoadMappingFile(this.options.Value.WebPartMappingFile);

            // Prepare the output object
            var result = new WebPartMappingProviderOutput();

            var sourceItem = input.Context.SourceItem as SharePointSourceItem;

            if (sourceItem == null) throw new ApplicationException(SharePointTransformationResources.Error_MissiningSharePointInputItem);

            // Transform the source Web Part into the the target Web Part
            result.WebPart = new PnP.Core.Transformation.Model.WebPartEntity();
            //{
            //    Title = webPartToRetrieve.WebPartDefinition.WebPart.Title,
            //    Type = webPartToRetrieve.WebPartType,
            //    Id = webPartToRetrieve.WebPartDefinition.Id,
            //    ServerControlId = webPartToRetrieve.Id,
            //    Row = webPartToRetrieve.Row,
            //    Column = webPartToRetrieve.Column,
            //    Order = webPartToRetrieve.Order,
            //    ZoneId = "",
            //    ZoneIndex = (uint)webPartToRetrieve.WebPartDefinition.WebPart.ZoneIndex,
            //    IsClosed = webPartToRetrieve.WebPartDefinition.WebPart.IsClosed,
            //    Hidden = webPartToRetrieve.WebPartDefinition.WebPart.Hidden,
            //    Properties = ExtractProperties(input, mapping);
            //};

            return Task.FromResult(result);
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

        private WebPartMapping LoadMappingFile(string mappingFilePath = null)
        {
            // Check if we already have the mapping file in the in-memory cache
            if (mappingsCache.ContainsKey(mappingFilePath))
            {
                return mappingsCache[mappingFilePath];
            }

            // Create the xml mapping serializer
            XmlSerializer xmlMapping = new XmlSerializer(typeof(WebPartMapping));

            // Prepare the result variable
            WebPartMapping result = null;

            // If we don't have the mapping file as an input
            if (!string.IsNullOrEmpty(mappingFilePath) ||
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
            mappingsCache.AddOrUpdate(mappingFilePath, result, (k, v) => result);
 
            return result;
        }

        private void ValidateSchema(Stream schema, FileStream stream)
        {
            // Load the template into an XDocument
            XDocument xml = XDocument.Load(stream);

            using (var schemaReader = new XmlTextReader(schema))
            {
                // Prepare the XML Schema Set
                XmlSchemaSet schemas = new XmlSchemaSet();
                schema.Seek(0, SeekOrigin.Begin);
                schemas.Add(SharePointConstants.PageTransformationSchema, schemaReader);

                // Set stream back to start
                stream.Seek(0, SeekOrigin.Begin);

                xml.Validate(schemas, (o, e) =>
                {
                    var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Error_WebPartMappingSchemaValidation, e.Message);
                    this.logger.LogError(errorMessage, SharePointTransformationResources.Heading_PageTransformationInfomation, e.Exception);
                    throw new ApplicationException(errorMessage);
                });
            }
        }
    }
}
