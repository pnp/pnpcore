using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint.Core.Public.Extensions
{
    /// <summary>
    /// Web extension methods
    /// </summary>
    public static class WebExtensions
    {
        #region IsNoScriptSite 
        /// <summary>
        /// Checks if this web is configured for NoScript
        /// </summary>
        /// <param name="web">Web to check</param>
        /// <returns>True if set to NoScript, false otherwise</returns>
        public static async Task<bool> IsNoScriptSiteAsync(this IWeb web)
        {
            await web.EnsurePropertiesAsync(w => w.EffectiveBasePermissions).ConfigureAwait(false);

            // Definition of no-script is not having the AddAndCustomizePages permission
            if (!web.EffectiveBasePermissions.Has(PermissionKind.AddAndCustomizePages))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if this web is configured for NoScript
        /// </summary>
        /// <param name="web">Web to check</param>
        /// <returns>True if set to NoScript, false otherwise</returns>
        public static bool IsNoScriptSite(this IWeb web)
        {
            return web.IsNoScriptSiteAsync().GetAwaiter().GetResult();
        }
        #endregion
    }
}
