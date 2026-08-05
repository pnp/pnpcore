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
                    // Apply the configuration parameters to the template
                    ApplyConfigurationParameters(configuration.Parameters, template.Parameters);

                    progressDelegate = configuration.ProgressDelegate;
                    messagesDelegate = SuppressRepeatedProblems(configuration.MessagesDelegate);
                    siteProvisionedDelegate = configuration.SiteProvisionedDelegate;
                }
                else
                {
                    // When no configuration was passed we want to execute all handlers
                    configuration = new ApplyConfiguration();
                }

                ProvisioningTemplateApplyingInformation applyingInformation = configuration.ToApplyingInformation();

                IWeb web = await context.Web.GetAsync(w => w.Url, w => w.Title, w => w.ServerRelativeUrl).ConfigureAwait(false);
                await context.Site.LoadAsync(s => s.ServerRelativeUrl).ConfigureAwait(false);

                // Check the template's scope against the target, when one was declared
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
                    // Warn when the target site does not share a base template with the template's source
                    await WarnOnAsymmetricBaseTemplatesAsync(web, template, messagesDelegate).ConfigureAwait(false);

                    List<ObjectHandlerBase> objectHandlers = BuildApplyHandlers(applyingInformation, calledFromHierarchy);

                    int count = objectHandlers.Count(o => o.ReportProgress && o.WillProvision(context, template, configuration)) + 1;
                    progressDelegate?.Invoke("Initializing engine", 1, count); // handlers + initializing message

                    tokenParser ??= await TokenParser.CreateAsync(context, template, applyingInformation).ConfigureAwait(false);

                    // Custom token providers run before the first handler - a token has to exist
                    // before anything can reference it. This is why ObjectExtensibilityHandlers has
                    // a second entry point rather than doing the work in ProvisionObjectsAsync.
                    ObjectExtensibilityHandlers extensibility = objectHandlers.OfType<ObjectExtensibilityHandlers>().FirstOrDefault();
                    if (extensibility != null)
                    {
                        tokenParser = await extensibility.AddExtendedTokensAsync(context, template, tokenParser, configuration).ConfigureAwait(false);
                    }

                    int step = 2;

                    // Remove artefacts a NoScript site would reject
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
                            // The parser is threaded, not copied: whatever this handler registered
                            // is visible to every handler after it. Do not "simplify" this to
                            // discard the return value.
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

                    // Notify the completed provisioning of the site
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

            // MIGRATION PHASES 6-8: each handler is added here as it lands. The list below mirrors
            // PnP Framework's ApplyRemoteTemplate ordering exactly, including the three passes over
            // fields/content types/lists, so porting a handler is a one line change rather than a
            // sequencing decision.
            //
            //  Phase 5 (wave 1): DONE - everything below is registered.
            //  Phase 6 (wave 2): Localization, Field x3, ContentType x2, ListInstance x3,
            //                    ListInstanceDataRows, Files, SiteSecurity, TermGroups
            //  Phase 7 (wave 3): AuditSettings, SitePolicy, Workflows, Pages, PageContents,
            //                    Publishing, ComposedLook, ImageRenditions, Navigation
            //  Phase 8:          ApplicationLifecycleManagement (done), Tenant (done)

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

            // Term groups come before the fields, and this is load bearing in the same way the three
            // passes below are: a taxonomy site column binds to its term set by id, so a template
            // that defines both fails outright if the field is created first - SharePoint rejects
            // the column's schema rather than creating a broken one. PnP Framework orders it the same
            // way (SiteToTemplateConversion.cs, TermGroups at :390 ahead of Fields at :395).
            //
            // This was the other way round until scenario 5 caught it. No per-handler test could
            // have: ObjectTermGroups and ObjectField each worked perfectly on their own.
            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.TermGroups))
            {
                objectHandlers.Add(new ObjectTermGroups());
            }

            // The three passes. ObjectField, ObjectContentType and ObjectListInstance are each
            // registered more than once with a different Step, because a lookup column cannot be
            // created before the list it points at and a template routinely defines both.
            //
            // Pass 1 - fields, content types and lists, minus anything that references a list.
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

            // Pass 2 - the lookup and calculated columns held back from pass 1, and the content type
            // links to them.
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

            // Between passes 2 and 3, as in PnP Framework: a file may be uploaded into a library the
            // second pass created, and a list view created in the third pass may point at it.
            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Files))
            {
                objectHandlers.Add(new ObjectFiles());
            }

            if (applyingInformation.HandlersToProcess.HasFlag(Handlers.Lists))
            {
                // Pass 3 - everything that can reference a column: views, default values, folders.
                // Only ObjectListInstance has work in this pass.
                objectHandlers.Add(new ObjectListInstance(FieldAndListProvisioningStepHelper.Step.ListSettings));

                // After all three passes: a row can carry a lookup into any list in the template,
                // so every list has to exist and have its columns before any row is written.
                objectHandlers.Add(new ObjectListInstanceDataRows());
            }

            // Tenant settings come before ALM, as in PnP Framework: a site design or a storage
            // entity the template defines is a token that later handlers resolve.
            //
            // Not when applying a hierarchy. ObjectHierarchyTenant already applied the tenant element
            // once, before any site in the sequence existed; running it again per site would repeat
            // every tenant-wide write once per site in the hierarchy.
            if (!calledFromHierarchy && applyingInformation.HandlersToProcess.HasFlag(Handlers.Tenant))
            {
                objectHandlers.Add(new ObjectTenant());
            }

            // Before the pages, as in PnP Framework: an app can bring the web parts and columns a
            // page then places, so installing it afterwards is too late.
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

            // Last, and gated: recording what was applied needs property bag write access, and the
            // engine is deliberately usable by callers who do not have it.
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

                // Create empty object, hooking up the connector so the resulting template can be
                // applied to another site straight away
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

                    // PnP Framework cloned the ClientContext per handler so that one handler's
                    // pending queries could not disturb another's. PnP Core executes each call on
                    // its own, so the same context is used throughout - one fewer authentication
                    // round trip per handler.
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

            // MIGRATION PHASES 5-8: as with BuildApplyHandlers, handlers are registered here as
            // they land. Extraction uses a single pass - the Step.Export variant of the field,
            // content type and list handlers - rather than the three passes apply needs.

            // An empty Handlers list means "everything", matching how ExtractConfiguration is
            // documented and how PnP Framework's Handlers.All behaved.
            bool all = configuration.Handlers.Count == 0;

            if (all || configuration.Handlers.Contains(ConfigurationHandler.RegionalSettings))
            {
                objectHandlers.Add(new ObjectRegionalSettings());
            }

            if (all || configuration.Handlers.Contains(ConfigurationHandler.SupportedUILanguages))
            {
                objectHandlers.Add(new ObjectSupportedUILanguages());
            }

            // Extraction needs only one pass - the three-pass ordering exists to satisfy SharePoint
            // on the way in, and a read has no such constraint.
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

                // After the lists, so the rows have list instances to attach themselves to.
                objectHandlers.Add(new ObjectListInstanceDataRows());
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

            // Always registered, and deliberately after every handler that collects resource values:
            // it writes the files they filled. Its own WillExtract gates on PersistMultiLanguageResources.
            objectHandlers.Add(new ObjectLocalization());

            if (all || configuration.Handlers.Contains(ConfigurationHandler.ExtensibilityProviders))
            {
                objectHandlers.Add(new ObjectExtensibilityHandlers());
            }

            // Always last, and always registered: it reads two entries ObjectPropertyBagEntry
            // produced, lifts them into the template's own metadata and deletes them from the
            // property bag. Registering it conditionally would leave engine bookkeeping in the
            // extracted template.
            objectHandlers.Add(new ObjectRetrieveTemplateInfo());

            // ObjectFeatures and ObjectTheme are apply-only - both return false from WillExtract,
            // so registering them here would only add a no-op to the progress count.

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
                    // Apply the configuration parameters to the hierarchy
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
            // The order is PnP Framework's and it is load bearing: tenant settings and term groups
            // precede the sites, because a site design or a term set the sites are created from has
            // to exist - and have published its tokens - before they are created. Teams come after
            // the sites because a team's site is one of them.
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
            // MIGRATION PHASE 8: ObjectHierarchySequenceSites when configuration.Tenant.Sequence is
            // set, ObjectTeams when configuration.Tenant.Teams is set.
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

            // Merge the webhooks at template level with those at global level
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

            // A plain client, not PnP Core's SharePoint one - see WebhookHttpClient for why.
            HttpClient httpClient = WebhookHttpClient.Instance;

            foreach (ProvisioningWebhookBase webhook in webhooks.Where(w => w.Kind == kind))
            {
                await WebhookSender.InvokeWebhookAsync(webhook, httpClient, kind, parser, objectHandler, exception, context.Logger).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
