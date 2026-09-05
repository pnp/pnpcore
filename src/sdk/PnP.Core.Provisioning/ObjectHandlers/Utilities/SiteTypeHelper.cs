using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Tells the modern site types apart from classic ones.
    /// </summary>
    internal static class SiteTypeHelper
    {
        /// <summary>Communication site.</summary>
        private const string CommunicationSiteTemplate = "SITEPAGEPUBLISHING";

        /// <summary>Modern, group-connected team site.</summary>
        private const string GroupTeamSiteTemplate = "GROUP";

        /// <summary>
        /// Whether the site collection is a communication site.
        /// </summary>
        internal static async Task<bool> IsCommunicationSiteAsync(PnPContext context)
        {
            return await HasRootWebTemplateAsync(context, CommunicationSiteTemplate).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether the site collection is a modern, group-connected team site.
        /// </summary>
        internal static async Task<bool> IsModernTeamSiteAsync(PnPContext context)
        {
            return await HasRootWebTemplateAsync(context, GroupTeamSiteTemplate).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether the site collection is a classic one - neither a communication site nor a
        /// group-connected team site.
        /// </summary>
        internal static async Task<bool> IsClassicSiteAsync(PnPContext context)
        {
            return !await IsCommunicationSiteAsync(context).ConfigureAwait(false)
                && !await IsModernTeamSiteAsync(context).ConfigureAwait(false);
        }

        private static async Task<bool> HasRootWebTemplateAsync(PnPContext context, string template)
        {
            IWeb rootWeb = await context.Site.RootWeb.GetAsync(w => w.WebTemplate).ConfigureAwait(false);

            return template.Equals(rootWeb.WebTemplate, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
