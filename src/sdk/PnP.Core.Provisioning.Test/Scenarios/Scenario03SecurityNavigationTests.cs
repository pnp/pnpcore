using Microsoft.VisualStudio.TestTools.UnitTesting;

using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomActionModel = PnP.Core.Provisioning.Model.CustomAction;
using NavigationModel = PnP.Core.Provisioning.Model.Navigation;
using NavigationNodeModel = PnP.Core.Provisioning.Model.NavigationNode;
using PropertyBagEntryModel = PnP.Core.Provisioning.Model.PropertyBagEntry;
using SiteGroupModel = PnP.Core.Provisioning.Model.SiteGroup;

namespace PnP.Core.Provisioning.Test.Scenarios
{
    /// <summary>
    /// Scenario 3 - site security, navigation, property bags and custom actions.
    /// </summary>
    [TestClass]
    public class Scenario03SecurityNavigationTests : ScenarioTestBase
    {
        private const string Prefix = "PnPCoreScenario3_";

        private static readonly string GroupTitle = $"{Prefix}Readers";
        private static readonly string PlainKey = $"{Prefix}Plain";
        private static readonly string IndexedKey = $"{Prefix}Indexed";
        private static readonly string ActionName = $"{Prefix}Action";
        private static readonly string ParentNodeTitle = $"{Prefix}Parent";
        private static readonly string ChildNodeTitle = $"{Prefix}Child";
        private static readonly Guid ComponentId = Guid.NewGuid();

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Scenario")]
        [Timeout(45 * 60 * 1000)]
        public async Task Scenario3_SecurityNavigationPropertyBagsAndCustomActions()
        {
            await RunScenarioAsync("s3", BuildTemplate(), new[]
            {
                ConfigurationHandler.SiteSecurity,
                ConfigurationHandler.Navigation,
                ConfigurationHandler.PropertyBagEntries,
                ConfigurationHandler.CustomActions,
            },
            AssertAsync,

            configuration => configuration.SiteSecurity.IncludeSiteGroups = true,

            allowScripting: true).ConfigureAwait(false);
        }

        private static ProvisioningTemplate BuildTemplate()
        {
            var template = new ProvisioningTemplate { Id = "SCENARIO-3" };

            template.Security.SiteGroups.Add(new SiteGroupModel
            {
                Title = GroupTitle,
                Description = "Created by the PnP Core provisioning scenario tests",
                AllowMembersEditMembership = false,
            });

            var parent = new NavigationNodeModel
            {
                Title = ParentNodeTitle,
                Url = "https://example.com/parent",
                IsExternal = true,
            };

            parent.NavigationNodes.Add(new NavigationNodeModel
            {
                Title = ChildNodeTitle,
                Url = "https://example.com/child",
                IsExternal = true,
            });

            var structural = new StructuralNavigation { RemoveExistingNodes = true };
            structural.NavigationNodes.Add(parent);

            template.Navigation = new NavigationModel(
                null,
                new CurrentNavigation(CurrentNavigationType.Structural, structural, null),
                null);

            template.PropertyBagEntries.Add(new PropertyBagEntryModel
            {
                Key = PlainKey,
                Value = "plain value",
                Overwrite = true,
            });

            template.PropertyBagEntries.Add(new PropertyBagEntryModel
            {
                Key = IndexedKey,
                Value = "indexed value",
                Overwrite = true,
                Indexed = true,
            });

            template.CustomActions.WebCustomActions.Add(new CustomActionModel
            {
                Name = ActionName,
                Title = ActionName,
                Description = "Created by the PnP Core provisioning scenario tests",
                Location = "ClientSideExtension.ApplicationCustomizer",
                ClientSideComponentId = ComponentId,
                ClientSideComponentProperties = "{\"scenario\":\"three\"}",
                Sequence = 100,
                Enabled = true,
            });

            return template;
        }

