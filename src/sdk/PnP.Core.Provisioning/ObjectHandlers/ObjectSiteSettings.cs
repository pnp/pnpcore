using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;
using SearchBoxInNavBarModel = PnP.Core.Provisioning.Model.SearchBoxInNavBar;
using SearchBoxInNavBarCore = PnP.Core.Model.SharePoint.SearchBoxInNavBar;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Provisions and extracts site collection level settings.
    /// </summary>
    internal class ObjectSiteSettings : ObjectHandlerBase
    {
        /// <summary>
        /// The property bag key SharePoint stores the site collection search centre url under.
        /// </summary>
        private const string SearchCenterUrlPropertyKey = "SRCH_ENH_FTR_URL_SITE";

        public override string Name => "Site Settings";

        public override string InternalName => "SiteSettings";

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return true;
        }

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return template.SiteSettings != null;
        }

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                ISite site = await context.Site.GetAsync(
                    s => s.AllowDesigner,
                    s => s.AllowCreateDeclarativeWorkflow,
                    s => s.AllowSaveDeclarativeWorkflowAsTemplate,
                    s => s.AllowSavePublishDeclarativeWorkflow,
                    s => s.SocialBarOnSitePagesDisabled,
                    s => s.SearchBoxInNavBar,
                    s => s.ShowPeoplePickerSuggestionsForGuestUsers).ConfigureAwait(false);

                template.SiteSettings = new SiteSettings
                {
                    AllowDesigner = site.AllowDesigner,
                    AllowCreateDeclarativeWorkflow = site.AllowCreateDeclarativeWorkflow,
                    AllowSaveDeclarativeWorkflowAsTemplate = site.AllowSaveDeclarativeWorkflowAsTemplate,
                    AllowSavePublishDeclarativeWorkflow = site.AllowSavePublishDeclarativeWorkflow,
                    SocialBarOnSitePagesDisabled = site.SocialBarOnSitePagesDisabled,
                    SearchBoxInNavBar = ToModel(site.SearchBoxInNavBar),
                    SearchCenterUrl = await GetSearchCenterUrlAsync(context).ConfigureAwait(false),
                    ShowPeoplePickerSuggestionsForGuestUsers = site.ShowPeoplePickerSuggestionsForGuestUsers,
                };

                return template;
            }
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                SiteSettings settings = template.SiteSettings;
                if (settings == null)
                {
                    return parser;
                }

                ISite site = await context.Site.GetAsync(s => s.SearchBoxInNavBar).ConfigureAwait(false);
                bool dirty = false;

                if (await SiteTypeHelper.IsClassicSiteAsync(context).ConfigureAwait(false))
                {
                    site.AllowDesigner = settings.AllowDesigner;
                    site.AllowCreateDeclarativeWorkflow = settings.AllowCreateDeclarativeWorkflow;
                    site.AllowSaveDeclarativeWorkflowAsTemplate = settings.AllowSaveDeclarativeWorkflowAsTemplate;
                    site.AllowSavePublishDeclarativeWorkflow = settings.AllowSavePublishDeclarativeWorkflow;
                    dirty = true;
                }

                if (await SiteTypeHelper.IsCommunicationSiteAsync(context).ConfigureAwait(false))
                {
                    site.SocialBarOnSitePagesDisabled = settings.SocialBarOnSitePagesDisabled;
                    dirty = true;
                }

                SearchBoxInNavBarCore wanted = ToCore(settings.SearchBoxInNavBar);
                if (site.SearchBoxInNavBar != wanted)
                {
                    site.SearchBoxInNavBar = wanted;
                    dirty = true;
                }

                site.ShowPeoplePickerSuggestionsForGuestUsers = settings.ShowPeoplePickerSuggestionsForGuestUsers;
                dirty = true;

                if (dirty)
                {
                    await site.UpdateAsync().ConfigureAwait(false);
                }

                if (!string.IsNullOrEmpty(settings.SearchCenterUrl))
                {
                    string current = await GetSearchCenterUrlAsync(context).ConfigureAwait(false);
                    if (current != settings.SearchCenterUrl)
                    {
                        await SetSearchCenterUrlAsync(context, settings.SearchCenterUrl).ConfigureAwait(false);
                    }
                }

                return parser;
            }
        }

        #region Search centre url - a root web property bag value

        private static async Task<string> GetSearchCenterUrlAsync(PnPContext context)
        {
            try
            {
                IWeb rootWeb = await context.Site.RootWeb.GetAsync(w => w.AllProperties).ConfigureAwait(false);
                return rootWeb.AllProperties.GetString(SearchCenterUrlPropertyKey, string.Empty);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static async Task SetSearchCenterUrlAsync(PnPContext context, string searchCenterUrl)
        {
            IWeb rootWeb = await context.Site.RootWeb.GetAsync(w => w.AllProperties).ConfigureAwait(false);
            rootWeb.AllProperties[SearchCenterUrlPropertyKey] = searchCenterUrl;
            await rootWeb.AllProperties.UpdateAsync().ConfigureAwait(false);
        }

        #endregion

        #region SearchBoxInNavBar conversion

        /// <summary>
        /// Converts PnP Core's <see cref="SearchBoxInNavBar"/> to the provisioning model's.
        /// </summary>
        private static SearchBoxInNavBarModel ToModel(SearchBoxInNavBarCore value)
        {
            switch (value)
            {
                case SearchBoxInNavBarCore.Inherit: return SearchBoxInNavBarModel.Inherit;
                case SearchBoxInNavBarCore.AllPages: return SearchBoxInNavBarModel.AllPages;
                case SearchBoxInNavBarCore.ModernOnly: return SearchBoxInNavBarModel.ModernOnly;
                case SearchBoxInNavBarCore.Hidden: return SearchBoxInNavBarModel.Hidden;
                default: return SearchBoxInNavBarModel.Inherit;
            }
        }

        private static SearchBoxInNavBarCore ToCore(SearchBoxInNavBarModel value)
        {
            switch (value)
            {
                case SearchBoxInNavBarModel.Inherit: return SearchBoxInNavBarCore.Inherit;
                case SearchBoxInNavBarModel.AllPages: return SearchBoxInNavBarCore.AllPages;
                case SearchBoxInNavBarModel.ModernOnly: return SearchBoxInNavBarCore.ModernOnly;
                case SearchBoxInNavBarModel.Hidden: return SearchBoxInNavBarCore.Hidden;
                default: return SearchBoxInNavBarCore.Inherit;
            }
        }

        #endregion
    }
}
