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
    /// Creates a term group with a caller-supplied id.
    /// </summary>
    internal sealed class CreateTermGroupRequest : TaxonomyRequestBase, IRequest<TermGroupInfo>
    {
        private readonly string name;
        private readonly Guid groupId;
        private readonly string description;

        internal CreateTermGroupRequest(string name, Guid groupId, string description = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A term group name is required.", nameof(name));
            }

            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("A term group id is required - the point of this request is to preserve it.", nameof(groupId));
            }

            this.name = name;
            this.groupId = groupId;
            this.description = description;
        }

        public TermGroupInfo Result { get; private set; }

        internal int GroupQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int termStoreId = EmitTermStorePrologue(idProvider, paths);

            int groupPathId = EmitMethod(idProvider, paths, termStoreId, "CreateGroup",
                new Parameter { Type = "String", Value = name },
                new Parameter { Type = "Guid", Value = groupId });

            if (description != null)
            {
                paths.Add(new ActionObjectPath
                {
                    Action = new SetPropertyAction
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = groupPathId.ToString(),
                        Name = "Description",
                        SetParameter = new Parameter { Type = "String", Value = description }
                    }
                });
            }

            EmitCommitAll(idProvider, paths, termStoreId);

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
                            new Property { Name = "Name" }
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
                Name = group.TryGetProperty("Name", out JsonElement n) ? n.GetString() : null
            };
        }
    }
}
