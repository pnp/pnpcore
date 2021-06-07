using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

            // Load the mapping configuration
            WebPartMapping mapping = LoadMappingFile(this.options.Value.WebPartMappingFile);

            // TODO: Do the actual web part mapping

            return Task.FromResult(new WebPartMappingProviderOutput());
        }

        private WebPartMapping LoadMappingFile(string mappingFilePath = null)
        {
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
