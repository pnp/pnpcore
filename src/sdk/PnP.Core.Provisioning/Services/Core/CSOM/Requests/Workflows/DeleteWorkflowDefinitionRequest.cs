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
    /// Deletes a workflow definition.
    /// </summary>
    internal sealed class DeleteWorkflowDefinitionRequest : WorkflowRequestBase, IRequest<object>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly Guid definitionId;

        internal DeleteWorkflowDefinitionRequest(Guid siteId, Guid webId, Guid definitionId)
        {
            this.siteId = siteId;
            this.webId = webId;
            this.definitionId = definitionId;
        }

        public object Result { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWorkflowServicesManager(idProvider, paths, siteId, webId);
            int deploymentId = EmitDeploymentService(idProvider, paths, managerId);

            paths.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = deploymentId.ToString(),
                    Name = "DeleteDefinition",
                    Parameters = new List<Parameter>
                    {
                        new Parameter { Type = "Guid", Value = definitionId }
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
