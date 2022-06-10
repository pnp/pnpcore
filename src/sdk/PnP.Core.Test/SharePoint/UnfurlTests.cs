using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class UnfurlTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;            
        }

        [TestMethod]
        public async Task UnfurlFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            try
            {
                (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var unfurledResource = context.Web.UnfurlLink($"https://{context.Uri.DnsSafeHost}{documentUrl}");

                    Assert.IsTrue(unfurledResource != null);

                    Assert.IsTrue(unfurledResource.LinkType == UnfurlLinkType.File);
                    Assert.IsTrue(unfurledResource.Thumbnails.Count == 3);
                    Assert.IsTrue(unfurledResource.WebUrl == context.Uri);
                    Assert.IsTrue(unfurledResource.WebId == context.Web.Id);
                    Assert.IsTrue(unfurledResource.SiteId == context.Site.Id);
                    Assert.IsTrue(unfurledResource.Resource != null);
                    Assert.IsTrue(unfurledResource.ListId != Guid.Empty);
                    Assert.IsTrue(unfurledResource.LastModified < DateTime.Now);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.LastModifiedBy));
                    Assert.IsTrue(unfurledResource.ListDisplayName == "Documents");
                    Assert.IsTrue(unfurledResource.ListUrl != null);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.Name));
                    Assert.IsTrue(unfurledResource.ListItemId > 0);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.FileDriveId));
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.FileDriveItemId));
                    Assert.IsTrue(unfurledResource.FileUniqueId != Guid.Empty);
                    Assert.IsTrue(unfurledResource.Size > 0);

                    UnfurlOptions unfurlOptions = new()
                    {
                        ThumbnailOptions = new()
                        {
                            StandardSizes = new List<ThumbnailSize>
                            {
                                ThumbnailSize.Medium,
                                ThumbnailSize.Large
                            },
                            CustomSizes = new List<CustomThumbnailOptions>
                            {
                                new CustomThumbnailOptions
                                {
                                    Width = 200,
                                    Height = 300,
                                },
                                new CustomThumbnailOptions
                                {
                                    Width = 400,
                                    Height = 500,
                                    Cropped = true,
                                },
                            }
                        }
                    };

                    unfurledResource = context.Web.UnfurlLink($"https://{context.Uri.DnsSafeHost}{documentUrl}", unfurlOptions);

                    Assert.IsTrue(unfurledResource.LinkType == UnfurlLinkType.File);
                    Assert.IsTrue(unfurledResource.WebUrl == context.Uri);
                    Assert.IsTrue(unfurledResource.WebId == context.Web.Id);
                    Assert.IsTrue(unfurledResource.SiteId == context.Site.Id);
                    Assert.IsTrue(unfurledResource.Resource != null);
                    Assert.IsTrue(unfurledResource.ListId != Guid.Empty);
                    Assert.IsTrue(unfurledResource.LastModified < DateTime.Now);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.LastModifiedBy));
                    Assert.IsTrue(unfurledResource.ListDisplayName == "Documents");
                    Assert.IsTrue(unfurledResource.ListUrl != null);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.Name));
                    Assert.IsTrue(unfurledResource.ListItemId > 0);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.FileDriveId));
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.FileDriveItemId));
                    Assert.IsTrue(unfurledResource.FileUniqueId != Guid.Empty);
                    Assert.IsTrue(unfurledResource.Size > 0);
                    Assert.IsTrue(unfurledResource.Thumbnails.Count == 4);

                    foreach (var thumbnail in unfurledResource.Thumbnails)
                    {
                        Assert.IsTrue(!string.IsNullOrEmpty(thumbnail.Size));
                        Assert.IsNotNull(thumbnail.Url);
                        Assert.IsTrue(thumbnail.Width > 0);
                        Assert.IsTrue(thumbnail.Height > 0);
                    }

                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task UnfurlListItemTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // Take the first list item
                // For now hardcoded to item 1 of https://bertonline.sharepoint.com/sites/prov-1/Lists/sharingtestdonotchange
                var unfurledResource = context.Web.UnfurlLink($"https://bertonline.sharepoint.com/:li:/s/prov-1/Ey-0-TrWIcxChNFTqyeIm7EB_ktJbcLFxa-EIkZedafGYQ?e=PhOfF5");

                Assert.IsTrue(unfurledResource != null);

                Assert.IsTrue(unfurledResource.LinkType == UnfurlLinkType.ListItem);
                Assert.IsTrue(unfurledResource.Thumbnails.Count == 0);
                Assert.IsTrue(unfurledResource.WebUrl == new Uri("https://bertonline.sharepoint.com/sites/prov-1"));
                Assert.IsTrue(unfurledResource.WebId != Guid.Empty);
                Assert.IsTrue(unfurledResource.SiteId != Guid.Empty);
                Assert.IsTrue(unfurledResource.Resource != null);
                Assert.IsTrue(unfurledResource.ListId != Guid.Empty);
                Assert.IsTrue(unfurledResource.LastModified < DateTime.Now);
                Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.LastModifiedBy));
                Assert.IsTrue(unfurledResource.ListDisplayName == "sharingtestdonotchange");
                Assert.IsTrue(unfurledResource.ListUrl != null);
                Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.Name));
                Assert.IsTrue(unfurledResource.ListItemId > 0);
                Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveId));
                Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveItemId));
                Assert.IsTrue(unfurledResource.FileUniqueId == Guid.Empty);
                Assert.IsTrue(unfurledResource.Size == 0);
            }
        }

        [TestMethod]
        public async Task UnfurlListTest()
        {
            //TestCommon.Instance.Mocking = false;

            // Create a new list
            string listTitle = TestCommon.GetPnPSdkTestAssetName("UnfurlListTest");

            IList myList = null;

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                        myList.Load(p => p.RootFolder.QueryProperties(p => p.ServerRelativeUrl));
                    }

                    var unfurledResource = context.Web.UnfurlLink($"https://{context.Uri.DnsSafeHost}{myList.RootFolder.ServerRelativeUrl}");

                    Assert.IsTrue(unfurledResource != null);

                    Assert.IsTrue(unfurledResource.LinkType == UnfurlLinkType.List);
                    Assert.IsTrue(unfurledResource.Thumbnails.Count == 0);
                    Assert.IsTrue(unfurledResource.WebUrl == context.Uri);
                    Assert.IsTrue(unfurledResource.WebId == context.Web.Id);
                    Assert.IsTrue(unfurledResource.SiteId == context.Site.Id);
                    Assert.IsTrue(unfurledResource.Resource != null);
                    Assert.IsTrue(unfurledResource.ListId != Guid.Empty);
                    Assert.IsTrue(unfurledResource.LastModified < DateTime.Now);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.LastModifiedBy));
                    Assert.IsTrue(unfurledResource.ListDisplayName == listTitle);
                    Assert.IsTrue(unfurledResource.ListUrl != null);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.Name));
                    Assert.IsTrue(unfurledResource.ListItemId == 0);
                    Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveId));
                    Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveItemId));
                    Assert.IsTrue(unfurledResource.FileUniqueId == Guid.Empty);
                    Assert.IsTrue(unfurledResource.Size == 0);
                }
            }
            finally
            {
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UnfurlLibraryTest()
        {
            //TestCommon.Instance.Mocking = false;

            // Create a new list
            string listTitle = TestCommon.GetPnPSdkTestAssetName("UnfurlLibraryTest");

            IList myList = null;

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                        myList.Load(p => p.RootFolder.QueryProperties(p => p.ServerRelativeUrl));
                    }

                    var unfurledResource = context.Web.UnfurlLink($"https://{context.Uri.DnsSafeHost}{myList.RootFolder.ServerRelativeUrl}");

                    Assert.IsTrue(unfurledResource != null);

                    Assert.IsTrue(unfurledResource.LinkType == UnfurlLinkType.Library);
                    Assert.IsTrue(unfurledResource.Thumbnails.Count == 0);
                    Assert.IsTrue(unfurledResource.WebUrl == context.Uri);
                    Assert.IsTrue(unfurledResource.WebId == context.Web.Id);
                    Assert.IsTrue(unfurledResource.SiteId == context.Site.Id);
                    Assert.IsTrue(unfurledResource.Resource != null);
                    Assert.IsTrue(unfurledResource.ListId != Guid.Empty);
                    Assert.IsTrue(unfurledResource.LastModified < DateTime.Now);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.LastModifiedBy));
                    Assert.IsTrue(unfurledResource.ListDisplayName == listTitle);
                    Assert.IsTrue(unfurledResource.ListUrl != null);
                    Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.Name));
                    Assert.IsTrue(unfurledResource.ListItemId == 0);
                    Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveId));
                    Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveItemId));
                    Assert.IsTrue(unfurledResource.FileUniqueId == Guid.Empty);
                    Assert.IsTrue(unfurledResource.Size == 0);
                }
            }
            finally
            {
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UnfurlSitePagesLibraryTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {                
                var unfurledResource = context.Web.UnfurlLink($"{context.Uri}/sitepages");

                Assert.IsTrue(unfurledResource != null);

                Assert.IsTrue(unfurledResource.LinkType == UnfurlLinkType.SitePagesLibrary);
                Assert.IsTrue(unfurledResource.Thumbnails.Count == 0);
                Assert.IsTrue(unfurledResource.WebUrl == context.Uri);
                Assert.IsTrue(unfurledResource.WebId == context.Web.Id);
                Assert.IsTrue(unfurledResource.SiteId == context.Site.Id);
                Assert.IsTrue(unfurledResource.Resource != null);
                Assert.IsTrue(unfurledResource.ListId != Guid.Empty);
                Assert.IsTrue(unfurledResource.LastModified < DateTime.Now);
                Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.LastModifiedBy));
                Assert.IsTrue(unfurledResource.ListDisplayName == "Site Pages");
                Assert.IsTrue(unfurledResource.ListUrl != null);
                Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.Name));
                Assert.IsTrue(unfurledResource.ListItemId == 0);
                Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveId));
                Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveItemId));
                Assert.IsTrue(unfurledResource.FileUniqueId == Guid.Empty);
                Assert.IsTrue(unfurledResource.Size == 0);
            }
        }

        [TestMethod]
        public async Task UnfurlSitePageTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var unfurledResource = context.Web.UnfurlLink($"{context.Uri}/sitepages/home.aspx");

                Assert.IsTrue(unfurledResource != null);

                Assert.IsTrue(unfurledResource.LinkType == UnfurlLinkType.SitePage);
                Assert.IsTrue(unfurledResource.Thumbnails.Count == 0);
                Assert.IsTrue(unfurledResource.WebUrl == context.Uri);
                Assert.IsTrue(unfurledResource.WebId == context.Web.Id);
                Assert.IsTrue(unfurledResource.SiteId == context.Site.Id);
                Assert.IsTrue(unfurledResource.Resource != null);
                Assert.IsTrue(unfurledResource.ListId != Guid.Empty);
                Assert.IsTrue(unfurledResource.LastModified < DateTime.Now);
                Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.LastModifiedBy));
                Assert.IsTrue(unfurledResource.ListDisplayName == "Site Pages");
                Assert.IsTrue(unfurledResource.ListUrl != null);
                Assert.IsTrue(!string.IsNullOrEmpty(unfurledResource.Name));
                Assert.IsTrue(unfurledResource.ListItemId > 0);
                Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveId));
                Assert.IsTrue(string.IsNullOrEmpty(unfurledResource.FileDriveItemId));
                Assert.IsTrue(unfurledResource.FileUniqueId == Guid.Empty);
                Assert.IsTrue(unfurledResource.Size > 0);
            }
        }

        [TestMethod]
        public async Task GetThumbnailsFromFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            try
            {
                (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    var thumbnails = file.GetThumbnails();

                    Assert.IsNotNull(thumbnails);
                    Assert.IsTrue(thumbnails.Count > 0);

                    foreach(var thumbnail in thumbnails)
                    {
                        Assert.IsTrue(!string.IsNullOrEmpty(thumbnail.SetId));
                        Assert.IsTrue(!string.IsNullOrEmpty(thumbnail.Size));
                        Assert.IsNotNull(thumbnail.Url);
                        Assert.IsTrue(thumbnail.Width > 0);
                        Assert.IsTrue(thumbnail.Height > 0);
                    }
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task GetThumbnailsWithOptionsFromFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            try
            {
                (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    ThumbnailOptions options = new()
                    {
                        StandardSizes = new List<ThumbnailSize>
                        {
                            ThumbnailSize.Medium,
                            ThumbnailSize.Large
                        },
                        CustomSizes = new List<CustomThumbnailOptions>
                        {
                            new CustomThumbnailOptions
                            {
                                Width = 200,
                                Height = 300,                                
                            },
                            new CustomThumbnailOptions
                            {
                                Width = 400,
                                Height = 500,
                                Cropped = true,
                            },
                        }
                    };

                    var thumbnails = file.GetThumbnails(options);

                    Assert.IsNotNull(thumbnails);
                    Assert.IsTrue(thumbnails.Count == 4);

                    foreach (var thumbnail in thumbnails)
                    {
                        Assert.IsTrue(!string.IsNullOrEmpty(thumbnail.Size));
                        Assert.IsNotNull(thumbnail.Url);
                        Assert.IsTrue(thumbnail.Width > 0);
                        Assert.IsTrue(thumbnail.Height > 0);
                    }
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task GetThumbnailsWithOptionsFromFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            try
            {
                (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    ThumbnailOptions options = new()
                    {
                        StandardSizes = new List<ThumbnailSize>
                        {
                            ThumbnailSize.Medium,
                            ThumbnailSize.Large
                        },
                        CustomSizes = new List<CustomThumbnailOptions>
                        {
                            new CustomThumbnailOptions
                            {
                                Width = 200,
                                Height = 300,
                            },
                            new CustomThumbnailOptions
                            {
                                Width = 400,
                                Height = 500,
                                Cropped = true,
                            },
                        }
                    };

                    var thumbnails = file.GetThumbnailsBatch(options);

                    Assert.IsNotNull(thumbnails);
                    Assert.IsFalse(thumbnails.IsAvailable);

                    context.Execute();

                    Assert.IsTrue(thumbnails.IsAvailable);
                    Assert.IsTrue(thumbnails.Count == 4);

                    foreach (var thumbnail in thumbnails)
                    {
                        Assert.IsTrue(!string.IsNullOrEmpty(thumbnail.Size));
                        Assert.IsNotNull(thumbnail.Url);
                        Assert.IsTrue(thumbnail.Width > 0);
                        Assert.IsTrue(thumbnail.Height > 0);
                    }
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

    }
}
