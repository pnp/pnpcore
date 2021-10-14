using PnP.Core.Transformation.Services.Core;
using System;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a URL mapping provider
    /// </summary>
    public class UserMappingProviderInput : BaseMappingProviderInput
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