        private static async Task AssertAsync(ProvisioningTemplate extracted, PnPContext site)
        {
            await site.Web.LoadAsync(w => w.SiteGroups.QueryProperties(g => g.Title)).ConfigureAwait(false);

            Assert.IsTrue(site.Web.SiteGroups.AsRequested().Any(g => g.Title == GroupTitle),
                $"The site group '{GroupTitle}' was not created.");

            await site.Web.LoadAsync(w => w.Navigation.QueryProperties(
                n => n.QuickLaunch.QueryProperties(node => node.Id, node => node.Title))).ConfigureAwait(false);

            List<INavigationNode> quickLaunch = site.Web.Navigation.QuickLaunch.AsRequested().ToList();

            Console.WriteLine($"Quick launch: {string.Join(", ", quickLaunch.Select(n => n.Title))}");

            INavigationNode parent = quickLaunch.FirstOrDefault(n => n.Title == ParentNodeTitle);

            Assert.IsNotNull(parent, "The parent navigation node was not created.");

            Assert.IsTrue(await HasChildAsync(site, parent.Id, ChildNodeTitle).ConfigureAwait(false),
                $"The child node '{ChildNodeTitle}' is not under '{ParentNodeTitle}'. A flat pair of " +
                "nodes looks like success from the quick launch alone.");

            await site.Web.LoadAsync(w => w.AllProperties).ConfigureAwait(false);

            Assert.AreEqual("plain value", ValueOf(site, PlainKey), "The plain property bag entry was not written.");
            Assert.AreEqual("indexed value", ValueOf(site, IndexedKey), "The indexed property bag entry was not written.");

            string indexedKeys = ValueOf(site, "vti_indexedpropertykeys") ?? string.Empty;

            Console.WriteLine($"Indexed property keys: {indexedKeys}");

            Assert.IsTrue(DecodeIndexedKeys(indexedKeys).Contains(IndexedKey, StringComparer.Ordinal),
                $"'{IndexedKey}' was written but not indexed. An entry marked Indexed that is not " +
                "indexed reads exactly like one that is, until someone searches for it.");

            Assert.IsFalse(DecodeIndexedKeys(indexedKeys).Contains(PlainKey, StringComparer.Ordinal),
                $"'{PlainKey}' is indexed although the template did not ask for it.");

            await site.Web.LoadAsync(w => w.UserCustomActions.QueryProperties(
                a => a.Name, a => a.ClientSideComponentId, a => a.Sequence)).ConfigureAwait(false);

            IUserCustomAction action = site.Web.UserCustomActions.AsRequested()
                .FirstOrDefault(a => a.Name == ActionName);

            Assert.IsNotNull(action, "The custom action was not created.");
            Assert.AreEqual(ComponentId, action.ClientSideComponentId,
                "The custom action has the wrong client side component id.");

            Assert.IsTrue(extracted.Security.SiteGroups.Any(g => g.Title == GroupTitle),
                "The extract did not report the site group.");

            Assert.IsTrue(extracted.PropertyBagEntries.Any(e => e.Key == IndexedKey),
                "The extract did not report the indexed property bag entry.");

            Assert.IsTrue(extracted.CustomActions.WebCustomActions.Any(a => a.Name == ActionName),
                "The extract did not report the custom action.");

            StructuralNavigation current = extracted.Navigation?.CurrentNavigation?.StructuralNavigation;

            Assert.IsNotNull(current, "The extract did not report the current navigation.");
            Assert.IsTrue(current.NavigationNodes.Any(n => n.Title == ParentNodeTitle),
                "The extract did not report the navigation node.");
        }

        /// <summary>
        /// Whether a navigation node has a child with the given title.
        /// </summary>
        private static async Task<bool> HasChildAsync(PnPContext site, int parentId, string childTitle)
        {
            ApiRequestResponse response = await site.Web.ExecuteRequestAsync(new ApiRequest(
                ApiRequestType.SPORest, $"_api/web/navigation/GetNodeById({parentId})/Children"))
                .ConfigureAwait(false);

            return !string.IsNullOrEmpty(response.Response)
                && response.Response.IndexOf(childTitle, StringComparison.Ordinal) >= 0;
        }

        private static string ValueOf(PnPContext site, string key)
        {
            return site.Web.AllProperties.Values.TryGetValue(key, out object value) ? value?.ToString() : null;
        }

        /// <summary>
        /// Decodes <c>vti_indexedpropertykeys</c> into the key names it holds.
        /// </summary>
        private static List<string> DecodeIndexedKeys(string encoded)
        {
            var keys = new List<string>();

            foreach (string part in encoded.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    keys.Add(System.Text.Encoding.Unicode.GetString(Convert.FromBase64String(part)));
                }
                catch (FormatException)
                {
                }
            }

            return keys;
        }
    }
}
