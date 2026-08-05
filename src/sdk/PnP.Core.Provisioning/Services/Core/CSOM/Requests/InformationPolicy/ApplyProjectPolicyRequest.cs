using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.InformationPolicy
{
    /// <summary>
    /// Applies a named site policy to a web.
    /// </summary>
    internal sealed class ApplyProjectPolicyRequest : IRequest<object>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly int policyIndex;

        /// <summary>
        /// Applies the policy at the given position in the web's available policy list.
        /// </summary>
        /// <param name="siteId">Site collection id</param>
        /// <param name="webId">Web id</param>
        /// <param name="policyIndex">Zero based index into <see cref="GetProjectPoliciesRequest"/>'s result</param>
        internal ApplyProjectPolicyRequest(Guid siteId, Guid webId, int policyIndex)
        {
            if (policyIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(policyIndex));
            }

            this.siteId = siteId;
            this.webId = webId;
            this.policyIndex = policyIndex;
        }

        public object Result { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int webIdentityId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = webIdentityId,
                    Name = CsomIdentity.Web(siteId, webId)
                }
            });

            // ProjectPolicy.GetProjectPolicies(web)
            var getPolicies = new StaticMethodPath
            {
                Id = idProvider.GetActionId(),
                TypeId = CsomTypeIds.ProjectPolicy,
                Name = "GetProjectPolicies",
                Parameters = new MethodParameter
                {
                    Properties = new List<Parameter>
                    {
                        new ObjectReferenceParameter { ObjectPathId = webIdentityId }
                    }
                }
            };

            result.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = getPolicies.Id.ToString()
                },
                ObjectPath = getPolicies
            });

            // ...[policyIndex] - an indexer on the returned collection
            var policyAtIndex = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = getPolicies.Id,
                Name = "GetItemAtIndex",
                Parameters = new MethodParameter
                {
                    Properties = new List<Parameter>
                    {
                        new Parameter { Type = "Number", Value = policyIndex }
                    }
                }
            };

            result.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = policyAtIndex.Id.ToString()
                },
                ObjectPath = policyAtIndex
            });

            // ProjectPolicy.ApplyProjectPolicy(web, policy)
            result.Add(new ActionObjectPath
            {
                Action = new StaticMethodAction
                {
                    Id = idProvider.GetActionId(),
                    TypeId = CsomTypeIds.ProjectPolicy,
                    Name = "ApplyProjectPolicy",
                    Parameters = new List<Parameter>
                    {
                        new ObjectReferenceParameter { ObjectPathId = webIdentityId },
                        new ObjectReferenceParameter { ObjectPathId = policyAtIndex.Id }
                    }
                }
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            // Nothing to read back.
        }
    }
}
