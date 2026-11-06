using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Services;
using System;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using System.Collections.Generic;
using System.Linq;
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

                if (string.IsNullOrEmpty(parsedName))
                {
                    const string unnamed = "The template's theme has no name, and a custom palette has to be "
                        + "registered with the tenant under a name before a site can use it. The theme was not applied.";

                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, unnamed);
                    WriteMessage(unnamed, ProvisioningMessageType.Warning);

                    return parser;
                }

                string palette = parser.ParseString(template.Theme.Palette);

                if (!string.IsNullOrEmpty(palette))
                {
                    try
                    {
                        HashSet<string> existing = await TenantThemes.GetNamesAsync(context).ConfigureAwait(false);

                        if (!existing.Contains(parsedName))
                        {
                            await TenantThemes.AddAsync(context, parsedName, palette, template.Theme.IsInverted).ConfigureAwait(false);
                        }
                        else if (template.Theme.Overwrite)
                        {
                            await TenantThemes.UpdateAsync(context, parsedName, palette, template.Theme.IsInverted).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = $"The theme '{parsedName}' could not be registered with the tenant, which "
                            + $"needs tenant administrator rights: {ErrorText.Describe(ex)}";

                        context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                        WriteMessage(message, ProvisioningMessageType.Warning);

                        return parser;
                    }
                }

                try
                {
                    List<ITheme> available = await branding.GetAvailableThemesAsync().ConfigureAwait(false);

                    ITheme theme = available.FirstOrDefault(t =>
                        string.Equals(t.Name, parsedName, StringComparison.OrdinalIgnoreCase));

                    if (theme == null)
                    {
                        string message = $"The theme '{parsedName}' is not available on this site, so it was not applied.";

                        context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                        WriteMessage(message, ProvisioningMessageType.Warning);

                        return parser;
                    }

                    await branding.SetThemeAsync(theme).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string message = $"The theme '{parsedName}' could not be applied: {ErrorText.Describe(ex)}";

                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);
                }

                return parser;
            }
        }
    }
}
