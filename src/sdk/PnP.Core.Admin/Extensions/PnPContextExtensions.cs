using PnP.Core.Admin.Model.Microsoft365;
using PnP.Core.Admin.Model.SharePoint;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extends the <see cref="PnPContext"/> with additional functionality
    /// </summary>
    public static class PnPContextExtensions
    {
        #region Extend context with SharePoint Admin and Microsoft 365 admin capabilities
        /// <summary>
        /// Extends a <see cref="PnPContext"/> with SharePoint admin functionality
        /// </summary>
        /// <param name="context"><see cref="PnPContext"/> to extend</param>
        /// <returns>An <see cref="ISharePointAdmin"/> instance enabling SharePoint admin operations</returns>
        public static ISharePointAdmin GetSharePointAdmin(this PnPContext context)
        {
            return new SharePointAdmin(context);
        }

        /// <summary>
        /// Extends a <see cref="PnPContext"/> with Microsoft 365 admin functionality
        /// </summary>
        /// <param name="context"><see cref="PnPContext"/> to extend</param>
        /// <returns>An <see cref="IMicrosoft365Admin"/> instance enabling Microsoft 365 admin operations</returns>
        public static IMicrosoft365Admin GetMicrosoft365Admin(this PnPContext context)
        {
            return new Microsoft365Admin(context);
        }

        #endregion
        /*
        #region Access token related extensions
        /// <summary>
        /// Checks if the current access token holds the requested role
        /// </summary>
        /// <param name="context"> used to get an access token</param>
        /// <param name="role">Role to check for</param>
        /// <returns>True if the token has the role, false otherwise</returns>
        public static async Task<bool> AccessTokenHasRoleAsync(this PnPContext context, string role)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return await context.AccessTokenHasRoleAsync(role).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if the current access token holds the requested role
        /// </summary>
        /// <param name="context"> used to get an access token</param>
        /// <param name="role">Role to check for</param>
        /// <returns>True if the token has the role, false otherwise</returns>
        public static bool AccessTokenHasRole(this PnPContext context, string role)
        {
            return AccessTokenHasRoleAsync(context, role).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks if the provided access token holds the requested role
        /// </summary>
        /// <param name="context"> used connect this method, no further use</param>
        /// <param name="accessToken">Accesstoken to inspect</param>
        /// <param name="role">Role to check for</param>
        /// <returns>True if the token has the role, false otherwise</returns>
#pragma warning disable IDE0060 // Remove unused parameter
        public static bool AccessTokenHasRole(this PnPContext context, string accessToken, string role)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return PnPContext.AccessTokenHasRole(accessToken, role);
        }

        /// <summary>
        /// Checks if the current access token holds the requested scope
        /// </summary>
        /// <param name="context"> used to get an access token</param>
        /// <param name="role">Scopee to check for</param>
        /// <returns>True if the token has the scope, false otherwise</returns>
        public static async Task<bool> AccessTokenHasScopeAsync(this PnPContext context, string role)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return await context.AccessTokenHasScopeAsync(role).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if the current access token holds the requested scope
        /// </summary>
        /// <param name="context"> used to get an access token</param>
        /// <param name="role">Scope to check for</param>
        /// <returns>True if the token has the scope, false otherwise</returns>
        public static bool AccessTokenHasScope(this PnPContext context, string role)
        {
            return AccessTokenHasScopeAsync(context, role).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks if the provided access token holds the requested scope
        /// </summary>
        /// <param name="context"> used connect this method, no further use</param>
        /// <param name="accessToken">Accesstoken to inspect</param>
        /// <param name="role">Scope to check for</param>
        /// <returns>True if the token has the scope, false otherwise</returns>
#pragma warning disable IDE0060 // Remove unused parameter
        public static bool AccessTokenHasScope(this PnPContext context, string accessToken, string role)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return PnPContext.AccessTokenHasScope(accessToken, role);
        }

        /// <summary>
        /// Checks if the current access token uses application permissions
        /// </summary>
        /// <param name="context"> used to get an access token</param>
        /// <returns>True if using application permissions, false otherwise</returns>
        public static async Task<bool> AccessTokenUsesApplicationPermissionsAsync(this PnPContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return await context.AccessTokenUsesApplicationPermissionsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if the current access token uses application permissions
        /// </summary>
        /// <param name="context"> used to get an access token</param>
        /// <returns>True if using application permissions, false otherwise</returns>
        public static bool AccessTokenUsesApplicationPermissions(this PnPContext context)
        {
            return AccessTokenUsesApplicationPermissionsAsync(context).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks if the provided access token uses application permissions
        /// </summary>
        /// <param name="context"> used connect this method, no further use</param>
        /// <param name="accessToken">Accesstoken to inspect</param>
        /// <returns>True if using application permissions, false otherwise</returns>
#pragma warning disable IDE0060 // Remove unused parameter
        public static bool AccessTokenUsesApplicationPermissions(this PnPContext context, string accessToken)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return PnPContext.AccessTokenUsesApplicationPermissions(accessToken);
        }
        #endregion
        */
    }
}
