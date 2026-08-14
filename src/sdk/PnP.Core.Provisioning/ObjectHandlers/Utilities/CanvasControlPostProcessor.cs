using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CanvasControlModel = PnP.Core.Provisioning.Model.CanvasControl;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Rewrites a canvas control's stored JSON so it points at artefacts on the <em>target</em>
    /// site rather than the one the template came from.
    /// </summary>
    internal static class CanvasControlPostProcessor
    {
        private const string SelectedListIdProperty = "selectedListId";
        private const string SelectedListUrlProperty = "selectedListUrl";
        private const string ListTitleProperty = "listTitle";

        /// <summary>
        /// Post-processes a control's JSON, in place.
        /// </summary>
        internal static async Task ProcessAsync(PnPContext context, CanvasControlModel control)
        {
            if (control.Type != WebPartType.List || string.IsNullOrWhiteSpace(control.JsonControlData))
            {
                return;
            }

            JsonObject properties;
            try
            {
                properties = JsonNode.Parse(control.JsonControlData) as JsonObject;
            }
            catch (JsonException ex)
            {
                context.Logger?.LogWarning(ex, "{Source}: the List web part's control data is not valid JSON and was left as it is.",
                    Constants.LOGGING_SOURCE);
                return;
            }

            if (properties == null)
            {
                return;
            }

            IList list = await ResolveListAsync(context, properties).ConfigureAwait(false);
            if (list == null)
            {
                context.Logger?.LogWarning("{Source}: the list a List web part points at was not found on this site - the web part will render empty.",
                    Constants.LOGGING_SOURCE);
                return;
            }

            await list.LoadAsync(l => l.Id, l => l.RootFolder).ConfigureAwait(false);
            await list.RootFolder.LoadAsync(f => f.ServerRelativeUrl).ConfigureAwait(false);

            properties[SelectedListIdProperty] = list.Id.ToString();
            properties[SelectedListUrlProperty] = list.RootFolder.ServerRelativeUrl;

            control.JsonControlData = properties.ToJsonString();
        }

        /// <summary>
        /// Finds the list a List web part refers to, trying url, then id, then title.
        /// </summary>
        private static async Task<IList> ResolveListAsync(PnPContext context, JsonObject properties)
        {
            string listUrl = GetString(properties, SelectedListUrlProperty);
            if (!string.IsNullOrWhiteSpace(listUrl))
            {
                IList byUrl = await GetListByUrlAsync(context, listUrl).ConfigureAwait(false);
                if (byUrl != null)
                {
                    return byUrl;
                }
            }

            string listId = GetString(properties, SelectedListIdProperty);
            if (Guid.TryParse(listId, out Guid id) && id != Guid.Empty)
            {
                try
                {
                    return await context.Web.Lists.GetByIdAsync(id).ConfigureAwait(false);
                }
                catch (Exception)
                {
                }
            }

            string listTitle = GetString(properties, ListTitleProperty);
            if (!string.IsNullOrWhiteSpace(listTitle))
            {
                await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title)).ConfigureAwait(false);

                return context.Web.Lists.AsRequested()
                    .FirstOrDefault(l => string.Equals(l.Title, listTitle, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        private static async Task<IList> GetListByUrlAsync(PnPContext context, string listUrl)
        {
            try
            {
                string serverRelativeUrl = listUrl.StartsWith("/", StringComparison.Ordinal)
                    ? listUrl
                    : $"{context.Web.ServerRelativeUrl.TrimEnd('/')}/{listUrl.TrimStart('/')}";

                return await context.Web.Lists.GetByServerRelativeUrlAsync(serverRelativeUrl).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetString(JsonObject properties, string name)
        {
            return properties.TryGetPropertyValue(name, out JsonNode node) && node != null
                ? node.ToString()
                : null;
        }
    }
}
