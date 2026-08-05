using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Web
{
    /// <summary>
    /// Which of the three associated groups to assign.
    /// </summary>
    internal enum AssociatedGroupKind
    {
        /// <summary>The site owners group.</summary>
        Owners,

        /// <summary>The site members group.</summary>
        Members,

        /// <summary>The site visitors group.</summary>
        Visitors,
    }

    /// <summary>
    /// Makes an existing SharePoint group the web's associated owner, member or visitor group.
    /// </summary>
    internal sealed class SetAssociatedGroupRequest : IRequest<object>
    {
        private readonly string webIdentity;
        private readonly int groupId;
        private readonly AssociatedGroupKind kind;

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="siteId">The site collection id</param>
        /// <param name="webId">The web id</param>
        /// <param name="groupId">The id of the group to associate</param>
        /// <param name="kind">Which association to set</param>
        internal SetAssociatedGroupRequest(Guid siteId, Guid webId, int groupId, AssociatedGroupKind kind)
        {
            webIdentity = CsomIdentity.Web(siteId, webId);
            this.groupId = groupId;
            this.kind = kind;
        }

        public object Result { get; private set; }

        /// <summary>
        /// The property name for each association.
        /// </summary>
        private string PropertyName => kind switch
        {
            AssociatedGroupKind.Owners => "AssociatedOwnerGroup",
            AssociatedGroupKind.Members => "AssociatedMemberGroup",
            _ => "AssociatedVisitorGroup",
        };

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int webPathId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new Identity { Id = webPathId, Name = webIdentity },
            });

            int siteGroupsPathId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new Property { Id = siteGroupsPathId, ParentId = webPathId, Name = "SiteGroups" },
            });

            int groupPathId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new ObjectPathMethod
                {
                    Id = groupPathId,
                    ParentId = siteGroupsPathId,
                    Name = "GetById",
                    Parameters = new MethodParameter
                    {
                        Properties = new List<Parameter>
                        {
                            new Parameter { Type = "Int32", Value = groupId },
                        },
                    },
                },
            });

            result.Add(new ActionObjectPath
            {
                Action = new SetPropertyAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = webPathId.ToString(),
                    Name = PropertyName,
                    SetParameter = new ObjectPathParameter { ReferencedObjectPathId = groupPathId },
                },
            });

            result.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = webPathId.ToString(),
                    Name = "Update",
                    Parameters = new List<Parameter>(),
                },
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            // Nothing to read back; the assignment either applied or the request failed.
        }
    }
}
