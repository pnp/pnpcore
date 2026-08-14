using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.ContentTypes
{
    /// <summary>
    /// Puts a content type's columns into the order a template specifies.
    /// </summary>
    internal sealed class ReorderFieldLinksRequest : IRequest<object>
    {
        private readonly string contentTypeIdentity;
        private readonly List<string> fieldInternalNames;

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="siteId">The site collection id</param>
        /// <param name="webId">The web id</param>
        /// <param name="contentTypeId">The content type's string id, for example <c>0x0100...</c></param>
        /// <param name="fieldInternalNames">The internal names, in the wanted order</param>
        internal ReorderFieldLinksRequest(Guid siteId, Guid webId, string contentTypeId, List<string> fieldInternalNames)
        {
            if (string.IsNullOrEmpty(contentTypeId))
            {
                throw new ArgumentException("A content type id is required.", nameof(contentTypeId));
            }

            if (fieldInternalNames == null || fieldInternalNames.Count == 0)
            {
                throw new ArgumentException("At least one field name is required.", nameof(fieldInternalNames));
            }

            contentTypeIdentity = CsomIdentity.ContentType(siteId, webId, contentTypeId);
            this.fieldInternalNames = fieldInternalNames;
        }

        public object Result { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int contentTypeId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new Identity { Id = contentTypeId, Name = contentTypeIdentity },
            });

            int fieldLinksId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new Property { Id = fieldLinksId, ParentId = contentTypeId, Name = "FieldLinks" },
            });

            result.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = fieldLinksId.ToString(),
                    Name = "Reorder",

                    Parameters = new List<Parameter> { new Parameter { Type = "String", Value = fieldInternalNames } },
                },
            });

            result.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = contentTypeId.ToString(),
                    Name = "Update",
                    Parameters = new List<Parameter> { new Parameter { Type = "Boolean", Value = false } },
                },
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
