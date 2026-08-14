using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources
{
    /// <summary>
    /// Sets a localized value for one culture on one resource property.
    /// </summary>
    internal sealed class SetValueForUICultureRequest : IRequest<object>
    {
        private readonly UserResourcePath resource;
        private readonly string cultureName;
        private readonly string value;

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="resource">Which resource property on which object</param>
        /// <param name="cultureName">A culture name such as <c>en-US</c> - not an LCID</param>
        /// <param name="value">The localized text</param>
        internal SetValueForUICultureRequest(UserResourcePath resource, string cultureName, string value)
        {
            this.resource = resource ?? throw new ArgumentNullException(nameof(resource));

            if (string.IsNullOrEmpty(cultureName))
            {
                throw new ArgumentException("A culture name is required.", nameof(cultureName));
            }

            this.cultureName = cultureName;
            this.value = value;
        }

        public object Result { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            var result = new List<ActionObjectPath>();

            int parentIdentityId = resource.AppendParentPath(idProvider, result);

            int resourcePropertyId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                ObjectPath = new Property
                {
                    Id = resourcePropertyId,
                    ParentId = parentIdentityId,
                    Name = resource.PropertyName
                }
            });

            result.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = resourcePropertyId.ToString(),
                    Name = "SetValueForUICulture",
                    Parameters = new List<Parameter>
                    {
                        new Parameter { Type = "String", Value = cultureName },
                        new Parameter { Type = "String", Value = value }
                    }
                }
            });

            result.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = parentIdentityId.ToString(),
                    Name = resource.ParentUpdateMethod,
                    Parameters = resource.ParentUpdateTakesFlag
                        ? new List<Parameter> { new Parameter { Type = "Boolean", Value = true } }
                        : new List<Parameter>()
                }
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
