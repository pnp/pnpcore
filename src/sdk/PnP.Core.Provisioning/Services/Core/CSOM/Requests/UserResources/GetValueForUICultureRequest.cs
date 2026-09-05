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
    /// Reads the localized value of a resource property for one culture.
    /// </summary>
    internal sealed class GetValueForUICultureRequest : IRequest<string>
    {
        private readonly UserResourcePath resource;
        private readonly string cultureName;

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="resource">Which resource property on which object</param>
        /// <param name="cultureName">A culture name such as <c>en-US</c> - not an LCID</param>
        internal GetValueForUICultureRequest(UserResourcePath resource, string cultureName)
        {
            this.resource = resource ?? throw new ArgumentNullException(nameof(resource));

            if (string.IsNullOrEmpty(cultureName))
            {
                throw new ArgumentException("A culture name is required.", nameof(cultureName));
            }

            this.cultureName = cultureName;
        }

        public string Result { get; private set; }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        internal int GetValueId { get; private set; }

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

            GetValueId = idProvider.GetActionId();
            result.Add(new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Id = GetValueId,
                    ObjectPathId = resourcePropertyId.ToString(),
                    Name = "GetValueForUICulture",
                    Parameters = new List<Parameter>
                    {
                        new Parameter { Type = "String", Value = cultureName }
                    }
                }
            });

            return result;
        }

        public void ProcessResponse(string response)
        {
            Result = ResponseHelper.ProcessResponse<string>(response, GetValueId);
        }
    }
}
