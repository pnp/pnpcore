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
    /// Creates a term set with a caller-supplied id, inside an existing group.
    /// </summary>
    internal sealed class CreateTermSetRequest : TaxonomyRequestBase, IRequest<TermSetInfo>
    {
        private readonly Guid groupId;
        private readonly string name;
        private readonly Guid termSetId;
        private readonly int lcid;
        private readonly string description;
        private readonly bool isOpenForTermCreation;
        private readonly string customSortOrder;

        internal CreateTermSetRequest(Guid groupId, string name, Guid termSetId, int lcid,
            string description = null, bool isOpenForTermCreation = false, string customSortOrder = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A term set name is required.", nameof(name));
            }

            if (termSetId == Guid.Empty)
            {
                throw new ArgumentException("A term set id is required - the point of this request is to preserve it.", nameof(termSetId));
            }

            this.groupId = groupId;
            this.name = name;
            this.termSetId = termSetId;
            this.lcid = lcid;
            this.description = description;
            this.isOpenForTermCreation = isOpenForTermCreation;
            this.customSortOrder = customSortOrder;
        }

        public TermSetInfo Result { get; private set; }

        internal int TermSetQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int termStoreId = EmitTermStorePrologue(idProvider, paths);
            int groupPathId = EmitGetGroup(idProvider, paths, termStoreId, groupId);

            int termSetPathId = EmitMethod(idProvider, paths, groupPathId, "CreateTermSet",
                new Parameter { Type = "String", Value = name },
                new Parameter { Type = "Guid", Value = termSetId },
                new Parameter { Type = "Number", Value = lcid });

            if (description != null)
            {
                paths.Add(SetProperty(idProvider, termSetPathId, "Description", "String", description));
            }

            if (isOpenForTermCreation)
            {
                paths.Add(SetProperty(idProvider, termSetPathId, "IsOpenForTermCreation", "Boolean", true));
            }

            if (!string.IsNullOrEmpty(customSortOrder))
            {
                // Graph has no representation for this at all - one of S1's "not supported" verdicts.
                paths.Add(SetProperty(idProvider, termSetPathId, "CustomSortOrder", "String", customSortOrder));
            }

            EmitCommitAll(idProvider, paths, termStoreId);

            TermSetQueryId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = TermSetQueryId,
                    ObjectPathId = termSetPathId.ToString(),
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
            JsonElement termSet = ResponseHelper.ProcessResponse<JsonElement>(response, TermSetQueryId);
            if (termSet.ValueKind != JsonValueKind.Object) return;

            Result = new TermSetInfo
            {
                Id = TaxonomyJson.ReadGuid(termSet, "Id"),
                Name = termSet.TryGetProperty("Name", out JsonElement n) ? n.GetString() : null
            };
        }
    }
}
