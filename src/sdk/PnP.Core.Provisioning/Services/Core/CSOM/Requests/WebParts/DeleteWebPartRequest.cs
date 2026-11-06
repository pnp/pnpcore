using PnP.Core.Provisioning.Model;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.WebParts
{    /// <summary>
    /// Removes a web part from a page.
    /// </summary>
    internal sealed class DeleteWebPartRequest : WebPartRequestBase, IRequest<object>
    {
        private readonly Guid siteId;
        private readonly Guid webId;
        private readonly string serverRelativeFileUrl;
        private readonly Guid webPartId;

        internal DeleteWebPartRequest(Guid siteId, Guid webId, string serverRelativeFileUrl, Guid webPartId)
        {
            if (string.IsNullOrEmpty(serverRelativeFileUrl))
            {
                throw new ArgumentException("A server relative file url is required.", nameof(serverRelativeFileUrl));
            }

            this.siteId = siteId;
            this.webId = webId;
            this.serverRelativeFileUrl = serverRelativeFileUrl;
            this.webPartId = webPartId;
        }

        public object Result { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var paths = new List<ActionObjectPath>();

            int managerId = EmitWebPartManager(idProvider, paths, siteId, webId, serverRelativeFileUrl);
            int webPartsId = idProvider.GetActionId();

            paths.Add(new ActionObjectPath
            {
                ObjectPath = new Property
                {
                    Id = webPartsId,
                    ParentId = managerId,
                    Name = "WebParts"
                }
            });

            int definitionId = EmitMethod(idProvider, paths, webPartsId, "GetById",
                new Parameter { Type = "Guid", Value = webPartId });

            paths.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = definitionId.ToString(),
                    Name = "DeleteWebPart",
                    Parameters = new List<Parameter>()
                }
            });

            return paths;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
