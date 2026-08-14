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
    /// Removes a workflow association.
    /// </summary>
    internal sealed class DeleteWorkflowSubscriptionRequest : WorkflowRequestBase, IRequest<object>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly Guid subscriptionId;

        internal DeleteWorkflowSubscriptionRequest(Guid siteId, Guid webId, Guid subscriptionId)
        {
            this.siteId = siteId;
            this.webId = webId;
            this.subscriptionId = subscriptionId;
        }

        public object Result { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWorkflowServicesManager(idProvider, paths, siteId, webId);
            int subscriptionServiceId = EmitSubscriptionService(idProvider, paths, managerId);

            paths.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = subscriptionServiceId.ToString(),
                    Name = "DeleteSubscription",
                    Parameters = new List<Parameter>
                    {
                        new Parameter { Type = "Guid", Value = subscriptionId }
                    }
                }
            });

            return paths;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
