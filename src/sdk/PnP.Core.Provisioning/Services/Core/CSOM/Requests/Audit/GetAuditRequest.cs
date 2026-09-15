using PnP.Core.Provisioning.Model;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Audit
{
    /// <summary>
    /// Reads the site collection's audit flags.
    /// </summary>
    internal sealed class GetAuditRequest : IRequest<AuditSettingsInfo>
    {
        private readonly Guid siteId;
        private readonly Guid webId;

        internal GetAuditRequest(Guid siteId, Guid webId)
        {
            this.siteId = siteId;
            this.webId = webId;
        }

        public AuditSettingsInfo Result { get; private set; }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        internal int AuditQueryId { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            #region XML payload
            #endregion

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
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = auditPropertyId.ToString()
                },
                ObjectPath = new Property
                {
                    Id = auditPropertyId,
                    ParentId = siteIdentityId,
                    Name = "Audit"
                }
            });

            AuditQueryId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                Action = new QueryAction
                {
                    Id = AuditQueryId,
                    ObjectPathId = auditPropertyId.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property> { new Property { Name = "AuditFlags" } }
                    }
                }
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            JsonElement audit = ResponseHelper.ProcessResponse<JsonElement>(response, AuditQueryId);

            if (audit.ValueKind == JsonValueKind.Object
                && audit.TryGetProperty("AuditFlags", out JsonElement flags)
                && flags.ValueKind == JsonValueKind.Number)
            {
                Result = new AuditSettingsInfo { AuditFlags = (AuditMaskType)flags.GetInt32() };
            }
        }
    }
}
