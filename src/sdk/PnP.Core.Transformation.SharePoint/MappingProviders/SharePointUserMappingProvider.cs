using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IUserMappingProvider"/>
    /// </summary>
    public class SharePointUserMappingProvider : IUserMappingProvider
    {
        private ILogger<SharePointUserMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="options">Configuration options</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointUserMappingProvider(ILogger<SharePointUserMappingProvider> logger,
            IOptions<SharePointTransformationOptions> options,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Maps a user UPN from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use, if any</param>
        /// <returns>The output of the mapping activity</returns>
        public async Task<UserMappingProviderOutput> MapUserAsync(UserMappingProviderInput input, CancellationToken token = default)
        {
            logger.LogInformation(
                $"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapUserAsync"
                .CorrelateString(input.Context.Task.Id));

            // Should never happen, but just in case
            if (string.IsNullOrEmpty(input?.UserPrincipalName))
            {
                return new UserMappingProviderOutput { UserPrincipalName = input?.UserPrincipalName };
            }

            // When transforming from SPO without explicit enabling spo to spo
            if (!this.options.Value.ShouldMapUsers)
            {
                return new UserMappingProviderOutput { UserPrincipalName = input?.UserPrincipalName };
            }

            logger.LogDebug(
                SharePointTransformationResources.Debug_UserTransformPrincipalInput
                .CorrelateString(input.Context.Task.Id), 
                input.UserPrincipalName.GetUserName());

            // In case we have a valid list of user mappings
            if (this.options.Value.UserMappings != null && this.options.Value.UserMappings.Count > 0)
            {
                logger.LogInformation(
                    SharePointTransformationResources.Info_UserTransformDefaultMapping
                    .CorrelateString(input.Context.Task.Id),
                    input.UserPrincipalName.GetUserName());

                // Try to find user mapping
                // We don't like mulitple matches, the first match wins
                var userNameToCheck = input.UserPrincipalName.GetUserName();
                var result = input.UserPrincipalName;

                var userMapping = this.options.Value.UserMappings.FirstOrDefault(o => o.SourceUser.Equals(userNameToCheck, StringComparison.InvariantCultureIgnoreCase));
                if (userMapping != null)
                {
                    // Let's get the target user
                    result = userMapping.TargetUser;

                    // Log successfull user mapping
                    logger.LogInformation(
                        SharePointTransformationResources.Info_UserTransformSuccess
                        .CorrelateString(input.Context.Task.Id),
                        userNameToCheck, result);

                    // Ensure user in the target site collection if not yet done
                    await input.Context.Task.TargetContext.Web.EnsureUserAsync(result).ConfigureAwait(false);
                }
                else
                {
                    // Log unsuccessfull user mapping
                    logger.LogInformation(
                        SharePointTransformationResources.Info_UserTransformMappingNotFound
                        .CorrelateString(input.Context.Task.Id),
                        userNameToCheck);
                }

                return new UserMappingProviderOutput { UserPrincipalName = result };
            }

            //Returns original input to pass through where re-mapping is not required
            return new UserMappingProviderOutput { UserPrincipalName = input.UserPrincipalName };
        }
    }
}
