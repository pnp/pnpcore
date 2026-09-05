using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A SharePoint 2013 workflow definition.
    /// </summary>
    internal sealed class WorkflowDefinitionInfo
    {
        internal Guid Id { get; set; }

        internal string DisplayName { get; set; }

        internal string Description { get; set; }

        /// <summary>
        /// The workflow's XAML. Required to save a definition; not returned when enumerating.
        /// </summary>
        internal string Xaml { get; set; }

        /// <summary>
        /// Whether the definition has been published. An unpublished definition cannot be
        /// associated with a list.
        /// </summary>
        internal bool Published { get; set; }

        /// <summary>
        /// The list id a list-scoped workflow is restricted to, as a string.
        /// </summary>
        internal string RestrictToScope { get; set; }

        /// <summary>
        /// <c>List</c>, <c>Site</c> or <c>Universal</c>.
        /// </summary>
        internal string RestrictToType { get; set; }
    }

    /// <summary>
    /// A SharePoint 2013 workflow association - a definition bound to a list or site.
    /// </summary>
    internal sealed class WorkflowSubscriptionInfo
    {
        internal Guid Id { get; set; }

        internal Guid DefinitionId { get; set; }

        internal string Name { get; set; }

        internal bool Enabled { get; set; }

        /// <summary>
        /// The list the workflow watches. <see cref="Guid.Empty"/> for a site workflow.
        /// </summary>
        internal Guid EventSourceId { get; set; }

        /// <summary>
        /// <c>ItemAdded</c>, <c>ItemUpdated</c>, <c>WorkflowStart</c>, ...
        /// </summary>
        internal List<string> EventTypes { get; set; } = new List<string>();

        /// <summary>
        /// The list field the workflow writes its status into.
        /// </summary>
        internal string StatusFieldName { get; set; }

        /// <summary>
        /// Association-time property values.
        /// </summary>
        internal Dictionary<string, string> PropertyDefinitions { get; set; } = new Dictionary<string, string>();
    }
}

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Workflows
{
    /// <summary>
    /// Reads values out of a workflow CSOM response.
    /// </summary>
    internal static class WorkflowJson
    {
        /// <summary>
        /// Reads a GUID property, unwrapping CSOM's <c>/Guid(...)/</c> encoding.
        /// </summary>
        internal static Guid ReadGuid(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property))
            {
                return Guid.Empty;
            }

            string raw = property.GetString();
            if (string.IsNullOrEmpty(raw))
            {
                return Guid.Empty;
            }

            raw = raw.Replace("/Guid(", "").Replace(")/", "");

            return Guid.TryParse(raw, out Guid value) ? value : Guid.Empty;
        }

        /// <summary>
        /// Reads a string property, tolerating its absence.
        /// </summary>
        internal static string ReadString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        }
    }
}