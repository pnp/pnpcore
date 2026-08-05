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
    /// Creates a term with a caller-supplied id, under a term set or another term.
    /// </summary>
    internal sealed class CreateTermRequest : TaxonomyRequestBase, IRequest<TermInfo>
    {
        private readonly Guid parentId;
        private readonly bool parentIsTermSet;
        private readonly string name;
        private readonly Guid termId;
        private readonly int lcid;
        private readonly string description;
        private readonly bool isAvailableForTagging;
        private readonly string customSortOrder;

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="parentId">The id of the term set or term to create under</param>
        /// <param name="parentIsTermSet"><c>true</c> for a root term, <c>false</c> for a child term</param>
        /// <param name="name">The term's default label</param>
        /// <param name="termId">The id to give the term</param>
        /// <param name="lcid">The language the name is in</param>
        /// <param name="description">Optional description, in the same language</param>
        /// <param name="isAvailableForTagging">Whether the term can be used to tag content</param>
        /// <param name="customSortOrder">Optional child ordering - unsupported by Graph</param>
        internal CreateTermRequest(Guid parentId, bool parentIsTermSet, string name, Guid termId, int lcid,
            string description = null, bool isAvailableForTagging = true, string customSortOrder = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A term name is required.", nameof(name));
            }

            if (termId == Guid.Empty)
            {
                throw new ArgumentException("A term id is required - the point of this request is to preserve it.", nameof(termId));
            }

            this.parentId = parentId;
            this.parentIsTermSet = parentIsTermSet;
            this.name = name;
            this.termId = termId;
            this.lcid = lcid;
            this.description = description;
            this.isAvailableForTagging = isAvailableForTagging;
            this.customSortOrder = customSortOrder;
        }

        public TermInfo Result { get; private set; }

        internal int TermQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int termStoreId = EmitTermStorePrologue(idProvider, paths);

            int parentPathId = parentIsTermSet
                ? EmitGetTermSet(idProvider, paths, termStoreId, parentId)
                : EmitGetTerm(idProvider, paths, termStoreId, parentId);

            int termPathId = EmitMethod(idProvider, paths, parentPathId, "CreateTerm",
                new Parameter { Type = "String", Value = name },
                new Parameter { Type = "Number", Value = lcid },
                new Parameter { Type = "Guid", Value = termId });

            if (description != null)
            {
                // Description is set per language, so it is a method rather than a property.
                paths.Add(new ActionObjectPath
                {
                    Action = new MethodAction
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = termPathId.ToString(),
                        Name = "SetDescription",
                        Parameters = new List<Parameter>
                        {
                            new Parameter { Type = "String", Value = description },
                            new Parameter { Type = "Number", Value = lcid }
                        }
                    }
                });
            }

            if (!isAvailableForTagging)
            {
                paths.Add(new ActionObjectPath
                {
                    Action = new SetPropertyAction
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = termPathId.ToString(),
                        Name = "IsAvailableForTagging",
                        SetParameter = new Parameter { Type = "Boolean", Value = false }
                    }
                });
            }

            if (!string.IsNullOrEmpty(customSortOrder))
            {
                paths.Add(new ActionObjectPath
                {
                    Action = new SetPropertyAction
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = termPathId.ToString(),
                        Name = "CustomSortOrder",
                        SetParameter = new Parameter { Type = "String", Value = customSortOrder }
                    }
                });
            }

            EmitCommitAll(idProvider, paths, termStoreId);

            TermQueryId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = TermQueryId,
                    ObjectPathId = termPathId.ToString(),
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
            JsonElement term = ResponseHelper.ProcessResponse<JsonElement>(response, TermQueryId);
            if (term.ValueKind != JsonValueKind.Object) return;

            Result = new TermInfo
            {
                Id = TaxonomyJson.ReadGuid(term, "Id"),
                Name = term.TryGetProperty("Name", out JsonElement n) ? n.GetString() : null
            };
        }
    }
}
