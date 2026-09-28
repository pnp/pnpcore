using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class FolderCreationTests
    {
        [DataTestMethod]
        [DataRow(ListBaseType.GenericList, 0)]
        [DataRow(ListBaseType.GenericList, 3)]
        [DataRow(ListBaseType.Issue, 1)]
        [DataRow(ListBaseType.DocumentLibrary, 0)]
        [DataRow(ListBaseType.DocumentLibrary, 3)]
        public async Task AddFolderUsesParentListBaseType(ListBaseType baseType, int depth)
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite))
            {
                var list = CreateList(context, baseType);
                IFolder parentFolder = list.RootFolder;
                for (int i = 0; i < depth; i++)
                {
                    parentFolder = new Folder
                    {
                        PnPContext = context,
                        Parent = parentFolder.Folders,
                        UniqueId = Guid.NewGuid()
                    };
                    ((Folder)parentFolder).Metadata[PnPConstants.MetaDataRestId] = parentFolder.UniqueId.ToString();
                }

                var batch = context.NewBatch();
                await parentFolder.Folders.AddBatchAsync(batch, "O'Brien%20# & + 100%");

                Assert.AreEqual(baseType == ListBaseType.DocumentLibrary ? 1 : 2, batch.Requests.Count);
                var request = batch.Requests.First().Value;
                string endpoint = baseType == ListBaseType.DocumentLibrary
                    ? "Folders/AddUsingPath(decodedurl="
                    : "AddSubFolderUsingPath(DecodedUrl=";
                Assert.AreEqual($"{context.Uri.AbsoluteUri.TrimEnd('/')}/_api/Web/getFolderById('{parentFolder.UniqueId}')/{endpoint}'O%27%27Brien%20%23%20%26%20%2B%20100%25')", request.ApiCall.Request);
                Assert.AreEqual(ApiType.SPORest, request.ApiCall.Type);
                Assert.AreEqual(HttpMethod.Post, request.Method);
                Assert.IsFalse(batch.Executed);
            }
        }

        [TestMethod]
        public async Task AddFolderWithoutParentListUsesExistingEndpoint()
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite))
            {
                var parentFolder = new Folder
                {
                    PnPContext = context,
                    Parent = context.Web,
                    UniqueId = Guid.NewGuid()
                };
                parentFolder.Metadata[PnPConstants.MetaDataRestId] = parentFolder.UniqueId.ToString();

                await parentFolder.AddFolderBatchAsync("child");

                Assert.AreEqual($"{context.Uri.AbsoluteUri.TrimEnd('/')}/_api/Web/getFolderById('{parentFolder.UniqueId}')/Folders/AddUsingPath(decodedurl='child')",
                    context.CurrentBatch.Requests.Single().Value.ApiCall.Request);
            }
        }

        [DataTestMethod]
        [DataRow(ListBaseType.GenericList, false)]
        [DataRow(ListBaseType.GenericList, true)]
        [DataRow(ListBaseType.DocumentLibrary, false)]
        [DataRow(ListBaseType.DocumentLibrary, true)]
        public async Task AddFolderLoadsParentListBaseTypeOnce(ListBaseType baseType, bool synchronous)
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite, (int)baseType))
            {
                var list = CreateList(context);
                Assert.IsFalse(list.IsPropertyAvailable(p => p.BaseType));

                var batch = context.CurrentBatch;
                await context.Web.LoadBatchAsync(batch, p => p.Title);

                // A missing BaseType is loaded immediately without executing pending requests.
                if (synchronous)
                {
                    list.RootFolder.Folders.AddBatch("first");
                    list.RootFolder.AddFolderBatch("second");
                }
                else
                {
                    await list.RootFolder.Folders.AddBatchAsync("first");
                    await list.RootFolder.AddFolderBatchAsync("second");
                }
                Assert.IsTrue(list.IsPropertyAvailable(p => p.BaseType));
                Assert.AreEqual(baseType, list.BaseType);

                Assert.AreSame(batch, context.CurrentBatch);
                Assert.AreEqual(baseType == ListBaseType.DocumentLibrary ? 3 : 5, batch.Requests.Count);
                Assert.IsFalse(batch.Executed);
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.Title));
                foreach (var request in batch.Requests.Values.Where(p => p.Method == HttpMethod.Post))
                {
                    StringAssert.Contains(request.ApiCall.Request, baseType == ListBaseType.DocumentLibrary
                        ? "/Folders/AddUsingPath("
                        : "/AddSubFolderUsingPath(");
                }
            }
        }

        [TestMethod]
        public async Task AddFolderPreservesHeadersWhenLoadingParentListBaseType()
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite))
            {
                var list = CreateList(context);
                var headers = new Dictionary<string, string> { { "X-Test-Folder", "creation" } };
                int responseCallbacks = 0;

                await list.RootFolder.WithHeaders(headers, _ => responseCallbacks++).AddFolderBatchAsync("child");

                // Headers and callbacks belong to the folder creation, not the prerequisite GET.
                Assert.AreEqual(0, responseCallbacks);
                var request = context.CurrentBatch.Requests.First().Value;
                var module = request.RequestModules.OfType<CustomHeadersRequestModule>().Single();
                Assert.AreEqual("creation", module.Headers["X-Test-Folder"]);
                Assert.IsNotNull(module.ResponseHandler);
                Assert.IsFalse(context.RequestModules.Any());
                Assert.IsFalse(context.CurrentBatch.Requests.Last().Value.RequestModules?.Any() == true);
            }
        }

        [TestMethod]
        public async Task AddFolderRestoresHeadersWhenParentListLookupFails()
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite))
            {
                var list = CreateList(context);
                var headers = new Dictionary<string, string> { { "X-Test-Folder", "creation" } };
                int responseCallbacks = 0;
                var folder = list.RootFolder.WithHeaders(headers, _ => responseCallbacks++);
                var requestModules = context.RequestModules;

                await Assert.ThrowsExceptionAsync<SharePointRestServiceException>(() => folder.AddFolderBatchAsync("child"));

                Assert.AreEqual(0, responseCallbacks);
                Assert.AreSame(requestModules, context.RequestModules);
                Assert.AreEqual(1, context.RequestModules.Count);
                Assert.AreEqual(0, context.CurrentBatch.Requests.Count);
            }
        }

        [TestMethod]
        public async Task EnsureListFolderRetainsParentListForExistingFolders()
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite))
            {
                var list = CreateList(context, ListBaseType.GenericList);

                // The offline responses represent an existing parent and a missing child.
                var folder = await list.RootFolder.EnsureFolderAsync("existing/child");

                Assert.AreEqual("child", folder.Name);
                Assert.AreSame(list, ((Folder)folder).GetParentByType(typeof(PnP.Core.Model.SharePoint.List)));
                Assert.AreEqual("existing", ((IFolder)folder.Parent.Parent).Name);

                await folder.AddFolderBatchAsync("grandchild");
                StringAssert.Contains(context.CurrentBatch.Requests.First().Value.ApiCall.Request,
                    $"getFolderById('{folder.UniqueId}')/AddSubFolderUsingPath(DecodedUrl='grandchild')");
            }
        }

        [TestMethod]
        public async Task EnsureListFolderRetainsParentListAfterRace()
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite))
            {
                var list = CreateList(context, ListBaseType.GenericList);

                // The folder is created by another caller between the lookup and the add.
                var folder = await list.RootFolder.EnsureFolderAsync("existing");

                Assert.AreSame(list, ((Folder)folder).GetParentByType(typeof(PnP.Core.Model.SharePoint.List)));
                await folder.AddFolderBatchAsync("child");
                StringAssert.Contains(context.CurrentBatch.Requests.First().Value.ApiCall.Request,
                    $"getFolderById('{folder.UniqueId}')/AddSubFolderUsingPath(DecodedUrl='child')");
            }
        }

        [DataTestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public async Task AddListFolderLoadsReturnedFolder(bool batched, bool synchronous)
        {
            using (var context = await TestCommon.Instance.GetContextWithoutInitializationAsync(TestCommon.TestSite, batched ? 1 : 0))
            {
                var list = CreateList(context, ListBaseType.GenericList);
                IFolder folder;
                if (batched)
                {
                    var batch = context.NewBatch();
                    folder = synchronous ? list.RootFolder.Folders.AddBatch(batch, "child")
                        : await list.RootFolder.Folders.AddBatchAsync(batch, "child");

                    Assert.IsFalse(folder.IsPropertyAvailable(p => p.UniqueId));
                    Assert.AreEqual(2, batch.Requests.Count);
                    Assert.AreEqual(HttpMethod.Post, batch.Requests.First().Value.Method);
                    Assert.AreEqual(HttpMethod.Get, batch.Requests.Last().Value.Method);
                    StringAssert.EndsWith(batch.Requests.Last().Value.ApiCall.Request,
                        "/Folders/GetByPath(DecodedUrl='child')");
                    if (synchronous)
                    {
                        context.Execute(batch);
                    }
                    else
                    {
                        await context.ExecuteAsync(batch);
                    }
                }
                else
                {
                    folder = synchronous ? list.RootFolder.Folders.Add("child")
                        : await list.RootFolder.Folders.AddAsync("child");
                }

                // SharePoint returns {"odata.null":true} from the POST. The follow-up GET
                // must populate the same object so callers can immediately create a child.
                Assert.IsTrue(folder.Exists);
                Assert.AreEqual(Guid.Parse("44444444-4444-4444-4444-444444444444"), folder.UniqueId);
                Assert.AreEqual("child", folder.Name);
                StringAssert.EndsWith(folder.ServerRelativeUrl, "/Lists/TestList/child");
                Assert.AreSame(list, ((Folder)folder).GetParentByType(typeof(PnP.Core.Model.SharePoint.List)));

                var childBatch = context.NewBatch();
                await folder.Folders.AddBatchAsync(childBatch, "grandchild");
                StringAssert.Contains(childBatch.Requests.First().Value.ApiCall.Request,
                    $"getFolderById('{folder.UniqueId}')/AddSubFolderUsingPath(DecodedUrl='grandchild')");
            }
        }

        private static PnP.Core.Model.SharePoint.List CreateList(PnPContext context, ListBaseType? baseType = null)
        {
            var list = new PnP.Core.Model.SharePoint.List
            {
                PnPContext = context,
                Parent = context.Web.Lists,
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111")
            };
            list.Metadata[PnPConstants.MetaDataRestId] = list.Id.ToString();
            if (baseType.HasValue)
            {
                list.BaseType = baseType.Value;
            }

            var rootFolder = (Folder)list.RootFolder;
            rootFolder.Requested = true;
            rootFolder.UniqueId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            rootFolder.Metadata[PnPConstants.MetaDataRestId] = rootFolder.UniqueId.ToString();
            rootFolder.ServerRelativeUrl = $"{context.Uri.AbsolutePath.TrimEnd('/')}/Lists/TestList";
            return list;
        }
    }
}
