using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.Model.Configuration.Tenant.Sequence;
using PnP.Core.Provisioning.Model.Configuration.Tenant.Teams;
using PnP.Core.Provisioning.Model.Teams;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.Providers;
using PnP.Core.Provisioning.Providers.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Covers which handlers a tenant template extract runs, how Graph's team resource is read, and
    /// that a hierarchy shaped the way an extract shapes it survives being saved and read back.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class TenantTemplateExtractionTests
    {
        #region Handlers

        [TestMethod]
        public void NoHandlerRunsWithoutATenantSection()
        {
            List<ObjectHierarchyHandlerBase> handlers = ProvisioningManager.BuildHierarchyExtractHandlers(new ExtractConfiguration());

            Assert.AreEqual(0, handlers.Count, "A configuration naming neither sites nor teams has nothing to extract.");
        }

        [TestMethod]
        public void TheSitesRunBeforeTheTeamsWhichFallBackOnThem()
        {
            var configuration = new ExtractConfiguration();
            configuration.Tenant.Sequence = new ExtractSequenceConfiguration { SiteUrls = { "https://contoso.sharepoint.com/sites/a" } };
            configuration.Tenant.Teams = new ExtractTeamsConfiguration();

            List<ObjectHierarchyHandlerBase> handlers = ProvisioningManager.BuildHierarchyExtractHandlers(configuration);

            Assert.AreEqual(2, handlers.Count);
            Assert.IsInstanceOfType(handlers[0], typeof(ObjectHierarchySequenceSites));
            Assert.IsInstanceOfType(handlers[1], typeof(ObjectTeams));
            Assert.AreSame(handlers[0], ((ObjectTeams)handlers[1]).SequenceSites,
                "The teams handler has to know the sequence handler to take the teams behind its sites.");
        }

        [TestMethod]
        public void TheTeamsRunOnTheirOwnWithoutASequence()
        {
            var configuration = new ExtractConfiguration();
            configuration.Tenant.Teams = new ExtractTeamsConfiguration { IncludeAllTeams = true };

            List<ObjectHierarchyHandlerBase> handlers = ProvisioningManager.BuildHierarchyExtractHandlers(configuration);

            Assert.AreEqual(1, handlers.Count);
            Assert.IsNull(((ObjectTeams)handlers[0]).SequenceSites);
        }

        [TestMethod]
        public void TheSequenceExtractsOnlyWhenItNamesSites()
        {
            var configuration = new ExtractConfiguration();
            configuration.Tenant.Sequence = new ExtractSequenceConfiguration();

            Assert.IsFalse(new ObjectHierarchySequenceSites().WillExtract(null, new ProvisioningHierarchy(), null, configuration));

            configuration.Tenant.Sequence.SiteUrls.Add("https://contoso.sharepoint.com/sites/a");

            Assert.IsTrue(new ObjectHierarchySequenceSites().WillExtract(null, new ProvisioningHierarchy(), null, configuration));
        }

        [TestMethod]
        public void TheTeamsExtractWhenTheyAreNamedOrTheSequenceHasSites()
        {
            var hierarchy = new ProvisioningHierarchy();

            var all = new ExtractConfiguration();
            all.Tenant.Teams = new ExtractTeamsConfiguration { IncludeAllTeams = true };
            Assert.IsTrue(new ObjectTeams().WillExtract(null, hierarchy, null, all), "All teams were asked for.");

            var named = new ExtractConfiguration();
            named.Tenant.Teams = new ExtractTeamsConfiguration { TeamSiteUrls = { "https://contoso.sharepoint.com/sites/team" } };
            Assert.IsTrue(new ObjectTeams().WillExtract(null, hierarchy, null, named), "A team site was named.");

            var unnamed = new ExtractConfiguration();
            unnamed.Tenant.Teams = new ExtractTeamsConfiguration();
            Assert.IsFalse(new ObjectTeams().WillExtract(null, hierarchy, null, unnamed),
                "Without a sequence there are no sites to take the teams of.");

            unnamed.Tenant.Sequence = new ExtractSequenceConfiguration { SiteUrls = { "https://contoso.sharepoint.com/sites/a" } };
            Assert.IsTrue(new ObjectTeams { SequenceSites = new ObjectHierarchySequenceSites() }.WillExtract(null, hierarchy, null, unnamed),
                "The teams behind the sequence's sites are taken when no team is named, as PnP Framework does.");
        }

        #endregion

        #region Graph

        [TestMethod]
        public void TheTeamResourceIsReadIntoTheTemplatesTeam()
        {
            const string json = @"{
                ""id"": ""b9a5a1c4-0000-4000-8000-000000000001"",
                ""displayName"": ""Marketing"",
                ""description"": ""The marketing team"",
                ""classification"": null,
                ""specialization"": ""none"",
                ""visibility"": ""hiddenMembership"",
                ""isArchived"": true,
                ""memberSettings"": { ""allowCreateUpdateChannels"": true, ""allowDeleteChannels"": false, ""allowAddRemoveApps"": true,
                                     ""allowCreateUpdateRemoveTabs"": true, ""allowCreateUpdateRemoveConnectors"": false, ""allowCreatePrivateChannels"": true },
                ""guestSettings"": { ""allowCreateUpdateChannels"": false, ""allowDeleteChannels"": false },
                ""messagingSettings"": { ""allowUserEditMessages"": true, ""allowUserDeleteMessages"": false, ""allowOwnerDeleteMessages"": true,
                                        ""allowTeamMentions"": true, ""allowChannelMentions"": false },
                ""funSettings"": { ""allowGiphy"": true, ""giphyContentRating"": ""strict"", ""allowStickersAndMemes"": false, ""allowCustomMemes"": true },
                ""discoverySettings"": { ""showInTeamsSearchAndSuggestions"": true }
            }";

            Team team = ObjectTeams.ParseTeam(json);

            Assert.AreEqual("Marketing", team.DisplayName);
            Assert.AreEqual("The marketing team", team.Description);
            Assert.IsNull(team.Classification);
            Assert.IsNull(team.Specialization, "'none' asks for no specialization, so none is written.");
            Assert.AreEqual(TeamVisibility.Private, team.Visibility);
            Assert.IsTrue(team.HiddenGroupMembershipEnabled, "Hidden membership is a private team whose members are hidden.");
            Assert.IsTrue(team.Archived, "PnP Framework loses isArchived, because its model calls it Archived.");

            Assert.IsTrue(team.MemberSettings.AllowCreateUpdateChannels);
            Assert.IsFalse(team.MemberSettings.AllowDeleteChannels);
            Assert.IsTrue(team.MemberSettings.AllowCreatePrivateChannels);
            Assert.IsFalse(team.MemberSettings.AllowCreateUpdateRemoveConnectors);
            Assert.IsFalse(team.GuestSettings.AllowCreateUpdateChannels);
            Assert.IsFalse(team.MessagingSettings.AllowUserDeleteMessages);
            Assert.IsFalse(team.MessagingSettings.AllowChannelMentions);
            Assert.AreEqual("strict", team.FunSettings.GiphyContentRating);
            Assert.IsTrue(team.FunSettings.AllowCustomMemes);
            Assert.IsTrue(team.DiscoverySettings.ShowInTeamsSearchAndSuggestions);
        }

        [TestMethod]
        public void AnEducationTeamKeepsItsSpecializationAndAPublicTeamItsVisibility()
        {
            Team team = ObjectTeams.ParseTeam(@"{ ""displayName"": ""Class"", ""specialization"": ""educationClass"", ""visibility"": ""public"" }");

            Assert.AreEqual(TeamSpecialization.EducationClass, team.Specialization);
            Assert.AreEqual(TeamVisibility.Public, team.Visibility);
            Assert.IsFalse(team.HiddenGroupMembershipEnabled);
            Assert.IsNull(team.FunSettings, "A settings block Graph did not return is not invented.");
        }

        [TestMethod]
        public void ANextLinkIsSentWithoutTheEndpointInFrontOfIt()
        {
            Assert.AreEqual("groups?$filter=x&$skiptoken=abc",
                ObjectTeams.RelativeGraphRequest("https://graph.microsoft.com/v1.0/groups?$filter=x&$skiptoken=abc"));

            Assert.AreEqual("teams/1/channels?$skiptoken=abc",
                ObjectTeams.RelativeGraphRequest("https://graph.microsoft.com/beta/teams/1/channels?$skiptoken=abc"));

            Assert.AreEqual("groups?$skiptoken=abc",
                ObjectTeams.RelativeGraphRequest("https://microsoftgraph.chinacloudapi.cn/v1.0/groups?$skiptoken=abc"),
                "A national cloud's endpoint is removed the same way.");

            Assert.IsNull(ObjectTeams.RelativeGraphRequest(null));
        }

        #endregion

        #region Saving an extracted hierarchy

        [TestMethod]
        public void AnExtractedHierarchySurvivesBeingSavedAndReadBack()
        {
            ProvisioningHierarchy hierarchy = ExtractedHierarchy();

            var formatter = (IProvisioningHierarchyFormatter)XMLPnPSchemaFormatter.LatestFormatter;

            ProvisioningHierarchy read;

            using (Stream saved = formatter.ToFormattedHierarchy(hierarchy))
            {
                read = formatter.ToProvisioningHierarchy(saved);
            }

            CollectionAssert.AreEquivalent(hierarchy.Parameters.ToList(), read.Parameters.ToList(), "The parameters changed.");

            CollectionAssert.AreEquivalent(hierarchy.Templates.Select(t => t.Id).ToList(), read.Templates.Select(t => t.Id).ToList(),
                "The templates changed.");

            ProvisioningSequence sequence = read.Sequences.Single();
            Assert.AreEqual(ObjectHierarchySequenceSites.ExtractedSequenceId, sequence.ID);
            Assert.AreEqual(4, sequence.SiteCollections.Count);

            var communication = sequence.SiteCollections.OfType<CommunicationSiteCollection>().Single();
            Assert.AreEqual("{parameter:SITECOLLECTION_11111111111111111111111111111111_URL}", communication.Url);
            Assert.AreEqual("{parameter:SITECOLLECTION_11111111111111111111111111111111_TITLE}", communication.Title);
            Assert.IsTrue(communication.IsHubSite);
            Assert.AreEqual("Hub title", communication.HubSiteTitle);
            Assert.AreEqual("owner@contoso.onmicrosoft.com", communication.Owner);
            Assert.AreEqual(1033, communication.Language);
            CollectionAssert.AreEqual(new[] { "TEMPLATE-11111111111111111111111111111111" }, communication.Templates);

            TeamNoGroupSubSite subSite = (TeamNoGroupSubSite)communication.Sites.Single();
            Assert.AreEqual("{parameter:SUBSITE_44444444444444444444444444444444_URL}", subSite.Url);
            Assert.IsFalse(subSite.UseSamePermissionsAsParentSite);
            Assert.AreEqual(4, subSite.TimeZoneId);
            CollectionAssert.AreEqual(new[] { "TEMPLATE-44444444444444444444444444444444" }, subSite.Templates);

            var team = sequence.SiteCollections.OfType<TeamSiteCollection>().Single();
            Assert.AreEqual("{parameter:SITECOLLECTION_22222222222222222222222222222222_ALIAS}", team.Alias);
            Assert.AreEqual("Team site", team.DisplayName);
            Assert.IsTrue(team.IsPublic);
            Assert.IsTrue(team.HideTeamify);

            var noGroup = sequence.SiteCollections.OfType<TeamNoGroupSiteCollection>().Single();
            Assert.AreEqual(4, noGroup.TimeZoneId);

            var classic = sequence.SiteCollections.OfType<ClassicSiteCollection>().Single();
            Assert.AreEqual("STS#0", classic.WebTemplate);
            Assert.AreEqual("{parameter:SITECOLLECTION_55555555555555555555555555555555_URL}", classic.Url);

            ProvisioningTemplate joined = read.Templates.Single(t => t.Id == "TEMPLATE-33333333333333333333333333333333");
            Assert.AreEqual("{parameter:SITECOLLECTION_11111111111111111111111111111111_URL}", joined.WebSettings.HubSiteUrl);

            Team readTeam = read.Teams.Teams.Single();
            Assert.AreEqual("Marketing", readTeam.DisplayName);
            Assert.AreEqual("marketing", readTeam.MailNickname);
            Assert.IsTrue(readTeam.Archived);
            Assert.AreEqual(TeamVisibility.Public, readTeam.Visibility);
            Assert.IsTrue(readTeam.MemberSettings.AllowCreateUpdateChannels);

            TeamChannel channel = readTeam.Channels.Single();
            Assert.AreEqual("General", channel.DisplayName);
            Assert.AreEqual("<p>Welcome</p>", channel.Messages.Single().Message, "A message's html must survive the trip.");

            TeamTab tab = channel.Tabs.Single();
            Assert.AreEqual("com.microsoft.teamspace.tab.web", tab.TeamsAppId);
            Assert.AreEqual("https://contoso.com", tab.Configuration.ContentUrl);

            Assert.AreEqual("owner@contoso.onmicrosoft.com", readTeam.Security.Owners.Single().UserPrincipalName);
            Assert.AreEqual(2, readTeam.Security.Members.Count);
            Assert.IsTrue(readTeam.Security.AllowToAddGuests);
            Assert.AreEqual("com.microsoft.teamspace.tab.planner", readTeam.Apps.Single().AppId);
        }

        /// <summary>
        /// A hierarchy with every kind of site collection and a team, as the extract writes them.
        /// </summary>
        private static ProvisioningHierarchy ExtractedHierarchy()
        {
            var hierarchy = new ProvisioningHierarchy();

            const string hub = "11111111111111111111111111111111";
            const string teamSite = "22222222222222222222222222222222";
            const string joinedSite = "33333333333333333333333333333333";
            const string subweb = "44444444444444444444444444444444";
            const string classicSite = "55555555555555555555555555555555";

            hierarchy.Parameters[$"SITECOLLECTION_{hub}_URL"] = "https://contoso.sharepoint.com/sites/hub";
            hierarchy.Parameters[$"SITECOLLECTION_{hub}_TITLE"] = "Hub";
            hierarchy.Parameters[$"SUBSITE_{subweb}_URL"] = "/projects";
            hierarchy.Parameters[$"SUBSITE_{subweb}_TITLE"] = "Projects";
            hierarchy.Parameters[$"SITECOLLECTION_{teamSite}_ALIAS"] = "teamsite";
            hierarchy.Parameters[$"SITECOLLECTION_{teamSite}_TITLE"] = "Team site";
            hierarchy.Parameters[$"SITECOLLECTION_{joinedSite}_URL"] = "https://contoso.sharepoint.com/sites/joined";
            hierarchy.Parameters[$"SITECOLLECTION_{joinedSite}_TITLE"] = "Joined";
            hierarchy.Parameters[$"SITECOLLECTION_{classicSite}_URL"] = "https://contoso.sharepoint.com/sites/classic";
            hierarchy.Parameters[$"SITECOLLECTION_{classicSite}_TITLE"] = "Classic";

            foreach (string id in new[] { hub, subweb, teamSite, joinedSite, classicSite })
            {
                hierarchy.Templates.Add(new ProvisioningTemplate { Id = $"TEMPLATE-{id}" });
            }

            hierarchy.Templates.Single(t => t.Id == $"TEMPLATE-{joinedSite}").WebSettings = new WebSettings
            {
                HubSiteUrl = $"{{parameter:SITECOLLECTION_{hub}_URL}}",
            };

            var sequence = new ProvisioningSequence { ID = ObjectHierarchySequenceSites.ExtractedSequenceId };

            var communication = new CommunicationSiteCollection
            {
                Url = $"{{parameter:SITECOLLECTION_{hub}_URL}}",
                Title = $"{{parameter:SITECOLLECTION_{hub}_TITLE}}",
                Description = "The hub",
                IsHubSite = true,
                HubSiteTitle = "Hub title",
                Owner = "owner@contoso.onmicrosoft.com",
                Language = 1033,
            };
            communication.Templates.Add($"TEMPLATE-{hub}");

            var subSite = new TeamNoGroupSubSite
            {
                Url = $"{{parameter:SUBSITE_{subweb}_URL}}",
                Title = $"{{parameter:SUBSITE_{subweb}_TITLE}}",
                Description = "Projects",
                Language = 1033,
                TimeZoneId = 4,
                QuickLaunchEnabled = true,
                UseSamePermissionsAsParentSite = false,
            };
            subSite.Templates.Add($"TEMPLATE-{subweb}");
            communication.Sites.Add(subSite);

            var team = new TeamSiteCollection
            {
                Alias = $"{{parameter:SITECOLLECTION_{teamSite}_ALIAS}}",
                Title = $"{{parameter:SITECOLLECTION_{teamSite}_TITLE}}",
                DisplayName = "Team site",
                Description = "A team site",
                IsPublic = true,
                HideTeamify = true,
                Language = 1033,
            };
            team.Templates.Add($"TEMPLATE-{teamSite}");

            var noGroup = new TeamNoGroupSiteCollection
            {
                Url = $"{{parameter:SITECOLLECTION_{joinedSite}_URL}}",
                Title = $"{{parameter:SITECOLLECTION_{joinedSite}_TITLE}}",
                Description = "Joined to the hub",
                Owner = "owner@contoso.onmicrosoft.com",
                Language = 1033,
                TimeZoneId = 4,
            };
            noGroup.Templates.Add($"TEMPLATE-{joinedSite}");

            var classic = new ClassicSiteCollection
            {
                Url = $"{{parameter:SITECOLLECTION_{classicSite}_URL}}",
                Title = $"{{parameter:SITECOLLECTION_{classicSite}_TITLE}}",
                Description = "A classic site",
                WebTemplate = "STS#0",
                Owner = "owner@contoso.onmicrosoft.com",
                Language = 1033,
                TimeZoneId = 4,
            };
            classic.Templates.Add($"TEMPLATE-{classicSite}");

            sequence.SiteCollections.Add(communication);
            sequence.SiteCollections.Add(team);
            sequence.SiteCollections.Add(noGroup);
            sequence.SiteCollections.Add(classic);
            hierarchy.Sequences.Add(sequence);

            var extractedTeam = new Team
            {
                DisplayName = "Marketing",
                Description = "The marketing team",
                MailNickname = "marketing",
                Visibility = TeamVisibility.Public,
                Archived = true,
                MemberSettings = new TeamMemberSettings { AllowCreateUpdateChannels = true },
                Security = new TeamSecurity { AllowToAddGuests = true },
            };

            extractedTeam.Security.Owners.Add(new TeamSecurityUser { UserPrincipalName = "owner@contoso.onmicrosoft.com" });
            extractedTeam.Security.Members.Add(new TeamSecurityUser { UserPrincipalName = "owner@contoso.onmicrosoft.com" });
            extractedTeam.Security.Members.Add(new TeamSecurityUser { UserPrincipalName = "member@contoso.onmicrosoft.com" });
            extractedTeam.Apps.Add(new TeamAppInstance { AppId = "com.microsoft.teamspace.tab.planner" });

            var channel = new TeamChannel
            {
                ID = "19:general@thread.tacv2",
                DisplayName = "General",
                Description = string.Empty,
            };

            channel.Tabs.Add(new TeamTab
            {
                DisplayName = "Website",
                TeamsAppId = "com.microsoft.teamspace.tab.web",
                Configuration = new TeamTabConfiguration
                {
                    EntityId = string.Empty,
                    ContentUrl = "https://contoso.com",
                    RemoveUrl = string.Empty,
                    WebsiteUrl = "https://contoso.com",
                },
            });

            channel.Messages.Add(new TeamChannelMessage { Message = "<p>Welcome</p>" });
            extractedTeam.Channels.Add(channel);
            hierarchy.Teams.Teams.Add(extractedTeam);

            return hierarchy;
        }

        #endregion
    }
}
