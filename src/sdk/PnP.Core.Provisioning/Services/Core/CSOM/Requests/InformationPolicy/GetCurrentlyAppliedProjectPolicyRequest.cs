using PnP.Core.Provisioning.Model;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.InformationPolicy
{
    /// <summary>
    /// Reads the site policy currently applied to a web, if any.
    /// </summary>
    internal sealed class GetCurrentlyAppliedProjectPolicyRequest : IRequest<SitePolicyInfo>
    {
        private readonly Guid siteId;
        private readonly Guid webId;

        internal GetCurrentlyAppliedProjectPolicyRequest(Guid siteId, Guid webId)
        {
            this.siteId = siteId;
            this.webId = webId;
        }

        public SitePolicyInfo Result { get; private set; }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        internal int PolicyQueryId { get; private set; }

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

            var getPolicy = new StaticMethodPath
            {
                Id = idProvider.GetActionId(),
                TypeId = CsomTypeIds.ProjectPolicy,
                Name = "GetCurrentlyAppliedProjectPolicyOnWeb",
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
                    ObjectPathId = getPolicy.Id.ToString()
                },
                ObjectPath = getPolicy
            });

            PolicyQueryId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = PolicyQueryId,
                    ObjectPathId = getPolicy.Id.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property>
                        {
                            new Property { Name = "Name" },
                            new Property { Name = "Description" }
                        }
                    }
                }
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            JsonElement policy = ResponseHelper.ProcessResponse<JsonElement>(response, PolicyQueryId);

            if (policy.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            Result = new SitePolicyInfo
            {
                Name = policy.TryGetProperty("Name", out JsonElement name) ? name.GetString() : null,
                Description = policy.TryGetProperty("Description", out JsonElement description) ? description.GetString() : null
            };
        }
    }
}
