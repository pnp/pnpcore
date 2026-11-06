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
    /// Creates or updates a workflow definition from its XAML.
    /// </summary>
    internal sealed class SaveWorkflowDefinitionRequest : WorkflowRequestBase, IRequest<WorkflowDefinitionInfo>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly WorkflowDefinitionInfo definition;

        internal SaveWorkflowDefinitionRequest(Guid siteId, Guid webId, WorkflowDefinitionInfo definition)
        {
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));

            if (string.IsNullOrEmpty(definition.Xaml))
            {
                throw new ArgumentException("A workflow definition needs its XAML.", nameof(definition));
            }

            this.siteId = siteId;
            this.webId = webId;
        }

        public WorkflowDefinitionInfo Result { get; private set; }

        internal int SaveDefinitionId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWorkflowServicesManager(idProvider, paths, siteId, webId);
            int deploymentId = EmitDeploymentService(idProvider, paths, managerId);

            var definitionPath = new ConstructorPath
            {
                Id = idProvider.GetActionId(),
                TypeId = CsomTypeIds.WorkflowDefinition,
                Parameters = new MethodParameter { Properties = new List<Parameter>() }
            };

            paths.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = definitionPath.Id.ToString()
                },
                ObjectPath = definitionPath
            });

            paths.Add(SetProperty(idProvider, definitionPath.Id, "Xaml", "String", definition.Xaml));

            if (definition.DisplayName != null)
            {
                paths.Add(SetProperty(idProvider, definitionPath.Id, "DisplayName", "String", definition.DisplayName));
            }

            if (definition.Description != null)
            {
                paths.Add(SetProperty(idProvider, definitionPath.Id, "Description", "String", definition.Description));
            }

            if (definition.Id != Guid.Empty)
            {
                paths.Add(SetProperty(idProvider, definitionPath.Id, "Id", "Guid", definition.Id));
            }

            if (definition.RestrictToScope != null)
            {
                paths.Add(SetProperty(idProvider, definitionPath.Id, "RestrictToScope", "String", definition.RestrictToScope));
            }

            if (definition.RestrictToType != null)
            {
                paths.Add(SetProperty(idProvider, definitionPath.Id, "RestrictToType", "String", definition.RestrictToType));
            }

            SaveDefinitionId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = SaveDefinitionId,
                    ObjectPathId = deploymentId.ToString(),
                    Name = "SaveDefinition",
                    Parameters = new List<Parameter>
                    {
                        new ObjectReferenceParameter { ObjectPathId = definitionPath.Id }
                    }
                }
            });

            return paths;
        }

        private static ActionObjectPath SetProperty(IIdProvider idProvider, int objectPathId, string name, string type, object value)
        {
            return new ActionObjectPath
            {
                Action = new SetPropertyAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathId.ToString(),
                    Name = name,
                    SetParameter = new Parameter { Type = type, Value = value }
                }
            };
        }

        public void ProcessResponse(string response)
        {
            string raw = ResponseHelper.ProcessResponse<string>(response, SaveDefinitionId);

            if (!string.IsNullOrEmpty(raw))
            {
                raw = raw.Replace("/Guid(", "").Replace(")/", "");
                if (Guid.TryParse(raw, out Guid id))
                {
                    Result = new WorkflowDefinitionInfo
                    {
                        Id = id,
                        DisplayName = definition.DisplayName,
                        Description = definition.Description,
                        Xaml = definition.Xaml
                    };
                }
            }
        }
    }
}
