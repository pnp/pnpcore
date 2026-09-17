using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Runs the provisioning engine: builds the handler list, orders it, threads the token parser
    /// through it, and fires the webhooks a template asks for.
    /// </summary>
    internal sealed class ProvisioningManager : IProvisioningManager
    {
        private readonly PnPContext context;

        internal ProvisioningManager(PnPContext pnpContext)
        {
            context = pnpContext ?? throw new ArgumentNullException(nameof(pnpContext));
        }

        #region Apply a template

        /// <inheritdoc/>
        public async Task ApplyTemplateAsync(ProvisioningTemplate template, ApplyConfiguration configuration = null)
        {
            await ApplyTemplateAsync(template, configuration, false, null).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void ApplyTemplate(ProvisioningTemplate template, ApplyConfiguration configuration = null)
        {
            ApplyTemplateAsync(template, configuration).GetAwaiter().GetResult();
        }

        internal async Task<TokenParser> ApplyTemplateAsync(ProvisioningTemplate template, ApplyConfiguration configuration,
            bool calledFromHierarchy, TokenParser tokenParser)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            ILogger logger = context.Logger;
            using (logger?.BeginScope(PnPCoreProvisioningResources.Provisioning_ObjectHandlers_Provisioning))
            {
                ProvisioningProgressDelegate progressDelegate = null;
                ProvisioningMessagesDelegate messagesDelegate = null;
                ProvisioningSiteProvisionedDelegate siteProvisionedDelegate = null;

                if (configuration != null)
                {
                    ApplyConfigurationParameters(configuration.Parameters, template.Parameters);

                    progressDelegate = configuration.ProgressDelegate;
                    messagesDelegate = SuppressRepeatedProblems(configuration.MessagesDelegate);
                    siteProvisionedDelegate = configuration.SiteProvisionedDelegate;
                }
                else
                {
                    configuration = new ApplyConfiguration();
                }

                ProvisioningTemplateApplyingInformation applyingInformation = configuration.ToApplyingInformation();

                IWeb web = await context.Web.GetAsync(w => w.Url, w => w.Title, w => w.ServerRelativeUrl).ConfigureAwait(false);
                await context.Site.LoadAsync(s => s.ServerRelativeUrl).ConfigureAwait(false);

                if (template.Scope == ProvisioningTemplateScope.RootSite && ObjectHandlerBase.IsSubSite(web))
                {
                    logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE,
                        PnPCoreProvisioningResources.SiteToTemplateConversion_ScopeOfTemplateDoesNotMatchTarget);
                    throw new InvalidOperationException(PnPCoreProvisioningResources.SiteToTemplateConversion_ScopeOfTemplateDoesNotMatchTarget);
                }

                CultureInfo currentCultureInfoValue = System.Threading.Thread.CurrentThread.CurrentCulture;
                if (!string.IsNullOrEmpty(template.TemplateCultureInfo))
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = int.TryParse(template.TemplateCultureInfo, out int cultureInfoValue)
                        ? new CultureInfo(cultureInfoValue)
                        : new CultureInfo(template.TemplateCultureInfo);
                }

                try
                {
                    await WarnOnAsymmetricBaseTemplatesAsync(web, template, messagesDelegate).ConfigureAwait(false);

                    List<ObjectHandlerBase> objectHandlers = BuildApplyHandlers(applyingInformation, calledFromHierarchy);

                    int count = objectHandlers.Count(o => o.ReportProgress && o.WillProvision(context, template, configuration)) + 1;
                    progressDelegate?.Invoke("Initializing engine", 1, count); // handlers + initializing message

                    tokenParser ??= await TokenParser.CreateAsync(context, template, applyingInformation).ConfigureAwait(false);

                    ObjectExtensibilityHandlers extensibility = objectHandlers.OfType<ObjectExtensibilityHandlers>().FirstOrDefault();
                    if (extensibility != null)
                    {
                        tokenParser = await extensibility.AddExtendedTokensAsync(context, template, tokenParser, configuration).ConfigureAwait(false);
                    }

                    int step = 2;

                    var cleaner = new NoScriptTemplateCleaner(context);
                    if (messagesDelegate != null)
                    {
                        cleaner.MessagesDelegate = messagesDelegate;
                    }
                    template = await cleaner.CleanUpBeforeProvisioningAsync(template).ConfigureAwait(false);

                    await CallWebHooksAsync(template, tokenParser, ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted).ConfigureAwait(false);

                    foreach (ObjectHandlerBase handler in objectHandlers)
                    {
                        if (!handler.WillProvision(context, template, configuration))
                        {
                            continue;
                        }

                        if (messagesDelegate != null)
                        {
                            handler.MessagesDelegate = messagesDelegate;
                        }

                        if (handler.ReportProgress && progressDelegate != null)
                        {
                            progressDelegate(handler.Name, step, count);
                            step++;
                        }

                        await CallWebHooksAsync(template, tokenParser,
                            ProvisioningTemplateWebhookKind.ObjectHandlerProvisioningStarted, handler.InternalName).ConfigureAwait(false);

                        try
                        {
                            tokenParser = await handler.ProvisionObjectsAsync(context, template, tokenParser, configuration).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            await CallWebHooksAsync(template, tokenParser,
                                ProvisioningTemplateWebhookKind.ExceptionOccurred, handler.InternalName, ex).ConfigureAwait(false);
                            throw;
                        }

                        await CallWebHooksAsync(template, tokenParser,
                            ProvisioningTemplateWebhookKind.ObjectHandlerProvisioningCompleted, handler.InternalName).ConfigureAwait(false);
                    }

                    siteProvisionedDelegate?.Invoke(web.Title, web.Url.ToString());

                    await CallWebHooksAsync(template, tokenParser, ProvisioningTemplateWebhookKind.ProvisioningTemplateCompleted).ConfigureAwait(false);

                    return tokenParser;
                }
                finally
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = currentCultureInfoValue;
                }
            }
        }

        /// <summary>
        /// Builds the handler list for an apply run, in execution order.
        /// </summary>
        private static List<ObjectHandlerBase> BuildApplyHandlers(ProvisioningTemplateApplyingInformation applyingInformation, bool calledFromHierarchy)
        {
            var objectHandlers = new List<ObjectHandlerBase>();


            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.RegionalSettings))
            {
                objectHandlers.Add(new ObjectRegionalSettings());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.SupportedUILanguages))
            {
                objectHandlers.Add(new ObjectSupportedUILanguages());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Features))
            {
                objectHandlers.Add(new ObjectFeatures());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.TermGroups))
            {
                objectHandlers.Add(new ObjectTermGroups());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Fields)
                || applyingInformation.HandlersToProcess.HasFlag(Handlers.Lists))
            {
                objectHandlers.Add(new ObjectField(FieldAndListProvisioningStepHelper.Step.ListAndStandardFields));
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.ContentTypes))
            {
                objectHandlers.Add(new ObjectContentType(FieldAndListProvisioningStepHelper.Step.ListAndStandardFields));
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Lists))
            {
                objectHandlers.Add(new ObjectListInstance(FieldAndListProvisioningStepHelper.Step.ListAndStandardFields));
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Fields)
                || applyingInformation.HandlersToProcess.HasFlag(Handlers.Lists))
            {
                objectHandlers.Add(new ObjectField(FieldAndListProvisioningStepHelper.Step.LookupFields));
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.ContentTypes))
            {
                objectHandlers.Add(new ObjectContentType(FieldAndListProvisioningStepHelper.Step.LookupFields));
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Lists))
            {
                objectHandlers.Add(new ObjectListInstance(FieldAndListProvisioningStepHelper.Step.LookupFields));
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Files))
            {
                objectHandlers.Add(new ObjectFiles());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Lists))
            {
                objectHandlers.Add(new ObjectListInstance(FieldAndListProvisioningStepHelper.Step.ListSettings));

                objectHandlers.Add(new ObjectListInstanceDataRows());
            }

            if (!calledFromHierarchy && applyingInformation.HandlersToProcess.HasFlag(Handlers.Tenant))
            {
                objectHandlers.Add(new ObjectTenant());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.ApplicationLifecycleManagement))
            {
                objectHandlers.Add(new ObjectApplicationLifecycleManagement());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Pages))
            {
                objectHandlers.Add(new ObjectClientSidePages());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.SiteHeader))
            {
                objectHandlers.Add(new ObjectSiteHeaderSettings());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.SiteFooter))
            {
                objectHandlers.Add(new ObjectSiteFooterSettings());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.CustomActions))
            {
                objectHandlers.Add(new ObjectCustomActions());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Pages))
            {
                objectHandlers.Add(new ObjectPages());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Navigation))
            {
                objectHandlers.Add(new ObjectNavigation());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Workflows))
            {
                objectHandlers.Add(new ObjectWorkflows());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Publishing))
            {
                objectHandlers.Add(new ObjectPublishing());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.ComposedLook))
            {
                objectHandlers.Add(new ObjectComposedLook());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.AuditSettings))
            {
                objectHandlers.Add(new ObjectAuditSettings());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.SitePolicy))
            {
                objectHandlers.Add(new ObjectSitePolicy());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.ImageRenditions))
            {
                objectHandlers.Add(new ObjectImageRenditions());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.SiteSecurity))
            {
                objectHandlers.Add(new ObjectSiteSecurity());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.SearchSettings))
            {
                objectHandlers.Add(new ObjectSearchSettings());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.PropertyBagEntries))
            {
                objectHandlers.Add(new ObjectPropertyBagEntry());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.WebSettings))
            {
                objectHandlers.Add(new ObjectWebSettings());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.SiteSettings))
            {
                objectHandlers.Add(new ObjectSiteSettings());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Theme))
            {
                objectHandlers.Add(new ObjectTheme());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.ExtensibilityProviders))
            {
                objectHandlers.Add(new ObjectExtensibilityHandlers());
            }

            if (applyingInformation.PersistTemplateInfo)
            {
                objectHandlers.Add(new ObjectPersistTemplateInfo());
            }

            return objectHandlers;
        }

        #endregion

        #region Extract a template

        /// <inheritdoc/>
        public async Task<ProvisioningTemplate> GetTemplateAsync(ExtractConfiguration configuration = null)
        {
            ILogger logger = context.Logger;
            using (logger?.BeginScope(PnPCoreProvisioningResources.Provisioning_ObjectHandlers_Extraction))
            {
                configuration ??= new ExtractConfiguration();

                ProvisioningTemplateCreationInformation creationInfo = configuration.ToCreationInformation();

                if (creationInfo.BaseTemplate != null)
                {
                    logger?.LogDebug(PnPCoreProvisioningResources.SiteToTemplateConversion_Base_template_available___0_, creationInfo.BaseTemplate.Id);
                }

                ProvisioningProgressDelegate progressDelegate = configuration.ProgressDelegate;
                ProvisioningMessagesDelegate messagesDelegate = configuration.MessagesDelegate;

                var template = new ProvisioningTemplate
                {
                    Connector = configuration.FileConnector
                };

                List<ObjectHandlerBase> objectHandlers = BuildExtractHandlers(configuration);

                await context.Web.LoadAsync(w => w.Url).ConfigureAwait(false);

                int step = 1;
                int count = objectHandlers.Count(o => o.ReportProgress && o.WillExtract(context, template, configuration));

                foreach (ObjectHandlerBase handler in objectHandlers)
                {
                    if (!handler.WillExtract(context, template, configuration))
                    {
                        continue;
                    }

                    if (messagesDelegate != null)
                    {
                        handler.MessagesDelegate = messagesDelegate;
                    }

                    if (handler.ReportProgress && progressDelegate != null)
                    {
                        progressDelegate(handler.Name, step, count);
                        step++;
                    }

                    template = await handler.ExtractObjectsAsync(context, template, configuration).ConfigureAwait(false);
                }

                return template;
            }
        }

        /// <inheritdoc/>
        public ProvisioningTemplate GetTemplate(ExtractConfiguration configuration = null)
        {
            return GetTemplateAsync(configuration).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Builds the handler list for an extract run, in execution order.
        /// </summary>
        private static List<ObjectHandlerBase> BuildExtractHandlers(ExtractConfiguration configuration)
        {
            var objectHandlers = new List<ObjectHandlerBase>();


            bool all = configuration.Handlers.Count == 0;

            if (all || configuration.Handlers.Contains(ConfigurationHandler.RegionalSettings))
            {
                objectHandlers.Add(new ObjectRegionalSettings());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.SupportedUILanguages))
            {
                objectHandlers.Add(new ObjectSupportedUILanguages());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.Fields))
            {
                objectHandlers.Add(new ObjectField(FieldAndListProvisioningStepHelper.Step.Export));
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.ContentTypes))
            {
                objectHandlers.Add(new ObjectContentType(FieldAndListProvisioningStepHelper.Step.Export));
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.Lists))
            {
                objectHandlers.Add(new ObjectListInstance(FieldAndListProvisioningStepHelper.Step.Export));

                objectHandlers.Add(new ObjectListInstanceDataRows());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.Files))
            {
                objectHandlers.Add(new ObjectFiles());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.CustomActions))
            {
                objectHandlers.Add(new ObjectCustomActions());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.Navigation))
            {
                objectHandlers.Add(new ObjectNavigation());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.Workflows))
            {
                objectHandlers.Add(new ObjectWorkflows());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.Publishing))
            {
                objectHandlers.Add(new ObjectPublishing());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.ComposedLook))
            {
                objectHandlers.Add(new ObjectComposedLook());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.AuditSettings))
            {
                objectHandlers.Add(new ObjectAuditSettings());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.SitePolicy))
            {
                objectHandlers.Add(new ObjectSitePolicy());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.ImageRenditions))
            {
                objectHandlers.Add(new ObjectImageRenditions());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.Taxonomy))
            {
                objectHandlers.Add(new ObjectTermGroups());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.SiteSecurity))
            {
                objectHandlers.Add(new ObjectSiteSecurity());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.SearchSettings))
            {
                objectHandlers.Add(new ObjectSearchSettings());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.Pages))
            {
                objectHandlers.Add(new ObjectClientSidePageContents());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.SiteHeader))
            {
                objectHandlers.Add(new ObjectSiteHeaderSettings());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.SiteFooter))
            {
                objectHandlers.Add(new ObjectSiteFooterSettings());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.PropertyBagEntries))
            {
                objectHandlers.Add(new ObjectPropertyBagEntry());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.WebSettings))
            {
                objectHandlers.Add(new ObjectWebSettings());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.SiteSettings))
            {
                objectHandlers.Add(new ObjectSiteSettings());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.SyntexModels))
            {
                objectHandlers.Add(new ObjectSyntexModels());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.ApplicationLifecycleManagement))
            {
                objectHandlers.Add(new ObjectApplicationLifecycleManagement());
            }

            objectHandlers.Add(new ObjectLocalization());

            if (all || configuration.Handlers.Contains(ConfigurationHandler.ExtensibilityProviders))
            {
                objectHandlers.Add(new ObjectExtensibilityHandlers());
            }

            objectHandlers.Add(new ObjectRetrieveTemplateInfo());


            return objectHandlers;
        }

        #endregion

        #region Apply and extract a tenant template (hierarchy)

        /// <inheritdoc/>
        public async Task ApplyTenantTemplateAsync(ProvisioningHierarchy hierarchy, string sequenceId, ApplyConfiguration configuration = null)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            ILogger logger = context.Logger;
            using (logger?.BeginScope(PnPCoreProvisioningResources.Provisioning_ObjectHandlers_Provisioning))
            {
                ProvisioningProgressDelegate progressDelegate = null;
                ProvisioningMessagesDelegate messagesDelegate = null;

                if (configuration == null)
                {
                    configuration = new ApplyConfiguration();
                }
                else
                {
                    ApplyConfigurationParameters(configuration.Parameters, hierarchy.Parameters);

                    progressDelegate = configuration.ProgressDelegate;
                    messagesDelegate = configuration.MessagesDelegate;
                }

                logger?.LogDebug("{Source}: Attaching object handlers", Constants.LOGGING_SOURCE);

                List<ObjectHierarchyHandlerBase> objectHandlers = BuildHierarchyApplyHandlers();

                int count = objectHandlers.Count(o => o.ReportProgress && o.WillProvision(context, hierarchy, sequenceId, configuration)) + 1;
                progressDelegate?.Invoke("Initializing engine", 1, count); // handlers + initializing message

                int step = 2;

                TokenParser sequenceTokenParser = await TokenParser.CreateAsync(context, hierarchy).ConfigureAwait(false);

                await CallWebHooksAsync(hierarchy.Templates.FirstOrDefault(), sequenceTokenParser,
                    ProvisioningTemplateWebhookKind.ProvisioningStarted).ConfigureAwait(false);

                foreach (ObjectHierarchyHandlerBase handler in objectHandlers)
                {
                    if (!handler.WillProvision(context, hierarchy, sequenceId, configuration))
                    {
                        continue;
                    }

                    if (messagesDelegate != null)
                    {
                        handler.MessagesDelegate = messagesDelegate;
                    }

                    if (handler.ReportProgress && progressDelegate != null)
                    {
                        progressDelegate(handler.Name, step, count);
                        step++;
                    }

                    try
                    {
                        sequenceTokenParser = await handler.ProvisionObjectsAsync(context, hierarchy, sequenceId, sequenceTokenParser, configuration).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await CallWebHooksAsync(hierarchy.Templates.FirstOrDefault(), sequenceTokenParser,
                            ProvisioningTemplateWebhookKind.ProvisioningExceptionOccurred, handler.Name, ex).ConfigureAwait(false);
                        throw;
                    }
                }

                await CallWebHooksAsync(hierarchy.Templates.FirstOrDefault(), sequenceTokenParser,
                    ProvisioningTemplateWebhookKind.ProvisioningCompleted).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void ApplyTenantTemplate(ProvisioningHierarchy hierarchy, string sequenceId, ApplyConfiguration configuration = null)
        {
            ApplyTenantTemplateAsync(hierarchy, sequenceId, configuration).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<ProvisioningHierarchy> GetTenantTemplateAsync(ExtractConfiguration configuration = null)
        {
            configuration ??= new ExtractConfiguration();

            ILogger logger = context.Logger;
            using (logger?.BeginScope(PnPCoreProvisioningResources.Provisioning_ObjectHandlers_Extraction))
            {
                var tenantTemplate = new ProvisioningHierarchy
                {
                    Connector = configuration.FileConnector
                };

                List<ObjectHierarchyHandlerBase> objectHandlers = BuildHierarchyExtractHandlers(configuration);

                int step = 1;
                int count = objectHandlers.Count(o => o.ReportProgress && o.WillExtract(context, tenantTemplate, null, configuration));

                foreach (ObjectHierarchyHandlerBase handler in objectHandlers)
                {
                    if (!handler.WillExtract(context, tenantTemplate, null, configuration))
                    {
                        continue;
                    }

                    if (configuration.MessagesDelegate != null)
                    {
                        handler.MessagesDelegate = (message, type) => configuration.MessagesDelegate(message, type);
                    }

                    if (handler.ReportProgress && configuration.ProgressDelegate != null)
                    {
                        configuration.ProgressDelegate(handler.Name, step, count);
                        step++;
                    }

                    tenantTemplate = await handler.ExtractObjectsAsync(context, tenantTemplate, configuration).ConfigureAwait(false);
                }

                return tenantTemplate;
            }
        }

        /// <inheritdoc/>
        public ProvisioningHierarchy GetTenantTemplate(ExtractConfiguration configuration = null)
        {
            return GetTenantTemplateAsync(configuration).GetAwaiter().GetResult();
        }

        private static List<ObjectHierarchyHandlerBase> BuildHierarchyApplyHandlers()
        {
            return new List<ObjectHierarchyHandlerBase>
            {
                new ObjectHierarchyTenant(),
                new ObjectHierarchySequenceTermGroups(),
                new ObjectHierarchySequenceSites(),
                new ObjectTeams(),
                new ObjectAzureActiveDirectory(),
            };
        }

        private static List<ObjectHierarchyHandlerBase> BuildHierarchyExtractHandlers(ExtractConfiguration configuration)
        {
            _ = configuration;
            return new List<ObjectHierarchyHandlerBase>();
        }

        #endregion

        #region Helpers

        private static void ApplyConfigurationParameters(Dictionary<string, string> sourceParameters, Dictionary<string, string> destParameters)
        {
            if (sourceParameters == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> p in sourceParameters)
            {
                destParameters[p.Key] = p.Value;
            }
        }

        /// <summary>
        /// Wraps a caller's messages delegate so an identical warning or error is reported once.
        /// </summary>
        private static ProvisioningMessagesDelegate SuppressRepeatedProblems(ProvisioningMessagesDelegate inner)
        {
            if (inner == null)
            {
                return null;
            }

            var reported = new HashSet<string>(StringComparer.Ordinal);

            return (message, messageType) =>
            {
                if ((messageType == ProvisioningMessageType.Warning || messageType == ProvisioningMessageType.Error)
                    && !reported.Add($"{messageType}|{message}"))
                {
                    return;
                }

                inner(message, messageType);
            };
        }

        private async Task WarnOnAsymmetricBaseTemplatesAsync(IWeb web, ProvisioningTemplate template, ProvisioningMessagesDelegate messagesDelegate)
        {
            if (string.IsNullOrEmpty(template.BaseSiteTemplate))
            {
                return;
            }

            await web.LoadAsync(w => w.WebTemplate, w => w.WebTemplateConfiguration).ConfigureAwait(false);
            string targetSiteTemplateId = $"{web.WebTemplate}#{BaseTemplates.BaseTemplateManager.GetConfiguration(web.WebTemplateConfiguration)}";

            if (targetSiteTemplateId.Equals(template.BaseSiteTemplate, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            string warning = string.Format(CultureInfo.CurrentCulture,
                PnPCoreProvisioningResources.Provisioning_Asymmetric_Base_Templates,
                template.BaseSiteTemplate, targetSiteTemplateId);

            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
            messagesDelegate?.Invoke(warning, ProvisioningMessageType.Warning);
        }

        private async Task CallWebHooksAsync(ProvisioningTemplate template, TokenParser parser, ProvisioningTemplateWebhookKind kind,
            string objectHandler = null, Exception exception = null)
        {
            if (template == null)
            {
                return;
            }

            var webhooks = new List<ProvisioningWebhookBase>();

            if (template.ProvisioningTemplateWebhooks != null && template.ProvisioningTemplateWebhooks.Any())
            {
                webhooks.AddRange(template.ProvisioningTemplateWebhooks);
            }

            if (template.ParentHierarchy?.ProvisioningWebhooks != null && template.ParentHierarchy.ProvisioningWebhooks.Any())
            {
                webhooks.AddRange(template.ParentHierarchy.ProvisioningWebhooks);
            }

            if (webhooks.Count == 0)
            {
                return;
            }

            HttpClient httpClient = WebhookHttpClient.Instance;

            foreach (ProvisioningWebhookBase webhook in webhooks.Where(w => w.Kind == kind))
            {
                await WebhookSender.InvokeWebhookAsync(webhook, httpClient, kind, parser, objectHandler, exception, context.Logger).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
