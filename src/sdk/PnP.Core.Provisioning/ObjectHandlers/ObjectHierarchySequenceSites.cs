using Microsoft.Extensions.Logging;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassicSiteCollectionModel = PnP.Core.Provisioning.Model.ClassicSiteCollection;
using CommunicationSiteCollectionModel = PnP.Core.Provisioning.Model.CommunicationSiteCollection;
using SiteCollectionModel = PnP.Core.Provisioning.Model.SiteCollection;
using TeamNoGroupSiteCollectionModel = PnP.Core.Provisioning.Model.TeamNoGroupSiteCollection;
using TeamSiteCollectionModel = PnP.Core.Provisioning.Model.TeamSiteCollection;
using TimeZone = PnP.Core.Admin.Model.SharePoint.TimeZone;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Creates the site collections a tenant template's sequence declares, and applies the templates
    /// each of them names.
    /// </summary>
    internal class ObjectHierarchySequenceSites : ObjectHierarchyHandlerBase
    {
        public override string Name => "Sequences";

        public override bool WillProvision(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ApplyConfiguration configuration)
        {
            _willProvision ??= SequenceOf(hierarchy, sequenceId)?.SiteCollections?.Count > 0;
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ExtractConfiguration configuration)
        {
            _willExtract ??= false;
            return _willExtract.Value;
        }

        public override Task<ProvisioningHierarchy> ExtractObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            ExtractConfiguration configuration)
        {
            return Task.FromResult(hierarchy);
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            string sequenceId, TokenParser parser, ApplyConfiguration configuration)
        {
            ProvisioningSequence sequence = SequenceOf(hierarchy, sequenceId);

            if (sequence == null || !(sequence.SiteCollections?.Count > 0))
            {
                return parser;
            }

            var created = new List<ProvisionedSite>();

            foreach (SiteCollectionModel siteCollection in sequence.SiteCollections)
            {
                ProvisionedSite site = await EnsureSiteAsync(context, siteCollection, parser, configuration)
                    .ConfigureAwait(false);

                if (site == null)
                {
                    continue;
                }

                created.Add(site);
                parser = await PublishTokensAsync(parser, siteCollection, site).ConfigureAwait(false);
            }

            foreach (ProvisionedSite site in created)
            {
                await ApplyHubSettingsAsync(context, site, parser).ConfigureAwait(false);
            }

            foreach (ProvisionedSite site in created)
            {
                await ApplyTemplatesAsync(hierarchy, site, parser, configuration).ConfigureAwait(false);
            }

            foreach (ProvisionedSite site in created)
            {
                site.Context.Dispose();
            }

            return parser;
        }

        /// <summary>
        /// Creates a site collection, or opens the one that is already there.
        /// </summary>
        private async Task<ProvisionedSite> EnsureSiteAsync(PnPContext context, SiteCollectionModel siteCollection,
            TokenParser parser, ApplyConfiguration configuration)
        {
            string title = parser.ParseString(siteCollection.Title);

            try
            {
                PnPContext siteContext = await OpenExistingAsync(context, siteCollection, parser).ConfigureAwait(false);

                if (siteContext != null)
                {
                    WriteMessage($"Using existing site {siteContext.Uri}", ProvisioningMessageType.Progress);
                }
                else
                {
                    WriteMessage($"Creating site {title}", ProvisioningMessageType.Progress);

                    siteContext = await CreateAsync(context, siteCollection, parser, configuration).ConfigureAwait(false);

                    if (siteContext == null)
                    {
                        return null;
                    }

                    WriteMessage($"Created site {siteContext.Uri}", ProvisioningMessageType.Progress);
                }

                await siteContext.Site.LoadAsync(s => s.Id, s => s.GroupId).ConfigureAwait(false);

                return new ProvisionedSite
                {
                    Model = siteCollection,
                    Context = siteContext,
                };
            }
            catch (Exception ex)
            {
                string warning = $"The site '{title}' could not be provisioned, so it and the templates " +
                    $"attached to it were skipped: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return null;
            }
        }

        /// <summary>
        /// Opens the site if it already exists.
        /// </summary>
        private static async Task<PnPContext> OpenExistingAsync(PnPContext context, SiteCollectionModel siteCollection,
            TokenParser parser)
        {
            ISiteCollectionManager manager = context.GetSiteCollectionManager();

            if (siteCollection is TeamSiteCollectionModel team)
            {
                string alias = SanitizeAlias(parser.ParseString(team.Alias));

                ISiteCollectionWithDetails match = (await manager.GetSiteCollectionsWithDetailsAsync()
                    .ConfigureAwait(false))
                    .FirstOrDefault(s => string.Equals(AliasOf(s.Url), alias, StringComparison.OrdinalIgnoreCase));

                return match == null ? null : await context.CloneAsync(match.Url).ConfigureAwait(false);
            }

            Uri url = UrlOf(siteCollection, parser);

            if (url == null || !await manager.SiteExistsAsync(url).ConfigureAwait(false))
            {
                return null;
            }

            return await context.CloneAsync(url).ConfigureAwait(false);
        }

        private static async Task<PnPContext> CreateAsync(PnPContext context, SiteCollectionModel siteCollection,
            TokenParser parser, ApplyConfiguration configuration)
        {
            ISiteCollectionManager manager = context.GetSiteCollectionManager();

            var creationOptions = new SiteCreationOptions
            {
                UsingApplicationPermissions = false,

                WaitForAsyncProvisioning = true,
            };

            CommonSiteOptions options = BuildOptions(siteCollection, parser);

            return options == null
                ? null
                : await manager.CreateSiteCollectionAsync(options, creationOptions).ConfigureAwait(false);
        }

        /// <summary>
        /// Turns a template's site collection into the options PnP Core creates from.
        /// </summary>
        private static CommonSiteOptions BuildOptions(SiteCollectionModel siteCollection, TokenParser parser)
        {
            string title = parser.ParseString(siteCollection.Title);
            string description = parser.ParseString(siteCollection.Description);

            switch (siteCollection)
            {
                case TeamSiteCollectionModel team:
                {
                    var options = new TeamSiteOptions(SanitizeAlias(parser.ParseString(team.Alias)), title)
                    {
                        Description = description,
                        Classification = parser.ParseString(team.Classification),
                        IsPublic = team.IsPublic,
                        Language = LanguageOf(team.Language),
                    };

                    if (Guid.TryParse(parser.ParseString(team.SiteDesign), out Guid designId))
                    {
                        options.SiteDesignId = designId;
                    }

                    return options;
                }

                case CommunicationSiteCollectionModel communication:
                {
                    var options = new CommunicationSiteOptions(
                        new Uri(parser.ParseString(communication.Url)), title)
                    {
                        Description = description,
                        Classification = parser.ParseString(communication.Classification),
                        Owner = parser.ParseString(communication.Owner),
                        ShareByEmailEnabled = communication.AllowFileSharingForGuestUsers,
                        Language = LanguageOf(communication.Language),
                    };

                    string design = parser.ParseString(communication.SiteDesign);

                    if (Guid.TryParse(design, out Guid customDesign))
                    {
                        options.SiteDesignId = customDesign;
                    }
                    else if (Enum.TryParse(design, true, out CommunicationSiteDesign builtIn))
                    {
                        options.SiteDesign = builtIn;
                    }

                    return options;
                }

                case TeamNoGroupSiteCollectionModel noGroup:
                {
                    return new TeamSiteWithoutGroupOptions(
                        new Uri(parser.ParseString(noGroup.Url)), title)
                    {
                        Description = description,
                        Classification = parser.ParseString(noGroup.Classification),
                        Owner = parser.ParseString(noGroup.Owner),
                        Language = LanguageOf(noGroup.Language),
                        TimeZone = TimeZoneOf(noGroup.TimeZoneId),
                    };
                }

                case ClassicSiteCollectionModel classic:
                {
                    return new ClassicSiteOptions(
                        new Uri(parser.ParseString(classic.Url)),
                        title,
                        parser.ParseString(classic.WebTemplate),
                        parser.ParseString(classic.Owner),
                        LanguageOf(classic.Language),
                        TimeZoneOf(classic.TimeZoneId) ?? TimeZone.UTCPLUS0100_BRUSSELS_COPENHAGEN_MADRID_PARIS);
                }

                default:
                    return null;
            }
        }

        #endregion

        #region Tokens

        /// <summary>
        /// Publishes the tokens other parts of the hierarchy address this site by.
        /// </summary>
        private static async Task<TokenParser> PublishTokensAsync(TokenParser parser, SiteCollectionModel siteCollection,
            ProvisionedSite site)
        {
            string provisioningId = siteCollection.ProvisioningId;

            if (string.IsNullOrEmpty(provisioningId))
            {
                return parser;
            }

            parser.AddToken(new SequenceSiteUrlUrlToken(site.Context, provisioningId, site.Context.Uri.ToString()));
            parser.AddToken(new SequenceSiteIdToken(site.Context, provisioningId, site.Context.Site.Id));
            parser.AddToken(new SequenceSiteCollectionIdToken(site.Context, provisioningId, site.Context.Site.Id));

            if (site.Context.Site.GroupId != Guid.Empty)
            {
                parser.AddToken(new SequenceSiteGroupIdToken(site.Context, provisioningId, site.Context.Site.GroupId));
            }

            await Task.CompletedTask.ConfigureAwait(false);

            return parser;
        }

        #endregion

        #region Hub sites

        /// <summary>
        /// Registers the site as a hub, or leaves it alone.
        /// </summary>
        private async Task ApplyHubSettingsAsync(PnPContext context, ProvisionedSite site, TokenParser parser)
        {
            if (!site.Model.IsHubSite)
            {
                return;
            }

            try
            {
                IHubSite hub = await ExistingHubAsync(site.Context).ConfigureAwait(false);

                if (hub == null)
                {
                    WriteMessage($"Registering {site.Context.Uri} as a hub site", ProvisioningMessageType.Progress);

                    hub = await site.Context.Site.RegisterHubSiteAsync().ConfigureAwait(false);
                }

                string title = parser.ParseString(site.Model.HubSiteTitle);
                string logo = parser.ParseString(site.Model.HubSiteLogoUrl);

                if (hub != null && (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(logo)))
                {
                    if (!string.IsNullOrEmpty(title))
                    {
                        hub.Title = title;
                    }

                    if (!string.IsNullOrEmpty(logo))
                    {
                        hub.LogoUrl = logo;
                    }

                    await hub.UpdateAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                string warning = $"{site.Context.Uri} could not be registered as a hub site: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private static async Task<IHubSite> ExistingHubAsync(PnPContext siteContext)
        {
            try
            {
                await siteContext.Site.LoadAsync(s => s.IsHubSite).ConfigureAwait(false);

                return siteContext.Site.IsHubSite
                    ? await siteContext.Site.GetHubSiteDataAsync(null).ConfigureAwait(false)
                    : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region Templates

        /// <summary>
        /// Applies the hierarchy's templates that this site names, in the order it names them.
        /// </summary>
        private async Task ApplyTemplatesAsync(ProvisioningHierarchy hierarchy, ProvisionedSite site,
            TokenParser parser, ApplyConfiguration configuration)
        {
            if (!(site.Model.Templates?.Count > 0))
            {
                return;
            }

            foreach (string templateId in site.Model.Templates)
            {
                string id = parser.ParseString(templateId);

                ProvisioningTemplate template = hierarchy.Templates.FirstOrDefault(
                    t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));

                if (template == null)
                {
                    string warning = $"The hierarchy has no template with id '{id}', which " +
                        $"{site.Context.Uri} refers to, so it was skipped.";
                    site.Context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                try
                {
                    WriteMessage($"Applying template {id} to {site.Context.Uri}", ProvisioningMessageType.Progress);

                    template.Connector ??= hierarchy.Connector;

                    var manager = (ProvisioningManager)site.Context.GetProvisioningManager();

                    await manager.ApplyTemplateAsync(template, configuration,
                        calledFromHierarchy: true, parser).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"The template '{id}' could not be applied to {site.Context.Uri}: " +
                        ErrorText.Describe(ex);
                    site.Context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        #endregion

        #region Helpers

        private static ProvisioningSequence SequenceOf(ProvisioningHierarchy hierarchy, string sequenceId)
        {
            if (hierarchy?.Sequences == null)
            {
                return null;
            }

            return string.IsNullOrEmpty(sequenceId)
                ? hierarchy.Sequences.FirstOrDefault()
                : hierarchy.Sequences.FirstOrDefault(s => s.ID == sequenceId);
        }

        private static Uri UrlOf(SiteCollectionModel siteCollection, TokenParser parser)
        {
            string url = siteCollection switch
            {
                CommunicationSiteCollectionModel communication => communication.Url,
                TeamNoGroupSiteCollectionModel noGroup => noGroup.Url,
                ClassicSiteCollectionModel classic => classic.Url,
                _ => null,
            };

            url = parser.ParseString(url);

            return Uri.TryCreate(url, UriKind.Absolute, out Uri parsed) ? parsed : null;
        }

        /// <summary>
        /// The alias part of a group connected site's url.
        /// </summary>
        private static string AliasOf(Uri url)
        {
            string path = url?.AbsolutePath?.TrimEnd('/');

            return string.IsNullOrEmpty(path) ? null : path.Substring(path.LastIndexOf('/') + 1);
        }

        /// <summary>
        /// Removes what SharePoint will not accept in a group alias.
        /// </summary>
        private static string SanitizeAlias(string alias)
        {
            if (string.IsNullOrEmpty(alias))
            {
                return alias;
            }

            return Provisioning.Utilities.UrlUtility.RemoveUnallowedCharacters(
                Provisioning.Utilities.UrlUtility.ReplaceAccentedCharactersWithLatin(alias));
        }

        /// <summary>
        /// Maps an LCID to PnP Core's language, falling back to English.
        /// </summary>
        private static Language LanguageOf(int lcid)
        {
            return Enum.IsDefined(typeof(Language), lcid) ? (Language)lcid : Language.Default;
        }

        private static TimeZone? TimeZoneOf(int id)
        {
            return Enum.IsDefined(typeof(TimeZone), id) ? (TimeZone)id : (TimeZone?)null;
        }

        /// <summary>
        /// A site the sequence created or adopted, and the context bound to it.
        /// </summary>
        private sealed class ProvisionedSite
        {
            internal SiteCollectionModel Model { get; set; }

            internal PnPContext Context { get; set; }
        }

        #endregion
    }
}
