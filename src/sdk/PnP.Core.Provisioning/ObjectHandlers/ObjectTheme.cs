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

                if (!string.IsNullOrEmpty(parsedName) && Enum.TryParse(parsedName, true, out SharePointTheme builtInTheme))
                {
                    await branding.SetThemeAsync(builtInTheme).ConfigureAwait(false);
                    return parser;
                }

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
