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
    /// Imports a web part from its XML and adds it to a zone on a page.
    /// </summary>
    internal sealed class AddWebPartRequest : WebPartRequestBase, IRequest<WebPartDefinitionInfo>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly string serverRelativeFileUrl;
        private readonly string webPartXml;
        private readonly string zoneId;
        private readonly int zoneIndex;

        internal AddWebPartRequest(Guid siteId, Guid webId, string serverRelativeFileUrl, string webPartXml, string zoneId, int zoneIndex)
        {
            if (string.IsNullOrEmpty(serverRelativeFileUrl))
            {
                throw new ArgumentException("A server relative file url is required.", nameof(serverRelativeFileUrl));
            }

            if (string.IsNullOrEmpty(webPartXml))
            {
                throw new ArgumentException("Web part XML is required.", nameof(webPartXml));
            }

            this.siteId = siteId;
            this.webId = webId;
            this.serverRelativeFileUrl = serverRelativeFileUrl;
            this.webPartXml = webPartXml;
            this.zoneId = zoneId ?? string.Empty;
            this.zoneIndex = zoneIndex;
        }

        public WebPartDefinitionInfo Result { get; private set; }

        internal int AddedWebPartQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWebPartManager(idProvider, paths, siteId, webId, serverRelativeFileUrl);

            int importedId = EmitMethod(idProvider, paths, managerId, "ImportWebPart",
                new Parameter { Type = "String", Value = webPartXml });

            // AddWebPart takes the WebPart, not the WebPartDefinition that ImportWebPart returned.
            int importedWebPartId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                ObjectPath = new Property
                {
                    Id = importedWebPartId,
                    ParentId = importedId,
                    Name = "WebPart"
                }
            });

            int addedId = EmitMethod(idProvider, paths, managerId, "AddWebPart",
                new ObjectReferenceParameter { ObjectPathId = importedWebPartId },
                new Parameter { Type = "String", Value = zoneId },
                new Parameter { Type = "Number", Value = zoneIndex });

            AddedWebPartQueryId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = AddedWebPartQueryId,
                    ObjectPathId = addedId.ToString(),
                    SelectQuery = new SelectQuery
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
            JsonElement definition = ResponseHelper.ProcessResponse<JsonElement>(response, AddedWebPartQueryId);
            if (definition.ValueKind != JsonValueKind.Object) return;

            Result = new WebPartDefinitionInfo
            {
                Id = WebPartJson.ReadGuid(definition, "Id"),
                ZoneId = definition.TryGetProperty("ZoneId", out JsonElement zone) ? zone.GetString() : null
            };
        }
    }
}
