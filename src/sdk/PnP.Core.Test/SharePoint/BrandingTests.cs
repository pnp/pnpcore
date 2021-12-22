using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System.Linq;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class BrandingTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public void GetAvailableThemes()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var themes = context.Web.GetBrandingManager().GetAvailableThemes();
                Assert.IsTrue(themes.Any());
                Assert.IsTrue(themes.Where(p => !p.IsCustomTheme).Count() == 9);
                Assert.IsTrue(!string.IsNullOrEmpty(themes.First().Name));
                Assert.IsTrue(!string.IsNullOrEmpty(themes.First().ThemeJson));
                Assert.IsTrue(themes.Last().IsCustomTheme == false);
            }
        }

        [TestMethod]
        public void GetAvailableThemesBatch()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var themes = context.Web.GetBrandingManager().GetAvailableThemesBatch();

                Assert.IsFalse(themes.IsAvailable);

                context.Execute();

                Assert.IsTrue(themes.IsAvailable);

                Assert.IsTrue(themes.Any());
                Assert.IsTrue(themes.Where(p => !p.IsCustomTheme).Count() == 9);
                Assert.IsTrue(!string.IsNullOrEmpty(themes.First().Name));
                Assert.IsTrue(!string.IsNullOrEmpty(themes.First().ThemeJson));
                Assert.IsTrue(themes.Last().IsCustomTheme == false);
            }
        }

        [TestMethod]
        public void SetTheme()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.Web.GetBrandingManager().SetTheme(Model.SharePoint.SharePointTheme.DarkYellow);

                // see if there are custom themes to apply
                var themes = context.Web.GetBrandingManager().GetAvailableThemes();

                var customTheme = themes.FirstOrDefault(p => p.IsCustomTheme);
                if (customTheme != null)
                {
                    context.Web.GetBrandingManager().SetTheme(customTheme);
                }

                // Reset to default theme again
                context.Web.GetBrandingManager().SetTheme(Model.SharePoint.SharePointTheme.Teal);
            }
        }

        [TestMethod]
        public void SetThemeBatch()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.Web.GetBrandingManager().SetThemeBatch(Model.SharePoint.SharePointTheme.DarkYellow);
                context.Execute();

                // see if there are custom themes to apply
                var themes = context.Web.GetBrandingManager().GetAvailableThemes();

                var customTheme = themes.FirstOrDefault(p => p.IsCustomTheme);
                if (customTheme != null)
                {
                    context.Web.GetBrandingManager().SetThemeBatch(customTheme);
                    context.Execute();
                }

                // Reset to default theme again
                context.Web.GetBrandingManager().SetThemeBatch(Model.SharePoint.SharePointTheme.Teal);
                context.Execute();
            }
        }

    }
}
