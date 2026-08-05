using PnP.Core.Provisioning.Model;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Audit
{
    /// <summary>
    /// Sets the site collection's audit flags.
    /// </summary>
    internal sealed class UpdateAuditRequest : IRequest<object>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly AuditMaskType auditFlags;

        internal UpdateAuditRequest(Guid siteId, Guid webId, AuditMaskType auditFlags)
        {
            this.siteId = siteId;
            this.webId = webId;
            this.auditFlags = auditFlags;
        }

        public object Result { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int siteIdentityId = idProvider.GetActionId();
            int auditPropertyId = idProvider.GetActionId();

            result.Add(new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = siteIdentityId,
                    Name = CsomIdentity.Site(siteId, webId)
                }
            });

            result.Add(new ActionObjectPath
            {
                ObjectPath = new Property
                {
                    Id = auditPropertyId,
                    ParentId = siteIdentityId,
                    Name = "Audit"
                }
            });

            // AuditMaskType is serialized as its underlying integer. Note the enum carries
            // All = -1 rather than the sum of its flags, and has no [Flags] attribute - both
            // reproduced from the real CSOM enum during the phase 1 model port.
            result.Add(new ActionObjectPath
            {
                Action = new SetPropertyAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = auditPropertyId.ToString(),
                    Name = "AuditFlags",
                    SetParameter = new Parameter { Type = "Number", Value = (int)auditFlags }
                }
            });

            result.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = auditPropertyId.ToString(),
                    Name = "Update",
                    Parameters = new List<Parameter>()
                }
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            // Nothing to read back - a failure surfaces as a CSOM error on the batch.
        }
    }
}
