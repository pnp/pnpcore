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
    /// Shared plumbing for the SharePoint 2013 workflow CSOM requests.
    /// </summary>
    internal abstract class WorkflowRequestBase
    {
        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        /// <summary>
        /// Emits <c>new WorkflowServicesManager(web)</c> and returns its object path id.
        /// </summary>
        protected static int EmitWorkflowServicesManager(IIdProvider idProvider, List<ActionObjectPath> paths, Guid siteId, Guid webId)
        {
            int webIdentityId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = webIdentityId,
                    Name = CsomIdentity.Web(siteId, webId)
                }
            });

            var manager = new ConstructorPath
            {
                Id = idProvider.GetActionId(),
                TypeId = CsomTypeIds.WorkflowServicesManager,
                Parameters = new MethodParameter
                {
                    Properties = new List<Parameter>
                    {
                        new ObjectReferenceParameter { ObjectPathId = webIdentityId }
                    }
                }
            };

            paths.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = manager.Id.ToString()
                },
                ObjectPath = manager
            });

            paths.Add(new ActionObjectPath
            {
                Action = new IdentityQueryAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = manager.Id.ToString()
                }
            });

            return manager.Id;
        }

        /// <summary>
        /// Emits <c>WorkflowServicesManager.GetWorkflowDeploymentService()</c>.
        /// </summary>
        protected static int EmitDeploymentService(IIdProvider idProvider, List<ActionObjectPath> paths, int managerId)
        {
            return EmitMethod(idProvider, paths, managerId, "GetWorkflowDeploymentService");
        }

        /// <summary>
        /// Emits <c>WorkflowServicesManager.GetWorkflowSubscriptionService()</c>.
        /// </summary>
        protected static int EmitSubscriptionService(IIdProvider idProvider, List<ActionObjectPath> paths, int managerId)
        {
            return EmitMethod(idProvider, paths, managerId, "GetWorkflowSubscriptionService");
        }

        /// <summary>
        /// Emits <c>WorkflowServicesManager.GetWorkflowInstanceService()</c>.
        /// </summary>
        protected static int EmitInstanceService(IIdProvider idProvider, List<ActionObjectPath> paths, int managerId)
        {
            return EmitMethod(idProvider, paths, managerId, "GetWorkflowInstanceService");
        }

        /// <summary>
        /// Emits a method call returning a client object, with an identity query.
        /// </summary>
        protected static int EmitMethod(IIdProvider idProvider, List<ActionObjectPath> paths, int parentPathId, string name, params Parameter[] parameters)
        {
            var method = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = parentPathId,
                Name = name,
                Parameters = new MethodParameter { Properties = new List<Parameter>(parameters) }
            };

            paths.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = method.Id.ToString()
                },
                ObjectPath = method
            });

            paths.Add(new ActionObjectPath
            {
                Action = new IdentityQueryAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = method.Id.ToString()
                }
            });

            return method.Id;
        }
    }
}
