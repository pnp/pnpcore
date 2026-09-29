using Microsoft.Extensions.Logging;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.Model.Configuration.Tenant.Sequence;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ClassicSiteCollectionModel = PnP.Core.Provisioning.Model.ClassicSiteCollection;
using CommunicationSiteCollectionModel = PnP.Core.Provisioning.Model.CommunicationSiteCollection;
using SiteCollectionModel = PnP.Core.Provisioning.Model.SiteCollection;
using TeamNoGroupSiteCollectionModel = PnP.Core.Provisioning.Model.TeamNoGroupSiteCollection;
using TeamSiteCollectionModel = PnP.Core.Provisioning.Model.TeamSiteCollection;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// The extract half of <see cref="ObjectHierarchySequenceSites"/>: describes existing site collections,
    /// and optionally their subsites, as one sequence, and extracts the template of each of them.
    /// </summary>
    internal partial class ObjectHierarchySequenceSites
    {
        /// <summary>
        /// The id of the sequence an extract writes the site collections to, the one PnP Framework uses.
        /// </summary>
        internal const string ExtractedSequenceId = "TENANTSEQUENCE";

        /// <summary>
        /// The tenant admin list which records the hub site every site collection is joined to.
        /// </summary>
        private const string AggregatedSiteCollectionsList = "DO_NOT_DELETE_SPLIST_TENANTADMIN_AGGREGATED_SITECOLLECTIONS";

        private readonly List<Guid> extractedGroupIds = new List<Guid>();

        /// <summary>
        /// The Microsoft 365 groups behind the site collections the extract took. The teams handler of
        /// the same extract falls back on these when the configuration names no teams of its own.
        /// </summary>
        internal IReadOnlyList<Guid> ExtractedGroupIds => extractedGroupIds;

        public override bool WillExtract(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ExtractConfiguration configuration)
        {
            _willExtract ??= configuration?.Tenant?.Sequence?.SiteUrls?.Count > 0;
            return _willExtract.Value;
        }

        public override async Task<ProvisioningHierarchy> ExtractObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            ExtractConfiguration configuration)
        {
            ExtractSequenceConfiguration sequenceConfiguration = configuration?.Tenant?.Sequence;

            if (!(sequenceConfiguration?.SiteUrls?.Count > 0))
            {
                return hierarchy;
            }

            using (context.Logger?.BeginScope(Name))
            {
                List<Uri> siteUrls = RequestedSiteUrls(context, sequenceConfiguration.SiteUrls);

                if (sequenceConfiguration.IncludeJoinedSites)
                {
                    await AddJoinedSitesAsync(context, siteUrls).ConfigureAwait(false);
                }

                var sequence = new ProvisioningSequence { ID = ExtractedSequenceId };
                var extracted = new List<ExtractedSite>();

                for (int i = 0; i < siteUrls.Count; i++)
                {
                    WriteSubProgress(Name, $"Extracting {siteUrls[i]}", i + 1, siteUrls.Count);

                    ExtractedSite site = await ExtractSiteCollectionAsync(context, siteUrls[i], configuration,
                        extracted).ConfigureAwait(false);

                    if (site == null)
                    {
                        continue;
                    }

                    foreach (KeyValuePair<string, string> parameter in site.Parameters)
                    {
                        hierarchy.Parameters[parameter.Key] = parameter.Value;
                    }

                    hierarchy.Templates.AddRange(site.Templates);
                    sequence.SiteCollections.Add(site.Model);
                    extracted.Add(site);
                }

                PointAtExtractedHubs(hierarchy, extracted);

                if (sequence.SiteCollections.Count > 0)
                {
                    hierarchy.Sequences.Add(sequence);
                }

                return hierarchy;
            }
        }

        #region Site collections

        /// <summary>
        /// Describes one site collection, extracts its template and, when asked, those of its subsites.
        /// </summary>
        /// <returns>What was extracted, or <c>null</c> when the site was skipped</returns>
        private async Task<ExtractedSite> ExtractSiteCollectionAsync(PnPContext context, Uri siteUrl,
            ExtractConfiguration configuration, List<ExtractedSite> extracted)
        {
            ISiteCollectionProperties properties;

            try
            {
                properties = await context.GetSiteCollectionManager()
                    .GetSiteCollectionPropertiesAsync(siteUrl).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(context, ex, $"The site {siteUrl} could not be read from the SharePoint admin center, so it was left " +
                    "out of the tenant template. Check that it is a site collection, and that you are a SharePoint " +
                    $"administrator: {ErrorText.Describe(ex)}");
                return null;
            }

            try
            {
                using (PnPContext siteContext = await context.CloneAsync(siteUrl).ConfigureAwait(false))
                {
                    await siteContext.Site.LoadAsync(s => s.Id, s => s.GroupId, s => s.ShareByEmailEnabled,
                        s => s.Classification).ConfigureAwait(false);

                    if (extracted.Any(e => e.SiteId == siteContext.Site.Id))
                    {
                        return null;
                    }

                    var site = new ExtractedSite
                    {
                        SiteId = siteContext.Site.Id,
                        HubSiteId = properties.HubSiteId != siteContext.Site.Id ? properties.HubSiteId : Guid.Empty,
                    };

                    site.Model = await DescribeAsync(siteContext, properties, site).ConfigureAwait(false);

                    if (site.Model == null)
                    {
                        return null;
                    }

                    if (properties.IsHubSite)
                    {
                        await AddHubSettingsAsync(siteContext, site.Model).ConfigureAwait(false);
                    }

                    site.Template = await ExtractTemplateAsync(siteContext, configuration, site.SiteId).ConfigureAwait(false);
                    site.Templates.Add(site.Template);
                    site.Model.Templates.Add(site.Template.Id);

                    ExtractSequenceConfiguration sequenceConfiguration = configuration.Tenant.Sequence;

                    if (sequenceConfiguration.IncludeSubsites)
                    {
                        await AddSubsitesAsync(siteContext, site.Model.Sites, site, configuration, 1,
                            sequenceConfiguration.MaxSubsiteDepth).ConfigureAwait(false);
                    }

                    if (site.Model is TeamSiteCollectionModel && siteContext.Site.GroupId != Guid.Empty
                        && !extractedGroupIds.Contains(siteContext.Site.GroupId))
                    {
                        extractedGroupIds.Add(siteContext.Site.GroupId);
                    }

                    return site;
                }
            }
            catch (Exception ex)
            {
                Warn(context, ex, $"The site {siteUrl} could not be extracted, so it was left out of the tenant " +
                    $"template: {ErrorText.Describe(ex)}");
                return null;
            }
        }

        /// <summary>
        /// Picks the kind of site collection the site is, and fills in what the template records of it.
        /// </summary>
        /// <remarks>
        /// The url and title are written as parameters of the hierarchy rather than as values, the way
        /// PnP Framework does it, so that the template can be applied as a copy under another url.
        /// </remarks>
        private async Task<SiteCollectionModel> DescribeAsync(PnPContext siteContext, ISiteCollectionProperties properties,
            ExtractedSite site)
        {
            string id = site.SiteId.ToString("N");
            string webTemplate = properties.Template ?? string.Empty;

            if (webTemplate.StartsWith("TEAMCHANNEL#", StringComparison.OrdinalIgnoreCase))
            {
                Warn(siteContext, null, $"{properties.Url} is the site of a Teams channel, which is created along with " +
                    "its channel rather than on its own, so it was left out of the tenant template.");
                return null;
            }

            if (siteContext.Site.GroupId != Guid.Empty || string.Equals(webTemplate, "GROUP#0", StringComparison.OrdinalIgnoreCase))
            {
                return await DescribeTeamSiteAsync(siteContext, properties, site, id).ConfigureAwait(false);
            }

            SiteCollectionModel model;

            switch (webTemplate.ToUpperInvariant())
            {
                case "SITEPAGEPUBLISHING#0":
                {
                    var communication = new CommunicationSiteCollectionModel
                    {
                        Language = properties.Lcid,
                        Owner = properties.OwnerEmail,
                        AllowFileSharingForGuestUsers = siteContext.Site.ShareByEmailEnabled,
                    };

                    if (!string.IsNullOrEmpty(siteContext.Site.Classification))
                    {
                        communication.Classification = siteContext.Site.Classification;
                    }

                    site.Parameters[$"SITECOLLECTION_{id}_URL"] = properties.Url;
                    communication.Url = $"{{parameter:SITECOLLECTION_{id}_URL}}";

                    model = communication;
                    break;
                }

                case "STS#3":
                {
                    var noGroup = new TeamNoGroupSiteCollectionModel
                    {
                        Language = properties.Lcid,
                        Owner = properties.OwnerEmail,
                        TimeZoneId = (int)properties.TimeZoneId,
                    };

                    if (!string.IsNullOrEmpty(siteContext.Site.Classification))
                    {
                        noGroup.Classification = siteContext.Site.Classification;
                    }

                    site.Parameters[$"SITECOLLECTION_{id}_URL"] = properties.Url;
                    noGroup.Url = $"{{parameter:SITECOLLECTION_{id}_URL}}";

                    model = noGroup;
                    break;
                }

                default:
                {
                    // PnP Framework stops on any other web template. A classic site collection is the one
                    // kind the schema has for them, and it is created with the web template it names.
                    var classic = new ClassicSiteCollectionModel
                    {
                        WebTemplate = webTemplate,
                        Language = properties.Lcid,
                        Owner = properties.OwnerEmail,
                        TimeZoneId = (int)properties.TimeZoneId,
                    };

                    if (!string.IsNullOrEmpty(siteContext.Site.Classification))
                    {
                        classic.Classification = siteContext.Site.Classification;
                    }

                    site.Parameters[$"SITECOLLECTION_{id}_URL"] = properties.Url;
                    classic.Url = $"{{parameter:SITECOLLECTION_{id}_URL}}";

                    model = classic;
                    break;
                }
            }

            model.IsHubSite = properties.IsHubSite;
            model.Description = properties.Description;

            site.Parameters[$"SITECOLLECTION_{id}_TITLE"] = properties.Title;
            model.Title = $"{{parameter:SITECOLLECTION_{id}_TITLE}}";

            return model;
        }

        /// <summary>
        /// Describes a site connected to a Microsoft 365 group, which is created through its group.
        /// </summary>
        private async Task<SiteCollectionModel> DescribeTeamSiteAsync(PnPContext siteContext,
            ISiteCollectionProperties properties, ExtractedSite site, string id)
        {
            var team = new TeamSiteCollectionModel
            {
                IsHubSite = properties.IsHubSite,
                Description = properties.Description,
                DisplayName = properties.Title,
                Language = properties.Lcid,
            };

            GroupInfo group = await ReadGroupAsync(siteContext, siteContext.Site.GroupId).ConfigureAwait(false);

            site.Parameters[$"SITECOLLECTION_{id}_ALIAS"] = !string.IsNullOrEmpty(group?.Alias)
                ? group.Alias
                : AliasOf(new Uri(properties.Url));
            team.Alias = $"{{parameter:SITECOLLECTION_{id}_ALIAS}}";

            if (group != null)
            {
                if (!string.IsNullOrEmpty(group.Classification))
                {
                    team.Classification = group.Classification;
                }

                team.IsPublic = group.IsPublic;
            }

            team.HideTeamify = await IsTeamifyPromptHiddenAsync(siteContext, new Uri(properties.Url)).ConfigureAwait(false);

            site.Parameters[$"SITECOLLECTION_{id}_TITLE"] = properties.Title;
            team.Title = $"{{parameter:SITECOLLECTION_{id}_TITLE}}";

            return team;
        }

        /// <summary>
        /// Reads the alias, classification and privacy of a site's Microsoft 365 group.
        /// </summary>
        /// <remarks>
        /// Read through SharePoint's directory session rather than Microsoft Graph, as PnP Framework does,
        /// so that extracting sites needs no Graph permission.
        /// </remarks>
        /// <returns>The group's details, or <c>null</c> when they could not be read</returns>
        private async Task<GroupInfo> ReadGroupAsync(PnPContext siteContext, Guid groupId)
        {
            if (groupId == Guid.Empty)
            {
                return null;
            }

            try
            {
                ApiRequestResponse response = await siteContext.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Get,
                    ApiRequestType.SPORest,
                    $"_api/SP.Directory.DirectorySession/Group('{groupId}')?$select=alias,classification,isPublic",
                    null,
                    new Dictionary<string, string> { ["Accept"] = "application/json;odata=nometadata" }))
                    .ConfigureAwait(false);

                return GroupInfo.Parse(response.Response);
            }
            catch (Exception ex)
            {
                Warn(siteContext, ex, $"The Microsoft 365 group of {siteContext.Uri} could not be read, so its alias " +
                    $"is taken from the site url and it is recorded as private: {ErrorText.Describe(ex)}");
                return null;
            }
        }

        private static async Task<bool> IsTeamifyPromptHiddenAsync(PnPContext siteContext, Uri siteUrl)
        {
            try
            {
                return await siteContext.GetSiteCollectionManager().IsAddTeamsPromptHiddenAsync(siteUrl).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                siteContext.Logger?.LogDebug(ex, "{Source}: whether {Site} hides the Add Microsoft Teams prompt could not be read",
                    Constants.LOGGING_SOURCE, siteUrl);
                return false;
            }
        }

        /// <summary>
        /// Records the title and logo a hub site shows.
        /// </summary>
        private async Task AddHubSettingsAsync(PnPContext siteContext, SiteCollectionModel model)
        {
            try
            {
                IHubSite hub = await siteContext.Site.GetHubSiteDataAsync(siteContext.Site.Id).ConfigureAwait(false);

                model.HubSiteTitle = string.IsNullOrEmpty(hub?.Title) ? null : hub.Title;
                model.HubSiteLogoUrl = string.IsNullOrEmpty(hub?.LogoUrl) ? null : hub.LogoUrl;
            }
            catch (Exception ex)
            {
                Warn(siteContext, ex, $"The hub settings of {siteContext.Uri} could not be read, so the template registers " +
                    $"it as a hub without its title and logo: {ErrorText.Describe(ex)}");
            }
        }

        /// <summary>
        /// Extracts a site's template, under the id the site collection or subsite refers to it by.
        /// </summary>
        private static async Task<ProvisioningTemplate> ExtractTemplateAsync(PnPContext siteContext,
            ExtractConfiguration configuration, Guid id)
        {
            ProvisioningTemplate template = await siteContext.GetProvisioningManager()
                .GetTemplateAsync(configuration).ConfigureAwait(false);

            template.Id = $"TEMPLATE-{id:N}";

            return template;
        }

        /// <summary>
        /// Makes a site joined to a hub which is part of the same extract refer to the hub by the hub's url
        /// parameter, so the association follows the hub when the template is applied under other urls.
        /// </summary>
        /// <remarks>
        /// A hub outside the extract keeps the url the web settings handler recorded, rather than a
        /// parameter nothing defines, which is what PnP Framework writes.
        /// </remarks>
        private static void PointAtExtractedHubs(ProvisioningHierarchy hierarchy, List<ExtractedSite> extracted)
        {
            foreach (ExtractedSite site in extracted.Where(s => s.HubSiteId != Guid.Empty && s.Template?.WebSettings != null))
            {
                string parameter = $"SITECOLLECTION_{site.HubSiteId:N}_URL";

                if (hierarchy.Parameters.ContainsKey(parameter))
                {
                    site.Template.WebSettings.HubSiteUrl = $"{{parameter:{parameter}}}";
                }
            }
        }

        #endregion

        #region Subsites

        /// <summary>
        /// Adds the subsites of a web, and theirs in turn down to the configured depth.
        /// </summary>
        /// <param name="parentContext">A context for the web whose subsites to add</param>
        /// <param name="target">The collection of that web's subsites in the sequence</param>
        /// <param name="site">The site collection being extracted, which collects the templates and parameters</param>
        /// <param name="configuration">How to extract the template of each subsite</param>
        /// <param name="depth">The depth of the subsites being added, 1 being those of the site collection's root web</param>
        /// <param name="maxDepth">The deepest subsites to add, or 0 for all of them</param>
        private async Task AddSubsitesAsync(PnPContext parentContext, SubSiteCollection target, ExtractedSite site,
            ExtractConfiguration configuration, int depth, int maxDepth)
        {
            await parentContext.Web.LoadAsync(w => w.Webs.QueryProperties(s => s.Url)).ConfigureAwait(false);

            List<Uri> subwebUrls = parentContext.Web.Webs.AsRequested().Select(w => w.Url).ToList();

            foreach (Uri subwebUrl in subwebUrls)
            {
                try
                {
                    using (PnPContext subContext = await parentContext.CloneAsync(subwebUrl).ConfigureAwait(false))
                    {
                        SubSite subSite = await ExtractSubSiteAsync(subContext, site, configuration).ConfigureAwait(false);

                        target.Add(subSite);

                        if (maxDepth <= 0 || depth < maxDepth)
                        {
                            await AddSubsitesAsync(subContext, subSite.Sites, site, configuration, depth + 1, maxDepth)
                                .ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Warn(parentContext, ex, $"The subsite {subwebUrl} could not be extracted, so it was left out of " +
                        $"the tenant template: {ErrorText.Describe(ex)}");
                }
            }
        }

        private static async Task<SubSite> ExtractSubSiteAsync(PnPContext subContext, ExtractedSite site,
            ExtractConfiguration configuration)
        {
            IWeb web = await subContext.Web.GetAsync(w => w.Id, w => w.Url, w => w.Title, w => w.Description,
                w => w.QuickLaunchEnabled, w => w.Language, w => w.HasUniqueRoleAssignments).ConfigureAwait(false);

            IRegionalSettings regionalSettings = await subContext.Web.RegionalSettings.GetAsync(r => r.TimeZone)
                .ConfigureAwait(false);

            ProvisioningTemplate template = await ExtractTemplateAsync(subContext, configuration, web.Id).ConfigureAwait(false);
            site.Templates.Add(template);

            string id = web.Id.ToString("N");

            site.Parameters[$"SUBSITE_{id}_URL"] = LeafOf(web.Url);
            site.Parameters[$"SUBSITE_{id}_TITLE"] = web.Title;

            var subSite = new TeamNoGroupSubSite
            {
                Url = $"{{parameter:SUBSITE_{id}_URL}}",
                Title = $"{{parameter:SUBSITE_{id}_TITLE}}",
                QuickLaunchEnabled = web.QuickLaunchEnabled,
                Description = web.Description,
                Language = web.Language,
                TimeZoneId = regionalSettings.TimeZone?.Id ?? 0,
                UseSamePermissionsAsParentSite = !web.HasUniqueRoleAssignments,
            };

            subSite.Templates.Add(template.Id);

            return subSite;
        }

        /// <summary>
        /// The last segment of a subsite's url with its leading slash, which is how PnP Framework writes a
        /// subsite's url: relative to the web above it.
        /// </summary>
        private static string LeafOf(Uri url)
        {
            string path = Uri.UnescapeDataString(url.AbsolutePath).TrimEnd('/');

            return path.Substring(path.LastIndexOf('/'));
        }

        #endregion

        #region Joined sites

        /// <summary>
        /// Adds the sites joined to each hub site in the list, after the ones already in it.
        /// </summary>
        private async Task AddJoinedSitesAsync(PnPContext context, List<Uri> siteUrls)
        {
            foreach (Uri siteUrl in siteUrls.ToList())
            {
                try
                {
                    using (PnPContext siteContext = await context.CloneAsync(siteUrl).ConfigureAwait(false))
                    {
                        await siteContext.Site.LoadAsync(s => s.Id, s => s.IsHubSite).ConfigureAwait(false);

                        if (!siteContext.Site.IsHubSite)
                        {
                            continue;
                        }

                        foreach (Uri joined in await JoinedSiteUrlsAsync(context, siteContext.Site.Id).ConfigureAwait(false))
                        {
                            if (!siteUrls.Any(u => IsSameSite(u, joined)))
                            {
                                siteUrls.Add(joined);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Warn(context, ex, $"The sites joined to {siteUrl} could not be listed, so they were left out of the " +
                        $"tenant template. Listing them needs read access to the SharePoint admin center: {ErrorText.Describe(ex)}");
                }
            }
        }

        /// <summary>
        /// The urls of the site collections joined to a hub, from the list the SharePoint admin center
        /// keeps of every site collection. PnP Core has no call of its own for this.
        /// </summary>
        private static async Task<List<Uri>> JoinedSiteUrlsAsync(PnPContext context, Guid hubSiteId)
        {
            var urls = new List<Uri>();

            using (PnPContext admin = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                IList list = await admin.Web.Lists.GetByTitleAsync(AggregatedSiteCollectionsList,
                    l => l.Title,
                    l => l.Fields.QueryProperties(f => f.InternalName, f => f.FieldTypeKind, f => f.TypeAsString, f => f.Title))
                    .ConfigureAwait(false);

                string viewXml =
                    "<View><Query><Where><And>" +
                    $"<Eq><FieldRef Name='HubSiteId' /><Value Type='Guid'>{hubSiteId}</Value></Eq>" +
                    "<And>" +
                    $"<Neq><FieldRef Name='SiteId' /><Value Type='Guid'>{hubSiteId}</Value></Neq>" +
                    "<IsNull><FieldRef Name='TimeDeleted' /></IsNull>" +
                    "</And>" +
                    "</And></Where></Query>" +
                    "<ViewFields><FieldRef Name='SiteUrl' /></ViewFields>" +
                    "<RowLimit Paged='TRUE'>500</RowLimit></View>";

                string nextPage = null;

                do
                {
                    Dictionary<string, object> page = await list.LoadListDataAsStreamAsync(new RenderListDataOptions
                    {
                        ViewXml = viewXml,
                        RenderOptions = RenderListDataOptionsFlags.ListData,
                        Paging = nextPage,
                    }).ConfigureAwait(false);

                    nextPage = page.TryGetValue("NextHref", out object next) ? next?.ToString()?.TrimStart('?') : null;
                }
                while (!string.IsNullOrEmpty(nextPage));

                foreach (IListItem item in list.Items.AsRequested())
                {
                    if (Uri.TryCreate(item["SiteUrl"]?.ToString(), UriKind.Absolute, out Uri url))
                    {
                        urls.Add(url);
                    }
                }
            }

            return urls;
        }

        #endregion

        #region Extract helpers

        /// <summary>
        /// The configured site urls as absolute urls, each once.
        /// </summary>
        private List<Uri> RequestedSiteUrls(PnPContext context, IEnumerable<string> configured)
        {
            var urls = new List<Uri>();

            foreach (string value in configured.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                Uri url;

                try
                {
                    url = AbsoluteUrl(context, value.Trim());
                }
                catch (Exception ex) when (ex is ArgumentException || ex is UriFormatException)
                {
                    Warn(context, null, $"'{value}' is not a site url, so it was left out of the tenant template.");
                    continue;
                }

                if (!urls.Any(u => IsSameSite(u, url)))
                {
                    urls.Add(url);
                }
            }

            return urls;
        }

        private static bool IsSameSite(Uri one, Uri other)
        {
            return string.Equals(one.GetLeftPart(UriPartial.Path).TrimEnd('/'), other.GetLeftPart(UriPartial.Path).TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase);
        }

        private void Warn(PnPContext context, Exception ex, string message)
        {
            context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            WriteMessage(message, ProvisioningMessageType.Warning);
        }

        /// <summary>
        /// A site collection the extract took: what the sequence records of it, and the templates and
        /// parameters it adds to the hierarchy - its own and its subsites'. They are added to the
        /// hierarchy only once all of them were extracted, so a site that fails halfway leaves nothing behind.
        /// </summary>
        private sealed class ExtractedSite
        {
            internal Guid SiteId { get; set; }

            /// <summary>
            /// The hub the site is joined to, or empty when it is joined to none or is that hub itself.
            /// </summary>
            internal Guid HubSiteId { get; set; }

            internal SiteCollectionModel Model { get; set; }

            internal ProvisioningTemplate Template { get; set; }

            internal List<ProvisioningTemplate> Templates { get; } = new List<ProvisioningTemplate>();

            internal Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();
        }

        /// <summary>
        /// The parts of a Microsoft 365 group a team site's description records.
        /// </summary>
        private sealed class GroupInfo
        {
            internal string Alias { get; private set; }

            internal string Classification { get; private set; }

            internal bool IsPublic { get; private set; }

            internal static GroupInfo Parse(string json)
            {
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    JsonElement root = document.RootElement;

                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("d", out JsonElement verbose))
                    {
                        root = verbose;
                    }

                    if (root.ValueKind != JsonValueKind.Object)
                    {
                        return null;
                    }

                    return new GroupInfo
                    {
                        Alias = StringProperty(root, "alias"),
                        Classification = StringProperty(root, "classification"),
                        IsPublic = BoolProperty(root, "isPublic"),
                    };
                }
            }

            private static string StringProperty(JsonElement element, string name)
            {
                JsonElement? value = Property(element, name);

                return value?.ValueKind == JsonValueKind.String ? value.Value.GetString() : null;
            }

            private static bool BoolProperty(JsonElement element, string name)
            {
                JsonElement? value = Property(element, name);

                return value?.ValueKind == JsonValueKind.True
                    || (value?.ValueKind == JsonValueKind.String && bool.TryParse(value.Value.GetString(), out bool parsed) && parsed);
            }

            /// <summary>
            /// A property by name, ignoring its case: the directory session answers in camel case, but not
            /// every version of it did.
            /// </summary>
            private static JsonElement? Property(JsonElement element, string name)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return property.Value;
                    }
                }

                return null;
            }
        }

        #endregion
    }
}
