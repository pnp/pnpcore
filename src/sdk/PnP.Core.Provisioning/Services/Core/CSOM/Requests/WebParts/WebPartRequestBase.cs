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
    /// Shared plumbing for the classic web part CSOM requests.
    /// </summary>
    internal abstract class WebPartRequestBase
    {
        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        /// <summary>
        /// Personalization scope. <c>Shared</c> is what provisioning always wants - a personal
        /// customization would only be visible to the account running the template.
        /// </summary>
        protected const int SharedScope = 1;

        /// <summary>
        /// Emits <c>Web.GetFileByServerRelativeUrl(url).GetLimitedWebPartManager(Shared)</c> and
        /// returns the manager's object path id.
        /// </summary>
        protected static int EmitWebPartManager(IIdProvider idProvider, List<ActionObjectPath> paths,
            Guid siteId, Guid webId, string serverRelativeFileUrl)
        {
            int webIdentityId = idProvider.GetActionId();
            paths.Add(new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = webIdentityId,
                    Name = CsomIdentity.Web(siteId, webId)
                }
            });

            int filePathId = EmitMethod(idProvider, paths, webIdentityId, "GetFileByServerRelativeUrl",
                new Parameter { Type = "String", Value = serverRelativeFileUrl });

            return EmitMethod(idProvider, paths, filePathId, "GetLimitedWebPartManager",
                new Parameter { Type = "Enum", Value = SharedScope });
        }

        /// <summary>
        /// Emits a method call returning a client object, with an identity query so later actions
        /// can reference the result.
        /// </summary>
        protected static int EmitMethod(IIdProvider idProvider, List<ActionObjectPath> paths, int parentPathId, string name, params Parameter[] parameters)
        {
            var method = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = parentPathId,
                Name = name,
                Parameters = new MethodParameter { Properties = new List<Parameter>(parameters) }
            };

            paths.Add(new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = method.Id.ToString()
                },
                ObjectPath = method
            });

            paths.Add(new ActionObjectPath
            {
                Action = new IdentityQueryAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = method.Id.ToString()
                }
            });

            return method.Id;
        }
    }
}
