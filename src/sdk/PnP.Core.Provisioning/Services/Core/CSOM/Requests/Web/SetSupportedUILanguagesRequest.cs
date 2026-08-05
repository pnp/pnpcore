using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Web
{
    /// <summary>
    /// Adds and removes the web's supported UI languages in one round trip.
    /// </summary>
    internal sealed class SetSupportedUILanguagesRequest : IRequest<object>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly IReadOnlyCollection<int> toAdd;
        private readonly IReadOnlyCollection<int> toRemove;

        internal SetSupportedUILanguagesRequest(Guid siteId, Guid webId,
            IReadOnlyCollection<int> toAdd, IReadOnlyCollection<int> toRemove)
        {
            this.siteId = siteId;
            this.webId = webId;
            this.toAdd = toAdd ?? Array.Empty<int>();
            this.toRemove = toRemove ?? Array.Empty<int>();

            if (this.toAdd.Count == 0 && this.toRemove.Count == 0)
            {
                throw new ArgumentException("At least one language to add or remove is required.", nameof(toAdd));
            }
        }

        public object Result { get; private set; }

        /// <summary>
        /// The raw CSOM response body.
        /// </summary>
        internal string RawResponse { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int webIdentityId = idProvider.GetActionId();

            paths.Add(new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = webIdentityId,
                    Name = CsomIdentity.Web(siteId, webId)
                }
            });

            // Remove first - see the remarks.
            foreach (int lcid in toRemove)
            {
                paths.Add(LanguageMethod(idProvider, webIdentityId, "RemoveSupportedUILanguage", lcid));
            }

            foreach (int lcid in toAdd)
            {
                paths.Add(LanguageMethod(idProvider, webIdentityId, "AddSupportedUILanguage", lcid));
            }

            // Persist. Without this the adds and removes are staged and discarded - which is
            // precisely how the REST attempt failed.
            paths.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = webIdentityId.ToString(),
                    Name = "Update",
                    Parameters = new List<Parameter>()
                }
            });

            return paths;
        }

        private static ActionObjectPath LanguageMethod(IIdProvider idProvider, int webIdentityId, string name, int lcid)
        {
            return new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = webIdentityId.ToString(),
                    Name = name,
                    Parameters = new List<Parameter>
                    {
                        new Parameter { Type = "Number", Value = lcid }
                    }
                }
            };
        }

        public void ProcessResponse(string response)
        {
            RawResponse = response;
        }
    }
}
