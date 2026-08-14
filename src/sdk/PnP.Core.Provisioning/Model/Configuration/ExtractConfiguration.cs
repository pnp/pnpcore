using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.ObjectHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration
{
    public partial class ExtractConfiguration
    {

        [JsonIgnore]
        internal ProvisioningTemplate BaseTemplate { get; set; }

        [JsonIgnore]
        public FileConnectorBase FileConnector { get; set; }

        [JsonIgnore]
        public ProvisioningProgressDelegate ProgressDelegate { get; set; }

        [JsonIgnore]
        public ProvisioningMessagesDelegate MessagesDelegate { get; set; }

        [JsonPropertyName("persistAssetFiles")]
        public bool PersistAssetFiles { get; set; }

        [JsonPropertyName("handlers")]
        [JsonConverter(typeof(ListEnumConverter<ConfigurationHandler>))]
        public List<ConfigurationHandler> Handlers { get; set; } = new List<ConfigurationHandler>();

        [JsonPropertyName("lists")]
        public Lists.ExtractListsConfiguration Lists { get; set; } = new Lists.ExtractListsConfiguration();

        [JsonPropertyName("pages")]
        public Pages.ExtractPagesConfiguration Pages { get; set; } = new Pages.ExtractPagesConfiguration();

        [JsonPropertyName("siteSecurity")]
        public SiteSecurity.ExtractConfiguration SiteSecurity { get; set; } = new SiteSecurity.ExtractConfiguration();

        [JsonPropertyName("taxonomy")]
        public Taxonomy.ExtractTaxonomyConfiguration Taxonomy { get; set; } = new Taxonomy.ExtractTaxonomyConfiguration();

        [JsonPropertyName("navigation")]
        public Navigation.ExtractNavigationConfiguration Navigation { get; set; } = new Navigation.ExtractNavigationConfiguration();

        [JsonPropertyName("siteFooter")]
        public SiteFooter.ExtractSiteFooterConfiguration SiteFooter { get; set; } = new SiteFooter.ExtractSiteFooterConfiguration();

        [JsonPropertyName("contentTypes")]
        public ContentTypes.ExtractContentTypeConfiguration ContentTypes { get; set; } = new ContentTypes.ExtractContentTypeConfiguration();

        [JsonPropertyName("searchSettings")]
        public SearchSettings.ExtractSearchConfiguration SearchSettings { get; set; } = new SearchSettings.ExtractSearchConfiguration();

        [JsonPropertyName("extensibility")]
        public Extensibility.ExtractExtensibilityConfiguration Extensibility { get; set; } = new Extensibility.ExtractExtensibilityConfiguration();

        /// <summary>
        /// Defines Tenant Extraction Settings
        /// </summary>
        [JsonPropertyName("tenant")]
        public Tenant.ExtractTenantConfiguration Tenant { get; set; } = new Tenant.ExtractTenantConfiguration();

        [JsonPropertyName("propertyBag")]
        public PropertyBag.ExtractPropertyBagConfiguration PropertyBag { get; set; } = new PropertyBag.ExtractPropertyBagConfiguration();

        [JsonPropertyName("multiLanguage")]
        public MultiLanguage.ExtractMultiLanguageConfiguration MultiLanguage { get; set; } = new MultiLanguage.ExtractMultiLanguageConfiguration();

        [JsonPropertyName("publishing")]
        public Publishing.ExtractPublishingConfiguration Publishing { get; set; } = new Publishing.ExtractPublishingConfiguration();

        [JsonPropertyName("syntexModels")]
        public SyntexModels.ExtractSyntexModelsConfiguration SyntexModels { get; set; } = new SyntexModels.ExtractSyntexModelsConfiguration();

        public static ExtractConfiguration FromCreationInformation(ProvisioningTemplateCreationInformation information)
        {
            var config = new ExtractConfiguration
            {
                BaseTemplate = information.BaseTemplate
            };
            config.ContentTypes.Groups = information.ContentTypeGroupsToInclude;
            config.Extensibility.Handlers = information.ExtensibilityHandlers;
            config.FileConnector = information.FileConnector;
            if (information.HandlersToProcess == Model.Handlers.All)
            {
                config.Handlers = new List<ConfigurationHandler>();
            }
            else
            {
                foreach (var handler in (Handlers[])Enum.GetValues(typeof(Handlers)))
                {
                    if (information.HandlersToProcess.HasFlag(handler))
                    {
                        if (Enum.TryParse<ConfigurationHandler>(handler.ToString(), out ConfigurationHandler configurationHandler))
                        {
                            config.Handlers.Add(configurationHandler);
                        }
                    }
                }
            }

            config.Pages.IncludeAllClientSidePages = information.IncludeAllClientSidePages;
            config.Taxonomy.IncludeAllTermGroups = information.IncludeAllTermGroups;
            config.Taxonomy.IncludeSiteCollectionTermGroup = information.IncludeSiteCollectionTermGroup;
            config.SiteSecurity.IncludeSiteGroups = information.IncludeSiteGroups;
            config.Taxonomy.IncludeSecurity = information.IncludeTermGroupsSecurity;
            if (information.ListsToExtract != null && information.ListsToExtract.Any())
            {
                foreach (var list in information.ListsToExtract)
                {
                    config.Lists.Lists.Add(new Configuration.Lists.Lists.ExtractListsListsConfiguration()
                    {
                        Title = list
                    });
                }
            }
            if (information.MessagesDelegate != null)
            {
                config.MessagesDelegate = (message, type) =>
                {
                    information.MessagesDelegate(message, type);
                };
            }
            config.PersistAssetFiles = information.PersistBrandingFiles || information.PersistPublishingFiles;
            config.MultiLanguage.PersistResources = information.PersistMultiLanguageResources;
            if (information.ProgressDelegate != null)
            {
                config.ProgressDelegate = (message, step, total) =>
                {
                    information.ProgressDelegate(message, step, total);
                };
            }
            config.PropertyBag.ValuesToPreserve = information.PropertyBagPropertiesToPreserve;
            config.MultiLanguage.ResourceFilePrefix = information.ResourceFilePrefix;
            config.Publishing.Persist = information.PersistPublishingFiles;
            config.Publishing.IncludeNativePublishingFiles = information.IncludeNativePublishingFiles;
            config.SearchSettings.Include = information.IncludeSearchConfiguration;
            return config;
        }

        /// <summary>
        /// Converts the Configuration to a ProvisioningTemplateCreationInformation object for backwards compatibility
        /// </summary>
        /// <param name="baseTemplate">
        /// The out of the box base template to diff the extracted template against, or null to extract everything.
        /// </param>
        /// <returns>The equivalent <see cref="ProvisioningTemplateCreationInformation"/></returns>
        public ProvisioningTemplateCreationInformation ToCreationInformation(ProvisioningTemplate baseTemplate = null)
        {
            if (creationInformationCache.TryGetValue(baseTemplate ?? NoBaseTemplate, out ProvisioningTemplateCreationInformation cached))
            {
                return cached;
            }

            var ci = new ProvisioningTemplateCreationInformation()
            {
                ExtractConfiguration = this,

                PersistBrandingFiles = PersistAssetFiles,
                PersistPublishingFiles = PersistAssetFiles,
                BaseTemplate = baseTemplate,
                FileConnector = this.FileConnector,
                IncludeAllClientSidePages = this.Pages.IncludeAllClientSidePages,
                IncludeHiddenLists = this.Lists.IncludeHiddenLists,
                IncludeSiteGroups = this.SiteSecurity.IncludeSiteGroups,
                ContentTypeGroupsToInclude = this.ContentTypes.Groups,
                IncludeContentTypesFromSyndication = !this.ContentTypes.ExcludeFromSyndication,
                IncludeTermGroupsSecurity = this.Taxonomy.IncludeSecurity,
                IncludeSiteCollectionTermGroup = this.Taxonomy.IncludeSiteCollectionTermGroup,
                IncludeSearchConfiguration = this.SearchSettings.Include,
                IncludeAllTermGroups = this.Taxonomy.IncludeAllTermGroups,
                ExtensibilityHandlers = this.Extensibility.Handlers
            };
            ci.IncludeAllTermGroups = this.Taxonomy.IncludeAllTermGroups;
            ci.IncludeNativePublishingFiles = this.Publishing.IncludeNativePublishingFiles;
            ci.ListsToExtract = this.Lists != null && this.Lists.Lists.Any() ? this.Lists.Lists.Select(l => l.Title).ToList() : null;
            ci.PersistMultiLanguageResources = this.MultiLanguage.PersistResources;
            ci.PersistPublishingFiles = this.Publishing.Persist;
            ci.ResourceFilePrefix = this.MultiLanguage.ResourceFilePrefix;

            if (Handlers.Any())
            {
                ci.HandlersToProcess = Model.Handlers.None;
                foreach (var handler in Handlers)
                {
                    Model.Handlers handlerEnumValue = Model.Handlers.None;
                    switch (handler)
                    {
                        case ConfigurationHandler.Pages:
                            handlerEnumValue = Model.Handlers.PageContents;
                            break;
                        case ConfigurationHandler.Taxonomy:
                            handlerEnumValue = Model.Handlers.TermGroups;
                            break;
                        default:
                            handlerEnumValue = (Model.Handlers)Enum.Parse(typeof(Model.Handlers), handler.ToString());
                            break;
                    }
                    ci.HandlersToProcess |= handlerEnumValue;
                }
            }
            else
            {
                ci.HandlersToProcess = Model.Handlers.All;
            }

            if (this.ProgressDelegate != null)
            {
                ci.ProgressDelegate = (message, step, total) =>
                {
                    ProgressDelegate(message, step, total);
                };
            }
            if (this.MessagesDelegate != null)
            {
                ci.MessagesDelegate = (message, type) =>
                {
                    MessagesDelegate(message, type);
                };
            }

            creationInformationCache[baseTemplate ?? NoBaseTemplate] = ci;

            return ci;
        }

        /// <summary>
        /// Stands in for "no base template" so the cache can be keyed without a null.
        /// </summary>
        private static readonly ProvisioningTemplate NoBaseTemplate = new ProvisioningTemplate();

        private readonly Dictionary<ProvisioningTemplate, ProvisioningTemplateCreationInformation> creationInformationCache
            = new Dictionary<ProvisioningTemplate, ProvisioningTemplateCreationInformation>(TemplateReferenceComparer.Instance);

        /// <summary>
        /// Compares templates by reference.
        /// </summary>
        private sealed class TemplateReferenceComparer : IEqualityComparer<ProvisioningTemplate>
        {
            internal static readonly TemplateReferenceComparer Instance = new TemplateReferenceComparer();

            public bool Equals(ProvisioningTemplate x, ProvisioningTemplate y) => ReferenceEquals(x, y);

            public int GetHashCode(ProvisioningTemplate obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        public static ExtractConfiguration FromString(string input)
        {
            return JsonSerializer.Deserialize<ExtractConfiguration>(input);
        }
    }
}
