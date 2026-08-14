using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TeamAppInstanceModel = PnP.Core.Provisioning.Model.Teams.TeamAppInstance;
using TeamChannelMessageModel = PnP.Core.Provisioning.Model.Teams.TeamChannelMessage;
using TeamChannelModel = PnP.Core.Provisioning.Model.Teams.TeamChannel;
using TeamModel = PnP.Core.Provisioning.Model.Teams.Team;
using TeamSecurityUserModel = PnP.Core.Provisioning.Model.Teams.TeamSecurityUser;
using TeamTabModel = PnP.Core.Provisioning.Model.Teams.TeamTab;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// The parts of <see cref="ObjectTeams"/> that configure a team once it exists - membership,
    /// apps, channels, tabs and welcome messages.
    /// </summary>
    internal partial class ObjectTeams
    {
        #region Security

        /// <summary>
        /// Brings the team's owners and members in line with the template.
        /// </summary>
        private async Task ApplySecurityAsync(PnPContext context, TeamModel team, string teamId, TokenParser parser)
        {
            if (team.Security == null)
            {
                return;
            }

            await ApplyMembershipAsync(context, teamId, "owners", team.Security.Owners,
                team.Security.ClearExistingOwners, parser).ConfigureAwait(false);

            await ApplyMembershipAsync(context, teamId, "members", team.Security.Members,
                team.Security.ClearExistingMembers, parser).ConfigureAwait(false);

            await ApplyGuestSettingAsync(context, teamId, team.Security.AllowToAddGuests).ConfigureAwait(false);
        }

        private async Task ApplyMembershipAsync(PnPContext context, string teamId, string collection,
            IEnumerable<TeamSecurityUserModel> wanted, bool clearExisting, TokenParser parser)
        {
            List<TeamSecurityUserModel> declared = wanted?.ToList() ?? new List<TeamSecurityUserModel>();

            if (declared.Count == 0 && !clearExisting)
            {
                return;
            }

            Dictionary<string, string> existing = await ReadMembershipAsync(context, teamId, collection)
                .ConfigureAwait(false);

            var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (TeamSecurityUserModel user in declared)
            {
                string principalName = parser.ParseString(user.UserPrincipalName);

                if (string.IsNullOrWhiteSpace(principalName))
                {
                    continue;
                }

                keep.Add(principalName);

                if (existing.ContainsKey(principalName))
                {
                    continue;
                }

                try
                {
                    var body = new Dictionary<string, object>
                    {
                        ["@odata.id"] = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(principalName)}",
                    };

                    await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.Graph,
                        $"groups/{teamId}/{collection}/$ref", JsonSerializer.Serialize(body))).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn(context, $"'{principalName}' could not be added to the team's {collection}: " +
                        ErrorText.Describe(ex));
                }
            }

            if (!clearExisting)
            {
                return;
            }

            foreach (KeyValuePair<string, string> stale in existing.Where(e => !keep.Contains(e.Key)))
            {
                try
                {
                    await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Delete, ApiRequestType.Graph,
                        $"groups/{teamId}/{collection}/{stale.Value}/$ref", null)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn(context, $"'{stale.Key}' could not be removed from the team's {collection}: " +
                        ErrorText.Describe(ex));
                }
            }
        }

        /// <summary>
        /// The team's current owners or members, keyed by principal name.
        /// </summary>
        private static async Task<Dictionary<string, string>> ReadMembershipAsync(PnPContext context, string teamId,
            string collection)
        {
            var found = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.Graph, $"groups/{teamId}/{collection}?$select=id,userPrincipalName"))
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(response.Response))
                {
                    return found;
                }

                using (JsonDocument document = JsonDocument.Parse(response.Response))
                {
                    if (!document.RootElement.TryGetProperty("value", out JsonElement value)
                        || value.ValueKind != JsonValueKind.Array)
                    {
                        return found;
                    }

                    foreach (JsonElement item in value.EnumerateArray())
                    {
                        string principalName = StringOf(item, "userPrincipalName");
                        string id = StringOf(item, "id");

                        if (principalName != null && id != null)
                        {
                            found[principalName] = id;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return found;
        }

        /// <summary>
        /// Sets whether guests may be added to the team's group.
        /// </summary>
        private async Task ApplyGuestSettingAsync(PnPContext context, string teamId, bool allowToAddGuests)
        {
            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.Graph, $"groups/{teamId}/settings")).ConfigureAwait(false);

                string settingId = null;

                if (!string.IsNullOrEmpty(response.Response))
                {
                    using (JsonDocument document = JsonDocument.Parse(response.Response))
                    {
                        if (document.RootElement.TryGetProperty("value", out JsonElement value)
                            && value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement setting in value.EnumerateArray())
                            {
                                if (StringOf(setting, "displayName") == "Group.Unified.Guest")
                                {
                                    settingId = StringOf(setting, "id");
                                    break;
                                }
                            }
                        }
                    }
                }

                var values = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        ["name"] = "AllowToAddGuests",
                        ["value"] = allowToAddGuests.ToString(),
                    },
                };

                if (settingId == null)
                {
                    string templateId = await GuestSettingTemplateIdAsync(context).ConfigureAwait(false);

                    if (templateId == null)
                    {
                        return;
                    }

                    var body = new Dictionary<string, object>
                    {
                        ["templateId"] = templateId,
                        ["values"] = values,
                    };

                    await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.Graph,
                        $"groups/{teamId}/settings", JsonSerializer.Serialize(body))).ConfigureAwait(false);
                }
                else
                {
                    var body = new Dictionary<string, object> { ["values"] = values };

                    await context.Web.ExecuteRequestAsync(new ApiRequest(new HttpMethod("PATCH"), ApiRequestType.Graph,
                        $"groups/{teamId}/settings/{settingId}", JsonSerializer.Serialize(body))).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Warn(context, $"The team's guest setting could not be applied: {ErrorText.Describe(ex)}");
            }
        }

        private static async Task<string> GuestSettingTemplateIdAsync(PnPContext context)
        {
            ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                ApiRequestType.Graph, "groupSettingTemplates")).ConfigureAwait(false);

            if (string.IsNullOrEmpty(response.Response))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(response.Response))
            {
                if (!document.RootElement.TryGetProperty("value", out JsonElement value)
                    || value.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                foreach (JsonElement template in value.EnumerateArray())
                {
                    if (StringOf(template, "displayName") == "Group.Unified.Guest")
                    {
                        return StringOf(template, "id");
                    }
                }
            }

            return null;
        }

        #endregion

        #region Apps

        /// <summary>
        /// Installs the apps the template names.
        /// </summary>
        private async Task ApplyAppsAsync(PnPContext context, TeamModel team, string teamId, TokenParser parser)
        {
            if (!(team.Apps?.Count > 0))
            {
                return;
            }

            HashSet<string> installed = await InstalledAppIdsAsync(context, teamId).ConfigureAwait(false);

            foreach (TeamAppInstanceModel app in team.Apps)
            {
                string appId = parser.ParseString(app.AppId);

                if (string.IsNullOrWhiteSpace(appId) || installed.Contains(appId))
                {
                    continue;
                }

                try
                {
                    var body = new Dictionary<string, object>
                    {
                        ["teamsApp@odata.bind"] = $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{appId}",
                    };

                    await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.Graph,
                        $"teams/{teamId}/installedApps", JsonSerializer.Serialize(body))).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn(context, $"The app '{appId}' could not be installed: {ErrorText.Describe(ex)}");
                }
            }
        }

        private static async Task<HashSet<string>> InstalledAppIdsAsync(PnPContext context, string teamId)
        {
            var installed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.Graph, $"teams/{teamId}/installedApps?$expand=teamsApp"))
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(response.Response))
                {
                    return installed;
                }

                using (JsonDocument document = JsonDocument.Parse(response.Response))
                {
                    if (!document.RootElement.TryGetProperty("value", out JsonElement value)
                        || value.ValueKind != JsonValueKind.Array)
                    {
                        return installed;
                    }

                    foreach (JsonElement item in value.EnumerateArray())
                    {
                        if (item.TryGetProperty("teamsApp", out JsonElement app))
                        {
                            string id = StringOf(app, "id");

                            if (id != null)
                            {
                                installed.Add(id);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return installed;
        }

        #endregion

        #region Channels

        private async Task ApplyChannelsAsync(PnPContext context, TeamModel team, string teamId, TokenParser parser)
        {
            if (!(team.Channels?.Count > 0))
            {
                return;
            }

            Dictionary<string, string> existing = await ReadChannelsAsync(context, teamId).ConfigureAwait(false);

            foreach (TeamChannelModel channel in team.Channels)
            {
                string displayName = parser.ParseString(channel.DisplayName);

                try
                {
                    string channelId = existing.TryGetValue(displayName, out string found)
                        ? await UpdateChannelAsync(context, teamId, found, channel, parser).ConfigureAwait(false)
                        : await CreateChannelAsync(context, teamId, channel, parser).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(channelId))
                    {
                        continue;
                    }

                    await ApplyTabsAsync(context, teamId, channelId, channel, parser).ConfigureAwait(false);
                    await ApplyMessagesAsync(context, teamId, channelId, channel, parser).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn(context, $"The channel '{displayName}' could not be provisioned: {ErrorText.Describe(ex)}");
                }
            }
        }

        private static async Task<string> CreateChannelAsync(PnPContext context, string teamId,
            TeamChannelModel channel, TokenParser parser)
        {
            var body = new Dictionary<string, object>
            {
                ["displayName"] = parser.ParseString(channel.DisplayName),
                ["description"] = parser.ParseString(channel.Description),
            };

            if (channel.IsFavoriteByDefault != null)
            {
                body["isFavoriteByDefault"] = channel.IsFavoriteByDefault.Value;
            }

            if (channel.MembershipType == Model.Teams.MembershipType.Private)
            {
                body["membershipType"] = "private";

                string owner = await FirstOwnerAsync(context, teamId).ConfigureAwait(false);

                if (owner != null)
                {
                    body["members@odata.bind"] = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["@odata.type"] = "#microsoft.graph.aadUserConversationMember",
                            ["user@odata.bind"] = $"https://graph.microsoft.com/v1.0/users('{owner}')",
                            ["roles"] = new[] { "owner" },
                        },
                    };
                }
            }

            ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                HttpMethod.Post, ApiRequestType.Graph, $"teams/{teamId}/channels",
                JsonSerializer.Serialize(body))).ConfigureAwait(false);

            return IdOf(response.Response);
        }

        private static async Task<string> UpdateChannelAsync(PnPContext context, string teamId, string channelId,
            TeamChannelModel channel, TokenParser parser)
        {
            var body = new Dictionary<string, object>
            {
                ["description"] = parser.ParseString(channel.Description),
            };

            if (channel.IsFavoriteByDefault != null)
            {
                body["isFavoriteByDefault"] = channel.IsFavoriteByDefault.Value;
            }

            await context.Web.ExecuteRequestAsync(new ApiRequest(new HttpMethod("PATCH"), ApiRequestType.Graph,
                $"teams/{teamId}/channels/{channelId}", JsonSerializer.Serialize(body))).ConfigureAwait(false);

            return channelId;
        }

        private static async Task<Dictionary<string, string>> ReadChannelsAsync(PnPContext context, string teamId)
        {
            var channels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.Graph, $"teams/{teamId}/channels?$select=id,displayName")).ConfigureAwait(false);

                foreach (JsonElement item in ArrayOf(response.Response))
                {
                    string name = StringOf(item, "displayName");
                    string id = StringOf(item, "id");

                    if (name != null && id != null)
                    {
                        channels[name] = id;
                    }
                }
            }
            catch (Exception)
            {
            }

            return channels;
        }

        private static async Task<string> FirstOwnerAsync(PnPContext context, string teamId)
        {
            Dictionary<string, string> owners = await ReadMembershipAsync(context, teamId, "owners")
                .ConfigureAwait(false);

            return owners.Values.FirstOrDefault();
        }

        #endregion

        #region Tabs

        /// <summary>
        /// Adds, updates or removes the channel's tabs.
        /// </summary>
        private async Task ApplyTabsAsync(PnPContext context, string teamId, string channelId,
            TeamChannelModel channel, TokenParser parser)
        {
            if (!(channel.Tabs?.Count > 0))
            {
                return;
            }

            Dictionary<string, string> existing = await ReadTabsAsync(context, teamId, channelId).ConfigureAwait(false);

            foreach (TeamTabModel tab in channel.Tabs)
            {
                string displayName = parser.ParseString(tab.DisplayName);
                existing.TryGetValue(displayName, out string tabId);

                try
                {
                    if (tab.Remove)
                    {
                        if (tabId != null)
                        {
                            await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Delete,
                                ApiRequestType.Graph, $"teams/{teamId}/channels/{channelId}/tabs/{tabId}", null))
                                .ConfigureAwait(false);
                        }

                        continue;
                    }

                    var body = new Dictionary<string, object>
                    {
                        ["displayName"] = displayName,
                    };

                    if (tab.Configuration != null)
                    {
                        body["configuration"] = new Dictionary<string, object>
                        {
                            ["entityId"] = parser.ParseString(tab.Configuration.EntityId),
                            ["contentUrl"] = parser.ParseString(tab.Configuration.ContentUrl),
                            ["removeUrl"] = parser.ParseString(tab.Configuration.RemoveUrl),
                            ["websiteUrl"] = parser.ParseString(tab.Configuration.WebsiteUrl),
                        };
                    }

                    if (tabId != null)
                    {
                        await context.Web.ExecuteRequestAsync(new ApiRequest(new HttpMethod("PATCH"),
                            ApiRequestType.Graph, $"teams/{teamId}/channels/{channelId}/tabs/{tabId}",
                            JsonSerializer.Serialize(body))).ConfigureAwait(false);
                    }
                    else
                    {
                        body["teamsApp@odata.bind"] =
                            $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{parser.ParseString(tab.TeamsAppId)}";

                        await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.Graph,
                            $"teams/{teamId}/channels/{channelId}/tabs", JsonSerializer.Serialize(body)))
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Warn(context, $"The tab '{displayName}' could not be provisioned: {ErrorText.Describe(ex)}");
                }
            }
        }

        private static async Task<Dictionary<string, string>> ReadTabsAsync(PnPContext context, string teamId,
            string channelId)
        {
            var tabs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.Graph, $"teams/{teamId}/channels/{channelId}/tabs?$select=id,displayName"))
                    .ConfigureAwait(false);

                foreach (JsonElement item in ArrayOf(response.Response))
                {
                    string name = StringOf(item, "displayName");
                    string id = StringOf(item, "id");

                    if (name != null && id != null)
                    {
                        tabs[name] = id;
                    }
                }
            }
            catch (Exception)
            {
            }

            return tabs;
        }

        #endregion

        #region Messages

        /// <summary>
        /// Posts the channel's welcome messages.
        /// </summary>
        private async Task ApplyMessagesAsync(PnPContext context, string teamId, string channelId,
            TeamChannelModel channel, TokenParser parser)
        {
            if (!(channel.Messages?.Count > 0))
            {
                return;
            }

            foreach (TeamChannelMessageModel message in channel.Messages)
            {
                try
                {
                    var body = new Dictionary<string, object>
                    {
                        ["body"] = new Dictionary<string, object>
                        {
                            ["contentType"] = "html",
                            ["content"] = parser.ParseString(message.Message),
                        },
                    };

                    await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.Graph,
                        $"teams/{teamId}/channels/{channelId}/messages", JsonSerializer.Serialize(body)))
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Warn(context, $"A welcome message could not be posted to '{channel.DisplayName}': " +
                        ErrorText.Describe(ex));
                }
            }
        }

        #endregion

        #region Json helpers

        private static string StringOf(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty(name, out JsonElement value)
                && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static IEnumerable<JsonElement> ArrayOf(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                yield break;
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                if (!document.RootElement.TryGetProperty("value", out JsonElement value)
                    || value.ValueKind != JsonValueKind.Array)
                {
                    yield break;
                }

                foreach (JsonElement item in value.EnumerateArray())
                {
                    yield return item.Clone();
                }
            }
        }

        #endregion
    }
}
