using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ListTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task CaseInsensitiveListByTitle()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByTitle("Site pages", p=>p.Title, p=>p.Fields.QueryProperties(p=>p.InternalName));
                Assert.IsTrue(list != null);
                Assert.IsTrue(list.Fields.AsRequested().Count() > 0);
                Assert.IsTrue(list.Fields.AsRequested().First().IsPropertyAvailable(p => p.InternalName));

                var list2 = context.Web.Lists.GetByTitle("Site Page");
                Assert.IsTrue(list2 == null);
            }         
        }

        [TestMethod]
        public async Task GetListDataByRenderListDataAsStream()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetListDataByRenderListDataAsStream";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        var result = await list2.LoadListDataAsStreamAsync(new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData });
                        Assert.IsTrue(list2.Items.Length == 5);
                        Assert.IsTrue(result.ContainsKey("FirstRow"));
                        Assert.IsTrue(result.ContainsKey("LastRow"));
                        Assert.IsTrue(result.ContainsKey("RowLimit"));
                        Assert.IsTrue((int)result["RowLimit"] == 5);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var list3 = context3.Web.Lists.GetByTitle(listTitle);
                    if (list3 != null)
                    {
                        var result = list3.LoadListDataAsStream(new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData });
                        Assert.IsTrue(list3.Items.Length == 5);
                        Assert.IsTrue(result.ContainsKey("FirstRow"));
                        Assert.IsTrue(result.ContainsKey("LastRow"));
                        Assert.IsTrue(result.ContainsKey("RowLimit"));
                        Assert.IsTrue((int)result["RowLimit"] == 5);

                        list3.Items.Clear();

                        var batch = context3.NewBatch();
                        var resultBatch = await list3.LoadListDataAsStreamBatchAsync(batch, new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData }); ;

                        Assert.IsFalse(resultBatch.IsAvailable);

                        // Execute the batch
                        await context3.ExecuteAsync(batch);

                        Assert.IsTrue(resultBatch.IsAvailable);

                        Assert.IsTrue(list3.Items.Length == 5);
                        Assert.IsTrue(resultBatch.Result.ContainsKey("FirstRow"));
                        Assert.IsTrue(resultBatch.Result.ContainsKey("LastRow"));
                        Assert.IsTrue(resultBatch.Result.ContainsKey("RowLimit"));
                        Assert.IsTrue((int)resultBatch.Result["RowLimit"] == 5);
                    }

                    using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                    {
                        var list4 = context4.Web.Lists.GetByTitle(listTitle);
                        if (list4 != null)
                        {
                            var resultBatch = list4.LoadListDataAsStreamBatch(new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData }); ;

                            Assert.IsFalse(resultBatch.IsAvailable);

                            // Execute the batch
                            await context4.ExecuteAsync();

                            Assert.IsTrue(resultBatch.IsAvailable);

                            Assert.IsTrue(list4.Items.Length == 5);
                            Assert.IsTrue(resultBatch.Result.ContainsKey("FirstRow"));
                            Assert.IsTrue(resultBatch.Result.ContainsKey("LastRow"));
                            Assert.IsTrue(resultBatch.Result.ContainsKey("RowLimit"));
                            Assert.IsTrue((int)resultBatch.Result["RowLimit"] == 5);

                        }
                    }

                    // Cleanup the created list
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task ListLinqGetMethods()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListLinqGetMethods";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                var listGuid = myList.Id;

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        Assert.IsTrue(list2.Title == listTitle);
                        Assert.IsTrue(list2.Id == listGuid);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    context3.GraphFirst = false;

                    var list3 = context3.Web.Lists.GetById(listGuid, p => p.TemplateType, p => p.Title);
                    if (list3 != null)
                    {
                        Assert.IsTrue(list3.Title == listTitle);
                        Assert.IsTrue(list3.Id == listGuid);
                    }
                }

                // BERT - filtering on Id does not work for SharePoint Lists with Graph - will be fixed with the new 2.1 VROOM backend
                using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    context4.GraphFirst = false;

                    var list4 = context4.Web.Lists.GetByTitle(listTitle);
                    if (list4 != null)
                    {
                        var listCheck = context4.Web.Lists.GetById(list4.Id);

                        Assert.IsTrue(listCheck.Title == listTitle);
                        Assert.IsTrue(listCheck.Id == listGuid);
                    }
                }


                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ListLinqGetAsyncMethods()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListLinqGetAsyncMethods";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                var listGuid = myList.Id;

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        Assert.IsTrue(list2.Title == listTitle);
                        Assert.IsTrue(list2.Id == listGuid);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    context3.GraphFirst = false;

                    var list3 = await context3.Web.Lists.GetByIdAsync(listGuid, p => p.TemplateType, p => p.Title);
                    if (list3 != null)
                    {
                        Assert.IsTrue(list3.Title == listTitle);
                        Assert.IsTrue(list3.Id == listGuid);
                    }
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ListGetBatchMethods()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var documentLibrary = context.Web.Lists.GetByTitleBatch("Documents", p => p.ContentTypes.QueryProperties(p => p.FieldLinks));
                var documentLibrary2 = context.Web.Lists.GetByServerRelativeUrlBatch($"{context.Uri.LocalPath}/siteassets", p => p.ContentTypes.QueryProperties(p => p.FieldLinks));

                Assert.IsFalse(documentLibrary.Requested);

                await context.ExecuteAsync();

                Assert.IsTrue(documentLibrary.Requested);
                Assert.IsTrue(documentLibrary.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(documentLibrary.ContentTypes.AsRequested().First().IsPropertyAvailable(p => p.FieldLinks));
                Assert.IsTrue(documentLibrary.ContentTypes.AsRequested().First().FieldLinks.AsRequested().First().IsPropertyAvailable(p => p.Name));
                Assert.IsTrue(documentLibrary2.Requested);
                Assert.IsTrue(documentLibrary2.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(documentLibrary2.ContentTypes.AsRequested().First().IsPropertyAvailable(p => p.FieldLinks));
                Assert.IsTrue(documentLibrary2.ContentTypes.AsRequested().First().FieldLinks.AsRequested().First().IsPropertyAvailable(p => p.Name));

                var documentLibrary3 = context.Web.Lists.GetByIdBatch(documentLibrary.Id, p => p.ContentTypes.QueryProperties(p => p.FieldLinks));
                await context.ExecuteAsync();

                Assert.IsTrue(documentLibrary3.Requested);
                Assert.IsTrue(documentLibrary3.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(documentLibrary3.ContentTypes.AsRequested().First().IsPropertyAvailable(p => p.FieldLinks));
                Assert.IsTrue(documentLibrary3.ContentTypes.AsRequested().First().FieldLinks.AsRequested().First().IsPropertyAvailable(p => p.Name));

            }
        }

        [TestMethod]
        public async Task ListLinqGetExceptionMethods()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListLinqGetExceptionMethods";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                var listGuid = myList.Id;

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        // Commented now that the GetBy methods are actual methods on the interface versus extention methods
                        //await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
                        //    IListCollection list = null;
                        //    await list.GetByIdAsync(listGuid);
                        //});

                        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                        {
                            await context2.Web.Lists.GetByIdAsync(Guid.Empty, p => p.TemplateType, p => p.Title);
                        });

                        // Commented now that the GetBy methods are actual methods on the interface versus extention methods
                        //Assert.ThrowsException<ArgumentNullException>(() => {
                        //    IListCollection list = null;
                        //    list.GetById(listGuid);
                        //});

                        Assert.ThrowsException<ArgumentNullException>(() =>
                        {
                            context2.Web.Lists.GetById(Guid.Empty, p => p.TemplateType, p => p.Title);
                        });

                        // Commented now that the GetBy methods are actual methods on the interface versus extention methods
                        //Assert.ThrowsException<ArgumentNullException>(() => {
                        //    IListCollection list = null;
                        //    list.GetByTitle(listTitle);
                        //});

                        Assert.ThrowsException<ArgumentNullException>(() =>
                        {
                            context2.Web.Lists.GetByTitle(null);
                        });

                        // Commented now that the GetBy methods are actual methods on the interface versus extention methods
                        //await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
                        //    IListCollection list = null;
                        //    await list.GetByTitleAsync(listTitle);
                        //});

                        // Commented now that the GetBy methods are actual methods on the interface versus extention methods
                        //await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
                        //    IListCollection list = null;
                        //    await list.GetByTitleAsync(null);
                        //});

                    }
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetItemsByCAMLQuery()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetItemsByCAMLQuery";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                // Check the item count in the list
                await myList.LoadAsync(p => p.ItemCount);
                Assert.IsTrue(myList.ItemCount == 10);

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        await list2.LoadItemsByCamlQueryAsync("<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>");
                        Assert.IsTrue(list2.Items.Length == 5);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var list3 = context3.Web.Lists.GetByTitle(listTitle);
                    if (list3 != null)
                    {
                        await list3.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                            DatesInUtc = true
                        });
                        Assert.IsTrue(list3.Items.Length == 10);
                    }
                }

                // Batch testing
                using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    var list4 = context4.Web.Lists.GetByTitle(listTitle);
                    if (list4 != null)
                    {
                        // Perform 2 queries, the first one limited to 5 items, the second one without limits. Total should be 10 items
                        await list4.LoadItemsByCamlQueryBatchAsync(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>",
                        });
                        await list4.LoadItemsByCamlQueryBatchAsync(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                        });
                        await context4.ExecuteAsync();

                        Assert.IsTrue(list4.Items.Length == 10);
                    }
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public void RewriteQueryStringTest()
        {
            // there are fieldrefs | no * provided in $select
            var rewrite = List.RewriteGetItemsQueryString("<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='FileRef' /></ViewFields><RowLimit>5</RowLimit></View>",
                                                          "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=Id,RoleAssignments/PrincipalId&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings");

            Assert.AreEqual(rewrite.ToLower(), "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=Id,RoleAssignments/PrincipalId,*,Title,FileRef&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings".ToLower());

            // there are fieldrefs | * provided in $select
            rewrite = List.RewriteGetItemsQueryString("<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='FileRef' /></ViewFields><RowLimit>5</RowLimit></View>",
                                                          "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=*,Id,RoleAssignments/PrincipalId&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings");

            Assert.AreEqual(rewrite.ToLower(), "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=*,Id,RoleAssignments/PrincipalId,Title,FileRef&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings".ToLower());

            // there are fieldrefs | * + a fieldref value provided in $select
            rewrite = List.RewriteGetItemsQueryString("<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='FileRef' /></ViewFields><RowLimit>5</RowLimit></View>",
                                                          "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=*,Id,Title,RoleAssignments/PrincipalId&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings");

            Assert.AreEqual(rewrite.ToLower(), "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=*,Id,Title,RoleAssignments/PrincipalId,FileRef&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings".ToLower());

            // there are duplicate fieldrefs | no * provided in $select
            rewrite = List.RewriteGetItemsQueryString("<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='FileRef' /><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>",
                                                          "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=Id,RoleAssignments/PrincipalId&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings");

            Assert.AreEqual(rewrite.ToLower(), "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=Id,RoleAssignments/PrincipalId,*,Title,FileRef&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings".ToLower());

            // there no fieldrefs | no * provided in $select
            rewrite = List.RewriteGetItemsQueryString("<View><RowLimit>5</RowLimit></View>",
                                                          "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=Id,RoleAssignments/PrincipalId&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings");

            Assert.AreEqual(rewrite.ToLower(), "https://bertonline.sharepoint.com/sites/prov-2/_api/Web/Lists(guid'3968c1e1-0a86-40db-95f8-614e53888609')/GetItems?$select=Id,RoleAssignments/PrincipalId&$expand=RoleAssignments,RoleAssignments/RoleDefinitionBindings".ToLower());
        }

        [TestMethod]
        public async Task GetItemsByCAMLQueryWithSelectors()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetItemsByCAMLQueryWithSelectors";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                // Check the item count in the list
                await myList.LoadAsync(p => p.ItemCount);
                Assert.IsTrue(myList.ItemCount == 10);

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        Assert.ThrowsException<ArgumentNullException>(() =>
                        {
                            list2.LoadItemsByCamlQuery(queryOptions: null);
                        });

                        Assert.ThrowsException<ArgumentNullException>(() =>
                        {
                            list2.LoadItemsByCamlQuery(query: null);
                        });

                        list2.LoadItemsByCamlQuery("<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='FileRef' /></ViewFields><RowLimit>5</RowLimit></View>",
                            p => p.RoleAssignments.QueryProperties(p => p.PrincipalId, p => p.RoleDefinitions));
                        Assert.IsTrue(list2.Items.Length == 5);
                        var first = list2.Items.AsRequested().First();
                        Assert.IsTrue(first["FileRef"] != null && !string.IsNullOrEmpty(first["FileRef"].ToString()));
                        Assert.IsTrue(first.RoleAssignments.Requested);
                        var firstRoleAssignment = first.RoleAssignments.AsRequested().First();
                        Assert.IsTrue(firstRoleAssignment.Requested);
                        Assert.IsTrue(firstRoleAssignment.IsPropertyAvailable(p => p.PrincipalId));
                        Assert.IsTrue(firstRoleAssignment.RoleDefinitions.Requested);
                        Assert.IsTrue(firstRoleAssignment.RoleDefinitions.AsRequested().First().IsPropertyAvailable(p => p.Id));

                    }
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task GetItemsByCAMLQuerySimpleNonAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetItemsByCAMLQuerySimpleAsyncTest";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = context.Web.Lists.Add(listTitle, ListTemplateType.GenericList);
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    myList.Items.AddBatch(values);
                }
                await context.ExecuteAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        list2.LoadItemsByCamlQuery("<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>");
                        Assert.IsTrue(list2.Items.Length == 5);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var list3 = context3.Web.Lists.GetByTitle(listTitle);
                    if (list3 != null)
                    {
                        list3.LoadItemsByCamlQuery(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                            DatesInUtc = true
                        });
                        Assert.IsTrue(list3.Items.Length == 10);
                    }
                }

                // Batch testing
                using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    var list4 = context4.Web.Lists.GetByTitle(listTitle);
                    if (list4 != null)
                    {
                        // Perform 2 queries, the first one limited to 5 items, the second one without limits. Total should be 10 items
                        list4.LoadItemsByCamlQueryBatch(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>",
                        });
                        list4.LoadItemsByCamlQueryBatch("<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>");
                        await context4.ExecuteAsync();

                        Assert.IsTrue(list4.Items.Length == 10);
                    }
                }

                // Batch testing
                using (var context5 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
                {
                    var list5 = context5.Web.Lists.GetByTitle(listTitle);
                    if (list5 != null)
                    {
                        var newBatch = context5.NewBatch();
                        // Perform 2 queries, the first one limited to 5 items, the second one without limits. Total should be 10 items
                        list5.LoadItemsByCamlQueryBatch(newBatch, new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>",
                        });
                        list5.LoadItemsByCamlQueryBatch(newBatch, new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                        });
                        context5.ExecuteAsync(newBatch).GetAwaiter().GetResult();

                        Assert.IsTrue(list5.Items.Length == 10);
                    }
                }

                using (var context6 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 5))
                {
                    var list6 = context6.Web.Lists.GetByTitle(listTitle);
                    if (list6 != null)
                    {
                        var newBatch = context6.NewBatch();
                        // Perform 2 queries, the first one limited to 5 items, the second one without limits. Total should be 10 items
                        list6.LoadItemsByCamlQueryBatch(newBatch, "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>");
                        context6.ExecuteAsync(newBatch).GetAwaiter().GetResult();

                        Assert.IsTrue(list6.Items.Length == 5);
                    }
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetItemsByCAMLQueryOnCustomField()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("GetItemsByCAMLQueryOnCustomField");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                IField customField;
                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    customField = await myList.Fields.AddTextAsync("CustomField", new FieldTextOptions()
                    {
                        Group = "Custom fields",
                        AddToDefaultView = true,
                    });
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" },
                            { "CustomField", $"Field{i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        string query = @"<View>
                                          <ViewFields>
                                            <FieldRef Name='Title' />
                                            <FieldRef Name='CustomField' />
                                          </ViewFields>
                                          <Query>
                                            <Where>
                                              <Eq>
                                                <FieldRef Name='CustomField'/>
                                                <Value Type='text'>Field6</Value>
                                              </Eq>
                                            </Where>
                                          </Query>
                                        </View>";

                        await list2.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
                        {
                            ViewXml = query,
                            DatesInUtc = true
                        });
                        Assert.IsTrue(list2.Items.Length == 1);
                        Assert.IsTrue(list2.Items.AsRequested().First()["CustomField"].ToString() == "Field6");
                    }
                }


                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RecycleList()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                int listCount = web.Lists.Length;

                string listTitle = "RecycleList";
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // recycle the list
                var recycleBinItemId = await myList.RecycleAsync();
                // A valid recycle returns a recuycle bin item id
                Assert.IsTrue(recycleBinItemId != Guid.Empty);
                // The recycled list should have been deleted from the lists collection
                Assert.IsTrue(web.Lists.Length == listCount);
                // Loading lists again should still result in the same original list count as the added list is in the recycle bin
                await context.Web.LoadAsync(p => p.Lists);
                Assert.IsTrue(web.Lists.Length == listCount);

            }
        }

        [TestMethod]
        public async Task RecycleListBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                await context.Web.LoadAsync(p => p.Lists);

                var web = context.Web;

                int listCount = web.Lists.Length;

                string listTitle = TestCommon.GetPnPSdkTestAssetName("RecycleListBatch");
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // recycle the list
                var recycleBatchResult = await myList.RecycleBatchAsync();
                Assert.IsFalse(recycleBatchResult.IsAvailable);
                // Execute the batch
                await context.ExecuteAsync();
                Assert.IsTrue(recycleBatchResult.IsAvailable);
                Assert.AreNotEqual(Guid.Empty, recycleBatchResult.Result.Value);

                // The recycled list should have been deleted from the lists collection
                Assert.IsTrue(web.Lists.Length == listCount);
                // Loading lists again should still result in the same original list count as the added list is in the recycle bin
                await context.Web.GetAsync(p => p.Lists);
                Assert.IsTrue(web.Lists.Length == listCount);

            }
        }

        [TestMethod]
        public async Task GetListIRMSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetListIRMSettingsTest";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                }

                // Enable IRM on the library
                await myList.LoadAsync();
                myList.IrmEnabled = true;
                await myList.UpdateAsync();

                // Load IRM settings
                await myList.InformationRightsManagementSettings.LoadAsync();

                // Verify default IRM list settings are returned
                Assert.IsTrue(myList.InformationRightsManagementSettings.Requested);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowPrint == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowScript == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowWriteCopy == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DisableDocumentBrowserView == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DocumentAccessExpireDays == 90);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DocumentLibraryProtectionExpireDate > new DateTime(2021, 1, 1));
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableDocumentAccessExpire == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableDocumentBrowserPublishingView == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableGroupProtection == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableLicenseCacheExpire == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.GroupName == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.LicenseCacheExpireDays == 30);
                Assert.IsTrue(myList.InformationRightsManagementSettings.PolicyDescription == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.PolicyTitle == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.TemplateId == "");

                // turn off IRM again
                myList.IrmEnabled = false;
                await myList.UpdateAsync();

                // delete the list
                await myList.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task GetListIRMSettingsBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetListIRMSettingsBatchTest";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                }

                // Enable IRM on the library
                await myList.LoadAsync();
                myList.IrmEnabled = true;
                await myList.UpdateAsync();

                // Load IRM settings
                await myList.InformationRightsManagementSettings.LoadBatchAsync();
                await context.ExecuteAsync();

                // Verify default IRM list settings are returned
                Assert.IsTrue(myList.InformationRightsManagementSettings.Requested);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowPrint == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowScript == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowWriteCopy == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DisableDocumentBrowserView == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DocumentAccessExpireDays == 90);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DocumentLibraryProtectionExpireDate > new DateTime(2021, 1, 1));
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableDocumentAccessExpire == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableDocumentBrowserPublishingView == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableGroupProtection == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableLicenseCacheExpire == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.GroupName == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.LicenseCacheExpireDays == 30);
                Assert.IsTrue(myList.InformationRightsManagementSettings.PolicyDescription == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.PolicyTitle == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.TemplateId == "");

                // turn off IRM again
                myList.IrmEnabled = false;
                await myList.UpdateAsync();

                // delete the list
                await myList.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task GetListByServerRelativeUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByServerRelativeUrlAsync($"{context.Uri.LocalPath}/shared%20documents", p => p.Title, p => p.ListExperience);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);

            }
        }

        [TestMethod]
        public async Task GetListByServerRelativeUrlSync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/shared%20documents", p => p.Title, p => p.ListExperience);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);

            }
        }

        [TestMethod]
        public async Task GetListByTitle()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);

            }
        }

        [TestMethod]
        public async Task GetListByTitleWithQueryPropertiesSync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByTitle("Documents", p => p.Title, p => p.Items, p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);
            }
        }

        [TestMethod]
        public async Task GetListByTitleSync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByTitle("Documents", p => p.Title, p => p.ListExperience);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);
            }
        }

        [TestMethod]
        public async Task GetListByTitleWithExpand()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByTitle("Documents", p => p.Title, p => p.ContentTypes);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);
                Assert.IsTrue(list.ContentTypes.Requested);
                Assert.IsTrue(list.ContentTypes.AsRequested().First().IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(list.ContentTypes.AsRequested().First().IsPropertyAvailable(p => p.Description));
            }
        }

        [TestMethod]
        public async Task GetListByTitleWithLoad()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.ContentTypes.QueryProperties(p => p.Id, p => p.Name));

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);
                Assert.IsTrue(list.ContentTypes.Requested);
                Assert.IsTrue(list.ContentTypes.AsRequested().First().IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(list.ContentTypes.AsRequested().First().IsPropertyAvailable(p => p.Name));
            }
        }

        [TestMethod]
        public async Task GetListByTitleWithLoadRecursive()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience,
                    p => p.ContentTypes.QueryProperties(p => p.Id, p => p.Name,
                        p => p.FieldLinks.QueryProperties(p => p.Id, p => p.Name)));

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);
                Assert.IsTrue(list.ContentTypes.Requested);

                var firstContentType = list.ContentTypes.AsRequested().First();
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(firstContentType.IsPropertyAvailable(p => p.Name));
                Assert.IsTrue(firstContentType.FieldLinks.Requested);

                var firstFieldLink = firstContentType.FieldLinks.AsRequested().First();
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(firstFieldLink.IsPropertyAvailable(p => p.Name));
                Assert.IsTrue(!string.IsNullOrEmpty(firstFieldLink.Name));
            }
        }

        [TestMethod]
        public async Task VerifyIsProperties()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByServerRelativeUrlAsync($"{context.Uri.LocalPath}/shared%20documents", p => p.IsApplicationList,
                    p => p.IsCatalog, p => p.IsDefaultDocumentLibrary, p => p.IsPrivate, p => p.IsSiteAssetsLibrary, p => p.IsSystemList,
                    p => p.Created, p => p.LastItemDeletedDate, p => p.LastItemModifiedDate, p => p.LastItemUserModifiedDate);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.IsApplicationList, false);
                Assert.AreEqual(list.IsCatalog, false);
                Assert.AreEqual(list.IsDefaultDocumentLibrary, true);
                Assert.AreEqual(list.IsPrivate, false);
                Assert.AreEqual(list.IsSiteAssetsLibrary, false);
                Assert.AreEqual(list.IsSystemList, false);
                Assert.IsTrue(list.Created > DateTime.MinValue && list.Created < DateTime.Now);
                Assert.IsTrue(list.LastItemDeletedDate > DateTime.MinValue && list.LastItemDeletedDate < DateTime.Now);
                Assert.IsTrue(list.LastItemModifiedDate > DateTime.MinValue && list.LastItemModifiedDate < DateTime.Now);
                Assert.IsTrue(list.LastItemUserModifiedDate > DateTime.MinValue && list.LastItemUserModifiedDate < DateTime.Now);

                list = await context.Web.Lists.GetByServerRelativeUrlAsync($"{context.Uri.LocalPath}/_catalogs/masterpage", p => p.IsApplicationList,
                    p => p.IsCatalog, p => p.IsDefaultDocumentLibrary, p => p.IsPrivate, p => p.IsSiteAssetsLibrary, p => p.IsSystemList);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.IsApplicationList, true);
                Assert.AreEqual(list.IsCatalog, true);
                Assert.AreEqual(list.IsDefaultDocumentLibrary, false);
                Assert.AreEqual(list.IsPrivate, false);
                Assert.AreEqual(list.IsSiteAssetsLibrary, false);
                Assert.AreEqual(list.IsSystemList, true);
            }
        }

        [TestMethod]
        public async Task GetListByIdFollowedByAdd()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("GetListByIdWithAdd");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                Guid listId = myList.Id;

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    // Filtering lists by Id is not yet supported by Microsoft Graph
                    context2.GraphFirst = false;

                    // Get list without root folder - will trigger rootfolder load
                    var list = await context2.Web.Lists.GetByIdAsync(listId,
                        l => l.Fields.QueryProperties(f => f.Id, f => f.Title, f => f.InternalName, f => f.TypeAsString));

                    // Add a list item
                    Dictionary<string, object> values = new Dictionary<string, object>
                    {
                        { "Title", "Yes" }
                    };

                    await list.Items.AddAsync(values);

                    // Get list with roorfolder, more optimized
                    list = await context2.Web.Lists.GetByIdAsync(listId,
                        l => l.RootFolder, l => l.Fields.QueryProperties(f => f.Id, f => f.Title, f => f.InternalName, f => f.TypeAsString));

                    await list.Items.AddAsync(values);

                    using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                    {
                        //context3.GraphFirst = false;

                        // We should have 2 list items
                        var list3 = await context3.Web.Lists.GetByIdAsync(listId, p => p.Items);

                        Assert.IsTrue(list3.Items.Length == 2);
                    }

                    // delete the list again
                    await list.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task BreakRoleInheritanceTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(p => p.Lists);

                var web = context.Web;

                string listTitle = TestCommon.GetPnPSdkTestAssetName("BreakRoleInheritanceTest");
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }


                myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);

                await myList.BreakRoleInheritanceAsync(false, false);

                await myList.EnsurePropertiesAsync(l => l.HasUniqueRoleAssignments);

                Assert.IsTrue(myList.HasUniqueRoleAssignments);
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ResetRoleInheritanceTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(p => p.Lists);

                var web = context.Web;

                string listTitle = TestCommon.GetPnPSdkTestAssetName("ResetRoleInheritanceTest");
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);

                await myList.BreakRoleInheritanceAsync(false, false);

                await myList.ResetRoleInheritanceAsync();

                await myList.EnsurePropertiesAsync(l => l.HasUniqueRoleAssignments);

                Assert.IsFalse(myList.HasUniqueRoleAssignments);

                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetRoleDefinitionsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(p => p.Lists, p => p.CurrentUser);

                var web = context.Web;

                string listTitle = TestCommon.GetPnPSdkTestAssetName("GetRoleDefinitionsTest");
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);

                await myList.BreakRoleInheritanceAsync(false, false);

                var roleDefinitions = await myList.GetRoleDefinitionsAsync(web.CurrentUser.Id);

                Assert.IsTrue(roleDefinitions.Length > 0);

                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddRoleDefinitionsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = TestCommon.GetPnPSdkTestAssetName("AddRoleDefinitionsTest");
                string roleDefName = TestCommon.GetPnPSdkTestAssetName("AddRoleDefinitionsTest");

                context.Web.Load(p => p.Lists, p => p.CurrentUser);

                var web = context.Web;

                var roleDefinition = web.RoleDefinitions.Add(roleDefName, RoleType.Administrator, new PermissionKind[] { PermissionKind.AddAndCustomizePages });

                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);

                await myList.BreakRoleInheritanceAsync(false, false);

                myList.AddRoleDefinitions(web.CurrentUser.Id, roleDefName);

                var roleDefinitions = await myList.GetRoleDefinitionsAsync(web.CurrentUser.Id);

                Assert.IsTrue(roleDefinitions.Length > 1 && roleDefinitions.AsRequested().FirstOrDefault(r => r.Name == roleDefName) != null);

                await myList.DeleteAsync();

                await roleDefinition.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddRoleDefinitionsSpecialCharsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = TestCommon.GetPnPSdkTestAssetName("AddRoleDefinitionsSpecialCharsTest");
                string roleDefName = "Fullständig Behörighet";

                context.Web.Load(p => p.Lists, p => p.CurrentUser);

                var web = context.Web;

                var roleDefinition = web.RoleDefinitions.Add(roleDefName, RoleType.Administrator, new PermissionKind[] { PermissionKind.AddAndCustomizePages });

                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);

                await myList.BreakRoleInheritanceAsync(false, false);

                myList.AddRoleDefinitions(web.CurrentUser.Id, roleDefName);

                var roleDefinitions = await myList.GetRoleDefinitionsAsync(web.CurrentUser.Id);

                Assert.IsTrue(roleDefinitions.Length > 1 && roleDefinitions.AsRequested().FirstOrDefault(r => r.Name == roleDefName) != null);

                await myList.DeleteAsync();

                await roleDefinition.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RemoveRoleDefinitionsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = TestCommon.GetPnPSdkTestAssetName("RemoveRoleDefinitionsTest");
                string roleDefName = TestCommon.GetPnPSdkTestAssetName("RemoveRoleDefinitionsTest");

                context.Web.Load(p => p.Lists, p => p.CurrentUser);

                var web = context.Web;

                var roleDefinition = web.RoleDefinitions.Add(roleDefName, RoleType.Administrator, new PermissionKind[] { PermissionKind.AddAndCustomizePages });

                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);

                await myList.BreakRoleInheritanceAsync(false, false);

                myList.AddRoleDefinitions(web.CurrentUser.Id, roleDefName);

                var roleDefinitionsBefore = await myList.GetRoleDefinitionsAsync(web.CurrentUser.Id);

                Assert.IsTrue(roleDefinitionsBefore.Length > 0 && roleDefinitionsBefore.AsRequested().FirstOrDefault(r => r.Name == roleDefName) != null);

                myList.RemoveRoleDefinitions(web.CurrentUser.Id, roleDefName);

                var roleDefinitionsAfter = await myList.GetRoleDefinitionsAsync(web.CurrentUser.Id);

                Assert.IsTrue(roleDefinitionsAfter.Length != roleDefinitionsBefore.Length && roleDefinitionsAfter.AsRequested().FirstOrDefault(r => r.Name == roleDefName) == null);

                await myList.DeleteAsync();

                await roleDefinition.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetListChangesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Id);
                var changes = await list.GetChangesAsync(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);

                var list2 = await context.Web.Lists.GetByTitleAsync("Site Assets", p => p.Id);

                var changesBatch = list.GetChangesBatch(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });
                var changes2Batch = list2.GetChangesBatch(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsFalse(changesBatch.IsAvailable);
                Assert.IsFalse(changes2Batch.IsAvailable);

                await context.ExecuteAsync();

                Assert.IsTrue(changesBatch.IsAvailable);
                Assert.IsTrue(changes2Batch.IsAvailable);

                Assert.IsTrue(changesBatch.Count > 0);
                Assert.IsTrue(changes2Batch.Count > 0);

            }
        }

        [TestMethod]
        public void GetListChangesTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByTitle("Documents", p => p.Id);
                var changes = list.GetChanges(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);
            }
        }

        [TestMethod]
        public async Task GetListChangesOnlyAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                // Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("GetListChangesOnlyAsyncTest");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Make a change
                myList.ContentTypesEnabled = true;
                await myList.UpdateAsync();

                var changes = await myList.GetChangesAsync(new ChangeQueryOptions(false, true)
                {
                    FetchLimit = 5,
                    List = true
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);

                var lastChange = (changes.Last() as IChangeList);
                Assert.IsTrue(lastChange.ChangeToken != null);
                Assert.IsTrue(!string.IsNullOrEmpty(lastChange.ChangeToken.StringValue));
                Assert.IsTrue(lastChange.Time != DateTime.MinValue);
                Assert.IsTrue(lastChange.Hidden == false);
                Assert.IsTrue(lastChange.ListId != Guid.Empty);
                Assert.IsTrue(lastChange.WebId != Guid.Empty);
                Assert.IsTrue(lastChange.ChangeType == ChangeType.Update);
                Assert.IsTrue(lastChange.SiteId != Guid.Empty);
                Assert.IsTrue(lastChange.TemplateType == ListTemplateType.NoListTemplate || lastChange.TemplateType == ListTemplateType.GenericList);

                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetListItemChangesOnlyAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                // Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("GetListItemChangesOnlyAsyncTest");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Add an item
                await myList.Items.AddAsync(new Dictionary<string, object>() { { "Title", "Test" } });

                var changes = await myList.GetChangesAsync(new ChangeQueryOptions(false, true)
                {
                    FetchLimit = 5,
                    Item = true,
                    RequireSecurityTrim = false
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);

                var lastChange = (changes.Last() as IChangeItem);
                Assert.IsTrue(lastChange.ChangeToken != null);
                Assert.IsTrue(!string.IsNullOrEmpty(lastChange.ChangeToken.StringValue));
                Assert.IsTrue(lastChange.Time != DateTime.MinValue);
                Assert.IsTrue(lastChange.ListId != Guid.Empty);
                Assert.IsTrue(lastChange.WebId != Guid.Empty);
                Assert.IsTrue(lastChange.ChangeType == ChangeType.Add);
                Assert.IsTrue(lastChange.SiteId != Guid.Empty);
                Assert.IsTrue(lastChange.UniqueId != Guid.Empty);
                Assert.IsTrue(lastChange.ItemId == 1);
                Assert.IsTrue(lastChange.IsPropertyAvailable<IChangeItem>(p => p.Editor));
                Assert.IsTrue(lastChange.IsPropertyAvailable<IChangeItem>(p => p.EditorEmailHint));
                Assert.IsFalse(changes.Last().IsPropertyAvailable<IChangeItem>(p => p.Hidden));


                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task EnsureAssetLibraryTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var assetLibrary = context.Web.Lists.EnsureSiteAssetsLibrary();

                Assert.IsNotNull(assetLibrary);
                Assert.IsTrue(assetLibrary.Id != Guid.Empty);


                var assetLibrary2 = context.Web.Lists.EnsureSiteAssetsLibrary(p => p.RootFolder.QueryProperties(p => p.Files));
                Assert.IsNotNull(assetLibrary2);
                Assert.IsTrue(assetLibrary2.Id != Guid.Empty);
                Assert.IsTrue(assetLibrary2.IsPropertyAvailable(p => p.RootFolder));

                var assetLibrary3 = context.Web.Lists.EnsureSiteAssetsLibraryBatch(p => p.RootFolder.QueryProperties(p => p.Files));

                Assert.IsFalse(assetLibrary3.Requested);

                await context.ExecuteAsync();
                Assert.IsTrue(assetLibrary3.Requested);
                Assert.IsNotNull(assetLibrary3);
                Assert.IsTrue(assetLibrary3.Id != Guid.Empty);
                Assert.IsTrue(assetLibrary3.IsPropertyAvailable(p => p.RootFolder));
            }
        }

        [TestMethod]
        public async Task ComplianceTagTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                // Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("ComplianceTagTest");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // Add an item
                await myList.Items.AddAsync(new Dictionary<string, object>() { { "Title", "Test" } });

                // Add a compliance tag
                // Ensure a retentionlabel is created first: https://compliance.microsoft.com/informationgovernance?viewid=labels
                myList.SetComplianceTag("Retain1Year", false, false, false);

                // Read the compliance tag again
                var complianceTag = myList.GetComplianceTag();

                Assert.IsTrue(complianceTag != null);

                var complianceTagViaBatch = myList.GetComplianceTagBatch();
                Assert.IsFalse(complianceTagViaBatch.IsAvailable);

                await context.ExecuteAsync();

                Assert.IsTrue(complianceTagViaBatch.IsAvailable);
                Assert.IsTrue(complianceTagViaBatch.Result.TagName == "Retain1Year");

                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task FindFilesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("FindFilesAsyncTest_LIST");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                }

                // Add files to the list
                using (MemoryStream ms = new())
                {
                    var sw = new StreamWriter(ms, System.Text.Encoding.Unicode);
                    try
                    {
                        sw.Write("[Your name here]");
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);

                        for (int i = 0; i < 5; i++)
                        {
                            myList.RootFolder.Files.Add($"367664-472-E-T0{i} - Artichoke.txt", ms);
                        }

                        var subfolder = myList.RootFolder.AddFolder("subfolder");
                        for (int i = 0; i < 3; i++)
                        {
                            subfolder.Files.Add($"99887-543-F-R0{i} - Courgette.txt", ms);
                        }

                        var subsubfolder = subfolder.AddFolder("subsubfolder");
                        for (int i = 0; i < 2; i++)
                        {
                            subsubfolder.Files.Add($"872374-522-G-X0{i} - Cucumber.txt", ms);
                        }
                    }
                    finally
                    {
                        sw.Dispose();
                    }
                }

                var result1 = await myList.FindFilesAsync("367664-472-E-T00*");
                Assert.IsTrue(result1.Count == 1);

                var result2 = await myList.FindFilesAsync("367664-472-E-*");
                Assert.IsTrue(result2.Count == 5);

                var result3 = await myList.FindFilesAsync("*T04*");
                Assert.IsTrue(result3.Count == 1);

                var result4 = await myList.FindFilesAsync("*- ArtiChoKE.txt");
                Assert.IsTrue(result4.Count == 5);

                var result5 = await myList.FindFilesAsync("*NODOCUMENTS*");
                Assert.IsTrue(result5.Count == 0);

                var result6 = await myList.FindFilesAsync("*");
                Assert.IsTrue(result6.Count > 1); //just testing if the single asteriks works, should return more than one
                                                  //also returns default sp file so count may vary per environment

                var result7 = await myList.FindFilesAsync("*courgETte.txt");
                Assert.IsTrue(result7.Count == 3);

                var result8 = await myList.FindFilesAsync("*cucuMber.txT");
                Assert.IsTrue(result8.Count == 2);

                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task FindFilesTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("FindFilesTest_LIST");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                }

                // Add file to the list
                using (MemoryStream ms = new())
                {
                    var sw = new StreamWriter(ms, System.Text.Encoding.Unicode);
                    try
                    {
                        sw.Write("[Your name here]");
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);

                        myList.RootFolder.Files.Add($"367664-472-E-S01 - Artichoke.txt", ms);

                    }
                    finally
                    {
                        sw.Dispose();
                    }
                }

                var result = myList.FindFiles("*E-s01*");
                Assert.IsTrue(result.Count == 1);

                await myList.DeleteAsync();
            }
        }

        #region Event Receivers

        [TestMethod]
        public async Task GetListEventReceiversAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(l => l.EventReceivers);

                Assert.IsNotNull(list.EventReceivers);
                Assert.AreEqual(list.EventReceivers.Requested, true);
            }
        }

        [TestMethod]
        public async Task AddListEventReceiverAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(l => l.EventReceivers);

                var eventReceiverOptions = new EventReceiverOptions
                {
                    ReceiverName = "PnP Test Receiver",
                    EventType = EventReceiverType.ItemAdding,
                    ReceiverUrl = "https://pnp.github.io",
                    SequenceNumber = new Random().Next(1, 50000),
                    Synchronization = EventReceiverSynchronization.Synchronous
                };

                var newReceiver = await list.EventReceivers.AddAsync(eventReceiverOptions);

                Assert.IsNotNull(newReceiver);
                Assert.AreEqual(newReceiver.Synchronization, EventReceiverSynchronization.Synchronous);
                Assert.AreEqual(newReceiver.ReceiverName, "PnP Test Receiver");
                Assert.AreEqual(newReceiver.EventType, EventReceiverType.ItemAdding);

                await newReceiver.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task GetListEventReceiversBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(l => l.EventReceivers);

                await context.Web.LoadBatchAsync(p => p.EventReceivers);
                await context.ExecuteAsync();

                Assert.IsNotNull(list.EventReceivers);
                Assert.AreEqual(list.EventReceivers.Requested, true);
            }
        }

        [TestMethod]
        public async Task AddListEventReceiverBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(l => l.EventReceivers);

                var eventReceiverOptions = new EventReceiverOptions
                {
                    ReceiverName = "PnP Test Receiver",
                    EventType = EventReceiverType.ItemAdding,
                    ReceiverUrl = "https://pnp.github.io",
                    SequenceNumber = new Random().Next(1, 50000),
                    Synchronization = EventReceiverSynchronization.Synchronous
                };

                var newReceiver = await list.EventReceivers.AddBatchAsync(eventReceiverOptions);
                await context.ExecuteAsync();

                Assert.IsNotNull(newReceiver);
                Assert.AreEqual(newReceiver.Synchronization, EventReceiverSynchronization.Synchronous);
                Assert.AreEqual(newReceiver.ReceiverName, "PnP Test Receiver");
                Assert.AreEqual(newReceiver.EventType, EventReceiverType.ItemAdding);

                await newReceiver.DeleteAsync();
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task AddListEventReceiverAsyncNoEventTypeExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(l => l.EventReceivers);

                var eventReceiverOptions = new EventReceiverOptions
                {

                };

                await list.EventReceivers.AddAsync(eventReceiverOptions);
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task AddListEventReceiverAsyncNoEventReceiverNameExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(l => l.EventReceivers);

                var eventReceiverOptions = new EventReceiverOptions
                {
                    EventType = EventReceiverType.ItemAdding
                };

                await list.EventReceivers.AddAsync(eventReceiverOptions);
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task AddListEventReceiverAsyncNoEventReceiverUrlExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(l => l.EventReceivers);

                var eventReceiverOptions = new EventReceiverOptions
                {
                    EventType = EventReceiverType.ItemAdding,
                    ReceiverName = "PnP Event Receiver Test"
                };

                await list.EventReceivers.AddAsync(eventReceiverOptions);
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task AddListEventReceiverAsyncNoEventReceiverSequenceNumberExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(l => l.EventReceivers);

                var eventReceiverOptions = new EventReceiverOptions
                {
                    EventType = EventReceiverType.ItemAdding,
                    ReceiverName = "PnP Event Receiver Test",
                    ReceiverUrl = "https://pnp.github.io",
                    Synchronization = EventReceiverSynchronization.Synchronous
                };

                await list.EventReceivers.AddAsync(eventReceiverOptions);
            }
        }

        #endregion

        #region Effective user permissions

        [TestMethod]
        public async Task GetEffectiveUserPermissionsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");

                var siteUser = await context.Web.SiteUsers.FirstOrDefaultAsync(y => y.PrincipalType == Model.Security.PrincipalType.User);

                var basePermissions = await list.GetUserEffectivePermissionsAsync(siteUser.UserPrincipalName);

                Assert.IsNotNull(basePermissions);
            }
        }


        [TestMethod]
        public async Task CheckIfUserHasPermissionsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");

                var siteUser = await context.Web.SiteUsers.FirstOrDefaultAsync(y => y.PrincipalType == Model.Security.PrincipalType.User);

                var hasPermissions = await list.CheckIfUserHasPermissionsAsync(siteUser.UserPrincipalName, PermissionKind.AddListItems);

                Assert.IsNotNull(hasPermissions);
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task CheckIfUserHasPermissionsExceptionAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");

                var hasPermissions = await list.CheckIfUserHasPermissionsAsync(null, PermissionKind.AddListItems);
            }
        }

        #endregion

        #region Default column value tests

        [TestMethod]
        public async Task DefaultValueTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Add a new library
                string listTitle = TestCommon.GetPnPSdkTestAssetName("DefaultValueTest");
                IList myList = null;
                try
                {
                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    var termStore = await context.TermStore.GetAsync(t => t.Groups);
                    var group = termStore.Groups.AsRequested().FirstOrDefault(g => g.Name == "System");
                    await group.LoadAsync(g => g.Sets);
                    var termSet = group.Sets.AsRequested().FirstOrDefault();
                    await termSet.LoadAsync(g => g.Terms.QueryProperties(p => p.Labels));
                    var term = termSet.Terms.AsRequested().FirstOrDefault();

                    string fieldTitle = "";
                    if (!TestCommon.Instance.Mocking)
                    {
                        fieldTitle = "tax_test_" + DateTime.UtcNow.Ticks;
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "FieldTitle", fieldTitle },
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        var properties = TestManager.GetProperties(context);
                        fieldTitle = properties["FieldTitle"];
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                        myList.Fields.AddText("MyField");
                        myList.Fields.AddTaxonomy(fieldTitle, new FieldTaxonomyOptions
                        {
                            TermSetId = new Guid(termSet.Id),
                            TermStoreId = new Guid(termStore.Id),
                        });
                    }

                    // Add some folders to put default values on
                    var batch = context.NewBatch();
                    myList.RootFolder.AddFolderBatch(batch, "Folder 1");
                    myList.RootFolder.AddFolderBatch(batch, "Folder 2");
                    context.Execute(batch);

                    // Set default values on these folders
                    List<DefaultColumnValueOptions> defaultColumnValues = new()
                    {
                        new DefaultColumnValueOptions
                        {
                            FolderRelativePath = "/Folder 1",
                            FieldInternalName = "MyField",
                            DefaultValue = "F1"
                        },
                        new DefaultColumnValueOptions
                        {
                            FolderRelativePath = "/Folder 2",
                            FieldInternalName = "MyField",
                            DefaultValue = "F2"
                        },
                        new DefaultColumnValueOptions
                        {
                            FolderRelativePath ="/Folder 1",
                            FieldInternalName = fieldTitle,
                            DefaultValue = $"-1;#{term.Labels.First().Name}|{term.Id}"
                        }
                    };

                    myList.SetDefaultColumnValues(defaultColumnValues);

                    // Load the default values again
                    var loadedDefaults = myList.GetDefaultColumnValues();

                    // verify that each added value was actually added
                    foreach(var addedValue in defaultColumnValues)
                    {
                        var foundValue = loadedDefaults.FirstOrDefault(p=>p.FolderRelativePath == addedValue.FolderRelativePath && 
                                                                       p.DefaultValue == addedValue.DefaultValue && p.FieldInternalName == addedValue.FieldInternalName);
                        Assert.IsTrue(foundValue != null);
                    }

                    // Clean the default values again
                    myList.ClearDefaultColumnValues();

                    // Load the default values again
                    loadedDefaults = myList.GetDefaultColumnValues();

                    Assert.IsFalse(loadedDefaults.Any());

                }
                finally
                {
                    myList.Delete();
                }
            }
        }

        [TestMethod]
        public async Task DefaultValueUpdateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Add a new library
                string listTitle = TestCommon.GetPnPSdkTestAssetName("DefaultValueUpdateTest");
                IList myList = null;
                try
                {
                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                        var batch2 = context.NewBatch();
                        myList.Fields.AddTextBatch(batch2, "Project_Number");
                        myList.Fields.AddTextBatch(batch2, "Project_Description");
                        context.Execute(batch2);
                    }

                    // Add some folders to put default values on
                    var batch = context.NewBatch();
                    var folder1 = myList.RootFolder.AddFolderBatch(batch, "Folder 1");
                    myList.RootFolder.AddFolderBatch(batch, "Folder 2");
                    context.Execute(batch);

                    // Set default values on these folders
                    List<DefaultColumnValueOptions> defaultColumnValues = new()
                    {
                        new DefaultColumnValueOptions
                        {
                            FolderRelativePath = "/Folder 1",
                            FieldInternalName = "Project_Number",
                            DefaultValue = "1"
                        },
                        new DefaultColumnValueOptions
                        {
                            FolderRelativePath = "/Folder 1",
                            FieldInternalName = "Project_Description",
                            DefaultValue = "Description 1"
                        }
                    };

                    myList.SetDefaultColumnValues(defaultColumnValues);

                    // Load the default values again
                    var loadedDefaults = myList.GetDefaultColumnValues();

                    // verify that each added value was actually added
                    foreach (var addedValue in defaultColumnValues)
                    {
                        var foundValue = loadedDefaults.FirstOrDefault(p => p.FolderRelativePath == addedValue.FolderRelativePath &&
                                                                       p.DefaultValue == addedValue.DefaultValue && p.FieldInternalName == addedValue.FieldInternalName);
                        Assert.IsTrue(foundValue != null);
                    }

                    // Update the values
                    await SetFolderDefaultValuesAsync(context, folder1, "2", "Description 2");

                    // Load the default values again
                    loadedDefaults = myList.GetDefaultColumnValues();

                    Assert.IsTrue(loadedDefaults.FirstOrDefault(p => p.FolderRelativePath == "/Folder 1" && p.DefaultValue == "2" && p.FieldInternalName == "Project_Number") != null);
                    Assert.IsTrue(loadedDefaults.FirstOrDefault(p => p.FolderRelativePath == "/Folder 1" && p.DefaultValue == "Description 2" && p.FieldInternalName == "Project_Description") != null);
                }
                finally
                {
                    myList.Delete();
                }
            }
        }

        private async Task SetFolderDefaultValuesAsync(PnPContext context, IFolder folder, string projectNumber, string projectDescription)
        {
            var list = await context.Web.Lists.GetByServerRelativeUrlAsync(folder.ServerRelativeUrl);
            var existingColumns = await list.GetDefaultColumnValuesAsync();

            foreach (var existingColumn in existingColumns)
            {
                var value = existingColumn.FieldInternalName switch
                {
                    "Project_Number" => projectNumber,
                    "Project_Description" => projectDescription,
                    _ => existingColumn.DefaultValue
                };

                if (!string.IsNullOrWhiteSpace(value))
                {
                    existingColumn.DefaultValue = value;
                }
            }
            await list.SetDefaultColumnValuesAsync(existingColumns);
        }

        #endregion

        #region reindex tests
        [TestMethod]
        public async Task ReIndexListTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Add a new library
                string listTitle = TestCommon.GetPnPSdkTestAssetName("ReIndexListTest");
                IList myList = null;
                try
                {
                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                    }

                    // Reindex the list 
                    myList.ReIndex();

                    if (!TestCommon.Instance.Mocking)
                    {
                        Thread.Sleep(2000); 
                    }

                    // Reindex again
                    myList.ReIndex();
                }
                finally
                {
                    myList.Delete();
                }
            }
        }
        #endregion

        #region Audience targeting tests

        [TestMethod]
        public async Task AudienceTargetingListTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Add a new library
                string listTitle = TestCommon.GetPnPSdkTestAssetName("AudienceTargetingListTest");
                IList myList = null;
                try
                {
                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                    }

                    myList.EnableAudienceTargeting();

                    var listInfo = await context.Web.Lists.GetByIdAsync(myList.Id,
                                                                        p => p.RootFolder.QueryProperties(p => p.ServerRelativeUrl),
                                                                                                  p => p.EventReceivers,
                                                                                                  p => p.ContentTypesEnabled,
                                                                                                  p => p.Fields.QueryProperties(p => p.InternalName)).ConfigureAwait(false);
                    bool addFirstModernTargetingField = listInfo.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "_ModernAudienceTargetUserField") != null;
                    bool addSecondModernTargetingField = listInfo.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "_ModernAudienceAadObjectIds") != null;
                    bool addItemAddingAudienceEventRecevier = listInfo.EventReceivers.AsRequested()
                                                        .FirstOrDefault(p => p.ReceiverClass == "Microsoft.SharePoint.Portal.AudienceEventRecevier" &&
                                                                        p.EventType == EventReceiverType.ItemAdding) != null;
                    bool addItemupdatingAudienceEventRecevier = listInfo.EventReceivers.AsRequested()
                                                                .FirstOrDefault(p => p.ReceiverClass == "Microsoft.SharePoint.Portal.AudienceEventRecevier" &&
                                                                                p.EventType == EventReceiverType.ItemUpdating) != null;

                    Assert.IsTrue(addFirstModernTargetingField);
                    Assert.IsTrue(addSecondModernTargetingField);
                    Assert.IsTrue(addItemAddingAudienceEventRecevier);
                    Assert.IsTrue(addItemupdatingAudienceEventRecevier);

                }
                finally
                {
                    myList.Delete();
                }
            }
        }

        [TestMethod]
        public async Task AudienceTargetDocumentInListTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Add a new library
                string listTitle = TestCommon.GetPnPSdkTestAssetName("AudienceTargetDocumentInListTest");
                IList myList = null;
                try
                {
                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                    }

                    // Load relevant data
                    myList = context.Web.Lists.GetByTitle(listTitle, p => p.Title, 
                                                                     p => p.RootFolder,
                                                                     p => p.Fields.QueryProperties(p => p.InternalName,
                                                                                                   p => p.FieldTypeKind,
                                                                                                   p => p.TypeAsString,
                                                                                                   p => p.Title));
                    // Enable audience targeting
                    myList.EnableAudienceTargeting();

                    // Upload document
                    var fileName = TestCommon.GetPnPSdkTestAssetName("test_added.docx");
                    IFile addedFile = await myList.RootFolder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));

                    // Set audience for uploaded item
                    await addedFile.LoadAsync(p => p.ListItemAllFields.QueryProperties(li => li.All));

                    var batch = context.NewBatch();
                    var myUser1 = await context.Web.EnsureUserBatchAsync(batch, "c:0o.c|federateddirectoryclaimprovider|06ed1f73-c58d-45e8-ad07-66f4d1eed723");
                    var myUser2 = await context.Web.EnsureUserBatchAsync(batch, "c:0o.c|federateddirectoryclaimprovider|7bf72917-4c72-4a83-91d6-1362fcf7222a");
                    var myUser3 = await context.Web.EnsureUserBatchAsync(batch, "c:0o.c|federateddirectoryclaimprovider|0402aa20-e67a-47e3-bad4-03801247be9e");
                    await context.ExecuteAsync(batch);

                    var userCollection = new FieldValueCollection();
                    userCollection.Values.Add(new FieldUserValue(myUser1));
                    userCollection.Values.Add(new FieldUserValue(myUser2));
                    userCollection.Values.Add(new FieldUserValue(myUser3));

                    addedFile.ListItemAllFields.Values.Add("_ModernAudienceTargetUserField", userCollection);
                    await addedFile.ListItemAllFields.UpdateAsync();

                }
                finally
                {
                    myList.Delete();
                }
            }
        }


        [TestMethod]
        public async Task ItemOpenListSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Add a new library
                string listTitle = TestCommon.GetPnPSdkTestAssetName("ItemOpenListSettingsTest");
                IList myList = null;
                try
                {
                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                    }

                    myList.DefaultItemOpenInBrowser = false;
                    await myList.UpdateAsync();

                    myList = context.Web.Lists.GetByTitle(listTitle, p => p.DefaultItemOpenInBrowser);

                    Assert.IsTrue(myList.DefaultItemOpenInBrowser == false);
                }
                finally
                {
                    myList.Delete();
                }
            }
        }

        #endregion
    }
}
