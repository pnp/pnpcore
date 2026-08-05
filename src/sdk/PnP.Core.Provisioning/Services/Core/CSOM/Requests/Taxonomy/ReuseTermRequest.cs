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
    /// Reuses an existing term under a different parent, optionally pinning it.
    /// </summary>
    internal sealed class ReuseTermRequest : TaxonomyRequestBase, IRequest<TermInfo>
    {
        private readonly Guid parentId;
        private readonly bool parentIsTermSet;
        private readonly Guid sourceTermId;
        private readonly bool reuseBranch;
        private readonly bool pin;

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="parentId">The term set or term to reuse the term under</param>
        /// <param name="parentIsTermSet"><c>true</c> if the parent is a term set</param>
        /// <param name="sourceTermId">The term being reused</param>
        /// <param name="reuseBranch">Reuse the term's descendants as well. Ignored when <paramref name="pin"/> is set.</param>
        /// <param name="pin">Pin the reuse, so source changes propagate</param>
        internal ReuseTermRequest(Guid parentId, bool parentIsTermSet, Guid sourceTermId, bool reuseBranch = false, bool pin = false)
        {
            this.parentId = parentId;
            this.parentIsTermSet = parentIsTermSet;
            this.sourceTermId = sourceTermId;
            this.reuseBranch = reuseBranch;
            this.pin = pin;
        }

        public TermInfo Result { get; private set; }

        internal int ReusedTermQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int termStoreId = EmitTermStorePrologue(idProvider, paths);

            // The term being reused has to be resolved before it can be passed by reference.
            int sourceTermPathId = EmitGetTerm(idProvider, paths, termStoreId, sourceTermId);

            int parentPathId = parentIsTermSet
                ? EmitGetTermSet(idProvider, paths, termStoreId, parentId)
                : EmitGetTerm(idProvider, paths, termStoreId, parentId);

            int reusedPathId = pin
                ? EmitMethod(idProvider, paths, parentPathId, "ReuseTermWithPinning",
                    new ObjectReferenceParameter { ObjectPathId = sourceTermPathId })
                : EmitMethod(idProvider, paths, parentPathId, "ReuseTerm",
                    new ObjectReferenceParameter { ObjectPathId = sourceTermPathId },
                    new Parameter { Type = "Boolean", Value = reuseBranch });

            EmitCommitAll(idProvider, paths, termStoreId);

            ReusedTermQueryId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = ReusedTermQueryId,
                    ObjectPathId = reusedPathId.ToString(),
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
            JsonElement term = ResponseHelper.ProcessResponse<JsonElement>(response, ReusedTermQueryId);
            if (term.ValueKind != JsonValueKind.Object) return;

            Result = new TermInfo
            {
                Id = TaxonomyJson.ReadGuid(term, "Id"),
                Name = term.TryGetProperty("Name", out JsonElement n) ? n.GetString() : null
            };
        }
    }
}
