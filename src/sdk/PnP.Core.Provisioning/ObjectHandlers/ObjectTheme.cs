using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies a theme to the web.
    /// </summary>
    internal class ObjectTheme : ObjectHandlerBase
    {
        public override string Name => "Theme";

        public override string InternalName => "Themes";

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            // Deliberately never extracted - see the remarks on the class.
            return false;
        }

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return template.Theme != null;
        }

        public override Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return Task.FromResult(template);
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                if (template.Theme == null)
                {
                    return parser;
                }

                string parsedName = parser.ParseString(template.Theme.Name);
                IBrandingManager branding = context.Web.GetBrandingManager();

                // 1. A built-in theme, named in the template.
                if (!string.IsNullOrEmpty(parsedName) && Enum.TryParse(parsedName, true, out SharePointTheme builtInTheme))
                {
                    await branding.SetThemeAsync(builtInTheme).ConfigureAwait(false);
                    return parser;
                }

                // 2 and 3. An inline palette, or a tenant-defined theme named in the template.
                //
                // MIGRATION PHASE 8: both are blocked on the same gap. IBrandingManager offers
                // SetThemeAsync(SharePointTheme) for built-ins and SetThemeAsync(ITheme) for
                // everything else - and there is no way to obtain an ITheme by name, nor to
                // register a palette as one. Both need tenant theme CRUD, which is backlog T13 and
                // lands in phase 8 with ObjectTenant.
                //
                // Reported rather than silently skipped: leaving a site unthemed while claiming
                // success is the failure mode this whole migration keeps running into.
                string reason = !string.IsNullOrEmpty(template.Theme.Palette)
                    ? "specifies an inline palette"
                    : "names a tenant theme";

                string message = string.Format(System.Globalization.CultureInfo.CurrentCulture,
                    "The theme '{0}' {1}, which requires tenant theme management (backlog T13, phase 8). " +
                    "Only the built-in SharePoint themes can be applied until then - this theme was NOT applied.",
                    template.Theme.Name ?? "<unnamed>", reason);

                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);

                return parser;
            }
        }
    }
}
