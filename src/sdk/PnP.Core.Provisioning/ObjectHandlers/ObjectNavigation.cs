using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CoreNavigationNode = PnP.Core.Model.SharePoint.INavigationNode;
using NavigationModel = PnP.Core.Provisioning.Model.Navigation;
using NavigationNodeModel = PnP.Core.Provisioning.Model.NavigationNode;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies and reads back the <c>&lt;pnp:Navigation&gt;</c> element - the site's global (top)
    /// and current (quick launch) navigation.
    /// </summary>
    internal class ObjectNavigation : ObjectHandlerBase
    {
        public override string Name => "Navigation";

        public override string InternalName => "Navigation";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.Navigation != null;
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
            NavigationModel navigation = template.Navigation;

            if (navigation == null)
            {
                return parser;
            }

            IWeb web = context.Web;
            await web.LoadAsync(w => w.Navigation.QueryProperties(
                n => n.QuickLaunch, n => n.TopNavigationBar)).ConfigureAwait(false);

            if (navigation.GlobalNavigation != null)
            {
                await ApplyKindAsync(context, navigation.GlobalNavigation.NavigationType.ToString(),
                    navigation.GlobalNavigation.StructuralNavigation,
                    navigation.GlobalNavigation.ManagedNavigation,
                    web.Navigation.TopNavigationBar, "global", parser).ConfigureAwait(false);
            }

            if (navigation.CurrentNavigation != null)
            {
                await ApplyKindAsync(context, navigation.CurrentNavigation.NavigationType.ToString(),
                    navigation.CurrentNavigation.StructuralNavigation,
                    navigation.CurrentNavigation.ManagedNavigation,
                    web.Navigation.QuickLaunch, "current", parser).ConfigureAwait(false);
            }

            if (navigation.SearchNavigation != null)
            {
                // Search navigation is a third node collection that PnP Core does not model at all -
                // it is only reachable through the publishing navigation API.
                string warning = "The template configures search navigation, which this engine cannot write. " +
                    "It was skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }

            WriteMessage("Done processing navigation", ProvisioningMessageType.Completed);

            return parser;
        }

        /// <summary>
        /// Applies one navigation kind, honouring what the template asked for.
        /// </summary>
        private async Task ApplyKindAsync(PnPContext context, string navigationType,
            StructuralNavigation structural, ManagedNavigation managed,
            INavigationNodeCollection nodes, string what, TokenParser parser)
        {
            if (string.Equals(navigationType, "Inherit", StringComparison.OrdinalIgnoreCase))
            {
                // Inheriting is a publishing navigation setting, not a node operation - there is no
                // PnP Core surface for it, and clearing the nodes instead would be a different thing
                // that happens to look similar.
                string warning = $"The template sets {what} navigation to Inherit, which this engine cannot write " +
                    "(it is a publishing navigation setting). It was skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            if (string.Equals(navigationType, "Managed", StringComparison.OrdinalIgnoreCase))
            {
                string termSet = managed?.TermSetId ?? "(unspecified)";

                // Backlog T9. Named, so a template author can see exactly what did not happen.
                string warning = $"The template sets {what} navigation to managed metadata (term set {termSet}). " +
                    "That is configured through the publishing navigation API, which has no REST, Graph or " +
                    "PnP Core equivalent, so it was skipped - the site keeps its current navigation.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            if (structural == null)
            {
                return;
            }

            try
            {
                if (structural.RemoveExistingNodes)
                {
                    // Cleared first, deliberately: without this a re-apply appends and the
                    // navigation grows on every run.
                    await nodes.DeleteAllNodesAsync().ConfigureAwait(false);
                }

                await AddNodesAsync(context, nodes, structural.NavigationNodes, null, parser).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The {what} navigation could not be applied: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        /// <summary>
        /// Adds a level of nodes and recurses into their children.
        /// </summary>
        private async Task AddNodesAsync(PnPContext context, INavigationNodeCollection nodes,
            IEnumerable<NavigationNodeModel> modelNodes, CoreNavigationNode parent, TokenParser parser)
        {
            foreach (NavigationNodeModel modelNode in modelNodes)
            {
                string title = parser.ParseString(modelNode.Title);

                try
                {
                    CoreNavigationNode created = await nodes.AddAsync(new NavigationNodeOptions
                    {
                        Title = title,
                        Url = parser.ParseString(modelNode.Url),
                        ParentNode = parent,
                    }).ConfigureAwait(false);

                    if (modelNode.NavigationNodes.Any())
                    {
                        await AddNodesAsync(context, nodes, modelNode.NavigationNodes, created, parser).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    string warning = $"The navigation node '{title}' could not be added: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            try
            {
                IWeb web = context.Web;

                // ServerRelativeUrl comes along because every node's url is tokenized against it.
                // Read lazily it throws on a context whose first operation is an extract - the same
                // way the client side page extract did.
                await web.LoadAsync(w => w.ServerRelativeUrl, w => w.Navigation.QueryProperties(
                    n => n.QuickLaunch, n => n.TopNavigationBar)).ConfigureAwait(false);

                template.Navigation = new NavigationModel(
                    new GlobalNavigation(GlobalNavigationType.Structural,
                        await ReadStructuralAsync(context, web.Navigation.TopNavigationBar).ConfigureAwait(false), null),
                    new CurrentNavigation(CurrentNavigationType.Structural,
                        await ReadStructuralAsync(context, web.Navigation.QuickLaunch).ConfigureAwait(false), null),
                    null);
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the site's navigation could not be read.",
                    Constants.LOGGING_SOURCE);
            }

            return template;
        }

        /// <summary>
        /// Reads one node collection into the template's shape.
        /// </summary>
        private async Task<StructuralNavigation> ReadStructuralAsync(PnPContext context, INavigationNodeCollection nodes)
        {
            var structural = new StructuralNavigation { RemoveExistingNodes = true };

            foreach (CoreNavigationNode node in nodes.AsRequested())
            {
                structural.NavigationNodes.Add(await ReadNodeAsync(context, node.Id, node.Title, node.Url, node.IsExternal)
                    .ConfigureAwait(false));
            }

            return structural;
        }

        /// <summary>
        /// Reads one navigation node and its children.
        /// </summary>
        private async Task<NavigationNodeModel> ReadNodeAsync(PnPContext context, int id, string title, string url, bool isExternal)
        {
            var model = new NavigationNodeModel
            {
                Title = title,
                Url = Tokenize(url, context.Web.ServerRelativeUrl, context.Web),
                IsExternal = isExternal,
            };

            foreach (ChildNode child in await ReadChildrenAsync(context, id).ConfigureAwait(false))
            {
                model.NavigationNodes.Add(await ReadNodeAsync(context, child.Id, child.Title, child.Url, child.IsExternal)
                    .ConfigureAwait(false));
            }

            return model;
        }

        /// <summary>
        /// A node's children, read over REST.
        /// </summary>
        private async Task<List<ChildNode>> ReadChildrenAsync(PnPContext context, int nodeId)
        {
            var children = new List<ChildNode>();

            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.SPORest,
                    $"_api/web/navigation/GetNodeById({nodeId})/Children")).ConfigureAwait(false);

                if (string.IsNullOrEmpty(response.Response))
                {
                    return children;
                }

                using (JsonDocument document = JsonDocument.Parse(response.Response))
                {
                    if (!TryGetResults(document.RootElement, out JsonElement results))
                    {
                        return children;
                    }

                    foreach (JsonElement element in results.EnumerateArray())
                    {
                        children.Add(new ChildNode
                        {
                            Id = element.TryGetProperty("Id", out JsonElement id) ? id.GetInt32() : 0,
                            Title = element.TryGetProperty("Title", out JsonElement t) ? t.GetString() : null,
                            Url = element.TryGetProperty("Url", out JsonElement u) ? u.GetString() : null,
                            IsExternal = element.TryGetProperty("IsExternal", out JsonElement e) && e.GetBoolean(),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the children of navigation node {NodeId} could not be read.",
                    Constants.LOGGING_SOURCE, nodeId);
            }

            return children;
        }

        /// <summary>
        /// Finds the array in either the verbose (<c>d.results</c>) or minimal (<c>value</c>) shape.
        /// </summary>
        private static bool TryGetResults(JsonElement root, out JsonElement results)
        {
            if (root.TryGetProperty("value", out results) && results.ValueKind == JsonValueKind.Array)
            {
                return true;
            }

            if (root.TryGetProperty("d", out JsonElement d)
                && d.TryGetProperty("results", out results)
                && results.ValueKind == JsonValueKind.Array)
            {
                return true;
            }

            results = default;
            return false;
        }

        private sealed class ChildNode
        {
            internal int Id { get; set; }

            internal string Title { get; set; }

            internal string Url { get; set; }

            internal bool IsExternal { get; set; }
        }

        #endregion
    }
}
