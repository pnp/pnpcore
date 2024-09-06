using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class NavigationTests
    {
        private readonly int AmountOfChildNodes = 5;
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task AddQuickLaunchItems()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var parentNode = await context.Web.Navigation.QuickLaunch.AddAsync(
                  new NavigationNodeOptions
                  {
                      Title = "Parent Node",
                      Url = context.Uri.AbsoluteUri
                  });

                for (var i = 0; i < AmountOfChildNodes; i++)
                {
                    await context.Web.Navigation.QuickLaunch.AddAsync(
                        new NavigationNodeOptions
                        {
                            Title = $"Sub node {i}",
                            Url = context.Uri.AbsoluteUri,
                            ParentNode = parentNode
                        });
                }
                
                var childNodes = await parentNode.GetChildNodesAsync();
                Assert.IsNotNull(childNodes);
                Assert.AreEqual(AmountOfChildNodes, childNodes.Count);
                
                // Delete newly created item
                foreach (var node in childNodes)
                {
                    await node.DeleteAsync();
                }

                await parentNode.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddTopNavItems()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var parentNode = await context.Web.Navigation.TopNavigationBar.AddAsync(
                  new NavigationNodeOptions
                  {
                      Title = "Parent Node",
                      Url = context.Uri.AbsoluteUri,
                  });

                for (var i = 0; i < AmountOfChildNodes; i++)
                {
                    await context.Web.Navigation.TopNavigationBar.AddAsync(
                        new NavigationNodeOptions
                        {
                            Title = $"Sub node {i}",
                            Url = context.Uri.AbsoluteUri,
                            ParentNode = parentNode,
                        });
                }

                var childNodes = await parentNode.GetChildNodesAsync();
                Assert.IsNotNull(childNodes);
                Assert.AreEqual(AmountOfChildNodes, childNodes.Count);

                // Delete newly created parent node, this will delete the children too.
                await parentNode.DeleteAsync();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNavigationItemWithoutNavigationOptions()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var parentNode = await context.Web.Navigation.TopNavigationBar.AddAsync(null);

                // Delete newly created parent node, this will delete the children too.
                await parentNode.DeleteAsync();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNavigationItemWithoutTitle()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var parentNode = await context.Web.Navigation.TopNavigationBar.AddAsync(
                  new NavigationNodeOptions
                  {
                      Url = context.Uri.AbsoluteUri,
                  });

                // Delete newly created parent node, this will delete the children too.
                await parentNode.DeleteAsync();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNavigationItemWithoutUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var parentNode = await context.Web.Navigation.TopNavigationBar.AddAsync(
                  new NavigationNodeOptions
                  {
                      Title = "Test node",
                  });

                // Delete newly created parent node, this will delete the children too.
                await parentNode.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddMultipleChildLevels()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var parentNode = await context.Web.Navigation.TopNavigationBar.AddAsync(
                  new NavigationNodeOptions
                  {
                      Title = "Parent Node",
                      Url = context.Uri.AbsoluteUri,
                  });

                var mainNode = parentNode;
                for (var i = 0; i < AmountOfChildNodes; i++)
                { 
                    var newNode = await context.Web.Navigation.TopNavigationBar.AddAsync(
                        new NavigationNodeOptions
                        {
                            Title = $"Sub node {i}",
                            Url = context.Uri.AbsoluteUri,
                            ParentNode = parentNode
                        });

                    var childNodes = parentNode.GetChildNodes();
                    Assert.AreEqual(childNodes.Count, 1);
                    Assert.AreEqual(childNodes.First().Title, newNode.Title);
                    parentNode = newNode;
                }
                await mainNode.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetQuickLaunchItemById()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newNode = await context.Web.Navigation.QuickLaunch.AddAsync(new NavigationNodeOptions
                {
                    Title = "Test node",
                    Url = context.Uri.AbsoluteUri,
                });

                var nn = await context.Web.Navigation.QuickLaunch.GetByIdAsync(newNode.Id);
                Assert.IsNotNull(nn);

                await nn.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetQuickLaunchItemThatDoesntExist()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var nn = await context.Web.Navigation.QuickLaunch.GetByIdAsync(0);
                Assert.IsNull(nn);
            }
        }

        [TestMethod]
        public async Task DeleteAllTopNavigationItemsAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                for (var i = 0; i < AmountOfChildNodes; i++)
                {
                    await context.Web.Navigation.TopNavigationBar.AddAsync(
                        new NavigationNodeOptions
                        {
                            Title = $"Node {i}",
                            Url = context.Uri.AbsoluteUri
                        });
                }

                await context.Web.Navigation.TopNavigationBar.DeleteAllNodesAsync();

                await context.Web.Navigation.LoadAsync(y => y.TopNavigationBar);
                Assert.AreEqual(context.Web.Navigation.TopNavigationBar.RequestedItems.Cast<NavigationNode>().Count(), 0);
            }
        }

        [TestMethod]
        public async Task DeleteAllTopNavigationItemsBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                for (var i = 0; i < AmountOfChildNodes; i++)
                {
                    await context.Web.Navigation.TopNavigationBar.AddAsync(
                        new NavigationNodeOptions
                        {
                            Title = $"Node {i}",
                            Url = context.Uri.AbsoluteUri
                        });
                }

                context.Web.Navigation.TopNavigationBar.DeleteAllNodesBatch();
                await context.ExecuteAsync().ConfigureAwait(false);
                
                await context.Web.Navigation.LoadAsync(y => y.TopNavigationBar);
                Assert.AreEqual(context.Web.Navigation.TopNavigationBar.RequestedItems.Cast<NavigationNode>().Count(), 0);
            }
        }

        [TestMethod]
        public async Task DeleteAllQuicklaunchNavigationItems()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                for (var i = 0; i < AmountOfChildNodes; i++)
                {
                    context.Web.Navigation.QuickLaunch.Add(
                        new NavigationNodeOptions
                        {
                            Title = $"Node {i}",
                            Url = context.Uri.AbsoluteUri
                        });
                }

                context.Web.Navigation.QuickLaunch.DeleteAllNodes();
            }
        }

        [TestMethod]
        public async Task UpdateNavigationNode()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newNode = context.Web.Navigation.QuickLaunch.Add(
                        new NavigationNodeOptions
                        {
                            Title = $"Test node",
                            Url = "https://google.be"
                        });

                newNode.Title = "Test node - Title Changed";
                newNode.Url = context.Uri.AbsoluteUri;

                await newNode.UpdateAsync();

                var newNodeObtained = context.Web.Navigation.QuickLaunch.GetById(newNode.Id);
                Assert.AreEqual(newNodeObtained.Title, newNode.Title);

                await newNodeObtained.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task MoveNavigationNodes()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var firstNode = await context.Web.Navigation.TopNavigationBar.AddAsync(
                  new NavigationNodeOptions
                  {
                      Title = "Node 1",
                      Url = context.Uri.AbsoluteUri,
                  });
                var secondNode = await context.Web.Navigation.TopNavigationBar.AddAsync(
                  new NavigationNodeOptions
                  {
                      Title = "Node 2",
                      Url = context.Uri.AbsoluteUri,
                  });

                await context.Web.Navigation.TopNavigationBar.MoveNodeAfterAsync(firstNode, secondNode);
                await context.Web.Navigation.LoadAsync(y => y.TopNavigationBar);

                var navigationItems = context.Web.Navigation.TopNavigationBar.RequestedItems.Cast<NavigationNode>();

                var firstNodeIndex = navigationItems.ToList().FindIndex(f => f.Id == firstNode.Id);
                var lastNodeIndex = navigationItems.ToList().FindIndex(f => f.Id == secondNode.Id);

                Assert.IsTrue(firstNodeIndex > lastNodeIndex);

                await firstNode.DeleteAsync();
                await secondNode.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddQuickLaunchItemUsingAudienceTargeting()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                INavigationNode parentNode = null;
                try
                {
                    // Ensure audience targeting is enabled for navigation nodes
                    context.Web.EnsureProperties(w => w.NavAudienceTargetingEnabled);
                    context.Web.NavAudienceTargetingEnabled = true;
                    context.Web.Update();

                    parentNode = await context.Web.Navigation.QuickLaunch.AddAsync(
                      new NavigationNodeOptions
                      {
                          Title = "Parent Node",
                          Url = context.Uri.AbsoluteUri,
                          AudienceIds = new System.Collections.Generic.List<Guid> { context.Site.GroupId }
                      });
                }
                finally
                {
                    if (parentNode != null)
                    {
                        await parentNode.DeleteAsync();
                    }
                    
                    context.Web.NavAudienceTargetingEnabled = false;
                    context.Web.Update();
                }
            }
        }

        [TestMethod]
        public async Task UpdateQuickLaunchItemUsingAudienceTargeting()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                INavigationNode parentNode = null;
                try
                {
                    // Ensure audience targeting is enabled for navigation nodes
                    context.Web.EnsureProperties(w => w.NavAudienceTargetingEnabled);
                    context.Web.NavAudienceTargetingEnabled = true;
                    context.Web.Update();

                    // Create the node without audience targeting
                    parentNode = await context.Web.Navigation.QuickLaunch.AddAsync(
                      new NavigationNodeOptions
                      {
                          Title = "Parent Node",
                          Url = context.Uri.AbsoluteUri,
                          //AudienceIds = new System.Collections.Generic.List<Guid> { context.Site.GroupId }
                      });

                    // Load the created node again
                    parentNode = context.Web.Navigation.QuickLaunch.GetById(parentNode.Id);

                    // Update the node adding an audience
                    parentNode.AudienceIds = new System.Collections.Generic.List<Guid> { context.Site.GroupId };
                    parentNode.Update();

                    parentNode = context.Web.Navigation.QuickLaunch.GetById(parentNode.Id);
                    
                    // Remove the audience again
                    parentNode.AudienceIds.Clear();
                    parentNode.Update();
                }
                finally
                {
                    if (parentNode != null)
                    {
                        await parentNode.DeleteAsync();
                    }

                    context.Web.NavAudienceTargetingEnabled = false;
                    context.Web.Update();
                }
            }
        }
    }
}
