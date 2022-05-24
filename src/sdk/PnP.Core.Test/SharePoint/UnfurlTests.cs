using Microsoft.VisualStudio.TestTools.UnitTesting;
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
