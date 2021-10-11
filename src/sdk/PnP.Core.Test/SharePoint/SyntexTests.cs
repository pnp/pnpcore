using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class SyntexTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task IsSyntexContentCenterNegative()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(await context.Web.IsSyntexContentCenterAsync());
                Assert.IsFalse(context.Web.IsSyntexContentCenter());
            }
        }

        [TestMethod]
        public async Task IsSyntexContentCenterPositive()
        {
            //TestCommon.Instance.Mocking = false;            
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                Assert.IsTrue(await context.Web.IsSyntexContentCenterAsync());
                Assert.IsTrue(context.Web.IsSyntexContentCenter());
            }
        }

        [TestMethod]
        public async Task AsSyntexContentCenterNegative()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsTrue(await context.Web.AsSyntexContentCenterAsync() == null);
                Assert.IsTrue(context.Web.AsSyntexContentCenter() == null);
            }
        }

        [TestMethod]
        public async Task AsSyntexContentCenterPositive()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                Assert.IsTrue(cc != null);
                Assert.IsTrue(cc.Web.Id == context.Web.Id);
                Assert.IsTrue(context.Web.AsSyntexContentCenter() != null);
            }
        }

        [TestMethod]
        public async Task GetSyntexModels()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                Assert.IsTrue(models.Any());
                Assert.IsTrue(!string.IsNullOrEmpty(models.First().Name));
                Assert.IsTrue(models.First().ModelLastTrained != DateTime.MinValue);
                Assert.IsTrue(models.First().Id > 0);
                string description = models.First().Description;                
            }
        }

        [TestMethod]
        public async Task GetSyntexModelsViaFilter()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = context.Web.AsSyntexContentCenter();
                var models = cc.GetSyntexModels("XYZ");
                Assert.IsFalse(models.Any());
            }
        }

        [TestMethod]
        public async Task PublishUnPublishModelToList()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelToList");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);

                // publish model to library
                var result = await modelToRegister.PublishModelAsync(testLibrary);
                Assert.IsTrue(result != null);
                Assert.IsTrue(result.ErrorMessage == null);
                Assert.IsTrue(result.StatusCode == 201);
                Assert.IsTrue(result.Succeeded);

                // unpublish model from library
                var unpublishResult = await modelToRegister.UnPublishModelAsync(testLibrary);
                Assert.IsTrue(unpublishResult != null);
                Assert.IsTrue(unpublishResult.ErrorMessage == null);
                Assert.IsTrue(unpublishResult.StatusCode == 200);
                Assert.IsTrue(unpublishResult.Succeeded);

                // Publish model again via batch request
                var batchPublishResult = await modelToRegister.PublishModelBatchAsync(testLibrary);
                Assert.IsFalse(batchPublishResult.IsAvailable);
                await context.ExecuteAsync();
                Assert.IsTrue(batchPublishResult.IsAvailable);
                Assert.IsTrue(batchPublishResult.Result.ErrorMessage == null);
                Assert.IsTrue(batchPublishResult.Result.StatusCode == 201);
                Assert.IsTrue(batchPublishResult.Result.Succeeded);

                // Unpublish model via implicit batch request
                var unpublishBatchResult = await modelToRegister.UnPublishModelBatchAsync(testLibrary);
                Assert.IsFalse(unpublishBatchResult.IsAvailable);
                await context.ExecuteAsync();
                Assert.IsTrue(unpublishBatchResult.IsAvailable);
                Assert.IsTrue(unpublishBatchResult.Result.ErrorMessage == null);
                Assert.IsTrue(unpublishBatchResult.Result.StatusCode == 200);
                Assert.IsTrue(unpublishBatchResult.Result.Succeeded);

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishUnPublishModelToList2()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelToList2");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);

                await context.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

                // publish model to library
                var result = await modelToRegister.PublishModelAsync(new SyntexModelPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });
                Assert.IsTrue(result != null);
                Assert.IsTrue(result.ErrorMessage == null);
                Assert.IsTrue(result.StatusCode == 201);
                Assert.IsTrue(result.Succeeded);

                // unpublish model from library
                var unpublishResult = await modelToRegister.UnPublishModelAsync(new SyntexModelUnPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });
                Assert.IsTrue(unpublishResult != null);
                Assert.IsTrue(unpublishResult.ErrorMessage == null);
                Assert.IsTrue(unpublishResult.StatusCode == 200);
                Assert.IsTrue(unpublishResult.Succeeded);

                // Publish via batch request
                var batchPublishResult = await modelToRegister.PublishModelBatchAsync(new SyntexModelPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });
                Assert.IsFalse(batchPublishResult.IsAvailable);
                await context.ExecuteAsync();
                Assert.IsTrue(batchPublishResult.IsAvailable);
                Assert.IsTrue(batchPublishResult.Result.ErrorMessage == null);
                Assert.IsTrue(batchPublishResult.Result.StatusCode == 201);
                Assert.IsTrue(batchPublishResult.Result.Succeeded);

                // unpublish model from library
                var unpublishBatchResult = await modelToRegister.UnPublishModelBatchAsync(new SyntexModelUnPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });
                Assert.IsFalse(unpublishBatchResult.IsAvailable);
                context.Execute();
                Assert.IsTrue(unpublishBatchResult.IsAvailable);
                Assert.IsTrue(unpublishBatchResult.Result.ErrorMessage == null);
                Assert.IsTrue(unpublishBatchResult.Result.StatusCode == 200);
                Assert.IsTrue(unpublishBatchResult.Result.Succeeded);

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishUnPublishModelsToList()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library to site
                var libraryName1 = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelsToList1");
                var libraryName2 = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelsToList2");
                var library1 = await context.Web.Lists.AddAsync(libraryName1, ListTemplateType.DocumentLibrary);
                var library2 = await context.Web.Lists.AddAsync(libraryName2, ListTemplateType.DocumentLibrary);

                List<IList> libraries = new();
                libraries.Add(library1);
                libraries.Add(library2);

                // publish model to library
                var results = await modelToRegister.PublishModelAsync(libraries);
                Assert.IsTrue(results != null);
                foreach (var result in results)
                {
                    Assert.IsTrue(result.ErrorMessage == null);
                    Assert.IsTrue(result.StatusCode == 201);
                    Assert.IsTrue(result.Succeeded);
                }

                // unpublish model from library
                var unpublishResults = await modelToRegister.UnPublishModelAsync(libraries);
                Assert.IsTrue(unpublishResults != null);
                foreach (var unpublishResult in unpublishResults)
                {
                    Assert.IsTrue(unpublishResult.ErrorMessage == null);
                    Assert.IsTrue(unpublishResult.StatusCode == 200);
                    Assert.IsTrue(unpublishResult.Succeeded);
                }

                // Publish model again via batch request
                var batchPublishResult = await modelToRegister.PublishModelBatchAsync(libraries);
                Assert.IsFalse(batchPublishResult.IsAvailable);
                await context.ExecuteAsync();

                foreach(var result in batchPublishResult)
                {
                    Assert.IsTrue(result.ErrorMessage == null);
                    Assert.IsTrue(result.StatusCode == 201);
                    Assert.IsTrue(result.Succeeded);
                }

                // unpublish model from library via batch
                var unpublishBatchResults = await modelToRegister.UnPublishModelBatchAsync(libraries);
                Assert.IsFalse(unpublishBatchResults.IsAvailable);
                await context.ExecuteAsync();
                Assert.IsTrue(unpublishBatchResults.IsAvailable);

                foreach (var unpublishResult in unpublishBatchResults)
                {
                    Assert.IsTrue(unpublishResult.ErrorMessage == null);
                    Assert.IsTrue(unpublishResult.StatusCode == 200);
                    Assert.IsTrue(unpublishResult.Succeeded);
                }

                // cleanup the libraries
                await library1.DeleteAsync();
                await library2.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishUnPublishModelsToList2()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                await context.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

                // Add library to site
                var libraryName1 = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelsToList21");
                var libraryName2 = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelsToList22");
                var library1 = await context.Web.Lists.AddAsync(libraryName1, ListTemplateType.DocumentLibrary);
                var library2 = await context.Web.Lists.AddAsync(libraryName2, ListTemplateType.DocumentLibrary);

                List<SyntexModelPublishOptions> publications = new();
                publications.Add(new SyntexModelPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName1}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });
                publications.Add(new SyntexModelPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName2}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });

                // publish model to library
                var results = await modelToRegister.PublishModelAsync(publications);
                Assert.IsTrue(results != null);
                foreach (var result in results)
                {
                    Assert.IsTrue(result.ErrorMessage == null);
                    Assert.IsTrue(result.StatusCode == 201);
                    Assert.IsTrue(result.Succeeded);
                }

                // unpublish model from library
                var unpublishResults = await modelToRegister.UnPublishModelAsync(publications.Cast<SyntexModelUnPublishOptions>().ToList());
                Assert.IsTrue(unpublishResults != null);
                foreach (var unpublishResult in unpublishResults)
                {
                    Assert.IsTrue(unpublishResult.ErrorMessage == null);
                    Assert.IsTrue(unpublishResult.StatusCode == 200);
                    Assert.IsTrue(unpublishResult.Succeeded);
                }

                // Publish model again via batch request
                var batchPublishResult = await modelToRegister.PublishModelBatchAsync(publications);
                Assert.IsFalse(batchPublishResult.IsAvailable);
                await context.ExecuteAsync();

                foreach (var result in batchPublishResult)
                {
                    Assert.IsTrue(result.ErrorMessage == null);
                    Assert.IsTrue(result.StatusCode == 201);
                    Assert.IsTrue(result.Succeeded);
                }

                // unpublish model from library via batch request
                var unpublishBatchResults = await modelToRegister.UnPublishModelBatchAsync(publications.Cast<SyntexModelUnPublishOptions>().ToList());
                Assert.IsFalse(unpublishBatchResults.IsAvailable);
                context.Execute();
                Assert.IsTrue(unpublishBatchResults.IsAvailable);
                foreach (var unpublishResult in unpublishBatchResults)
                {
                    Assert.IsTrue(unpublishResult.ErrorMessage == null);
                    Assert.IsTrue(unpublishResult.StatusCode == 200);
                    Assert.IsTrue(unpublishResult.Succeeded);
                }

                // cleanup the libraries
                await library1.DeleteAsync();
                await library2.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetModelPublicationsTest()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = context.Web.AsSyntexContentCenter();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("GetModelPublicationsTest");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);

                await context.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

                // publish model to library
                var result = await modelToRegister.PublishModelAsync(new SyntexModelPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });

                // Get the model publication results
                var results = await modelToRegister.GetModelPublicationsAsync();
                Assert.IsTrue(results.Any());

                var modelPublication = results.FirstOrDefault(p => p.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication != null);
                Assert.IsTrue(modelPublication.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(modelPublication.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                Assert.IsTrue(modelPublication.ViewOption == MachineLearningPublicationViewOption.NewViewAsDefault);

                results = modelToRegister.GetModelPublications();
                Assert.IsTrue(results.Any());

                modelPublication = results.FirstOrDefault(p => p.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication != null);
                Assert.IsTrue(modelPublication.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(modelPublication.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                Assert.IsTrue(modelPublication.ViewOption == MachineLearningPublicationViewOption.NewViewAsDefault);

                var batch = context.NewBatch();
                var batchResult = await modelToRegister.GetModelPublicationsBatchAsync(batch);
                Assert.IsFalse(batchResult.IsAvailable);                
                await context.ExecuteAsync(batch);
                Assert.IsTrue(batchResult.IsAvailable);

                modelPublication = batchResult.FirstOrDefault(p => p.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication != null);
                Assert.IsTrue(modelPublication.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(modelPublication.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                Assert.IsTrue(modelPublication.ViewOption == MachineLearningPublicationViewOption.NewViewAsDefault);

                batch = context.NewBatch();
                batchResult = modelToRegister.GetModelPublicationsBatch(batch);
                Assert.IsFalse(batchResult.IsAvailable);
                await context.ExecuteAsync(batch);
                Assert.IsTrue(batchResult.IsAvailable);

                modelPublication = batchResult.FirstOrDefault(p => p.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication != null);
                Assert.IsTrue(modelPublication.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(modelPublication.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                Assert.IsTrue(modelPublication.ViewOption == MachineLearningPublicationViewOption.NewViewAsDefault);

                batchResult = await modelToRegister.GetModelPublicationsBatchAsync();
                Assert.IsFalse(batchResult.IsAvailable);
                await context.ExecuteAsync();
                Assert.IsTrue(batchResult.IsAvailable);

                modelPublication = batchResult.FirstOrDefault(p => p.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication != null);
                Assert.IsTrue(modelPublication.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(modelPublication.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                Assert.IsTrue(modelPublication.ViewOption == MachineLearningPublicationViewOption.NewViewAsDefault);

                batchResult = modelToRegister.GetModelPublicationsBatch();
                Assert.IsFalse(batchResult.IsAvailable);
                await context.ExecuteAsync();
                Assert.IsTrue(batchResult.IsAvailable);

                modelPublication = batchResult.FirstOrDefault(p => p.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication != null);
                Assert.IsTrue(modelPublication.TargetLibraryServerRelativeUrl == $"{context.Web.ServerRelativeUrl}/{libraryName}");
                Assert.IsTrue(modelPublication.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(modelPublication.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                Assert.IsTrue(modelPublication.ViewOption == MachineLearningPublicationViewOption.NewViewAsDefault);

                // unpublish model
                var unpublishResult = await modelToRegister.UnPublishModelAsync(new SyntexModelUnPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ClassifyAndExtractFile()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                await context.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl);

                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library with file to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("ClassifyAndExtractFile");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);
                await testLibrary.EnsurePropertiesAsync(p => p.RootFolder);
                IFile testDocument = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument2 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile2.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument3 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile3.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);

                // publish model to library
                var result = await modelToRegister.PublishModelAsync(testLibrary);
                Assert.IsTrue(result != null);
                Assert.IsTrue(result.ErrorMessage == null);
                Assert.IsTrue(result.StatusCode == 201);
                Assert.IsTrue(result.Succeeded);

                // Classify and extract existing content for one file
                var classifyInformation = await testDocument.ClassifyAndExtractAsync();
                Assert.IsTrue(classifyInformation != null);
                Assert.IsTrue(classifyInformation.Created != DateTime.MinValue);
                Assert.IsTrue(classifyInformation.DeliverDate != DateTime.MinValue);
                Assert.IsTrue(classifyInformation.ErrorMessage == null);
                Assert.IsTrue(classifyInformation.StatusCode == 201);
                Assert.IsTrue(classifyInformation.Status == "ExponentialBackoff");
                Assert.IsTrue(classifyInformation.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(classifyInformation.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                Assert.IsTrue(classifyInformation.TargetServerRelativeUrl == testDocument.ServerRelativeUrl);

                // Classify and extract via batch request
                var file1BatchResult = await testDocument2.ClassifyAndExtractBatchAsync();
                var file2BatchResult = await testDocument3.ClassifyAndExtractBatchAsync();
                Assert.IsFalse(file1BatchResult.IsAvailable);
                Assert.IsFalse(file2BatchResult.IsAvailable);
                var results = await context.ExecuteAsync();
                Assert.IsTrue(file1BatchResult.IsAvailable);
                Assert.IsTrue(file2BatchResult.IsAvailable);

                Assert.IsTrue(file1BatchResult.Result.Created != DateTime.MinValue);
                Assert.IsTrue(file1BatchResult.Result.DeliverDate != DateTime.MinValue);
                Assert.IsTrue(file1BatchResult.Result.ErrorMessage == null);
                Assert.IsTrue(file1BatchResult.Result.StatusCode == 201);
                Assert.IsTrue(file1BatchResult.Result.Status == "ExponentialBackoff");
                Assert.IsTrue(file1BatchResult.Result.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(file1BatchResult.Result.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                Assert.IsTrue(file1BatchResult.Result.TargetServerRelativeUrl == testDocument2.ServerRelativeUrl);

                Assert.IsTrue(file2BatchResult.Result.Created != DateTime.MinValue);
                Assert.IsTrue(file2BatchResult.Result.DeliverDate != DateTime.MinValue);
                Assert.IsTrue(file2BatchResult.Result.ErrorMessage == null);
                Assert.IsTrue(file2BatchResult.Result.StatusCode == 201);
                Assert.IsTrue(file2BatchResult.Result.Status == "ExponentialBackoff");
                Assert.IsTrue(file2BatchResult.Result.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(file2BatchResult.Result.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                Assert.IsTrue(file2BatchResult.Result.TargetServerRelativeUrl == testDocument3.ServerRelativeUrl);

                // Review the batch results

                // unpublish model from library
                var unpublishResult = await modelToRegister.UnPublishModelAsync(testLibrary);
                Assert.IsTrue(unpublishResult != null);
                Assert.IsTrue(unpublishResult.ErrorMessage == null);
                Assert.IsTrue(unpublishResult.StatusCode == 200);
                Assert.IsTrue(unpublishResult.Succeeded);

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ClassifyAndExtractList()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                await context.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl);

                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library with file to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("ClassifyAndExtractList");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);
                await testLibrary.EnsurePropertiesAsync(p => p.RootFolder);
                IFile testDocument = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument2 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile2.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument3 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile3.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument4 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile4.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument5 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile5.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument6 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile6.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument7 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile7.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument8 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile8.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument9 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile9.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument10 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile10.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);

                // publish model to library
                var result = await modelToRegister.PublishModelAsync(testLibrary);
                Assert.IsTrue(result != null);
                Assert.IsTrue(result.ErrorMessage == null);
                Assert.IsTrue(result.StatusCode == 201);
                Assert.IsTrue(result.Succeeded);

                // Classify and extract the library, use a small page size to trigger the paging logic
                var results = await testLibrary.ClassifyAndExtractAsync(pageSize:4);

                Assert.IsTrue(results.Count == 10);

                // Validate results
                foreach(var classifyInformation in results)
                {
                    Assert.IsTrue(classifyInformation.Created != DateTime.MinValue);
                    Assert.IsTrue(classifyInformation.DeliverDate != DateTime.MinValue);
                    Assert.IsTrue(classifyInformation.ErrorMessage == null);
                    Assert.IsTrue(classifyInformation.StatusCode == 201);
                    Assert.IsTrue(classifyInformation.Status == "ExponentialBackoff");
                    Assert.IsTrue(classifyInformation.TargetSiteUrl == context.Uri.ToString());
                    Assert.IsTrue(classifyInformation.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);
                }

                // unpublish model from library
                var unpublishResult = await modelToRegister.UnPublishModelAsync(testLibrary);
                Assert.IsTrue(unpublishResult != null);
                Assert.IsTrue(unpublishResult.ErrorMessage == null);
                Assert.IsTrue(unpublishResult.StatusCode == 200);
                Assert.IsTrue(unpublishResult.Succeeded);

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ClassifyAndExtractListOffPeak()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                await context.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl);

                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library with file to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("ClassifyAndExtractListOffPeak");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);
                await testLibrary.EnsurePropertiesAsync(p => p.RootFolder);
                IFile testDocument = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument2 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile2.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument3 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile3.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument4 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile4.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument5 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile5.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument6 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile6.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument7 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile7.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument8 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile8.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument9 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile9.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument10 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile10.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);

                // publish model to library
                var result = await modelToRegister.PublishModelAsync(testLibrary);
                Assert.IsTrue(result != null);
                Assert.IsTrue(result.ErrorMessage == null);
                Assert.IsTrue(result.StatusCode == 201);
                Assert.IsTrue(result.Succeeded);

                // Classify and extract the library off peak
                var classifyResult = testLibrary.ClassifyAndExtractOffPeak();

                // Validate results
                Assert.IsTrue(classifyResult.Created != DateTime.MinValue);
                Assert.IsTrue(classifyResult.DeliverDate != DateTime.MinValue);
                Assert.IsTrue(classifyResult.ErrorMessage == null);
                Assert.IsTrue(classifyResult.StatusCode == 201);
                Assert.IsTrue(classifyResult.Status == "ExponentialBackoff");
                Assert.IsTrue(classifyResult.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(classifyResult.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);

                // unpublish model from library
                var unpublishResult = await modelToRegister.UnPublishModelAsync(testLibrary);
                Assert.IsTrue(unpublishResult != null);
                Assert.IsTrue(unpublishResult.ErrorMessage == null);
                Assert.IsTrue(unpublishResult.StatusCode == 200);
                Assert.IsTrue(unpublishResult.Succeeded);

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ClassifyAndExtractListFolderOffPeak()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointSyntexTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                await context.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl);

                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library with file to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("ClassifyAndExtractListFolderOffPeak");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);
                await testLibrary.EnsurePropertiesAsync(p => p.RootFolder);

                var targetFolder = await testLibrary.RootFolder.EnsureFolderAsync("level1/level11");

                IFile testDocument = await targetFolder.Files.AddAsync("ClassifyAndExtractFile.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument2 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile2.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument3 = await targetFolder.Files.AddAsync("ClassifyAndExtractFile3.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument4 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile4.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument5 = await targetFolder.Files.AddAsync("ClassifyAndExtractFile5.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument6 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile6.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument7 = await targetFolder.Files.AddAsync("ClassifyAndExtractFile7.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument8 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile8.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument9 = await targetFolder.Files.AddAsync("ClassifyAndExtractFile9.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFile testDocument10 = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile10.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);

                // publish model to library
                var result = await modelToRegister.PublishModelAsync(testLibrary);
                Assert.IsTrue(result != null);
                Assert.IsTrue(result.ErrorMessage == null);
                Assert.IsTrue(result.StatusCode == 201);
                Assert.IsTrue(result.Succeeded);

                // Classify and extract the files in the level11 folder off peak
                var classifyResult = targetFolder.ClassifyAndExtractOffPeak();

                // Validate results
                Assert.IsTrue(classifyResult.Created != DateTime.MinValue);
                Assert.IsTrue(classifyResult.DeliverDate != DateTime.MinValue);
                Assert.IsTrue(classifyResult.ErrorMessage == null);
                Assert.IsTrue(classifyResult.StatusCode == 201);
                Assert.IsTrue(classifyResult.Status == "ExponentialBackoff");
                Assert.IsTrue(classifyResult.TargetSiteUrl == context.Uri.ToString());
                Assert.IsTrue(classifyResult.TargetWebServerRelativeUrl == context.Web.ServerRelativeUrl);

                // unpublish model from library
                var unpublishResult = await modelToRegister.UnPublishModelAsync(testLibrary);
                Assert.IsTrue(unpublishResult != null);
                Assert.IsTrue(unpublishResult.ErrorMessage == null);
                Assert.IsTrue(unpublishResult.StatusCode == 200);
                Assert.IsTrue(unpublishResult.Succeeded);

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }


    }
}
