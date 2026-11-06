using PnP.Core.Provisioning.Model;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Workflows
{    /// <summary>
    /// Lists the workflow definitions published on a site.
    /// </summary>
    internal sealed class GetWorkflowDefinitionsRequest : WorkflowRequestBase, IRequest<List<WorkflowDefinitionInfo>>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly bool publishedOnly;

        internal GetWorkflowDefinitionsRequest(Guid siteId, Guid webId, bool publishedOnly = false)
        {
            this.siteId = siteId;
            this.webId = webId;
            this.publishedOnly = publishedOnly;
        }

        public List<WorkflowDefinitionInfo> Result { get; private set; } = new List<WorkflowDefinitionInfo>();

        internal int DefinitionsQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWorkflowServicesManager(idProvider, paths, siteId, webId);
            int deploymentId = EmitDeploymentService(idProvider, paths, managerId);

            int definitionsId = EmitMethod(idProvider, paths, deploymentId, "EnumerateDefinitions",
                new Parameter { Type = "Boolean", Value = publishedOnly });

            DefinitionsQueryId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = DefinitionsQueryId,
                    ObjectPathId = definitionsId.ToString(),
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
                            new Property { Name = "DisplayName" },
                            new Property { Name = "Description" },
                            new Property { Name = "Published" },
                            new Property { Name = "RestrictToScope" },
                            new Property { Name = "RestrictToType" }
                        }
                    }
                }
            });

            return paths;
        }

        public void ProcessResponse(string response)
        {
            JsonElement definitions = ResponseHelper.ProcessResponse<JsonElement>(response, DefinitionsQueryId);

            if (definitions.ValueKind != JsonValueKind.Object
                || !definitions.TryGetProperty("_Child_Items_", out JsonElement items)
                || items.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var parsed = new List<WorkflowDefinitionInfo>();
            foreach (JsonElement item in items.EnumerateArray())
            {
                parsed.Add(new WorkflowDefinitionInfo
                {
                    Id = WorkflowJson.ReadGuid(item, "Id"),
                    DisplayName = WorkflowJson.ReadString(item, "DisplayName"),
                    Description = WorkflowJson.ReadString(item, "Description"),
                    Published = item.TryGetProperty("Published", out JsonElement p) && p.ValueKind == JsonValueKind.True,
                    RestrictToScope = WorkflowJson.ReadString(item, "RestrictToScope"),
                    RestrictToType = WorkflowJson.ReadString(item, "RestrictToType")
                });
            }

            Result = parsed;
        }
    }
}
