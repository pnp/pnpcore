using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ListViewTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetListViewAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);
                Assert.IsNotNull(list.Views);
                Assert.IsTrue(list.Views.Length > 0);
            }
        }

        [TestMethod]
        public async Task AddListViewAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                var viewTitle = "PnPCoreTestAsync";
                var result = await list.Views.AddAsync(new ViewOptions()
                {
                    Title = viewTitle,
                    RowLimit = 3
                });

                Assert.IsNotNull(result);
                Assert.AreEqual(viewTitle, result.Title);

                // Removes the view
                await result.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListViewWithCaml()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                var viewTitle = "PnPCoreTestWithCaml";
                var result = list.Views.Add(new ViewOptions()
                {
                    Title = viewTitle,
                    RowLimit = 3,
                    Query = "<Where><Eq><FieldRef Name='LinkFilename' /><Value Type='Text'>General</Value></Eq></Where>"

                });

                Assert.IsNotNull(result);
                Assert.AreEqual(viewTitle, result.Title);

                // Removes the view
                await result.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task AddListViewViewFields()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                var viewTitle = "PnPCoreTestWithViewFields";
                var result = list.Views.Add(new ViewOptions()
                {
                    Title = viewTitle,
                    RowLimit = 3,
                    Query = "<Where><Eq><FieldRef Name='LinkFilename' /><Value Type='Text'>General</Value></Eq></Where>",
                    ViewFields = new string[] { "DocIcon", "LinkFilenameNoMenu", "Modified" }
                });

                Assert.IsNotNull(result);
                Assert.AreEqual(viewTitle, result.Title);

                // Removes the view
                await result.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task AddListView()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                var viewTitle = "PnPCoreTest";
                var result = list.Views.Add(new ViewOptions()
                {
                    Title = viewTitle,
                    RowLimit = 3
                });

                Assert.IsNotNull(result);
                Assert.AreEqual(viewTitle, result.Title);

                // Removes the view
                await result.DeleteAsync();

            }
        }


        [TestMethod]
        public async Task AddListViewBatchAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                var viewTitle = "PnPCoreTestBatchAsync";
                var result = await list.Views.AddBatchAsync(new ViewOptions()
                {
                    Title = viewTitle,
                    RowLimit = 3
                });
                await context.ExecuteAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(viewTitle, result.Title);

                // Removes the view
                await result.DeleteBatchAsync();
                await context.ExecuteAsync();
            }
        }

        [TestMethod]
        public async Task AddListViewBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                var viewTitle = "PnPCoreTestBatch";
                var result = list.Views.AddBatch(new ViewOptions()
                {
                    Title = viewTitle,
                    RowLimit = 3
                });
                await context.ExecuteAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(viewTitle, result.Title);

                // Removes the view
                await result.DeleteBatchAsync();
                await context.ExecuteAsync();

            }
        }

        [TestMethod]
        public async Task AddListViewSpecificBatchAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                var batch = context.NewBatch();
                var viewTitle = "PnPCoreTestSpecificBatchAsync";
                var result = await list.Views.AddBatchAsync(batch, new ViewOptions()
                {
                    Title = viewTitle,
                    RowLimit = 3
                });
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(result);
                Assert.AreEqual(viewTitle, result.Title);

                // Removes the view
                var removeBatch = context.NewBatch();
                await result.DeleteBatchAsync(removeBatch);
                await context.ExecuteAsync(removeBatch);
            }
        }

        [TestMethod]
        public async Task AddListViewSpecificBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                var batch = context.NewBatch();
                var viewTitle = "PnPCoreTestSpecificBatch";
                var result = list.Views.AddBatch(batch, new ViewOptions()
                {
                    Title = viewTitle,
                    RowLimit = 3
                });
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(result);
                Assert.AreEqual(viewTitle, result.Title);

                // Removes the view
                var removeBatch = context.NewBatch();
                await result.DeleteBatchAsync(removeBatch);
                await context.ExecuteAsync(removeBatch);

            }
        }


        [TestMethod]
        public async Task AddListViewType2CompactList()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.Views);

                IView result = null;

                try
                {
                    var viewTitle = "PnPCoreTestViewType2CompactList";
                    result = list.Views.Add(new ViewOptions()
                    {
                        Title = viewTitle,
                        RowLimit = 3,
                        ViewType2 = ViewType2.COMPACTLIST,
                        ViewTypeKind = ViewTypeKind.Html
                    });

                    Assert.IsNotNull(result);
                    Assert.AreEqual(viewTitle, result.Title);

                    var list2 = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.Views);
                    var newView = list2.Views.AsRequested().FirstOrDefault(o => o.Title == viewTitle);
                    Assert.IsNotNull(newView);
                    Assert.IsTrue(newView.ViewType2 == ViewType2.COMPACTLIST);

                    // Set viewtype2 
                    newView.ViewType2 = ViewType2.NONE;
                    newView.Update();

                    list2 = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.Views);
                    newView = list2.Views.AsRequested().FirstOrDefault(o => o.Title == viewTitle);
                    Assert.IsNotNull(newView);
                    Assert.IsTrue(newView.ViewType2 == ViewType2.NONE);

                    // Update other view property
                    newView.Title = "PnPCoreTestViewType2CompactListUpdated";
                    newView.Update();

                    list2 = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.Views);
                    newView = list2.Views.AsRequested().FirstOrDefault(o => o.Title == "PnPCoreTestViewType2CompactListUpdated");
                    Assert.IsNotNull(newView);                    

                }
                finally
                {
                    // Removes the view
                    await result.DeleteAsync();
                }

            }
        }

        [TestMethod]
        public async Task GetViewByNewGuidTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/Shared Documents", p => p.Views);
                var requestedView = list.Views.AsRequested().First();

                var idString = requestedView.Id.ToString();
                var view = list.Views.FirstOrDefault(v => v.Id == new Guid(idString));

                Assert.IsTrue(view != null);
            }
        }

        [TestMethod]
        public async Task ReorderListViewViewFields()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IView result = null;
                try
                {
                    var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                    var viewTitle = "ReorderListViewViewFields";
                    result = list.Views.Add(new ViewOptions()
                    {
                        Title = viewTitle,
                        RowLimit = 3,
                        Query = "<Where><Eq><FieldRef Name='LinkFilename' /><Value Type='Text'>General</Value></Eq></Where>",
                        ViewFields = new string[] { "DocIcon", "LinkFilenameNoMenu", "Modified" }
                    });

                    Assert.IsNotNull(result);
                    Assert.AreEqual(viewTitle, result.Title);

                    // Reorder the listview fields
                    result.MoveViewFieldTo("Modified", 0);

                    // reload view
                    var listViews = await list.GetAsync(p => p.Views.QueryProperties(p=>p.Title, p=>p.ViewFields));
                    var view = listViews.Views.AsRequested().FirstOrDefault(v => v.Title == viewTitle);
                    Assert.IsTrue(view.ViewFields.Items.First() == "Modified");

                }
                finally
                {
                    // Removes the view
                    await result.DeleteAsync();
                }

            }
        }

        [TestMethod]
        public async Task AddRemoveListViewViewFields()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IView result = null;
                try
                {
                    var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                    var viewTitle = "AddRemoveListViewViewFields";
                    result = list.Views.Add(new ViewOptions()
                    {
                        Title = viewTitle,
                        RowLimit = 3,
                        Query = "<Where><Eq><FieldRef Name='LinkFilename' /><Value Type='Text'>General</Value></Eq></Where>",
                        ViewFields = new string[] { "DocIcon", "LinkFilenameNoMenu"}
                    });

                    Assert.IsNotNull(result);
                    Assert.AreEqual(viewTitle, result.Title);

                    // Add the modified field 
                    result.AddViewField("Modified");

                    // reload view
                    var listViews = await list.GetAsync(p => p.Views.QueryProperties(p => p.Title, p => p.ViewFields));
                    var view = listViews.Views.AsRequested().FirstOrDefault(v => v.Title == viewTitle);
                    Assert.IsTrue(view.ViewFields.Items.Last() == "Modified");

                    // remove the modified field
                    view.RemoveViewField("Modified");
                    
                    listViews = await list.GetAsync(p => p.Views.QueryProperties(p => p.Title, p => p.ViewFields));
                    view = listViews.Views.AsRequested().FirstOrDefault(v => v.Title == viewTitle);
                    Assert.IsTrue(view.ViewFields.Items.Last() == "LinkFilenameNoMenu");

                }
                finally
                {
                    // Removes the view
                    await result.DeleteAsync();
                }

            }
        }
    }
}
