using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.ObjectHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration
{
    public partial class ApplyConfiguration
    {
        private Dictionary<String, String> _accessTokens;

        [JsonIgnore]
        public FileConnectorBase FileConnector { get; set; }

        [JsonIgnore]
        public ProvisioningProgressDelegate ProgressDelegate { get; set; }

        [JsonIgnore]
        public ProvisioningMessagesDelegate MessagesDelegate { get; set; }

        [JsonIgnore]
        public ProvisioningSiteProvisionedDelegate SiteProvisionedDelegate { get; set; }

        [JsonIgnore]
        public Dictionary<String, String> AccessTokens
        {
            get
            {
                if (this._accessTokens == null)
                {
                    this._accessTokens = new Dictionary<string, string>();
                }
                return (this._accessTokens);
            }
            set
            {
                this._accessTokens = value;
            }
        }



        [JsonPropertyName("handlers")]
        [JsonConverter(typeof(ListEnumConverter<ConfigurationHandler>))]
        public List<ConfigurationHandler> Handlers { get; set; } = new List<ConfigurationHandler>();

        [JsonPropertyName("parameters")]
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Defines Tenant Extraction Settings
        /// </summary>
        [JsonPropertyName("tenant")]
        public Tenant.ApplyTenantConfiguration Tenant { get; set; } = new Tenant.ApplyTenantConfiguration();

        [JsonPropertyName("contentTypes")]
        public ContentTypes.ApplyContentTypeConfiguration ContentTypes { get; set; } = new ContentTypes.ApplyContentTypeConfiguration();

        [JsonPropertyName("propertyBag")]
        public PropertyBag.ApplyPropertyBagConfiguration PropertyBag { get; set; } = new PropertyBag.ApplyPropertyBagConfiguration();

        [JsonPropertyName("fields")]
        public Fields.ApplyFieldsConfiguration Fields { get; set; } = new Fields.ApplyFieldsConfiguration();

        [JsonPropertyName("lists")]
        public Lists.ApplyListsConfiguration Lists { get; set; } = new Lists.ApplyListsConfiguration();

        [JsonPropertyName("navigation")]
        public Navigation.ApplyNavigationConfiguration Navigation { get; set; } = new Navigation.ApplyNavigationConfiguration();

        [JsonPropertyName("extensibility")]
        public Extensibility.ApplyExtensibilityConfiguration Extensibility { get; set; } = new Extensibility.ApplyExtensibilityConfiguration();

        public ProvisioningTemplateApplyingInformation ToApplyingInformation()
        {
            var ai = new ProvisioningTemplateApplyingInformation
            {
                ApplyConfiguration = this
            };

            if (this.AccessTokens != null && this.AccessTokens.Any())
            {
                ai.AccessTokens = this.AccessTokens;
            }

            ai.ProvisionContentTypesToSubWebs = this.ContentTypes.ProvisionContentTypesToSubWebs;
            ai.OverwriteSystemPropertyBagValues = this.PropertyBag.OverwriteSystemValues;
            ai.IgnoreDuplicateDataRowErrors = this.Lists.IgnoreDuplicateDataRowErrors;
            ai.ClearNavigation = this.Navigation.ClearNavigation;
            ai.ProvisionFieldsToSubWebs = this.Fields.ProvisionFieldsToSubWebs;

            if (Handlers.Any())
            {
                ai.HandlersToProcess = Model.Handlers.None;
                foreach (var handler in Handlers)
                {
                    Model.Handlers handlerEnumValue = Model.Handlers.None;
                    switch (handler)
                    {
                        case ConfigurationHandler.Pages:
                            handlerEnumValue = Model.Handlers.Pages
                                | Model.Handlers.PageContents;
                            break;
                        case ConfigurationHandler.Taxonomy:
                            handlerEnumValue = Model.Handlers.TermGroups;
                            break;
                        default:
                            handlerEnumValue = (Model.Handlers)Enum.Parse(typeof(Model.Handlers), handler.ToString());
                            break;
                    }
                    ai.HandlersToProcess |= handlerEnumValue;
                }
            }
            else
            {
                ai.HandlersToProcess = Model.Handlers.All;
            }

            if (this.ProgressDelegate != null)
            {
                ai.ProgressDelegate = (message, step, total) =>
                {
                    ProgressDelegate(message, step, total);
                };
            }
            if (this.MessagesDelegate != null)
            {
                ai.MessagesDelegate = (message, type) =>
                {
                    MessagesDelegate(message, type);
                };
            }
            if (this.SiteProvisionedDelegate != null)
            {
                ai.SiteProvisionedDelegate = (title, siteUrl) =>
                {
                    SiteProvisionedDelegate(title, siteUrl);
                };
            }

            return ai;
        }

        public static ApplyConfiguration FromApplyingInformation(ProvisioningTemplateApplyingInformation information)
        {
            var config = new ApplyConfiguration
            {
                AccessTokens = information.AccessTokens
            };
            config.Navigation.ClearNavigation = information.ClearNavigation;
#pragma warning disable CS0618 // obsolete
            config.Tenant.DelayAfterModernSiteCreation = information.DelayAfterModernSiteCreation;
#pragma warning restore CS0618
            config.Extensibility.Handlers = information.ExtensibilityHandlers;
            if (information.HandlersToProcess == Model.Handlers.All)
            {
                config.Handlers = new List<ConfigurationHandler>();
            }
            else
            {
                foreach (var enumValue in (Handlers[])Enum.GetValues(typeof(Handlers)))
                {
                    if (information.HandlersToProcess.Has(enumValue))
                    {
                        if (Enum.TryParse<ConfigurationHandler>(enumValue.ToString(), out ConfigurationHandler configHandler))
                        {
                            config.Handlers.Add(configHandler);
                        }
                    }
                }
            }
            config.Lists.IgnoreDuplicateDataRowErrors = information.IgnoreDuplicateDataRowErrors;
            config.MessagesDelegate = information.MessagesDelegate;
            config.PropertyBag.OverwriteSystemValues = information.OverwriteSystemPropertyBagValues;
            config.ProgressDelegate = information.ProgressDelegate;
            config.ContentTypes.ProvisionContentTypesToSubWebs = information.ProvisionContentTypesToSubWebs;
            config.Fields.ProvisionFieldsToSubWebs = information.ProvisionFieldsToSubWebs;
            config.SiteProvisionedDelegate = information.SiteProvisionedDelegate;
            return config;
        }

        public static ApplyConfiguration FromString(string input)
        {
          
            return JsonSerializer.Deserialize<ApplyConfiguration>(input);
        }
    }
}
