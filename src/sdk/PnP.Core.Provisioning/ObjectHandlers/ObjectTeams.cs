using Microsoft.Extensions.Logging;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TeamModel = PnP.Core.Provisioning.Model.Teams.Team;
using TeamTemplateModel = PnP.Core.Provisioning.Model.Teams.TeamTemplate;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Creates and configures the Microsoft Teams a tenant template declares.
    /// </summary>
    internal partial class ObjectTeams : ObjectHierarchyHandlerBase
    {
        /// <summary>How long a newly created team is given to become configurable.</summary>
        private static readonly TimeSpan ReadyTimeout = TimeSpan.FromMinutes(5);

        private static readonly TimeSpan ReadyPollInterval = TimeSpan.FromSeconds(5);

        public override string Name => "Teams";

        public override bool WillProvision(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ApplyConfiguration configuration)
        {
            _willProvision ??= hierarchy?.Teams?.Teams?.Count > 0
                || hierarchy?.Teams?.TeamTemplates?.Count > 0;

            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ExtractConfiguration configuration)
        {
            // Deferred with the rest of hierarchy extraction - see ObjectHierarchySequenceSites.
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
            Model.Teams.ProvisioningTeams teams = hierarchy?.Teams;

            if (teams == null)
            {
                return parser;
            }

            int total = (teams.TeamTemplates?.Count ?? 0) + (teams.Teams?.Count ?? 0);
            int progress = 0;

            foreach (TeamTemplateModel teamTemplate in teams.TeamTemplates ?? Enumerable.Empty<TeamTemplateModel>())
            {
                WriteSubProgress("Teams", "Team template", ++progress, total);

                await CreateFromJsonTemplateAsync(context, teamTemplate, parser).ConfigureAwait(false);
            }

            foreach (TeamModel team in teams.Teams ?? Enumerable.Empty<TeamModel>())
            {
                WriteSubProgress("Teams", parser.ParseString(team.DisplayName), ++progress, total);

                await ProvisionTeamAsync(context, hierarchy, team, parser).ConfigureAwait(false);
            }

            WriteMessage("Done processing teams", ProvisioningMessageType.Completed);

            return parser;
        }

        private async Task ProvisionTeamAsync(PnPContext context, ProvisioningHierarchy hierarchy, TeamModel team,
            TokenParser parser)
        {
            string displayName = parser.ParseString(team.DisplayName);

            try
            {
                string teamId = await EnsureTeamAsync(context, team, parser).ConfigureAwait(false);

                if (string.IsNullOrEmpty(teamId))
                {
                    return;
                }

                if (!await WaitUntilReadyAsync(context, teamId).ConfigureAwait(false))
                {
                    Warn(context, $"The team '{displayName}' was created but did not become configurable " +
                        $"within {ReadyTimeout.TotalMinutes} minutes, so its channels, apps and settings were skipped.");
                    return;
                }

                await ApplySecurityAsync(context, team, teamId, parser).ConfigureAwait(false);
                await ApplyAppsAsync(context, team, teamId, parser).ConfigureAwait(false);
                await ApplyChannelsAsync(context, team, teamId, parser).ConfigureAwait(false);

                ReportPhoto(context, team, displayName);

                await ApplyArchiveStateAsync(context, team, teamId, displayName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Warn(context, $"The team '{displayName}' could not be provisioned: {ErrorText.Describe(ex)}");
            }
        }

        #endregion

        #region Team lifecycle

        /// <summary>
        /// Produces the team the template asks for, whichever of the three ways it asks.
        /// </summary>
        private async Task<string> EnsureTeamAsync(PnPContext context, TeamModel team, TokenParser parser)
        {
            if (!string.IsNullOrWhiteSpace(team.CloneFrom))
            {
                return await CloneTeamAsync(context, team, parser).ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(team.GroupId))
            {
                return await TeamifyGroupAsync(context, team, parser).ConfigureAwait(false);
            }

            return await CreateOrUpdateTeamAsync(context, team, parser).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a team from scratch, or updates the one that already has its mail nickname.
        /// </summary>
        private async Task<string> CreateOrUpdateTeamAsync(PnPContext context, TeamModel team, TokenParser parser)
        {
            string mailNickname = parser.ParseString(team.MailNickname);
            string existingId = await FindGroupIdByMailNicknameAsync(context, mailNickname).ConfigureAwait(false);

            Dictionary<string, object> body = BuildTeamBody(team, parser);

            if (existingId != null)
            {
                WriteMessage($"Updating existing team {parser.ParseString(team.DisplayName)}", ProvisioningMessageType.Progress);

                // A team's template cannot be changed after creation, and sending it back is an error
                // rather than a no-op.
                body.Remove("template@odata.bind");

                await context.Web.ExecuteRequestAsync(new ApiRequest(new HttpMethod("PATCH"), ApiRequestType.Graph,
                    $"teams/{existingId}", JsonSerializer.Serialize(body))).ConfigureAwait(false);

                return existingId;
            }

            WriteMessage($"Creating team {parser.ParseString(team.DisplayName)}", ProvisioningMessageType.Progress);

            ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                HttpMethod.Post, ApiRequestType.Graph, "teams", JsonSerializer.Serialize(body)))
                .ConfigureAwait(false);

            // Creating a team is asynchronous: the response carries no team, and the id has to be
            // found by the nickname the request asked for.
            string created = IdOf(response.Response);

            return created ?? await WaitForGroupAsync(context, mailNickname).ConfigureAwait(false);
        }

        /// <summary>
        /// Promotes an existing Microsoft 365 group into a team.
        /// </summary>
        private async Task<string> TeamifyGroupAsync(PnPContext context, TeamModel team, TokenParser parser)
        {
            string groupId = parser.ParseString(team.GroupId);

            if (!Guid.TryParse(groupId, out Guid parsedGroupId))
            {
                Warn(context, $"'{team.GroupId}' is not a usable group id, so the team was skipped.");
                return null;
            }

            if (!await GroupExistsAsync(context, groupId).ConfigureAwait(false))
            {
                Warn(context, $"The group {groupId} does not exist, so the team based on it was skipped.");
                return null;
            }

            // Security first, matching PnP Framework: the group's owners become the members of any
            // private channel created below, so they have to be in place before the channels are.
            await ApplySecurityAsync(context, team, groupId, parser).ConfigureAwait(false);

            try
            {
                using (PnPContext groupContext = await context.CloneAsync(parsedGroupId).ConfigureAwait(false))
                using (PnPContext teamContext = await groupContext.GetTeamManager()
                    .CreateTeamAsync(new Admin.Model.Teams.TeamOptions
                    {
                        Specialization = SpecializationOf(team),
                    }).ConfigureAwait(false))
                {
                    WriteMessage($"Promoted group {groupId} to a team", ProvisioningMessageType.Progress);
                }
            }
            catch (Exception ex)
            {
                // Already a team is the common case on a re-apply, and it is not a failure.
                context.Logger?.LogDebug(ex, "{Source}: the group {GroupId} could not be teamified; " +
                    "it is most likely already a team.", Constants.LOGGING_SOURCE, groupId);
            }

            // Patch the team's own settings, which teamifying does not carry.
            Dictionary<string, object> body = BuildTeamBody(team, parser);
            body.Remove("template@odata.bind");

            await context.Web.ExecuteRequestAsync(new ApiRequest(new HttpMethod("PATCH"), ApiRequestType.Graph,
                $"teams/{groupId}", JsonSerializer.Serialize(body))).ConfigureAwait(false);

            return groupId;
        }

        /// <summary>
        /// Copies an existing team.
        /// </summary>
        private async Task<string> CloneTeamAsync(PnPContext context, TeamModel team, TokenParser parser)
        {
            string source = parser.ParseString(team.CloneFrom);
            string mailNickname = parser.ParseString(team.MailNickname);

            WriteMessage($"Cloning team {source}", ProvisioningMessageType.Progress);

            var body = new Dictionary<string, object>
            {
                ["displayName"] = parser.ParseString(team.DisplayName),
                ["description"] = parser.ParseString(team.Description),
                ["mailNickname"] = mailNickname,
                ["classification"] = parser.ParseString(team.Classification),
                ["visibility"] = VisibilityOf(team),

                // Everything worth copying. PnP Framework sends the same set.
                ["partsToClone"] = "apps,tabs,settings,channels,members",
            };

            await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.Graph,
                $"teams/{source}/clone", JsonSerializer.Serialize(body))).ConfigureAwait(false);

            string cloned = await WaitForGroupAsync(context, mailNickname).ConfigureAwait(false);

            if (cloned == null)
            {
                Warn(context, $"The clone of '{source}' was requested but no team called '{mailNickname}' " +
                    "appeared, so it could not be configured further.");
            }

            return cloned;
        }

        /// <summary>
        /// Creates a team from a raw Graph JSON template carried by the provisioning template.
        /// </summary>
        private async Task CreateFromJsonTemplateAsync(PnPContext context, TeamTemplateModel teamTemplate, TokenParser parser)
        {
            if (string.IsNullOrWhiteSpace(teamTemplate.JsonTemplate))
            {
                return;
            }

            try
            {
                string json = parser.ParseString(teamTemplate.JsonTemplate);

                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    var body = new Dictionary<string, object>();

                    foreach (JsonProperty property in document.RootElement.EnumerateObject())
                    {
                        body[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                    }

                    Override(body, "displayName", parser.ParseString(teamTemplate.DisplayName));
                    Override(body, "description", parser.ParseString(teamTemplate.Description));

                    if (teamTemplate.Visibility != null)
                    {
                        body["visibility"] = teamTemplate.Visibility.ToString();
                    }

                    // Not GetValueOrDefault: that extension does not exist on netstandard2.0, which
                    // this project still targets.
                    body.TryGetValue("displayName", out object name);

                    WriteMessage($"Creating team from JSON template {name}", ProvisioningMessageType.Progress);

                    await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.Graph,
                        "teams", JsonSerializer.Serialize(body))).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Warn(context, $"The team JSON template could not be applied: {ErrorText.Describe(ex)}");
            }
        }

        private static void Override(Dictionary<string, object> body, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                body[name] = value;
            }
        }

        #endregion

        #region Team body

        /// <summary>
        /// Builds the Graph <c>team</c> resource from the template's team.
        /// </summary>
        private static Dictionary<string, object> BuildTeamBody(TeamModel team, TokenParser parser)
        {
            var body = new Dictionary<string, object>
            {
                ["template@odata.bind"] = "https://graph.microsoft.com/v1.0/teamsTemplates('standard')",
                ["displayName"] = parser.ParseString(team.DisplayName),
                ["description"] = parser.ParseString(team.Description),
                ["mailNickname"] = parser.ParseString(team.MailNickname),
                ["classification"] = parser.ParseString(team.Classification),
                ["visibility"] = VisibilityOf(team),
            };

            if (team.Specialization != null)
            {
                body["specialization"] = team.Specialization.ToString();
            }

            AddSettings(body, "funSettings", new Dictionary<string, object>
            {
                ["allowGiphy"] = team.FunSettings?.AllowGiphy,
                ["giphyContentRating"] = team.FunSettings?.GiphyContentRating.ToString(),
                ["allowStickersAndMemes"] = team.FunSettings?.AllowStickersAndMemes,
                ["allowCustomMemes"] = team.FunSettings?.AllowCustomMemes,
            }, team.FunSettings != null);

            AddSettings(body, "guestSettings", new Dictionary<string, object>
            {
                ["allowCreateUpdateChannels"] = team.GuestSettings?.AllowCreateUpdateChannels,
                ["allowDeleteChannels"] = team.GuestSettings?.AllowDeleteChannels,
            }, team.GuestSettings != null);

            AddSettings(body, "memberSettings", new Dictionary<string, object>
            {
                ["allowCreateUpdateChannels"] = team.MemberSettings?.AllowCreateUpdateChannels,
                ["allowDeleteChannels"] = team.MemberSettings?.AllowDeleteChannels,
                ["allowAddRemoveApps"] = team.MemberSettings?.AllowAddRemoveApps,
                ["allowCreateUpdateRemoveTabs"] = team.MemberSettings?.AllowCreateUpdateRemoveTabs,
                ["allowCreateUpdateRemoveConnectors"] = team.MemberSettings?.AllowCreateUpdateRemoveConnectors,
            }, team.MemberSettings != null);

            AddSettings(body, "messagingSettings", new Dictionary<string, object>
            {
                ["allowUserEditMessages"] = team.MessagingSettings?.AllowUserEditMessages,
                ["allowUserDeleteMessages"] = team.MessagingSettings?.AllowUserDeleteMessages,
                ["allowOwnerDeleteMessages"] = team.MessagingSettings?.AllowOwnerDeleteMessages,
                ["allowTeamMentions"] = team.MessagingSettings?.AllowTeamMentions,
                ["allowChannelMentions"] = team.MessagingSettings?.AllowChannelMentions,
            }, team.MessagingSettings != null);

            AddSettings(body, "discoverySettings", new Dictionary<string, object>
            {
                ["showInTeamsSearchAndSuggestions"] = team.DiscoverySettings?.ShowInTeamsSearchAndSuggestions,
            }, team.DiscoverySettings != null);

            foreach (string empty in body.Where(p => p.Value is string s && string.IsNullOrEmpty(s))
                .Select(p => p.Key).ToList())
            {
                body.Remove(empty);
            }

            return body;
        }

        private static void AddSettings(Dictionary<string, object> body, string name,
            Dictionary<string, object> values, bool declared)
        {
            if (!declared)
            {
                return;
            }

            var settings = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> value in values.Where(v => v.Value != null))
            {
                settings[value.Key] = value.Value;
            }

            if (settings.Count > 0)
            {
                body[name] = settings;
            }
        }

        private static string VisibilityOf(TeamModel team)
        {
            return team.Visibility?.ToString() ?? "Private";
        }

        private static Core.Model.Teams.TeamSpecialization SpecializationOf(TeamModel team)
        {
            return team.Specialization != null
                && Enum.TryParse(team.Specialization.ToString(), true,
                    out Core.Model.Teams.TeamSpecialization specialization)
                ? specialization
                : Core.Model.Teams.TeamSpecialization.None;
        }

        #endregion

        #region Readiness and lookup

        /// <summary>
        /// Waits until the team can be configured.
        /// </summary>
        private static async Task<bool> WaitUntilReadyAsync(PnPContext context, string teamId)
        {
            DateTime deadline = DateTime.UtcNow.Add(ReadyTimeout);

            while (true)
            {
                try
                {
                    ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                        ApiRequestType.Graph, $"teams/{teamId}?$select=id")).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(response.Response))
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    // Not ready yet. The deadline below is what ends this, not the first failure.
                }

                if (DateTime.UtcNow >= deadline)
                {
                    return false;
                }

                await Task.Delay(ReadyPollInterval).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Waits for a group with the given mail nickname to appear, and returns its id.
        /// </summary>
        private static async Task<string> WaitForGroupAsync(PnPContext context, string mailNickname)
        {
            if (string.IsNullOrEmpty(mailNickname))
            {
                return null;
            }

            DateTime deadline = DateTime.UtcNow.Add(ReadyTimeout);

            while (true)
            {
                string id = await FindGroupIdByMailNicknameAsync(context, mailNickname).ConfigureAwait(false);

                if (id != null)
                {
                    return id;
                }

                if (DateTime.UtcNow >= deadline)
                {
                    return null;
                }

                await Task.Delay(ReadyPollInterval).ConfigureAwait(false);
            }
        }

        private static async Task<string> FindGroupIdByMailNicknameAsync(PnPContext context, string mailNickname)
        {
            if (string.IsNullOrEmpty(mailNickname))
            {
                return null;
            }

            try
            {
                string filter = Uri.EscapeDataString($"mailNickname eq '{mailNickname.Replace("'", "''")}'");

                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.Graph, $"groups?$filter={filter}&$select=id")).ConfigureAwait(false);

                return FirstIdOf(response.Response);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<bool> GroupExistsAsync(PnPContext context, string groupId)
        {
            try
            {
                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(new ApiRequest(
                    ApiRequestType.Graph, $"groups/{groupId}?$select=id")).ConfigureAwait(false);

                return !string.IsNullOrEmpty(response.Response);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Archive and photo

        private async Task ApplyArchiveStateAsync(PnPContext context, TeamModel team, string teamId, string displayName)
        {
            try
            {
                using (PnPContext teamContext = await context.CloneAsync(Guid.Parse(teamId)).ConfigureAwait(false))
                {
                    await teamContext.Team.LoadAsync(t => t.IsArchived).ConfigureAwait(false);

                    if (teamContext.Team.IsArchived == team.Archived)
                    {
                        return;
                    }

                    WriteMessage($"{(team.Archived ? "Archiving" : "Unarchiving")} team {displayName}",
                        ProvisioningMessageType.Progress);

                    // ITeam rather than Graph here: both calls answer with an operation to wait on,
                    // where the raw endpoint leaves the caller polling - which is exactly what PnP
                    // Framework does by hand.
                    if (team.Archived)
                    {
                        await teamContext.Team.ArchiveAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await teamContext.Team.UnarchiveAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Warn(context, $"The archive state of '{displayName}' could not be set: {ErrorText.Describe(ex)}");
            }
        }

        /// <summary>
        /// Reports a declared team photo, which this engine cannot yet upload.
        /// </summary>
        private void ReportPhoto(PnPContext context, TeamModel team, string displayName)
        {
            if (string.IsNullOrEmpty(team.Photo))
            {
                return;
            }

            Warn(context, $"A photo is declared for team '{displayName}' but this engine cannot upload one " +
                "yet: the Graph call needs a binary request body, and PnP Core's ApiRequest only carries text.");
        }

        #endregion

        #region Helpers

        private static string IdOf(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                return document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty("id", out JsonElement id)
                    && id.ValueKind == JsonValueKind.String
                    ? id.GetString()
                    : null;
            }
        }

        private static string FirstIdOf(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                if (!document.RootElement.TryGetProperty("value", out JsonElement value)
                    || value.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                foreach (JsonElement item in value.EnumerateArray())
                {
                    if (item.TryGetProperty("id", out JsonElement id) && id.ValueKind == JsonValueKind.String)
                    {
                        return id.GetString();
                    }
                }
            }

            return null;
        }

        private void Warn(PnPContext context, string message)
        {
            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            WriteMessage(message, ProvisioningMessageType.Warning);
        }

        #endregion
    }
}
