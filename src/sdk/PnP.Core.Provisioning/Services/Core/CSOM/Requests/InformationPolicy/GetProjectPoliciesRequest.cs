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
    /// Lists the site policies available to a web.
    /// </summary>
    internal sealed class GetProjectPoliciesRequest : IRequest<List<SitePolicyInfo>>
    {
        private readonly Guid siteId;
        private readonly Guid webId;

        internal GetProjectPoliciesRequest(Guid siteId, Guid webId)
        {
            this.siteId = siteId;
            this.webId = webId;
        }

        public List<SitePolicyInfo> Result { get; private set; } = new List<SitePolicyInfo>();

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        internal int PoliciesQueryId { get; private set; }

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

            PoliciesQueryId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = PoliciesQueryId,
                    ObjectPathId = getPolicies.Id.ToString(),
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
            JsonElement policies = ResponseHelper.ProcessResponse<JsonElement>(response, PoliciesQueryId);

            if (policies.ValueKind != JsonValueKind.Object
                || !policies.TryGetProperty("_Child_Items_", out JsonElement items)
                || items.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var parsed = new List<SitePolicyInfo>();
            foreach (JsonElement item in items.EnumerateArray())
            {
                parsed.Add(new SitePolicyInfo
                {
                    Name = item.TryGetProperty("Name", out JsonElement name) ? name.GetString() : null,
                    Description = item.TryGetProperty("Description", out JsonElement description) ? description.GetString() : null
                });
            }

            Result = parsed;
        }
    }
}
