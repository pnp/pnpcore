using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using System.Net.Http;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ComposedLookModel = PnP.Core.Provisioning.Model.ComposedLook;
using CoreList = PnP.Core.Model.SharePoint.IList;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies and reads back the <c>&lt;pnp:ComposedLook&gt;</c> element - a classic site's colour
    /// palette, font scheme, background image and master page.
    /// </summary>
    internal class ObjectComposedLook : ObjectHandlerBase
    {
        /// <summary>
        /// The property bag entry PnP Framework records the applied look in.
        /// </summary>
        private const string ComposedLookInfoKey = "_PnP_ProvisioningTemplateComposedLookInfo";

        public override string Name => "Composed look";

        public override string InternalName => "ComposedLooks";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.ComposedLook != null && !IsEmpty(template.ComposedLook);
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            _willExtract ??= true;
            return _willExtract.Value;
        }

        /// <summary>
        /// Whether the element says nothing worth applying.
        /// </summary>
        private static bool IsEmpty(ComposedLookModel look)
        {
            return string.IsNullOrEmpty(look.ColorFile)
                && string.IsNullOrEmpty(look.FontFile)
                && string.IsNullOrEmpty(look.BackgroundFile)
                && (string.IsNullOrEmpty(look.Name)
                    || look.Name.Equals("Current", StringComparison.OrdinalIgnoreCase));
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            ComposedLookModel look = template.ComposedLook;

            if (look == null || IsEmpty(look))
            {
                return parser;
            }

            if (await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false))
            {
                string warning = "This is a NoScript site, so classic composed looks are not supported. " +
                    "The composed look was skipped; use <pnp:Theme> for a modern theme instead.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return parser;
            }

            string colorFile = parser.ParseString(look.ColorFile);
            string fontFile = parser.ParseString(look.FontFile);
            string backgroundFile = parser.ParseString(look.BackgroundFile);
            string masterUrl = parser.ParseString(template.WebSettings?.MasterPageUrl);

            try
            {
                if (string.IsNullOrEmpty(colorFile) && string.IsNullOrEmpty(fontFile) && string.IsNullOrEmpty(backgroundFile))
                {
                    // A name with no files means an existing entry in the Design Catalog, so the
                    // files come from that entry rather than from the template.
                    (colorFile, fontFile, backgroundFile, string catalogMaster) =
                        await ReadFromCatalogAsync(context, parser.ParseString(look.Name)).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(masterUrl))
                    {
                        masterUrl = catalogMaster;
                    }

                    if (string.IsNullOrEmpty(colorFile))
                    {
                        string warning = $"The composed look '{look.Name}' is not in this site's design catalog, " +
                            "and the template supplies no files of its own, so nothing was applied.";
                        context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                        WriteMessage(warning, ProvisioningMessageType.Warning);
                        return parser;
                    }
                }

                await ApplyMasterPageAsync(context, masterUrl).ConfigureAwait(false);
                await ApplyThemeAsync(context, colorFile, fontFile, backgroundFile).ConfigureAwait(false);
                await RecordAppliedLookAsync(context, look).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The composed look '{look.Name}' could not be applied: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }

            return parser;
        }

        /// <summary>
        /// Applies the palette, font scheme and background image.
        /// </summary>
        private static async Task ApplyThemeAsync(PnPContext context, string colorFile, string fontFile, string backgroundFile)
        {
            string request = "_api/web/applytheme"
                + $"(colorpaletteurl={Quote(colorFile)}"
                + $",fontschemeurl={Quote(fontFile)}"
                + $",backgroundimageurl={Quote(backgroundFile)}"
                + ",sharegenerated=true)";

            await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest, request, null))
                .ConfigureAwait(false);
        }

        private static string Quote(string url)
        {
            return string.IsNullOrWhiteSpace(url)
                ? "null"
                : "'" + url.Replace("'", "''") + "'";
        }

        private static async Task ApplyMasterPageAsync(PnPContext context, string masterUrl)
        {
            if (string.IsNullOrEmpty(masterUrl))
            {
                return;
            }

            IWeb web = context.Web;
            await web.LoadAsync(w => w.MasterUrl, w => w.CustomMasterUrl).ConfigureAwait(false);

            bool dirty = false;

            if (!string.Equals(web.MasterUrl, masterUrl, StringComparison.OrdinalIgnoreCase))
            {
                web.MasterUrl = masterUrl;
                dirty = true;
            }

            if (!string.Equals(web.CustomMasterUrl, masterUrl, StringComparison.OrdinalIgnoreCase))
            {
                web.CustomMasterUrl = masterUrl;
                dirty = true;
            }

            if (dirty)
            {
                await web.UpdateAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Records the applied look so a later extract can recover its name.
        /// </summary>
        private async Task RecordAppliedLookAsync(PnPContext context, ComposedLookModel look)
        {
            try
            {
                IWeb web = context.Web;
                await web.LoadAsync(w => w.AllProperties).ConfigureAwait(false);

                web.AllProperties[ComposedLookInfoKey] = JsonSerializer.Serialize(new StoredComposedLook
                {
                    Name = look.Name,
                    ColorFile = look.ColorFile,
                    FontFile = look.FontFile,
                    BackgroundFile = look.BackgroundFile,
                    Version = look.Version,
                });

                await web.AllProperties.UpdateAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Losing the record costs the look's name on a later extract, not the look itself.
                context.Logger?.LogDebug(ex, "{Source}: the applied composed look could not be recorded.",
                    Constants.LOGGING_SOURCE);
            }
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            ComposedLookModel recorded = await ReadRecordedLookAsync(context).ConfigureAwait(false);

            if (recorded == null)
            {
                // Nothing recorded, so there is nothing to say. An earlier version emitted
                // Name="Current" here, matching PnP Framework - but "Current" is not a look anyone
                // can apply, and on a modern site there is no composed look at all. Worse, the
                // schema marks ColorFile, FontFile and BackgroundFile as *required*, so an element
                // carrying only a name does not validate: extracting a clean communication site
                // produced a template that no schema-aware tool would accept.
                context.Logger?.LogInformation(
                    "{Source}: this site has no recorded composed look, so none was extracted.",
                    Constants.LOGGING_SOURCE);

                return template;
            }

            template.ComposedLook = recorded;

            return template;
        }

        private async Task<ComposedLookModel> ReadRecordedLookAsync(PnPContext context)
        {
            try
            {
                IWeb web = context.Web;
                await web.LoadAsync(w => w.AllProperties, w => w.Url).ConfigureAwait(false);

                if (!web.AllProperties.Values.TryGetValue(ComposedLookInfoKey, out object raw)
                    || raw == null
                    || string.IsNullOrWhiteSpace(raw.ToString()))
                {
                    return null;
                }

                StoredComposedLook stored = JsonSerializer.Deserialize<StoredComposedLook>(raw.ToString());

                if (stored == null || string.IsNullOrEmpty(stored.Name))
                {
                    return null;
                }

                string webUrl = web.Url.ToString();

                // Empty string rather than null for the three file attributes. The schema marks all
                // three "required", and a look recorded by name alone - which is every built-in
                // theme, since they carry no .spcolor or .spfont of their own - has none of them.
                // Left null the serializer omits the attribute entirely and the template does not
                // validate.
                return new ComposedLookModel
                {
                    Name = stored.Name,
                    ColorFile = Tokenize(stored.ColorFile, webUrl) ?? string.Empty,
                    FontFile = Tokenize(stored.FontFile, webUrl) ?? string.Empty,
                    BackgroundFile = Tokenize(stored.BackgroundFile, webUrl) ?? string.Empty,
                    Version = stored.Version,
                };
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the recorded composed look could not be read.",
                    Constants.LOGGING_SOURCE);
                return null;
            }
        }

        #endregion

        #region Design catalog

        /// <summary>
        /// Reads the file urls of a named entry in the site's Design Catalog.
        /// </summary>
        private static async Task<(string Color, string Font, string Background, string Master)> ReadFromCatalogAsync(
            PnPContext context, string lookName)
        {
            if (string.IsNullOrEmpty(lookName))
            {
                return (null, null, null, null);
            }

            await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.TemplateType))
                .ConfigureAwait(false);

            CoreList catalog = context.Web.Lists.AsRequested()
                .FirstOrDefault(l => l.TemplateType == ListTemplateType.DesignCatalog);

            if (catalog == null)
            {
                return (null, null, null, null);
            }

            string query = "<View><Query><Where><Eq><FieldRef Name=\"Name\"/>" +
                $"<Value Type=\"Text\">{System.Security.SecurityElement.Escape(lookName)}</Value>" +
                "</Eq></Where></Query><RowLimit>1</RowLimit></View>";

            await catalog.LoadItemsByCamlQueryAsync(query).ConfigureAwait(false);

            IListItem item = catalog.Items.AsRequested().FirstOrDefault();

            if (item == null)
            {
                return (null, null, null, null);
            }

            return (
                ServerRelative(item, "ThemeUrl"),
                ServerRelative(item, "FontSchemeUrl"),
                ServerRelative(item, "ImageUrl"),
                ServerRelative(item, "MasterPageUrl"));
        }

        private static string ServerRelative(IListItem item, string fieldName)
        {
            if (!item.Values.TryGetValue(fieldName, out object value) || value == null)
            {
                return null;
            }

            string url = value is IFieldUrlValue urlValue ? urlValue.Url : value.ToString();

            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            // The catalog stores absolute urls; ApplyTheme takes server relative ones, and the
            // difference is not something SharePoint corrects for.
            return Uri.TryCreate(url, UriKind.Absolute, out Uri absolute)
                ? Uri.UnescapeDataString(absolute.AbsolutePath)
                : Uri.UnescapeDataString(url);
        }

        #endregion

        /// <summary>
        /// The shape written into the property bag.
        /// </summary>
        private sealed class StoredComposedLook
        {
            public string Name { get; set; }

            public string ColorFile { get; set; }

            public string FontFile { get; set; }

            public string BackgroundFile { get; set; }

            public int Version { get; set; }
        }
    }
}
