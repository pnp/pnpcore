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

        [TestMethod]
        public void GetChromeOptionsForTeamSite()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var chrome = context.Web.GetBrandingManager().GetChromeOptions();

                Assert.IsTrue(chrome != null);                
                Assert.IsNotNull(chrome.Header);
                Assert.IsNull(chrome.Footer);
                Assert.IsNull(chrome.Navigation);
            }
        }

        [TestMethod]
        public void GetChromeOptionsForCommunicationSite()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.NoGroupTestSite))
            {
                var chrome = context.Web.GetBrandingManager().GetChromeOptions();

                Assert.IsTrue(chrome != null);
                Assert.IsNotNull(chrome.Header);
                Assert.IsNotNull(chrome.Footer);
                Assert.IsNotNull(chrome.Navigation);
            }
        }

        [TestMethod]
        public void GetChromeOptionsForTeamSiteBatch()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var chrome = context.Web.GetBrandingManager().GetChromeOptionsBatch();

                Assert.IsFalse(chrome.IsAvailable);

                context.Execute();

                Assert.IsTrue(chrome.IsAvailable);

                Assert.IsTrue(chrome.Result != null);
                Assert.IsNotNull(chrome.Result.Header);
                Assert.IsNull(chrome.Result.Footer);
                Assert.IsNull(chrome.Result.Navigation);
            }
        }

        [TestMethod]
        public void GetChromeOptionsForCommunicationSiteBatch()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.NoGroupTestSite))
            {
                var chrome = context.Web.GetBrandingManager().GetChromeOptionsBatch();

                Assert.IsFalse(chrome.IsAvailable);

                context.Execute();

                Assert.IsTrue(chrome.IsAvailable);

                Assert.IsTrue(chrome.Result != null);
                Assert.IsNotNull(chrome.Result.Header);
                Assert.IsNotNull(chrome.Result.Footer);
                Assert.IsNotNull(chrome.Result.Navigation);
            }
        }

        [TestMethod]
        public void SetChromeOptionsForTeamSite()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var chrome = context.Web.GetBrandingManager().GetChromeOptions();

                Assert.IsTrue(chrome != null);

                chrome.Header.Emphasis = Model.SharePoint.VariantThemeType.Strong;
                chrome.Header.HideTitle = true;
                chrome.Header.Layout = Model.SharePoint.HeaderLayoutType.Extended;
                chrome.Header.LogoAlignment = Model.SharePoint.LogoAlignment.Middle;

                context.Web.GetBrandingManager().SetChromeOptions(chrome);

                chrome = context.Web.GetBrandingManager().GetChromeOptions();
                
                Assert.IsTrue(chrome != null);
                Assert.IsTrue(chrome.Header.Emphasis == Model.SharePoint.VariantThemeType.Strong);
                Assert.IsTrue(chrome.Header.HideTitle == true);
                Assert.IsTrue(chrome.Header.Layout == Model.SharePoint.HeaderLayoutType.Extended);
                Assert.IsTrue(chrome.Header.LogoAlignment == Model.SharePoint.LogoAlignment.Middle);

                // Also verify the respective web properties are updated
                Assert.IsTrue(context.Web.HeaderEmphasis == Model.SharePoint.VariantThemeType.Strong);
                Assert.IsTrue(context.Web.HideTitleInHeader == true);
                Assert.IsTrue(context.Web.HeaderLayout == Model.SharePoint.HeaderLayoutType.Extended);
                Assert.IsTrue(context.Web.LogoAlignment == Model.SharePoint.LogoAlignment.Middle);

                // Reset chrome options again
                chrome.Header.Emphasis = Model.SharePoint.VariantThemeType.None;
                chrome.Header.HideTitle = false;
                chrome.Header.Layout = Model.SharePoint.HeaderLayoutType.None;
                chrome.Header.LogoAlignment = Model.SharePoint.LogoAlignment.Left;

                context.Web.GetBrandingManager().SetChromeOptions(chrome);
            }
        }

        [TestMethod]
        public void SetChromeOptionsForTeamSiteBatch()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var chrome = context.Web.GetBrandingManager().GetChromeOptions();

                Assert.IsTrue(chrome != null);

                chrome.Header.Emphasis = Model.SharePoint.VariantThemeType.Strong;
                chrome.Header.HideTitle = true;
                chrome.Header.Layout = Model.SharePoint.HeaderLayoutType.Extended;
                chrome.Header.LogoAlignment = Model.SharePoint.LogoAlignment.Middle;

                context.Web.GetBrandingManager().SetChromeOptionsBatch(chrome);

                context.Execute();

                // Also verify the respective web properties are updated
                Assert.IsTrue(context.Web.HeaderEmphasis == Model.SharePoint.VariantThemeType.Strong);
                Assert.IsTrue(context.Web.HideTitleInHeader == true);
                Assert.IsTrue(context.Web.HeaderLayout == Model.SharePoint.HeaderLayoutType.Extended);
                Assert.IsTrue(context.Web.LogoAlignment == Model.SharePoint.LogoAlignment.Middle);

                chrome = context.Web.GetBrandingManager().GetChromeOptions();

                Assert.IsTrue(chrome != null);
                Assert.IsTrue(chrome.Header.Emphasis == Model.SharePoint.VariantThemeType.Strong);
                Assert.IsTrue(chrome.Header.HideTitle == true);
                Assert.IsTrue(chrome.Header.Layout == Model.SharePoint.HeaderLayoutType.Extended);
                Assert.IsTrue(chrome.Header.LogoAlignment == Model.SharePoint.LogoAlignment.Middle);

                // Reset chrome options again
                chrome.Header.Emphasis = Model.SharePoint.VariantThemeType.None;
                chrome.Header.HideTitle = false;
                chrome.Header.Layout = Model.SharePoint.HeaderLayoutType.None;
                chrome.Header.LogoAlignment = Model.SharePoint.LogoAlignment.Left;

                context.Web.GetBrandingManager().SetChromeOptionsBatch(chrome);

                context.Execute();
            }
        }

        [TestMethod]
        public void SetChromeOptionsForCommunicationSite()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.NoGroupTestSite))
            {
                var chrome = context.Web.GetBrandingManager().GetChromeOptions();

                Assert.IsTrue(chrome != null);

                chrome.Header.Emphasis = Model.SharePoint.VariantThemeType.Strong;
                chrome.Header.HideTitle = true;
                chrome.Header.Layout = Model.SharePoint.HeaderLayoutType.Extended;
                chrome.Header.LogoAlignment = Model.SharePoint.LogoAlignment.Middle;
                chrome.Navigation.MegaMenuEnabled = true;
                chrome.Navigation.Visible = false;
                chrome.Footer.Enabled = true;
                chrome.Footer.Emphasis = Model.SharePoint.FooterVariantThemeType.None;
                chrome.Footer.Layout = Model.SharePoint.FooterLayoutType.Extended;

                context.Web.GetBrandingManager().SetChromeOptions(chrome);

                chrome = context.Web.GetBrandingManager().GetChromeOptions();

                Assert.IsTrue(chrome != null);
                Assert.IsTrue(chrome.Header.Emphasis == Model.SharePoint.VariantThemeType.Strong);
                Assert.IsTrue(chrome.Header.HideTitle == true);
                Assert.IsTrue(chrome.Header.Layout == Model.SharePoint.HeaderLayoutType.Extended);
                Assert.IsTrue(chrome.Header.LogoAlignment == Model.SharePoint.LogoAlignment.Middle);
                Assert.IsTrue(chrome.Navigation.MegaMenuEnabled == true);
                Assert.IsTrue(chrome.Navigation.Visible == false);
                Assert.IsTrue(chrome.Footer.Enabled == true);
                Assert.IsTrue(chrome.Footer.Emphasis == Model.SharePoint.FooterVariantThemeType.None);
                Assert.IsTrue(chrome.Footer.Layout == Model.SharePoint.FooterLayoutType.Extended);

                // Also verify the respective web properties are updated
                Assert.IsTrue(context.Web.HeaderEmphasis == Model.SharePoint.VariantThemeType.Strong);
                Assert.IsTrue(context.Web.HideTitleInHeader == true);
                Assert.IsTrue(context.Web.HeaderLayout == Model.SharePoint.HeaderLayoutType.Extended);
                Assert.IsTrue(context.Web.LogoAlignment == Model.SharePoint.LogoAlignment.Middle);
                Assert.IsTrue(context.Web.MegaMenuEnabled == true);
                Assert.IsTrue(context.Web.QuickLaunchEnabled == false);
                Assert.IsTrue(context.Web.FooterEnabled == true);
                Assert.IsTrue(context.Web.FooterEmphasis == Model.SharePoint.FooterVariantThemeType.None);
                Assert.IsTrue(context.Web.FooterLayout == Model.SharePoint.FooterLayoutType.Extended);

                // Reset chrome options again
                chrome.Header.Emphasis = Model.SharePoint.VariantThemeType.None;
                chrome.Header.HideTitle = false;
                chrome.Header.Layout = Model.SharePoint.HeaderLayoutType.None;
                chrome.Header.LogoAlignment = Model.SharePoint.LogoAlignment.Left;
                chrome.Navigation.MegaMenuEnabled = false;
                chrome.Navigation.Visible = true;
                chrome.Footer.Enabled = true;
                chrome.Footer.Emphasis = Model.SharePoint.FooterVariantThemeType.Strong;
                chrome.Footer.Layout = Model.SharePoint.FooterLayoutType.Simple;

                context.Web.GetBrandingManager().SetChromeOptions(chrome);
            }
        }
    }
}
