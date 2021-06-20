using System;
using System.Collections.Concurrent;
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
using PnP.Core.Transformation.SharePoint.MappingFiles.Publishing;
using PnP.Core.Transformation.SharePoint.Extensions;
using System.Linq;
using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.SharePoint.Publishing;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IPageLayoutMappingProvider"/>
    /// </summary>
    public class SharePointPageLayoutMappingProvider : IPageLayoutMappingProvider
    {
        private ILogger<SharePointPageLayoutMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly IServiceProvider serviceProvider;

        #region Local in memory cache for the page layout mapping file

        private ConcurrentDictionary<string, PublishingPageTransformation> mappingsCache = new ConcurrentDictionary<string, PublishingPageTransformation>();

        #endregion

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="options">Configuration options</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointPageLayoutMappingProvider(ILogger<SharePointPageLayoutMappingProvider> logger,
            IOptions<SharePointTransformationOptions> options,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Maps a classic Page Layout into a modern Page Layout
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use, if any</param>
        /// <returns>The output of the mapping activity</returns>
        public async Task<PageLayoutMappingProviderOutput> MapPageLayoutAsync(PageLayoutMappingProviderInput input, CancellationToken token = default)
        {
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapPageLayoutAsync");

            // Check that we have input data
            if (input == null) throw new ArgumentNullException(nameof(input));

            // Validate source item
            var sourceItem = input.Context.SourceItem as SharePointSourceItem;
            if (sourceItem == null) throw new ApplicationException(SharePointTransformationResources.Error_MissiningSharePointInputItem);

            // Get a reference to the file and item that we need to transform
            var sourceContext = sourceItem.SourceContext;
            var pageFile = sourceItem.SourceContext.Web.GetFileByServerRelativeUrl(sourceItem.Id.ServerRelativeUrl);
            var pageItem = pageFile.ListItemAllFields;
            sourceContext.Load(pageFile);
            sourceContext.Load(pageItem);
            await sourceContext.ExecuteQueryAsync().ConfigureAwait(false);

            // Load the mapping configuration
            PublishingPageTransformation mapping = LoadMappingFile(this.options.Value.PageLayoutMappingFile);

            // Retrieve the mapping page layout from the mapping file, if any
            var publishingPageTransformationModel = mapping.PageLayouts.FirstOrDefault(p => p.Name.Equals(input.PageLayout, StringComparison.InvariantCultureIgnoreCase));

            // No dedicated layout mapping found, let's see if there's an other page layout mapping that also applies for this page layout
            if (publishingPageTransformationModel == null)
            {
                // Fill a list of additional page layout mappings that can be used
                Dictionary<string, string> additionalMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var pageLayout in mapping.PageLayouts.Where(p => !String.IsNullOrEmpty(p.AlsoAppliesTo)))
                {
                    var possiblePageLayouts = pageLayout.AlsoAppliesTo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (possiblePageLayouts.Length > 0)
                    {
                        foreach (var possiblePageLayout in possiblePageLayouts)
                        {
                            // Only add the first possible page layout mapping, if a given page layout is defined multiple times then the first reference wins
                            if (!additionalMappings.ContainsKey(possiblePageLayout))
                            {
                                additionalMappings.Add(possiblePageLayout, pageLayout.Name);
                            }
                        }
                    }
                }

                if (additionalMappings.Count > 0)
                {
                    if (additionalMappings.ContainsKey(input.PageLayout))
                    {
                        publishingPageTransformationModel = mapping.PageLayouts.FirstOrDefault(p => p.Name.Equals(additionalMappings[input.PageLayout], StringComparison.InvariantCultureIgnoreCase));
                    }
                }
            }

            // No layout provided via either the default mapping or custom mapping file provided
            if (publishingPageTransformationModel == null)
            {
                publishingPageTransformationModel = GeneratePageLayout(pageItem);

                logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Info_PageLayoutMappingGeneration, 
                    pageFile.ServerRelativeUrl, input.PageLayout));
            }

            // Still no layout...can't continue...
            if (publishingPageTransformationModel == null)
            {
                var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Error_NoPageLayoutTransformationModel, input.PageLayout, pageFile.ServerRelativeUrl);
                logger.LogInformation(errorMessage);
                throw new Exception(errorMessage);
            }

            logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                SharePointTransformationResources.Info_PageLayoutMappingBeingUsed, 
                pageFile.ServerRelativeUrl, input.PageLayout, publishingPageTransformationModel.Name));

            // TODO: Think about a solution to manage the output, unless we make the Page Layout
            // mapper specific for SP transformations ...
            return new PageLayoutMappingProviderOutput { PageLayout = publishingPageTransformationModel };
        }

        private PageLayout GeneratePageLayout(Microsoft.SharePoint.Client.ListItem pageItem)
        {
            var pageLayoutFileUrl = pageItem.GetPageLayoutFileUrl();

            // TODO: Consider using caching

            // Let's try to generate a 'basic' model and use that...not optimal, but better than bailing out.
            PageLayoutAnalyser pageLayoutAnalyzer = new PageLayoutAnalyser();
            var newPageLayoutMapping = pageLayoutAnalyzer.AnalysePageLayoutFromPublishingPage(pageItem);

            // TODO: Consider caching the result

            // Return to requestor
            return newPageLayoutMapping;

        }

        private PublishingPageTransformation LoadMappingFile(string mappingFilePath = null)
        {
            // Check if we already have the mapping file in the in-memory cache
            if (mappingsCache.ContainsKey(mappingFilePath))
            {
                return mappingsCache[mappingFilePath];
            }

            // Create the xml mapping serializer
            XmlSerializer xmlMapping = new XmlSerializer(typeof(PublishingPageTransformation));

            // Prepare the result variable
            PublishingPageTransformation result = null;

            // If we don't have the mapping file as an input
            if (!string.IsNullOrEmpty(mappingFilePath) ||
                !System.IO.File.Exists(mappingFilePath))
            {
                // We use the default one, without validation
                using (var mappingStream = this.GetType().Assembly.GetManifestResourceStream("PnP.Core.Transformation.SharePoint.MappingFiles.pagelayoutmapping.xml"))
                {
                    using (var reader = XmlReader.Create(mappingStream))
                    {
                        result = (PublishingPageTransformation)xmlMapping.Deserialize(reader);
                    }
                }
            }
            else
            {
                using (Stream schema = this.GetType().Assembly.GetManifestResourceStream("PnP.Core.Transformation.SharePoint.MappingFiles.pagelayoutmapping.xsd"))
                {
                    using (var mappingStream = new FileStream(mappingFilePath, FileMode.Open))
                    {
                        // Ensure the provided file complies with the current schema
                        ValidateSchema(schema, mappingStream);

                        using (var reader = XmlReader.Create(mappingStream))
                        {
                            result = (PublishingPageTransformation)xmlMapping.Deserialize(reader);
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
                schemas.Add(SharePointConstants.PageLayoutMappingSchema, schemaReader);

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

    }
}
