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
    /// Creates or updates a workflow association, at site or list scope.
    /// </summary>
    internal sealed class PublishWorkflowSubscriptionRequest : WorkflowRequestBase, IRequest<WorkflowSubscriptionInfo>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly WorkflowSubscriptionInfo subscription;
        private readonly Guid? listId;

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="siteId">Site collection id</param>
        /// <param name="webId">Web id</param>
        /// <param name="subscription">The association to publish</param>
        /// <param name="listId">The list to associate with, or <c>null</c> for a site workflow</param>
        internal PublishWorkflowSubscriptionRequest(Guid siteId, Guid webId, WorkflowSubscriptionInfo subscription, Guid? listId = null)
        {
            this.subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));

            if (subscription.DefinitionId == Guid.Empty)
            {
                throw new ArgumentException("A subscription must name the definition it associates.", nameof(subscription));
            }

            this.siteId = siteId;
            this.webId = webId;
            this.listId = listId;
        }

        public WorkflowSubscriptionInfo Result { get; private set; }

        internal int PublishId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWorkflowServicesManager(idProvider, paths, siteId, webId);
            int subscriptionServiceId = EmitSubscriptionService(idProvider, paths, managerId);

            var subscriptionPath = new ConstructorPath
            {
                Id = idProvider.GetActionId(),
                TypeId = CsomTypeIds.WorkflowSubscription,
                Parameters = new MethodParameter { Properties = new List<Parameter>() }
            };

            paths.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = subscriptionPath.Id.ToString()
                },
                ObjectPath = subscriptionPath
            });

            paths.Add(SetProperty(idProvider, subscriptionPath.Id, "DefinitionId", "Guid", subscription.DefinitionId));
            paths.Add(SetProperty(idProvider, subscriptionPath.Id, "Enabled", "Boolean", subscription.Enabled));

            if (subscription.Name != null)
            {
                paths.Add(SetProperty(idProvider, subscriptionPath.Id, "Name", "String", subscription.Name));
            }

            if (subscription.Id != Guid.Empty)
            {
                paths.Add(SetProperty(idProvider, subscriptionPath.Id, "Id", "Guid", subscription.Id));
            }

            if (subscription.EventSourceId != Guid.Empty)
            {
                paths.Add(SetProperty(idProvider, subscriptionPath.Id, "EventSourceId", "Guid", subscription.EventSourceId));
            }

            if (subscription.StatusFieldName != null)
            {
                paths.Add(SetProperty(idProvider, subscriptionPath.Id, "StatusFieldName", "String", subscription.StatusFieldName));
            }

            if (subscription.EventTypes != null && subscription.EventTypes.Count > 0)
            {
                paths.Add(SetProperty(idProvider, subscriptionPath.Id, "EventTypes", "String", subscription.EventTypes));
            }

            if (subscription.PropertyDefinitions != null)
            {
                foreach (KeyValuePair<string, string> property in subscription.PropertyDefinitions)
                {
                    paths.Add(new ActionObjectPath
                    {
                        Action = new MethodAction
                        {
                            Id = idProvider.GetActionId(),
                            ObjectPathId = subscriptionPath.Id.ToString(),
                            Name = "SetProperty",
                            Parameters = new List<Parameter>
                            {
                                new Parameter { Type = "String", Value = property.Key },
                                new Parameter { Type = "String", Value = property.Value }
                            }
                        }
                    });
                }
            }

            var publishParameters = new List<Parameter>
            {
                new ObjectReferenceParameter { ObjectPathId = subscriptionPath.Id }
            };

            if (listId.HasValue)
            {
                publishParameters.Add(new Parameter { Type = "Guid", Value = listId.Value });
            }

            PublishId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = PublishId,
                    ObjectPathId = subscriptionServiceId.ToString(),
                    Name = listId.HasValue ? "PublishSubscriptionForList" : "PublishSubscription",
                    Parameters = publishParameters
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
            string raw = ResponseHelper.ProcessResponse<string>(response, PublishId);

            if (!string.IsNullOrEmpty(raw))
            {
                raw = raw.Replace("/Guid(", "").Replace(")/", "");
                if (Guid.TryParse(raw, out Guid id))
                {
                    subscription.Id = id;
                    Result = subscription;
                }
            }
        }
    }
}
