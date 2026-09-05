using PnP.Core.Provisioning.Model;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.WebParts
{    /// <summary>
    /// Lists the web parts on a page.
    /// </summary>
    internal sealed class GetWebPartDefinitionsRequest : WebPartRequestBase, IRequest<List<WebPartDefinitionInfo>>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly string serverRelativeFileUrl;

        internal GetWebPartDefinitionsRequest(Guid siteId, Guid webId, string serverRelativeFileUrl)
        {
            if (string.IsNullOrEmpty(serverRelativeFileUrl))
            {
                throw new ArgumentException("A server relative file url is required.", nameof(serverRelativeFileUrl));
            }

            this.siteId = siteId;
            this.webId = webId;
            this.serverRelativeFileUrl = serverRelativeFileUrl;
        }

        public List<WebPartDefinitionInfo> Result { get; private set; } = new List<WebPartDefinitionInfo>();

        internal int WebPartsQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWebPartManager(idProvider, paths, siteId, webId, serverRelativeFileUrl);

            int webPartsId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = webPartsId.ToString()
                },
                ObjectPath = new Property
                {
                    Id = webPartsId,
                    ParentId = managerId,
                    Name = "WebParts"
                }
            });

            WebPartsQueryId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = WebPartsQueryId,
                    ObjectPathId = webPartsId.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property>()
                    },
                    ChildItemQuery = new ChildItemQuery
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property>
                        {
                            new Property { Name = "Id" },
                            new Property { Name = "ZoneId" }
                        }
                    }
                }
            });

            return paths;
        }

        public void ProcessResponse(string response)
        {
            JsonElement webParts = ResponseHelper.ProcessResponse<JsonElement>(response, WebPartsQueryId);

            if (webParts.ValueKind != JsonValueKind.Object
                || !webParts.TryGetProperty("_Child_Items_", out JsonElement items)
                || items.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var parsed = new List<WebPartDefinitionInfo>();
            foreach (JsonElement item in items.EnumerateArray())
            {
                parsed.Add(new WebPartDefinitionInfo
                {
                    Id = WebPartJson.ReadGuid(item, "Id"),
                    ZoneId = item.TryGetProperty("ZoneId", out JsonElement zone) ? zone.GetString() : null
                });
            }

            Result = parsed;
        }
    }
}
