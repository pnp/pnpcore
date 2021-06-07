using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Classic;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a URL mapping provider
    /// </summary>
    public class UserMappingProviderInput : MappingProviderInput
    {
        /// <summary>
        /// Creates an instance for user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userPrincipalName"></param>
        public UserMappingProviderInput(PageTransformationContext context, string userPrincipalName) : base(context)
        {
            UserPrincipalName = userPrincipalName ?? throw new ArgumentNullException(nameof(userPrincipalName));
        }

        /// <summary>
        /// Defines the source UPN to map
        /// </summary>
        public string UserPrincipalName { get; }
    }
}
