using Microsoft.Extensions.Logging;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.Model.Configuration.Tenant.Teams;
using PnP.Core.Provisioning.Model.Teams;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TeamAppInstanceModel = PnP.Core.Provisioning.Model.Teams.TeamAppInstance;
using TeamChannelMessageModel = PnP.Core.Provisioning.Model.Teams.TeamChannelMessage;
using TeamChannelModel = PnP.Core.Provisioning.Model.Teams.TeamChannel;
using TeamModel = PnP.Core.Provisioning.Model.Teams.Team;
using TeamSecurityModel = PnP.Core.Provisioning.Model.Teams.TeamSecurity;
using TeamSecurityUserModel = PnP.Core.Provisioning.Model.Teams.TeamSecurityUser;
using TeamTabModel = PnP.Core.Provisioning.Model.Teams.TeamTab;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// The extract half of <see cref="ObjectTeams"/>: reads existing teams through Microsoft Graph, with
    /// their settings, channels, tabs, apps and membership.
    /// </summary>
    internal partial class ObjectTeams
    {
        /// <summary>
        /// How many messages of a channel an extract takes at most, when it is asked for them.
        /// </summary>
        private const int MessagesPerChannel = 50;

        /// <summary>
        /// The sequence handler of the same extract. When the configuration names no team sites, the
        /// teams behind the site collections it took are extracted, as PnP Framework does.
        /// </summary>
        internal ObjectHierarchySequenceSites SequenceSites { get; set; }

        public override bool WillExtract(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ExtractConfiguration configuration)
        {
            ExtractTeamsConfiguration teams = configuration?.Tenant?.Teams;

            _willExtract ??= teams != null
                && (teams.IncludeAllTeams
                    || teams.TeamSiteUrls?.Count > 0
                    || (SequenceSites != null && configuration.Tenant.Sequence?.SiteUrls?.Count > 0));

            return _willExtract.Value;
        }

        public override async Task<ProvisioningHierarchy> ExtractObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            ExtractConfiguration configuration)
        {
            ExtractTeamsConfiguration teamsConfiguration = configuration?.Tenant?.Teams;

            if (teamsConfiguration == null)
            {
                return hierarchy;
            }

            using (context.Logger?.BeginScope(Name))
            {
                var wanted = new List<WantedTeam>();

                if (teamsConfiguration.IncludeAllTeams)
                {
                    foreach (string groupId in await AllTeamGroupIdsAsync(context).ConfigureAwait(false))
                    {
                        Want(wanted, groupId, named: false);
                    }
                }

                if (teamsConfiguration.TeamSiteUrls?.Count > 0)
                {
                    foreach (string siteUrl in teamsConfiguration.TeamSiteUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                    {
                        string groupId = await GroupIdOfSiteAsync(context, siteUrl.Trim()).ConfigureAwait(false);

                        if (groupId != null)
                        {
                            Want(wanted, groupId, named: true);
                        }
                    }
                }
                else if (SequenceSites != null)
                {
                    foreach (Guid groupId in SequenceSites.ExtractedGroupIds)
                    {
                        Want(wanted, groupId.ToString(), named: false);
                    }
                }

                for (int i = 0; i < wanted.Count; i++)
                {
                    WriteSubProgress(Name, $"Extracting team {wanted[i].GroupId}", i + 1, wanted.Count);

                    TeamModel team = await ExtractTeamAsync(context, wanted[i], teamsConfiguration).ConfigureAwait(false);

                    if (team != null)
                    {
                        hierarchy.Teams.Teams.Add(team);
                    }
                }

                return hierarchy;
            }
        }

        #region Which teams

        private static void Want(List<WantedTeam> wanted, string groupId, bool named)
        {
            WantedTeam existing = wanted.FirstOrDefault(w => string.Equals(w.GroupId, groupId, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                wanted.Add(new WantedTeam { GroupId = groupId, Named = named });
            }
            else
            {
                existing.Named |= named;
            }
        }

        /// <summary>
        /// The ids of every group in the tenant which has a team.
        /// </summary>
        private async Task<List<string>> AllTeamGroupIdsAsync(PnPContext context)
        {
            try
            {
                string filter = Uri.EscapeDataString("resourceProvisioningOptions/Any(x:x eq 'Team')");

                List<JsonElement> groups = await GraphItemsAsync(context, ApiRequestType.Graph,
                    $"groups?$filter={filter}&$select=id").ConfigureAwait(false);

                return groups.Select(g => StringOf(g, "id")).Where(id => !string.IsNullOrEmpty(id)).ToList();
            }
            catch (Exception ex)
            {
                Warn(context, $"Every team of the tenant was asked for, but the teams could not be listed: " +
                    ErrorText.Describe(ex));
                return new List<string>();
            }
        }

        /// <summary>
        /// The id of the Microsoft 365 group behind a site, which is also the id of its team.
        /// </summary>
        /// <returns>The group id, or <c>null</c> when the site has no group or could not be read</returns>
        private async Task<string> GroupIdOfSiteAsync(PnPContext context, string siteUrl)
        {
            try
            {
                Uri url = Uri.TryCreate(siteUrl, UriKind.Absolute, out Uri absolute)
                    ? absolute
                    : new Uri(new Uri($"{context.Uri.Scheme}://{context.Uri.Host}"),
                        siteUrl.StartsWith("/", StringComparison.Ordinal) ? siteUrl : $"/{siteUrl}");

                using (PnPContext siteContext = await context.CloneAsync(url).ConfigureAwait(false))
                {
                    await siteContext.Site.LoadAsync(s => s.GroupId).ConfigureAwait(false);

                    if (siteContext.Site.GroupId == Guid.Empty)
                    {
                        Warn(context, $"{url} is not connected to a Microsoft 365 group, so it has no team to extract.");
                        return null;
                    }

                    return siteContext.Site.GroupId.ToString();
                }
            }
            catch (Exception ex)
            {
                Warn(context, $"The team of {siteUrl} could not be looked up, so it was not extracted: {ErrorText.Describe(ex)}");
                return null;
            }
        }

        #endregion

        #region Team

        /// <summary>
        /// Reads one team. A part of it that cannot be read is reported and left out, rather than costing
        /// the whole team.
        /// </summary>
        /// <returns>The team, or <c>null</c> when the group has no team or it could not be read</returns>
        private async Task<TeamModel> ExtractTeamAsync(PnPContext context, WantedTeam wanted,
            ExtractTeamsConfiguration teamsConfiguration)
        {
            string groupId = wanted.GroupId;
            TeamModel team;

            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.Graph, $"teams/{groupId}")).ConfigureAwait(false);

                team = ParseTeam(response.Response);
            }
            catch (Exception ex) when (IsNotFound(ex))
            {
                if (wanted.Named)
                {
                    Warn(context, $"The group {groupId} has no team, so none was extracted.");
                }
                else
                {
                    context.Logger?.LogDebug("{Source}: the group {GroupId} has no team", Constants.LOGGING_SOURCE, groupId);
                }

                return null;
            }
            catch (Exception ex)
            {
                Warn(context, $"The team {groupId} could not be read, so it was not extracted: {ErrorText.Describe(ex)}");
                return null;
            }

            if (team == null)
            {
                return null;
            }

            team.MailNickname = await MailNicknameAsync(context, groupId).ConfigureAwait(false);

            if (teamsConfiguration.IncludeGroupId)
            {
                team.GroupId = groupId;
            }

            try
            {
                await ExtractChannelsAsync(context, groupId, team, teamsConfiguration.IncludeMessages).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(context, $"The channels of the team '{team.DisplayName}' could not be read, so the template " +
                    $"has none of them: {ErrorText.Describe(ex)}");
            }

            try
            {
                await ExtractAppsAsync(context, groupId, team).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(context, $"The apps installed in the team '{team.DisplayName}' could not be read, so the " +
                    $"template has none of them: {ErrorText.Describe(ex)}");
            }

            try
            {
                team.Security = await ExtractSecurityAsync(context, groupId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(context, $"The owners and members of the team '{team.DisplayName}' could not be read, so the " +
                    $"template has none of them: {ErrorText.Describe(ex)}");
            }

            return team;
        }

        /// <summary>
        /// Turns Graph's <c>team</c> resource into the template's team.
        /// </summary>
        internal static TeamModel ParseTeam(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var team = new TeamModel
                {
                    DisplayName = StringOf(root, "displayName"),
                    Description = StringOf(root, "description"),
                    Classification = StringOf(root, "classification"),
                    Archived = BoolOf(root, "isArchived") ?? false,
                };

                string visibility = StringOf(root, "visibility");

                if (string.Equals(visibility, "public", StringComparison.OrdinalIgnoreCase))
                {
                    team.Visibility = TeamVisibility.Public;
                }
                else if (!string.IsNullOrEmpty(visibility))
                {
                    team.Visibility = TeamVisibility.Private;
                    team.HiddenGroupMembershipEnabled = string.Equals(visibility, "hiddenMembership", StringComparison.OrdinalIgnoreCase);
                }

                // "none" is left out rather than written, so the template asks for no specialization at all.
                string specialization = StringOf(root, "specialization");

                if (!string.IsNullOrEmpty(specialization)
                    && !string.Equals(specialization, "none", StringComparison.OrdinalIgnoreCase)
                    && Enum.TryParse(specialization, true, out TeamSpecialization parsed))
                {
                    team.Specialization = parsed;
                }

                if (ObjectOf(root, "funSettings") is JsonElement fun)
                {
                    team.FunSettings = new TeamFunSettings
                    {
                        AllowGiphy = BoolOf(fun, "allowGiphy") ?? false,
                        GiphyContentRating = StringOf(fun, "giphyContentRating"),
                        AllowStickersAndMemes = BoolOf(fun, "allowStickersAndMemes") ?? false,
                        AllowCustomMemes = BoolOf(fun, "allowCustomMemes") ?? false,
                    };
                }

                if (ObjectOf(root, "guestSettings") is JsonElement guest)
                {
                    team.GuestSettings = new TeamGuestSettings
                    {
                        AllowCreateUpdateChannels = BoolOf(guest, "allowCreateUpdateChannels") ?? false,
                        AllowDeleteChannels = BoolOf(guest, "allowDeleteChannels") ?? false,
                    };
                }

                if (ObjectOf(root, "memberSettings") is JsonElement member)
                {
                    team.MemberSettings = new TeamMemberSettings
                    {
                        AllowCreateUpdateChannels = BoolOf(member, "allowCreateUpdateChannels") ?? false,
                        AllowDeleteChannels = BoolOf(member, "allowDeleteChannels") ?? false,
                        AllowAddRemoveApps = BoolOf(member, "allowAddRemoveApps") ?? false,
                        AllowCreateUpdateRemoveTabs = BoolOf(member, "allowCreateUpdateRemoveTabs") ?? false,
                        AllowCreateUpdateRemoveConnectors = BoolOf(member, "allowCreateUpdateRemoveConnectors") ?? false,
                        AllowCreatePrivateChannels = BoolOf(member, "allowCreatePrivateChannels") ?? false,
                    };
                }

                if (ObjectOf(root, "messagingSettings") is JsonElement messaging)
                {
                    team.MessagingSettings = new TeamMessagingSettings
                    {
                        AllowUserEditMessages = BoolOf(messaging, "allowUserEditMessages") ?? false,
                        AllowUserDeleteMessages = BoolOf(messaging, "allowUserDeleteMessages") ?? false,
                        AllowOwnerDeleteMessages = BoolOf(messaging, "allowOwnerDeleteMessages") ?? false,
                        AllowTeamMentions = BoolOf(messaging, "allowTeamMentions") ?? false,
                        AllowChannelMentions = BoolOf(messaging, "allowChannelMentions") ?? false,
                    };
                }

                if (ObjectOf(root, "discoverySettings") is JsonElement discovery)
                {
                    team.DiscoverySettings = new TeamDiscoverySettings
                    {
                        ShowInTeamsSearchAndSuggestions = BoolOf(discovery, "showInTeamsSearchAndSuggestions") ?? false,
                    };
                }

                return team;
            }
        }

        /// <summary>
        /// The mail nickname of the team's group, which the apply side finds an existing team by.
        /// </summary>
        private static async Task<string> MailNicknameAsync(PnPContext context, string groupId)
        {
            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.Graph, $"groups/{groupId}?$select=mailNickname")).ConfigureAwait(false);

                if (string.IsNullOrEmpty(response.Response))
                {
                    return null;
                }

                using (JsonDocument document = JsonDocument.Parse(response.Response))
                {
                    return StringOf(document.RootElement, "mailNickname");
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the mail nickname of the group {GroupId} could not be read",
                    Constants.LOGGING_SOURCE, groupId);
                return null;
            }
        }

        #endregion

        #region Channels, tabs and messages

        private async Task ExtractChannelsAsync(PnPContext context, string groupId, TeamModel team, bool includeMessages)
        {
            // The beta endpoint, as PnP Framework uses, because only it returns the moderation settings.
            List<JsonElement> channels = await GraphItemsAsync(context, ApiRequestType.GraphBeta,
                $"teams/{groupId}/channels").ConfigureAwait(false);

            bool readMessages = includeMessages;

            foreach (JsonElement item in channels)
            {
                var channel = new TeamChannelModel
                {
                    ID = StringOf(item, "id"),
                    DisplayName = StringOf(item, "displayName"),
                    // The schema requires a description.
                    Description = StringOf(item, "description") ?? string.Empty,
                    IsFavoriteByDefault = BoolOf(item, "isFavoriteByDefault"),
                    MembershipType = EnumOf(item, "membershipType", MembershipType.Standard),
                };

                if (ObjectOf(item, "moderationSettings") is JsonElement moderation)
                {
                    channel.UserNewMessageRestriction = EnumOf(moderation, "userNewMessageRestriction", UserNewMessageRestriction.Everyone);
                    channel.ReplyRestriction = EnumOf(moderation, "replyRestriction", ReplyRestriction.Everyone);
                    channel.AllowNewMessageFromBots = BoolOf(moderation, "allowNewMessageFromBots") ?? false;
                    channel.AllowNewMessageFromConnectors = BoolOf(moderation, "allowNewMessageFromConnectors") ?? false;
                }

                if (!string.IsNullOrEmpty(channel.ID))
                {
                    try
                    {
                        channel.Tabs.AddRange(await ExtractTabsAsync(context, groupId, channel.ID).ConfigureAwait(false));
                    }
                    catch (Exception ex)
                    {
                        Warn(context, $"The tabs of the channel '{channel.DisplayName}' in '{team.DisplayName}' could not " +
                            $"be read, so the template has none of them: {ErrorText.Describe(ex)}");
                    }

                    if (readMessages)
                    {
                        try
                        {
                            channel.Messages.AddRange(await ExtractMessagesAsync(context, groupId, channel.ID).ConfigureAwait(false));
                        }
                        catch (Exception ex)
                        {
                            // Reading messages needs a permission of its own, so one failure means all of
                            // them would fail - reported once per team rather than once per channel.
                            readMessages = false;
                            Warn(context, $"The channel messages of the team '{team.DisplayName}' could not be read, " +
                                $"so the template has none of them: {ErrorText.Describe(ex)}");
                        }
                    }
                }

                team.Channels.Add(channel);
            }
        }

        private static async Task<List<TeamTabModel>> ExtractTabsAsync(PnPContext context, string groupId, string channelId)
        {
            var tabs = new List<TeamTabModel>();

            foreach (JsonElement item in await GraphItemsAsync(context, ApiRequestType.Graph,
                $"teams/{groupId}/channels/{channelId}/tabs?$expand=teamsApp").ConfigureAwait(false))
            {
                var tab = new TeamTabModel
                {
                    ID = StringOf(item, "id"),
                    DisplayName = StringOf(item, "displayName"),
                    TeamsAppId = ObjectOf(item, "teamsApp") is JsonElement app ? StringOf(app, "id") : null,
                };

                // A tab without a content url, such as the wiki tab, is configured by Teams itself.
                if (ObjectOf(item, "configuration") is JsonElement configuration
                    && !string.IsNullOrEmpty(StringOf(configuration, "contentUrl")))
                {
                    tab.Configuration = new TeamTabConfiguration
                    {
                        EntityId = StringOf(configuration, "entityId") ?? string.Empty,
                        ContentUrl = StringOf(configuration, "contentUrl"),
                        RemoveUrl = StringOf(configuration, "removeUrl") ?? string.Empty,
                        WebsiteUrl = StringOf(configuration, "websiteUrl") ?? string.Empty,
                    };
                }

                tabs.Add(tab);
            }

            return tabs;
        }

        /// <summary>
        /// The channel's messages, oldest first, as the html the apply side posts them with.
        /// </summary>
        /// <remarks>
        /// Only real messages are taken: the events Teams records in a channel, and deleted messages,
        /// cannot be posted again. PnP Framework writes each message as the JSON Graph returns; the
        /// body is written here instead, which the apply side of either engine posts as it is.
        /// </remarks>
        private static async Task<List<TeamChannelMessageModel>> ExtractMessagesAsync(PnPContext context, string groupId,
            string channelId)
        {
            // The beta endpoint, as PnP Framework uses: it still accepts Group.ReadWrite.All, where v1.0 insists
            // on ChannelMessage.Read.All, which an app registered for PnP PowerShell is rarely consented to.
            List<JsonElement> items = await GraphItemsAsync(context, ApiRequestType.GraphBeta,
                $"teams/{groupId}/channels/{channelId}/messages?$top={MessagesPerChannel}", maxPages: 1).ConfigureAwait(false);

            return items
                .Where(m => string.Equals(StringOf(m, "messageType"), "message", StringComparison.OrdinalIgnoreCase))
                .Where(m => string.IsNullOrEmpty(StringOf(m, "deletedDateTime")))
                .OrderBy(m => StringOf(m, "createdDateTime"), StringComparer.Ordinal)
                .Select(m => ObjectOf(m, "body") is JsonElement body ? StringOf(body, "content") : null)
                .Where(content => !string.IsNullOrWhiteSpace(content))
                .Select(content => new TeamChannelMessageModel { Message = content })
                .ToList();
        }

        #endregion

        #region Apps and security

        private static async Task ExtractAppsAsync(PnPContext context, string groupId, TeamModel team)
        {
            foreach (JsonElement item in await GraphItemsAsync(context, ApiRequestType.Graph,
                $"teams/{groupId}/installedApps?$expand=teamsAppDefinition").ConfigureAwait(false))
            {
                string appId = ObjectOf(item, "teamsAppDefinition") is JsonElement definition
                    ? StringOf(definition, "teamsAppId")
                    : null;

                if (!string.IsNullOrEmpty(appId) && !team.Apps.Any(a => a.AppId == appId))
                {
                    team.Apps.Add(new TeamAppInstanceModel { AppId = appId });
                }
            }
        }

        private static async Task<TeamSecurityModel> ExtractSecurityAsync(PnPContext context, string groupId)
        {
            var security = new TeamSecurityModel();

            foreach (string principalName in await PrincipalNamesAsync(context, groupId, "owners").ConfigureAwait(false))
            {
                security.Owners.Add(new TeamSecurityUserModel { UserPrincipalName = principalName });
            }

            foreach (string principalName in await PrincipalNamesAsync(context, groupId, "members").ConfigureAwait(false))
            {
                security.Members.Add(new TeamSecurityUserModel { UserPrincipalName = principalName });
            }

            security.AllowToAddGuests = await AllowToAddGuestsAsync(context, groupId).ConfigureAwait(false);

            return security;
        }

        /// <summary>
        /// The principal names of a group's owners or members. Members which are not users, such as
        /// service principals, have none and are left out.
        /// </summary>
        private static async Task<List<string>> PrincipalNamesAsync(PnPContext context, string groupId, string collection)
        {
            List<JsonElement> principals = await GraphItemsAsync(context, ApiRequestType.Graph,
                $"groups/{groupId}/{collection}?$select=userPrincipalName").ConfigureAwait(false);

            return principals
                .Select(p => StringOf(p, "userPrincipalName"))
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Whether guests may be added to the group.
        /// </summary>
        /// <remarks>
        /// A group without a setting of its own follows the tenant's, which the template cannot express, so
        /// it is recorded as allowing guests: applying that leaves the tenant's policy in charge. PnP
        /// Framework records it as not allowing them, which blocks guests on every team applied from the template.
        /// </remarks>
        private static async Task<bool> AllowToAddGuestsAsync(PnPContext context, string groupId)
        {
            try
            {
                foreach (JsonElement setting in await GraphItemsAsync(context, ApiRequestType.Graph,
                    $"groups/{groupId}/settings").ConfigureAwait(false))
                {
                    if (StringOf(setting, "displayName") != "Group.Unified.Guest"
                        || !setting.TryGetProperty("values", out JsonElement values)
                        || values.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (JsonElement value in values.EnumerateArray())
                    {
                        if (StringOf(value, "name") == "AllowToAddGuests"
                            && bool.TryParse(StringOf(value, "value"), out bool allowed))
                        {
                            return allowed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the guest setting of the group {GroupId} could not be read",
                    Constants.LOGGING_SOURCE, groupId);
            }

            return true;
        }

        #endregion

        #region Graph helpers

        /// <summary>
        /// Reads a Graph collection, following its next links.
        /// </summary>
        /// <param name="context">The context to send the requests with</param>
        /// <param name="type">Which Graph endpoint to read from</param>
        /// <param name="request">The collection's request, relative to the endpoint</param>
        /// <param name="maxPages">How many pages to read at most, or 0 for all of them</param>
        private static async Task<List<JsonElement>> GraphItemsAsync(PnPContext context, ApiRequestType type, string request,
            int maxPages = 0)
        {
            var items = new List<JsonElement>();
            string next = request;
            int pages = 0;

            while (!string.IsNullOrEmpty(next))
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(type, next)).ConfigureAwait(false);

                next = null;
                pages++;

                if (string.IsNullOrEmpty(response.Response))
                {
                    break;
                }

                using (JsonDocument document = JsonDocument.Parse(response.Response))
                {
                    if (document.RootElement.TryGetProperty("value", out JsonElement value) && value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement item in value.EnumerateArray())
                        {
                            items.Add(item.Clone());
                        }
                    }

                    if ((maxPages <= 0 || pages < maxPages)
                        && document.RootElement.TryGetProperty("@odata.nextLink", out JsonElement link)
                        && link.ValueKind == JsonValueKind.String)
                    {
                        next = RelativeGraphRequest(link.GetString());
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// A next link without the Graph endpoint and version in front of it, which is how PnP Core takes
        /// a Graph request.
        /// </summary>
        internal static string RelativeGraphRequest(string nextLink)
        {
            if (string.IsNullOrEmpty(nextLink))
            {
                return null;
            }

            foreach (string version in new[] { "/v1.0/", "/beta/" })
            {
                int at = nextLink.IndexOf(version, StringComparison.OrdinalIgnoreCase);

                if (at >= 0)
                {
                    return nextLink.Substring(at + version.Length);
                }
            }

            return null;
        }

        private static bool IsNotFound(Exception ex)
        {
            for (Exception current = ex; current != null; current = current.InnerException)
            {
                if (current is ServiceException serviceException && serviceException.Error is ServiceError error
                    && error.HttpResponseCode == 404)
                {
                    return true;
                }
            }

            return false;
        }

        private static JsonElement? ObjectOf(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty(name, out JsonElement value)
                && value.ValueKind == JsonValueKind.Object
                ? value
                : (JsonElement?)null;
        }

        private static bool? BoolOf(JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out JsonElement value))
            {
                return null;
            }

            switch (value.ValueKind)
            {
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                default:
                    return null;
            }
        }

        private static TEnum EnumOf<TEnum>(JsonElement element, string name, TEnum fallback) where TEnum : struct
        {
            string value = StringOf(element, name);

            return !string.IsNullOrEmpty(value) && Enum.TryParse(value, true, out TEnum parsed) ? parsed : fallback;
        }

        /// <summary>
        /// A team the extract is to take, and whether the configuration named it: a named one which turns
        /// out to have no team is reported, one found by looking through groups or sites is not.
        /// </summary>
        private sealed class WantedTeam
        {
            internal string GroupId { get; set; }

            internal bool Named { get; set; }
        }

        #endregion
    }
}
