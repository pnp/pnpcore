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
    /// Sets a content type's properties and commits them, optionally pushing the change to child
    /// content types.
    /// </summary>
    internal sealed class UpdateContentTypeRequest : IRequest<object>
    {
        private readonly string contentTypeIdentity;
        private readonly bool updateChildren;
        private readonly List<(string Name, string Type, object Value)> properties = new List<(string, string, object)>();

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="siteId">The site collection id</param>
        /// <param name="webId">The web id</param>
        /// <param name="contentTypeId">The content type's string id</param>
        /// <param name="updateChildren">Whether to push the change to derived content types</param>
        internal UpdateContentTypeRequest(Guid siteId, Guid webId, string contentTypeId, bool updateChildren)
        {
            if (string.IsNullOrEmpty(contentTypeId))
            {
                throw new ArgumentException("A content type id is required.", nameof(contentTypeId));
            }

            contentTypeIdentity = CsomIdentity.ContentType(siteId, webId, contentTypeId);
            this.updateChildren = updateChildren;
        }

        /// <summary>Whether any property was queued.</summary>
        internal bool HasChanges => properties.Count > 0;

        internal void SetString(string name, string value)
        {
            properties.Add((name, "String", value));
        }

        internal void SetBoolean(string name, bool value)
        {
            properties.Add((name, "Boolean", value));
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

            foreach ((string name, string type, object value) in properties)
            {
                result.Add(new ActionObjectPath
                {
                    Action = new SetPropertyAction
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = contentTypeId.ToString(),
                        Name = name,
                        SetParameter = new Parameter { Type = type, Value = value },
                    },
                });
            }

            result.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = contentTypeId.ToString(),
                    Name = "Update",
                    Parameters = new List<Parameter> { new Parameter { Type = "Boolean", Value = updateChildren } },
                },
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
