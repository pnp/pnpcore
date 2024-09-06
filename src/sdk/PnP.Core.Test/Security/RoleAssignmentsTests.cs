using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class RoleAssignmentsTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetWebRoleAssignments()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(w => w.RoleAssignments);
                Assert.IsTrue(context.Web.RoleAssignments.Length > 0);
            }
        }


        [TestMethod]
        public async Task WebWithCustomPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "permissionweb";

                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });

                try
                {
                    using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                    {
                        // break permission inheritance
                        context2.Web.BreakRoleInheritance(false, true);

                        // get current user
                        var currentUser = await context2.Web.GetCurrentUserAsync();

                        var roleDefinitions = (await context2.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                        var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                        // Assign current user "Full Control"
                        context2.Web.AddRoleDefinitions(currentUser.Id, new string[] { "Full Control"});

                        // Assign "edit" role
                        context2.Web.AddRoleDefinition(currentUser.Id, editRole);

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length == 1);
                        Assert.IsTrue(context2.Web.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                        // remove the editor role
                        context2.Web.RemoveRoleDefinitions(currentUser.Id, new string[] { "Full Control" });
                        context2.Web.RemoveRoleDefinition(currentUser.Id, editRole);

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length == 0);

                        // reset permission inheritance
                        context2.Web.ResetRoleInheritance();
                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsFalse(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length > 1);
                    }
                }
                finally
                {
                    // Delete the created web again
                    await addedWeb.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task WebWithCustomPermissionsBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "permissionwebbatch";

                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });

                try
                {
                    using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                    {
                        // get current user
                        var currentUser = await context2.Web.GetCurrentUserAsync();
                        var roleDefinitions = (await context2.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                        var fullControlRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Full Control");
                        var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                        // break permission inheritance
                        context2.Web.BreakRoleInheritanceBatch(false, true);

                        // Assign current user "Full Control"
                        context2.Web.AddRoleDefinitionBatch(currentUser.Id, fullControlRole);
                        context2.Web.AddRoleDefinitionBatch(currentUser.Id, editRole);

                        // Execute batch
                        context2.Execute();

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length == 1);
                        Assert.IsTrue(context2.Web.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                        // remove the editor role
                        context2.Web.RemoveRoleDefinitionBatch(currentUser.Id, editRole);

                        // Execute batch
                        context2.Execute();

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length == 1);
                        Assert.IsTrue(context2.Web.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 1);

                        // reset permission inheritance
                        context2.Web.ResetRoleInheritanceBatch();
                        // Execute batch
                        context2.Execute();

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsFalse(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length > 1);
                    }
                }
                finally
                {
                    // Delete the created web again
                    await addedWeb.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task ListWithCustomPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListWithCustomPermissions";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                try
                {
                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    // break permission inheritance
                    myList.BreakRoleInheritance(false, true);

                    // get current user
                    var currentUser = await context.Web.GetCurrentUserAsync();

                    var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                    var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                    // Assign current user "Full Control"
                    myList.AddRoleDefinitions(currentUser.Id, new string[] { "Full Control" });
                    myList.AddRoleDefinition(currentUser.Id, editRole);

                    // reload web
                    await myList.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(myList.HasUniqueRoleAssignments);
                    Assert.IsTrue(myList.RoleAssignments.Length == 1);
                    Assert.IsTrue(myList.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                    // remove the editor role
                    myList.RemoveRoleDefinitions(currentUser.Id, new string[] { "Full Control" });
                    myList.RemoveRoleDefinition(currentUser.Id, editRole);

                    // reload web
                    await myList.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(myList.HasUniqueRoleAssignments);
                    Assert.IsTrue(myList.RoleAssignments.Length == 0);

                    // reset permission inheritance
                    myList.ResetRoleInheritance();
                    // reload web
                    await myList.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsFalse(myList.HasUniqueRoleAssignments);
                    Assert.IsTrue(myList.RoleAssignments.Length > 1);

                }
                finally
                {
                    // Delete the created list again
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task ListWithCustomPermissionsBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListWithCustomPermissionsBatch";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                try
                {
                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    // get current user
                    var currentUser = await context.Web.GetCurrentUserAsync();
                    var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                    var fullControlRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Full Control");
                    var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                    // break permission inheritance
                    myList.BreakRoleInheritanceBatch(false, true);

                    // Assign current user "Full Control"
                    myList.AddRoleDefinitionBatch(currentUser.Id, fullControlRole);
                    myList.AddRoleDefinitionBatch(currentUser.Id, editRole);

                    // Execute batch
                    context.Execute();

                    // reload web
                    await myList.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(myList.HasUniqueRoleAssignments);
                    Assert.IsTrue(myList.RoleAssignments.Length == 1);
                    Assert.IsTrue(myList.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                    // remove the editor role
                    myList.RemoveRoleDefinitionBatch(currentUser.Id, editRole);

                    // Execute batch
                    context.Execute();

                    // reload web
                    await myList.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(myList.HasUniqueRoleAssignments);
                    Assert.IsTrue(myList.RoleAssignments.Length == 1);
                    Assert.IsTrue(myList.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 1);

                    // reset permission inheritance
                    myList.ResetRoleInheritanceBatch();
                    // Execute batch
                    context.Execute();

                    // reload web
                    await myList.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsFalse(myList.HasUniqueRoleAssignments);
                    Assert.IsTrue(myList.RoleAssignments.Length > 1);
                }
                finally
                {
                    // Delete the created list again
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task ListItemWithCustomPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListItemWithCustomPermissions";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                try
                {
                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
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
                    }

                    var first = myList.Items.First();

                    // break permission inheritance
                    first.BreakRoleInheritance(false, true);

                    // get current user
                    var currentUser = await context.Web.GetCurrentUserAsync();

                    var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                    var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                    // Assign current user "Full Control"
                    first.AddRoleDefinitions(currentUser.Id, new string[] { "Full Control" });
                    first.AddRoleDefinition(currentUser.Id, editRole);

                    // reload web
                    await first.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(first.HasUniqueRoleAssignments);
                    Assert.IsTrue(first.RoleAssignments.Length == 1);
                    Assert.IsTrue(first.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                    // remove the editor role
                    first.RemoveRoleDefinitions(currentUser.Id, new string[] { "Full Control" });
                    first.RemoveRoleDefinition(currentUser.Id, editRole);

                    // reload web
                    await first.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(first.HasUniqueRoleAssignments);
                    Assert.IsTrue(first.RoleAssignments.Length == 0);

                    // reset permission inheritance
                    first.ResetRoleInheritance();
                    // reload web
                    await first.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsFalse(first.HasUniqueRoleAssignments);
                    Assert.IsTrue(first.RoleAssignments.Length > 1);

                }
                finally
                {
                    // Delete the created list again
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task ListItemWithCustomPermissionsBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListItemWithCustomPermissionsBatch";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                try
                {
                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
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
                    }

                    var first = myList.Items.First();

                    // get current user
                    var currentUser = await context.Web.GetCurrentUserAsync();
                    var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                    var fullControlRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Full Control");
                    var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                    // break permission inheritance
                    first.BreakRoleInheritanceBatch(false, true);

                    // Assign current user "Full Control"
                    first.AddRoleDefinitionBatch(currentUser.Id, fullControlRole);
                    first.AddRoleDefinitionBatch(currentUser.Id, editRole);

                    // Execute batch
                    context.Execute();

                    // reload web
                    await first.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(first.HasUniqueRoleAssignments);
                    Assert.IsTrue(first.RoleAssignments.Length == 1);
                    Assert.IsTrue(first.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                    // remove the editor role
                    first.RemoveRoleDefinitionBatch(currentUser.Id, editRole);

                    // Execute batch
                    context.Execute();

                    // reload web
                    await first.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(first.HasUniqueRoleAssignments);
                    Assert.IsTrue(first.RoleAssignments.Length == 1);
                    Assert.IsTrue(first.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 1);

                    // reset permission inheritance
                    first.ResetRoleInheritanceBatch();
                    // Execute batch
                    context.Execute();

                    // reload web
                    await first.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsFalse(first.HasUniqueRoleAssignments);
                    Assert.IsTrue(first.RoleAssignments.Length > 1);
                }
                finally
                {
                    // Delete the created list again
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task DeleteWebRoleAssignment()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "permissionweb2";

                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });

                try
                {
                    using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                    {
                        // break permission inheritance
                        context2.Web.BreakRoleInheritance(false, true);

                        // get current user
                        var currentUser = await context2.Web.GetCurrentUserAsync();

                        var roleDefinitions = (await context2.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                        var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                        // Assign current user "Full Control"
                        context2.Web.AddRoleDefinitions(currentUser.Id, new string[] { "Full Control" });

                        // Assign "edit" role
                        context2.Web.AddRoleDefinition(currentUser.Id, editRole);

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length == 1);
                        Assert.IsTrue(context2.Web.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                        int count = context2.Web.RoleAssignments.AsRequested().Count();
                        await context2.Web.RoleAssignments.AsRequested().First().DeleteAsync();

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.RoleAssignments.AsRequested().Count() == (count - 1));
                    }
                }
                finally
                {
                    // Delete the created web again
                    await addedWeb.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task DeleteListRoleAssignment()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "DeleteListRoleAssignment";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                try
                {
                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    // break permission inheritance
                    myList.BreakRoleInheritance(false, true);

                    // get current user
                    var currentUser = await context.Web.GetCurrentUserAsync();

                    var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                    var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                    // Assign current user "Full Control"
                    myList.AddRoleDefinitions(currentUser.Id, new string[] { "Full Control" });
                    myList.AddRoleDefinition(currentUser.Id, editRole);

                    // reload web
                    await myList.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(myList.HasUniqueRoleAssignments);
                    Assert.IsTrue(myList.RoleAssignments.Length == 1);
                    Assert.IsTrue(myList.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                    int count = myList.RoleAssignments.AsRequested().Count();
                    await myList.RoleAssignments.AsRequested().First().DeleteAsync();

                    // reload web
                    await myList.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(myList.RoleAssignments.AsRequested().Count() == (count - 1));
                }
                finally
                {
                    // Delete the created list again
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task DeleteListItemRoleAssignment()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListItemWithCustomPermissions";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                try
                {
                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
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
                    }

                    var first = myList.Items.First();

                    // break permission inheritance
                    first.BreakRoleInheritance(false, true);

                    // get current user
                    var currentUser = await context.Web.GetCurrentUserAsync();

                    var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                    var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                    // Assign current user "Full Control"
                    first.AddRoleDefinitions(currentUser.Id, new string[] { "Full Control" });
                    first.AddRoleDefinition(currentUser.Id, editRole);

                    // reload web
                    await first.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(first.HasUniqueRoleAssignments);
                    Assert.IsTrue(first.RoleAssignments.Length == 1);
                    Assert.IsTrue(first.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                    int count = first.RoleAssignments.AsRequested().Count();
                    await first.RoleAssignments.AsRequested().First().DeleteAsync();

                    // reload web
                    await first.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                    Assert.IsTrue(first.RoleAssignments.AsRequested().Count() == (count - 1));
                }
                finally
                {
                    // Delete the created list again
                    await myList.DeleteAsync();
                }
            }
        }
    }
}
