using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class NavigationTests
    {
        public int AmountOfChildNodes = 5;
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
                  new Model.SharePoint.NavigationNodeOptions
                  {
                      Title = "Parent Node",
                      Url = context.Uri.AbsoluteUri
                  });
                for (var i = 0; i < AmountOfChildNodes; i++)
                {
                    await context.Web.Navigation.QuickLaunch.AddAsync(
                        new Model.SharePoint.NavigationNodeOptions
                        {
                            Title = $"Sub node {i}",
                            Url = context.Uri.AbsoluteUri,
                            ParentNode = parentNode
                        });
                }
                
                var childNodes = await parentNode.GetChildNodes();
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
                  new Model.SharePoint.NavigationNodeOptions
                  {
                      Title = "Parent Node",
                      Url = context.Uri.AbsoluteUri,
                  });
                for (var i = 0; i < AmountOfChildNodes; i++)
                {
                    await context.Web.Navigation.TopNavigationBar.AddAsync(
                        new Model.SharePoint.NavigationNodeOptions
                        {
                            Title = $"Sub node {i}",
                            Url = context.Uri.AbsoluteUri,
                            ParentNode = parentNode,
                        });
                }


                var childNodes = await parentNode.GetChildNodes();
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
        public async Task AddMultipleChildLevels()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var parentNode = await context.Web.Navigation.TopNavigationBar.AddAsync(
                  new Model.SharePoint.NavigationNodeOptions
                  {
                      Title = "Parent Node",
                      Url = context.Uri.AbsoluteUri,
                  });
                var mainNode = parentNode;
                for (var i = 0; i < AmountOfChildNodes; i++)
                { 
                    parentNode = await context.Web.Navigation.TopNavigationBar.AddAsync(
                        new Model.SharePoint.NavigationNodeOptions
                        {
                            Title = $"Sub node {i}",
                            Url = context.Uri.AbsoluteUri,
                            ParentNode = parentNode
                        });
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
                var newNode = await context.Web.Navigation.QuickLaunch.AddAsync(new Model.SharePoint.NavigationNodeOptions
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
        public async Task DeleteAllTopNavigationItemsAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                for (var i = 0; i < AmountOfChildNodes; i++)
                {
                    await context.Web.Navigation.TopNavigationBar.AddAsync(
                        new Model.SharePoint.NavigationNodeOptions
                        {
                            Title = $"Node {i}",
                            Url = context.Uri.AbsoluteUri
                        });
                }

                await context.Web.Navigation.TopNavigationBar.DeleteAllNodesAsync();
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
                        new Model.SharePoint.NavigationNodeOptions
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
                        new Model.SharePoint.NavigationNodeOptions
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
    }
}
