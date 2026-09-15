using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// The first end-to-end run of the engine with a real handler registered.
    /// </summary>
    [TestClass]
    public class ObjectSiteSettingsLiveTests : LiveTestBase
    {
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task GetTemplate_ExtractsSiteSettings()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                IProvisioningManager manager = context.GetProvisioningManager();

                ProvisioningTemplate template = await manager.GetTemplateAsync().ConfigureAwait(false);

                Assert.IsNotNull(template, "The engine returned no template.");
                Assert.IsNotNull(template.SiteSettings,
                    "ObjectSiteSettings is registered for extraction but contributed nothing.");

                Console.WriteLine($"AllowDesigner                            : {template.SiteSettings.AllowDesigner}");
                Console.WriteLine($"SocialBarOnSitePagesDisabled             : {template.SiteSettings.SocialBarOnSitePagesDisabled}");
                Console.WriteLine($"SearchBoxInNavBar                        : {template.SiteSettings.SearchBoxInNavBar}");
                Console.WriteLine($"SearchCenterUrl                          : '{template.SiteSettings.SearchCenterUrl}'");
                Console.WriteLine($"ShowPeoplePickerSuggestionsForGuestUsers : {template.SiteSettings.ShowPeoplePickerSuggestionsForGuestUsers}");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ApplyTemplate_RoundTripsSiteSettings()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                IProvisioningManager manager = context.GetProvisioningManager();

                ProvisioningTemplate original = await manager.GetTemplateAsync().ConfigureAwait(false);
                Assert.IsNotNull(original.SiteSettings);

                bool originalPeoplePicker = original.SiteSettings.ShowPeoplePickerSuggestionsForGuestUsers;

                try
                {
                    var template = new ProvisioningTemplate
                    {
                        SiteSettings = new SiteSettings
                        {
                            AllowDesigner = original.SiteSettings.AllowDesigner,
                            AllowCreateDeclarativeWorkflow = original.SiteSettings.AllowCreateDeclarativeWorkflow,
                            AllowSaveDeclarativeWorkflowAsTemplate = original.SiteSettings.AllowSaveDeclarativeWorkflowAsTemplate,
                            AllowSavePublishDeclarativeWorkflow = original.SiteSettings.AllowSavePublishDeclarativeWorkflow,
                            SocialBarOnSitePagesDisabled = original.SiteSettings.SocialBarOnSitePagesDisabled,
                            SearchBoxInNavBar = original.SiteSettings.SearchBoxInNavBar,
                            ShowPeoplePickerSuggestionsForGuestUsers = !originalPeoplePicker,
                        },
                    };

                    await manager.ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext freshContext = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        ProvisioningTemplate after = await freshContext.GetProvisioningManager()
                            .GetTemplateAsync().ConfigureAwait(false);

                        Assert.AreEqual(!originalPeoplePicker,
                            after.SiteSettings.ShowPeoplePickerSuggestionsForGuestUsers,
                            "The applied setting did not survive a re-extract.");
                    }
                }
                finally
                {
                    try
                    {
                        var restore = new ProvisioningTemplate { SiteSettings = original.SiteSettings };
                        await manager.ApplyTemplateAsync(restore).ConfigureAwait(false);
                        Console.WriteLine("Restored the original site settings.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE site settings: {Describe(ex)}");
                    }
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ApplyTemplate_WithNoSiteSettingsIsANoOp()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                var empty = new ProvisioningTemplate();

                await context.GetProvisioningManager().ApplyTemplateAsync(empty).ConfigureAwait(false);

                Assert.IsNull(empty.SiteSettings, "Applying a template must not mutate it.");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ExtractConfiguration_CanSelectJustThisHandler()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                var configuration = new ExtractConfiguration();
                configuration.Handlers.Add(ConfigurationHandler.SiteSettings);

                ProvisioningTemplate template = await context.GetProvisioningManager()
                    .GetTemplateAsync(configuration).ConfigureAwait(false);

                Assert.IsNotNull(template.SiteSettings,
                    "Selecting SiteSettings explicitly should still run the handler.");
            }
        }
    }
}
