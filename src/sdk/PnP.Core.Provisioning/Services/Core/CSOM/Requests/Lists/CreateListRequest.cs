using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Lists
{
    // This namespace sits under ...Provisioning.Services.Core.CSOM, so the bare name QueryAction
    // resolves to that enclosing namespace rather than to PnP Core's action type. Enclosing
    // namespaces beat using directives, so the alias has to live inside the namespace body.
    using CsomQueryAction = PnP.Core.Services.Core.CSOM.QueryAction.QueryAction;

    /// <summary>
    /// Creates a list with the url, description and template a provisioning template specifies.
    /// </summary>
    internal sealed class CreateListRequest : IRequest<CreatedListInfo>
    {
        private readonly string webIdentity;
        private readonly string title;
        private readonly string url;
        private readonly string description;
        private readonly int templateType;
        private readonly Guid templateFeatureId;
        private readonly bool onQuickLaunch;

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="siteId">The site collection id</param>
        /// <param name="webId">The web id</param>
        /// <param name="title">The list's display title</param>
        /// <param name="url">The list's web-relative url, for example <c>Lists/MyList</c></param>
        /// <param name="description">The list's description, or null</param>
        /// <param name="templateType">The list template type, for example 100 for a generic list</param>
        /// <param name="templateFeatureId">The feature that supplies the template, or <see cref="Guid.Empty"/></param>
        /// <param name="onQuickLaunch">Whether the list appears in the quick launch</param>
        internal CreateListRequest(Guid siteId, Guid webId, string title, string url, string description,
            int templateType, Guid templateFeatureId, bool onQuickLaunch)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("A list title is required.", nameof(title));
            }

            webIdentity = CsomIdentity.Web(siteId, webId);
            this.title = title;
            this.url = url;
            this.description = description;
            this.templateType = templateType;
            this.templateFeatureId = templateFeatureId;
            this.onQuickLaunch = onQuickLaunch;
        }

        /// <summary>
        /// The list that was created.
        /// </summary>
        public CreatedListInfo Result { get; private set; }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        private int listIdQueryId;

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int webPathId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new Identity { Id = webPathId, Name = webIdentity },
            });

            int listsPathId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new Property { Id = listsPathId, ParentId = webPathId, Name = "Lists" },
            });

            int addPathId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new ObjectPathMethod
                {
                    Id = addPathId,
                    ParentId = listsPathId,
                    Name = "Add",
                    Parameters = new MethodParameter
                    {
                        Properties = new List<Parameter> { new ListCreationParameter(this) },
                    },
                },
            });

            // Read the new list's id back, so the caller does not have to find it by title - two
            // lists can legitimately share a title while their urls differ.
            listIdQueryId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                Action = new CsomQueryAction
                {
                    Id = listIdQueryId,
                    ObjectPathId = addPathId.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property> { new Property { Name = "Id" } },
                    },
                },
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            using (JsonDocument document = JsonDocument.Parse(response))
            {
                foreach (JsonElement element in document.RootElement.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty("Id", out JsonElement id))
                    {
                        continue;
                    }

                    // A CSOM guid comes back prefixed, e.g. "/Guid(0e0e...)/".
                    string raw = id.GetString();
                    if (Guid.TryParse(raw?.Replace("/Guid(", "").Replace(")/", ""), out Guid listId))
                    {
                        Result = new CreatedListInfo { Id = listId };
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Serializes <c>SP.ListCreationInformation</c>.
        /// </summary>
        private sealed class ListCreationParameter : Parameter
        {
            private readonly CreateListRequest request;

            internal ListCreationParameter(CreateListRequest request)
            {
                this.request = request;
                TypeId = CsomTypeIds.ListCreationInformation;
            }

            internal override string SerializeParameter()
            {
                var properties = new List<NamedProperty>
                {
                    new NamedProperty { Name = "Title", Type = "String", Value = request.title },
                    new NamedProperty { Name = "Description", Type = "String", Value = request.description ?? string.Empty },
                    new NamedProperty { Name = "TemplateType", Type = "Int32", Value = request.templateType.ToString(CultureInfo.InvariantCulture) },

                    // 1 = On, 0 = Off. The created list's OnQuickLaunch is set again afterwards,
                    // because this option alone does not stick on every template type.
                    new NamedProperty { Name = "QuickLaunchOption", Type = "Enum", Value = request.onQuickLaunch ? "1" : "0" },
                };

                if (!string.IsNullOrEmpty(request.url))
                {
                    properties.Add(new NamedProperty { Name = "Url", Type = "String", Value = request.url });
                }

                if (request.templateFeatureId != Guid.Empty)
                {
                    properties.Add(new NamedProperty { Name = "TemplateFeatureId", Type = "Guid", Value = request.templateFeatureId.ToString() });
                }

                return $"<{ParameterTagName} TypeId=\"{TypeId}\">{string.Concat(properties)}</{ParameterTagName}>";
            }
        }
    }
}
