using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NavigationModel = PnP.Core.Provisioning.Model.Navigation;
using NavigationNodeModel = PnP.Core.Provisioning.Model.NavigationNode;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectNavigation</c>
    /// </summary>
    [TestClass]
    public class ObjectNavigationLiveTests : LiveTestBase
    {
        private static string ParentTitle => $"{TestPrefix}Parent";

        private static string ChildTitle => $"{TestPrefix}Child";

        #region Apply

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Navigation_CreatesQuickLaunchNodesIncludingAChild()
        {
            using (PnPContext context = await GetNoGroupContextAsync().ConfigureAwait(false))
            {
                List<NavigationNodeModel> original = await ReadQuickLaunchAsync(context).ConfigureAwait(false);
                Console.WriteLine($"Quick launch had {original.Count} node(s)");

                try
                {
                    var parent = new NavigationNodeModel
                    {
                        Title = ParentTitle,
                        Url = "https://example.com/parent",
                        IsExternal = true,
                    };

                    parent.NavigationNodes.Add(new NavigationNodeModel
                    {
                        Title = ChildTitle,
                        Url = "https://example.com/child",
                        IsExternal = true,
                    });

                    var structural = new StructuralNavigation { RemoveExistingNodes = true };
                    structural.NavigationNodes.Add(parent);

                    var template = new ProvisioningTemplate
                    {
                        Navigation = new NavigationModel(
                            null,
                            new CurrentNavigation(CurrentNavigationType.Structural, structural, null),
                            null),
                    };

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetNoGroupContextAsync(1).ConfigureAwait(false))
                    {
                        var configuration = new ExtractConfiguration();
                        configuration.Handlers.Add(ConfigurationHandler.Navigation);

                        ProvisioningTemplate extracted = await fresh.GetProvisioningManager()
                            .GetTemplateAsync(configuration).ConfigureAwait(false);

                        StructuralNavigation current = extracted.Navigation?.CurrentNavigation?.StructuralNavigation;
                        Assert.IsNotNull(current, "The current navigation was not extracted.");

                        Console.WriteLine($"Extracted: {string.Join(", ", current.NavigationNodes.Select(n => n.Title))}");

                        NavigationNodeModel extractedParent = current.NavigationNodes
                            .FirstOrDefault(n => n.Title == ParentTitle);

                        Assert.IsNotNull(extractedParent, "The parent node was not created.");

                        Assert.AreEqual(1, current.NavigationNodes.Count,
                            "RemoveExistingNodes did not clear the quick launch before adding.");

                        Assert.AreEqual(1, extractedParent.NavigationNodes.Count,
                            "The child node was not created, or nesting was lost on extract.");

                        Assert.AreEqual(ChildTitle, extractedParent.NavigationNodes[0].Title);
                    }
                }
                finally
                {
                    await RestoreQuickLaunchAsync(original).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Navigation_AppliedTwiceDoesNotAccumulateNodes()
        {
            using (PnPContext context = await GetNoGroupContextAsync().ConfigureAwait(false))
            {
                List<NavigationNodeModel> original = await ReadQuickLaunchAsync(context).ConfigureAwait(false);

                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    await manager.ApplyTemplateAsync(BuildTemplate(), Reporting()).ConfigureAwait(false);
                    await manager.ApplyTemplateAsync(BuildTemplate(), Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetNoGroupContextAsync(1).ConfigureAwait(false))
                    {
                        List<NavigationNodeModel> nodes = await ReadQuickLaunchAsync(fresh).ConfigureAwait(false);
                        Console.WriteLine($"After two applies: {nodes.Count} node(s)");
                        Assert.AreEqual(1, nodes.Count, "The second apply appended instead of replacing.");
                    }
                }
                finally
                {
                    await RestoreQuickLaunchAsync(original).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region T9 - managed metadata navigation

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Navigation_ManagedMetadataIsRefusedByNameRatherThanSilentlyIgnored()
        {
            using (PnPContext context = await GetNoGroupContextAsync().ConfigureAwait(false))
            {
                List<NavigationNodeModel> original = await ReadQuickLaunchAsync(context).ConfigureAwait(false);
                string warning = null;

                try
                {
                    var template = new ProvisioningTemplate
                    {
                        Navigation = new NavigationModel(
                            null,
                            new CurrentNavigation(CurrentNavigationType.Managed, null,
                                new ManagedNavigation
                                {
                                    TermStoreId = "11111111-1111-1111-1111-111111111111",
                                    TermSetId = "22222222-2222-2222-2222-222222222222",
                                }),
                            null),
                    };

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, new ApplyConfiguration
                    {
                        MessagesDelegate = (message, type) =>
                        {
                            Console.WriteLine($"[{type}] {message}");
                            if (type == ProvisioningMessageType.Warning)
                            {
                                warning = message;
                            }
                        },
                    }).ConfigureAwait(false);

                    Assert.IsNotNull(warning, "Managed metadata navigation was skipped without saying so.");
                    StringAssert.Contains(warning, "22222222-2222-2222-2222-222222222222",
                        "The warning does not name the term set that was skipped.");

                    using (PnPContext fresh = await GetNoGroupContextAsync(1).ConfigureAwait(false))
                    {
                        List<NavigationNodeModel> nodes = await ReadQuickLaunchAsync(fresh).ConfigureAwait(false);

                        Assert.AreEqual(original.Count, nodes.Count,
                            "Refusing managed navigation still changed the site's quick launch.");
                    }
                }
                finally
                {
                    await RestoreQuickLaunchAsync(original).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        private static ProvisioningTemplate BuildTemplate()
        {
            var structural = new StructuralNavigation { RemoveExistingNodes = true };

            structural.NavigationNodes.Add(new NavigationNodeModel
            {
                Title = ParentTitle,
                Url = "https://example.com/parent",
                IsExternal = true,
            });

            return new ProvisioningTemplate
            {
                Navigation = new NavigationModel(
                    null,
                    new CurrentNavigation(CurrentNavigationType.Structural, structural, null),
                    null),
            };
        }

        private static ApplyConfiguration Reporting()
        {
            return new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        Console.WriteLine($"[{type}] {message}");
                    }
                },
            };
        }

        private static async Task<List<NavigationNodeModel>> ReadQuickLaunchAsync(PnPContext context)
        {
            var nodes = new List<NavigationNodeModel>();

            await context.Web.LoadAsync(w => w.Navigation.QueryProperties(n => n.QuickLaunch)).ConfigureAwait(false);

            foreach (INavigationNode node in context.Web.Navigation.QuickLaunch.AsRequested())
            {
                nodes.Add(new NavigationNodeModel
                {
                    Title = node.Title,
                    Url = node.Url,
                    IsExternal = node.IsExternal,
                });
            }

            return nodes;
        }

        /// <summary>
        /// Rebuilds the quick launch as the test found it.
        /// </summary>
        private static async Task RestoreQuickLaunchAsync(List<NavigationNodeModel> original)
        {
            try
            {
                using (PnPContext context = await GetNoGroupContextAsync(2).ConfigureAwait(false))
                {
                    await context.Web.LoadAsync(w => w.Navigation.QueryProperties(n => n.QuickLaunch))
                        .ConfigureAwait(false);

                    await context.Web.Navigation.QuickLaunch.DeleteAllNodesAsync().ConfigureAwait(false);

                    foreach (NavigationNodeModel node in original)
                    {
                        await context.Web.Navigation.QuickLaunch.AddAsync(new NavigationNodeOptions
                        {
                            Title = node.Title,
                            Url = node.Url,
                        }).ConfigureAwait(false);
                    }

                    Console.WriteLine($"Restored {original.Count} quick launch node(s).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT RESTORE the quick launch: {Describe(ex)}");
            }
        }

        #endregion
    }
}
