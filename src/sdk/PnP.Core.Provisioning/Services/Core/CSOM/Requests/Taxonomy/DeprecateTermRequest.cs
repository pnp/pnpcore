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
    /// Deprecates or un-deprecates a term.
    /// </summary>
    internal sealed class DeprecateTermRequest : TaxonomyRequestBase, IRequest<object>
    {
        private readonly Guid termId;
        private readonly bool deprecate;

        internal DeprecateTermRequest(Guid termId, bool deprecate)
        {
            this.termId = termId;
            this.deprecate = deprecate;
        }

        public object Result { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int termStoreId = EmitTermStorePrologue(idProvider, paths);
            int termPathId = EmitGetTerm(idProvider, paths, termStoreId, termId);

            paths.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = termPathId.ToString(),
                    Name = "Deprecate",
                    Parameters = new List<Parameter>
                    {
                        new Parameter { Type = "Boolean", Value = deprecate }
                    }
                }
            });

            EmitCommitAll(idProvider, paths, termStoreId);

            return paths;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
