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
    /// Counts the running instances of a workflow association.
    /// </summary>
    internal sealed class EnumerateWorkflowInstancesRequest : WorkflowRequestBase, IRequest<WorkflowInstanceCountInfo>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly Guid subscriptionId;

        internal EnumerateWorkflowInstancesRequest(Guid siteId, Guid webId, Guid subscriptionId)
        {
            this.siteId = siteId;
            this.webId = webId;
            this.subscriptionId = subscriptionId;
        }

        public WorkflowInstanceCountInfo Result { get; private set; }

        internal int CountId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWorkflowServicesManager(idProvider, paths, siteId, webId);
            int subscriptionServiceId = EmitSubscriptionService(idProvider, paths, managerId);
            int instanceServiceId = EmitInstanceService(idProvider, paths, managerId);

            int subscriptionPathId = EmitMethod(idProvider, paths, subscriptionServiceId, "GetSubscription",
                new Parameter { Type = "Guid", Value = subscriptionId });

            CountId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = CountId,
                    ObjectPathId = instanceServiceId.ToString(),
                    Name = "CountInstances",
                    Parameters = new List<Parameter>
                    {
                        new ObjectReferenceParameter { ObjectPathId = subscriptionPathId }
                    }
                }
            });

            return paths;
        }

        public void ProcessResponse(string response)
        {
            JsonElement count = ResponseHelper.ProcessResponse<JsonElement>(response, CountId);

            if (count.ValueKind == JsonValueKind.Number)
            {
                Result = new WorkflowInstanceCountInfo { Count = count.GetInt32() };
            }
        }
    }
}
