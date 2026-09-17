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
    /// Lists workflow associations, optionally narrowed to one list.
    /// </summary>
    internal sealed class GetWorkflowSubscriptionsRequest : WorkflowRequestBase, IRequest<List<WorkflowSubscriptionInfo>>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly Guid? listId;

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="siteId">Site collection id</param>
        /// <param name="webId">Web id</param>
        /// <param name="listId">Narrow to this list, or <c>null</c> for every subscription on the site</param>
        internal GetWorkflowSubscriptionsRequest(Guid siteId, Guid webId, Guid? listId = null)
        {
            this.siteId = siteId;
            this.webId = webId;
            this.listId = listId;
        }

        public List<WorkflowSubscriptionInfo> Result { get; private set; } = new List<WorkflowSubscriptionInfo>();

        internal int SubscriptionsQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWorkflowServicesManager(idProvider, paths, siteId, webId);
            int subscriptionServiceId = EmitSubscriptionService(idProvider, paths, managerId);

            int subscriptionsId = listId.HasValue
                ? EmitMethod(idProvider, paths, subscriptionServiceId, "EnumerateSubscriptionsByList",
                    new Parameter { Type = "Guid", Value = listId.Value })
                : EmitMethod(idProvider, paths, subscriptionServiceId, "EnumerateSubscriptions");

            SubscriptionsQueryId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = SubscriptionsQueryId,
                    ObjectPathId = subscriptionsId.ToString(),
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
                            new Property { Name = "DefinitionId" },
                            new Property { Name = "Name" },
                            new Property { Name = "Enabled" },
                            new Property { Name = "EventSourceId" },
                            new Property { Name = "StatusFieldName" }
                        }
                    }
                }
            });

            return paths;
        }

        public void ProcessResponse(string response)
        {
            JsonElement subscriptions = ResponseHelper.ProcessResponse<JsonElement>(response, SubscriptionsQueryId);

            if (subscriptions.ValueKind != JsonValueKind.Object
                || !subscriptions.TryGetProperty("_Child_Items_", out JsonElement items)
                || items.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var parsed = new List<WorkflowSubscriptionInfo>();
            foreach (JsonElement item in items.EnumerateArray())
            {
                parsed.Add(new WorkflowSubscriptionInfo
                {
                    Id = WorkflowJson.ReadGuid(item, "Id"),
                    DefinitionId = WorkflowJson.ReadGuid(item, "DefinitionId"),
                    Name = WorkflowJson.ReadString(item, "Name"),
                    Enabled = item.TryGetProperty("Enabled", out JsonElement e) && e.ValueKind == JsonValueKind.True,
                    EventSourceId = WorkflowJson.ReadGuid(item, "EventSourceId"),
                    StatusFieldName = WorkflowJson.ReadString(item, "StatusFieldName")
                });
            }

            Result = parsed;
        }
    }
}
