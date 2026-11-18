using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Pins which surveyed pages <see cref="ObjectClientSidePageContents"/> extracts.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class ClientSidePageSelectionTests
    {
        private static HashSet<string> Requested(params string[] names) =>
            new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);

        private static bool IsSelected(PageToExport page, HashSet<string> requested, bool includeAllPages = false) =>
            ObjectClientSidePageContents.IsSelected(page, requested, includeAllPages);

        [TestMethod]
        public void WithoutPageNames_OnlyTheHomePageIsSelected()
        {
            Assert.IsTrue(IsSelected(new PageToExport { PageName = "Home.aspx", IsHomePage = true }, Requested()));
            Assert.IsFalse(IsSelected(new PageToExport { PageName = "News.aspx" }, Requested()));
        }

        [TestMethod]
        public void WithoutPageNames_IncludeAllPagesSelectsEveryPage()
        {
            Assert.IsTrue(IsSelected(new PageToExport { PageName = "News.aspx" }, Requested(), includeAllPages: true));
        }

        [TestMethod]
        public void AnExplicitPageListWinsOverIncludeAllPages()
        {
            var home = new PageToExport { PageName = "Home.aspx", IsHomePage = true };
            var news = new PageToExport { PageName = "News.aspx" };

            Assert.IsFalse(IsSelected(home, Requested("News.aspx"), includeAllPages: true));
            Assert.IsTrue(IsSelected(news, Requested("News.aspx"), includeAllPages: true));
        }

        [TestMethod]
        public void PageNamesAreMatchedCaseInsensitively()
        {
            Assert.IsTrue(IsSelected(new PageToExport { PageName = "News.aspx" }, Requested("news.ASPX")));
        }

        [TestMethod]
        public void PagesInSubfoldersNeedTheirFolder()
        {
            var page = new PageToExport { PageName = "Folder/News.aspx" };

            Assert.IsTrue(IsSelected(page, Requested("Folder/News.aspx")));
            Assert.IsFalse(IsSelected(page, Requested("News.aspx")));
        }

        [TestMethod]
        public void PageTemplatesNeedTheTemplatesFolder()
        {
            var template = new PageToExport { PageName = "Templates/Page.aspx", IsTemplate = true };

            Assert.IsTrue(IsSelected(template, Requested("Templates/Page.aspx")));
            Assert.IsFalse(IsSelected(template, Requested("Page.aspx")));
        }

        [TestMethod]
        public void TranslationsFollowTheirRequestedSourcePage()
        {
            var translation = new PageToExport { PageName = "sv/News.aspx", IsTranslation = true, SourcePageName = "News.aspx" };

            Assert.IsTrue(IsSelected(translation, Requested("News.aspx")));
        }

        [TestMethod]
        public void TranslationsOfOtherPagesAreNotSelected()
        {
            var translation = new PageToExport { PageName = "sv/Other.aspx", IsTranslation = true, SourcePageName = "Other.aspx" };

            Assert.IsFalse(IsSelected(translation, Requested("News.aspx")));
        }
    }
}
