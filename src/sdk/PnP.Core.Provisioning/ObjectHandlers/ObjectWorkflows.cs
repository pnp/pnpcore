using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Workflows;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreList = PnP.Core.Model.SharePoint.IList;
using WorkflowDefinitionModel = PnP.Core.Provisioning.Model.WorkflowDefinition;
using WorkflowSubscriptionModel = PnP.Core.Provisioning.Model.WorkflowSubscription;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies and reads back the <c>&lt;pnp:Workflows&gt;</c> element - SharePoint 2013 workflow
    /// definitions and the subscriptions that bind them to a list or site.
    /// </summary>
    internal class ObjectWorkflows : ObjectHandlerBase
    {
        public override string Name => "Workflows";

        public override string InternalName => "Workflows";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.Workflows != null
                && (template.Workflows.WorkflowDefinitions.Any() || template.Workflows.WorkflowSubscriptions.Any());

            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            _willExtract ??= true;
            return _willExtract.Value;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            if (!WillProvision(context, template, configuration))
            {
                return parser;
            }

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            // One probe before doing anything: if the service is off, every later call fails the
            // same way and reporting it eight times helps nobody.
            if (!await IsWorkflowServiceAvailableAsync(context, siteId, webId).ConfigureAwait(false))
            {
                return parser;
            }

            Dictionary<Guid, Guid> definitionIds = await ProvisionDefinitionsAsync(
                context, template, parser, siteId, webId).ConfigureAwait(false);

            await ProvisionSubscriptionsAsync(context, template, parser, siteId, webId, definitionIds).ConfigureAwait(false);

            WriteMessage("Done processing workflows", ProvisioningMessageType.Completed);

            return parser;
        }

        /// <summary>
        /// Saves and publishes the template's workflow definitions.
        /// </summary>
        /// <returns>
        /// A map from the template's definition id to the one the server assigned, so the
        /// subscriptions can be bound to what actually exists.
        /// </returns>
        private async Task<Dictionary<Guid, Guid>> ProvisionDefinitionsAsync(PnPContext context,
            ProvisioningTemplate template, TokenParser parser, Guid siteId, Guid webId)
        {
            var assigned = new Dictionary<Guid, Guid>();

            int index = 0;

            foreach (WorkflowDefinitionModel definition in template.Workflows.WorkflowDefinitions)
            {
                index++;
                WriteSubProgress("Workflow definition", definition.DisplayName, index,
                    template.Workflows.WorkflowDefinitions.Count);

                string xaml = ReadXaml(context, template, definition, parser);

                if (xaml == null)
                {
                    continue;
                }

                try
                {
                    var info = new WorkflowDefinitionInfo
                    {
                        Id = definition.Id,
                        DisplayName = parser.ParseString(definition.DisplayName),
                        Description = parser.ParseString(definition.Description),
                        Xaml = xaml,
                        RestrictToScope = parser.ParseString(definition.RestrictToScope),
                        RestrictToType = definition.RestrictToType,
                    };

                    WorkflowDefinitionInfo saved = await CsomRequestSender.SendAsync(context,
                        new SaveWorkflowDefinitionRequest(siteId, webId, info)).ConfigureAwait(false);

                    Guid savedId = saved?.Id ?? definition.Id;

                    if (definition.Id != Guid.Empty)
                    {
                        assigned[definition.Id] = savedId;
                    }

                    if (definition.Published)
                    {
                        await CsomRequestSender.SendAsync(context,
                            new PublishWorkflowDefinitionRequest(siteId, webId, savedId)).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    string warning = $"The workflow definition '{definition.DisplayName}' could not be provisioned: " +
                        ErrorText.Describe(ex);
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }

            return assigned;
        }

        /// <summary>
        /// Reads a definition's XAML from the template's files.
        /// </summary>
        private string ReadXaml(PnPContext context, ProvisioningTemplate template,
            WorkflowDefinitionModel definition, TokenParser parser)
        {
            string path = parser.ParseString(definition.XamlPath);

            if (string.IsNullOrEmpty(path))
            {
                string warning = $"The workflow definition '{definition.DisplayName}' has no XamlPath, so it was skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return null;
            }

            byte[] bytes = TemplateFileUtilities.TryGetFileBytes(template, path);

            if (bytes == null)
            {
                string warning = $"The workflow definition '{definition.DisplayName}' points at '{path}', " +
                    "which is not in the template's files, so it was skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return null;
            }

            // The XAML itself carries tokens - list ids and field names a workflow acts on - so it
            // goes through the parser like any other content.
            return parser.ParseString(Encoding.UTF8.GetString(bytes));
        }

        private async Task ProvisionSubscriptionsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, Guid siteId, Guid webId, Dictionary<Guid, Guid> definitionIds)
        {
            if (!template.Workflows.WorkflowSubscriptions.Any())
            {
                return;
            }

            int index = 0;

            foreach (WorkflowSubscriptionModel subscription in template.Workflows.WorkflowSubscriptions)
            {
                index++;
                WriteSubProgress("Workflow subscription", subscription.Name, index,
                    template.Workflows.WorkflowSubscriptions.Count);

                try
                {
                    Guid definitionId = definitionIds.TryGetValue(subscription.DefinitionId, out Guid mapped)
                        ? mapped
                        : subscription.DefinitionId;

                    Guid? listId = await ResolveListIdAsync(context, subscription, parser).ConfigureAwait(false);

                    var info = new WorkflowSubscriptionInfo
                    {
                        DefinitionId = definitionId,
                        Name = parser.ParseString(subscription.Name),
                        Enabled = subscription.Enabled,
                        EventTypes = subscription.EventTypes?.ToList() ?? new List<string>(),
                        StatusFieldName = parser.ParseString(subscription.StatusFieldName),
                        EventSourceId = listId ?? Guid.Empty,
                    };

                    foreach (KeyValuePair<string, string> property in subscription.PropertyDefinitions)
                    {
                        info.PropertyDefinitions[property.Key] = parser.ParseString(property.Value);
                    }

                    await CsomRequestSender.SendAsync(context,
                        new PublishWorkflowSubscriptionRequest(siteId, webId, info, listId)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"The workflow subscription '{subscription.Name}' could not be provisioned: " +
                        ErrorText.Describe(ex);
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        /// <summary>
        /// Resolves the list a subscription is bound to, when it names one.
        /// </summary>
        private async Task<Guid?> ResolveListIdAsync(PnPContext context, WorkflowSubscriptionModel subscription, TokenParser parser)
        {
            string value = parser.ParseString(subscription.ListId);

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (Guid.TryParse(value, out Guid listId))
            {
                return listId;
            }

            // Not a guid, so treat it as a title - a hand written template routinely does.
            await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title)).ConfigureAwait(false);

            CoreList list = context.Web.Lists.AsRequested()
                .FirstOrDefault(l => string.Equals(l.Title, value, StringComparison.OrdinalIgnoreCase));

            if (list != null)
            {
                return list.Id;
            }

            string warning = $"The workflow subscription '{subscription.Name}' names the list '{value}', " +
                "which does not exist on this site, so it was bound at site scope instead.";
            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
            WriteMessage(warning, ProvisioningMessageType.Warning);

            return null;
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            List<WorkflowDefinitionInfo> definitions;
            try
            {
                definitions = await CsomRequestSender.SendAsync(context,
                    new GetWorkflowDefinitionsRequest(siteId, webId)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Retired workflow services. Not worth a warning on every extract of every site.
                context.Logger?.LogDebug(ex, "{Source}: the workflow services are unavailable on this tenant.",
                    Constants.LOGGING_SOURCE);
                return template;
            }

            if (definitions == null || definitions.Count == 0)
            {
                return template;
            }

            ProvisioningTemplateCreationInformation creationInfo =
                configuration?.ToCreationInformation() ?? new ProvisioningTemplateCreationInformation();

            // ProvisioningTemplate.Workflows is null until something assigns it - unlike most of the
            // template's collections, which self-initialise. Created only now that there is
            // something to put in it, so a site with no workflows produces no element.
            template.Workflows ??= new Model.Workflows();

            foreach (WorkflowDefinitionInfo definition in definitions)
            {
                template.Workflows.WorkflowDefinitions.Add(new WorkflowDefinitionModel
                {
                    Id = definition.Id,
                    DisplayName = definition.DisplayName,
                    Description = definition.Description,
                    Published = definition.Published,
                    RestrictToScope = definition.RestrictToScope,
                    RestrictToType = definition.RestrictToType,

                    // The XAML is written into the template's files, not inline, so the path is what
                    // the element carries. Persisting the bytes needs a connector, which is the
                    // caller's to supply.
                    XamlPath = $"{definition.Id}.xaml",
                });

                PersistXaml(context, creationInfo, definition);
            }

            try
            {
                List<WorkflowSubscriptionInfo> subscriptions = await CsomRequestSender.SendAsync(context,
                    new GetWorkflowSubscriptionsRequest(siteId, webId)).ConfigureAwait(false);

                template.Workflows ??= new Model.Workflows();

                foreach (WorkflowSubscriptionInfo subscription in subscriptions ?? new List<WorkflowSubscriptionInfo>())
                {
                    var model = new WorkflowSubscriptionModel
                    {
                        DefinitionId = subscription.DefinitionId,
                        Name = subscription.Name,
                        Enabled = subscription.Enabled,
                        StatusFieldName = subscription.StatusFieldName,
                        ListId = subscription.EventSourceId != Guid.Empty
                            ? subscription.EventSourceId.ToString()
                            : null,
                    };

                    model.EventTypes.AddRange(subscription.EventTypes);

                    foreach (KeyValuePair<string, string> property in subscription.PropertyDefinitions)
                    {
                        model.PropertyDefinitions[property.Key] = property.Value;
                    }

                    template.Workflows.WorkflowSubscriptions.Add(model);
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the workflow subscriptions could not be read.",
                    Constants.LOGGING_SOURCE);
            }

            return template;
        }

        /// <summary>
        /// Writes a definition's XAML into the template's file connector, when one was supplied.
        /// </summary>
        private void PersistXaml(PnPContext context, ProvisioningTemplateCreationInformation creationInfo,
            WorkflowDefinitionInfo definition)
        {
            if (creationInfo.FileConnector == null || string.IsNullOrEmpty(definition.Xaml))
            {
                // Without a connector there is nowhere to put it. The element still names the file,
                // so the template says what it needs - it just does not carry it.
                return;
            }

            try
            {
                using (var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(definition.Xaml)))
                {
                    creationInfo.FileConnector.SaveFileStream($"{definition.Id}.xaml", stream);
                }
            }
            catch (Exception ex)
            {
                string warning = $"The XAML of workflow '{definition.DisplayName}' could not be saved: " +
                    ErrorText.Describe(ex);
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        #endregion

        /// <summary>
        /// Whether the tenant still has SharePoint 2013 workflow services.
        /// </summary>
        private async Task<bool> IsWorkflowServiceAvailableAsync(PnPContext context, Guid siteId, Guid webId)
        {
            try
            {
                await CsomRequestSender.SendAsync(context,
                    new GetWorkflowDefinitionsRequest(siteId, webId)).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                string warning = "SharePoint 2013 workflow services are not available on this tenant, so the " +
                    "template's workflows were skipped. The platform is retired; consider Power Automate instead.";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return false;
            }
        }
    }
}
