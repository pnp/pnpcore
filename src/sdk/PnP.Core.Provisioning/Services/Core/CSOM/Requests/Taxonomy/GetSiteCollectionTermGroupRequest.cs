using PnP.Core.Provisioning.Model;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Taxonomy
{    /// <summary>
    /// Gets the term group scoped to the current site collection, creating it if asked.
    /// </summary>
    internal sealed class GetSiteCollectionTermGroupRequest : TaxonomyRequestBase, IRequest<TermGroupInfo>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly bool createIfMissing;

        internal GetSiteCollectionTermGroupRequest(Guid siteId, Guid webId, bool createIfMissing)
        {
            this.siteId = siteId;
            this.webId = webId;
            this.createIfMissing = createIfMissing;
        }

        public TermGroupInfo Result { get; private set; }

        internal int GroupQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int siteIdentityId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = siteIdentityId,
                    Name = CsomIdentity.Site(siteId, webId)
                }
            });

            int termStoreId = EmitTermStorePrologue(idProvider, paths);

            int groupPathId = EmitMethod(idProvider, paths, termStoreId, "GetSiteCollectionGroup",
                new ObjectReferenceParameter { ObjectPathId = siteIdentityId },
                new Parameter { Type = "Boolean", Value = createIfMissing });

            if (createIfMissing)
            {
                EmitCommitAll(idProvider, paths, termStoreId);
            }

            GroupQueryId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = GroupQueryId,
                    ObjectPathId = groupPathId.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property>
                        {
                            new Property { Name = "Id" },
                            new Property { Name = "Name" },
                            new Property { Name = "IsSiteCollectionGroup" }
                        }
                    }
                }
            });

            return paths;
        }

        public void ProcessResponse(string response)
        {
            JsonElement group = ResponseHelper.ProcessResponse<JsonElement>(response, GroupQueryId);
            if (group.ValueKind != JsonValueKind.Object) return;

            Result = new TermGroupInfo
            {
                Id = TaxonomyJson.ReadGuid(group, "Id"),
                Name = group.TryGetProperty("Name", out JsonElement n) ? n.GetString() : null,
                IsSiteCollectionGroup = group.TryGetProperty("IsSiteCollectionGroup", out JsonElement scg)
                    && scg.ValueKind == JsonValueKind.True
            };
        }
    }
}
