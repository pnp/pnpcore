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
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task LoadNavigation()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadAsync(p => p.Navigation);//
                var nn = await context.Web.Navigation.QuickLaunch.GetByIdAsync(1031);
                var nnList = await context.Web.Navigation.QuickLaunch.GetByTitleAsync("Home");
                await nn.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddQuickLaunchItem()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newNode = await context.Web.Navigation.QuickLaunch.AddAsync(
                    new Model.SharePoint.NavigationNodeOptions
                    {
                        Title = "Home",
                        Url = context.Uri.AbsoluteUri,
                        IsVisible = true
                    });

                var nn = await context.Web.Navigation.QuickLaunch.GetByIdAsync(newNode.Id);
                Assert.IsNotNull(nn);
                // Delete newly created item
                await newNode.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetQuickLaunchItemById()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                

                var nn = await context.Web.Navigation.QuickLaunch.GetByIdAsync(2022);
                Assert.IsNotNull(nn);
            }
        }


    }
}
