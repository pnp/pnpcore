using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class TaxonomyTests
    {

        internal static string GetGroupName(PnPContext context)
        {
            string newGroupName;
            if (!TestCommon.Instance.Mocking)
            {
                newGroupName = $"PnP-{Guid.NewGuid()}";
                Dictionary<string, string> properties = new()
                {
                    { "GroupName", newGroupName }
                };
                TestManager.SaveProperties(context, properties);
            }
            else
            {
                var properties = TestManager.GetProperties(context);
                newGroupName = properties["GroupName"];
            }

            return newGroupName;
        }

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetTermStore()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var termStore = await context.TermStore.GetAsync();
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(!string.IsNullOrEmpty(termStore.DefaultLanguage));
                Assert.IsTrue(termStore.Languages.Count > 0);
            }
        }

        [TestMethod]
        public async Task UpdateTermStore()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var termStore = await context.TermStore.GetAsync();
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(!string.IsNullOrEmpty(termStore.DefaultLanguage));
                Assert.IsTrue(termStore.Languages.Count > 0);

                if (!termStore.Languages.Contains("cy-GB"))
                {
                    // Add a new language
                    termStore.Languages.Add("cy-GB");
                    await termStore.UpdateAsync();

                    // Load the termstore again and verify the language is there
                    termStore = await context.TermStore.GetAsync();
                    Assert.IsTrue(termStore.Languages.Contains("cy-GB"));

                    termStore.Languages.Remove("cy-GB");
                    await termStore.UpdateAsync();
                    Assert.IsFalse(termStore.Languages.Contains("cy-GB"));
                }
            }
        }

        [TestMethod]
        public async Task GetTermGroups()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);
            }
        }

        [TestMethod]
        public async Task LoadTermGroups()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.TermStore.LoadAsync(p => p.Groups);
                Assert.IsTrue(context.TermStore.Requested);
                Assert.IsTrue(context.TermStore.Groups.Length > 0);
            }
        }

        [TestMethod]
        public async Task GetTermGroupsAsyncEnumeration()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                bool groupFound = false;

                await foreach (var group in context.TermStore.Groups)
                {
                    groupFound = true;
                }

                Assert.IsTrue(groupFound);
            }
        }

        [TestMethod]
        public async Task GetTermGroupViaLinqFilter()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);
                // Add new group
                await context.TermStore.Groups.AddAsync(newGroupName, "pnp group description");

                // Get group via Linq filter
                var addedGroup = await context.TermStore.Groups.Where(p => p.Name == newGroupName).FirstOrDefaultAsync();
                Assert.IsTrue(addedGroup.Name == newGroupName);

                // Delete the created group
                await addedGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetTermSetsFromTermStoreWithoutKnowingTheGroup()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                if (group != null)
                {
                    // Add some term sets in batch
                    var pnpSet1 = await group.Sets.AddBatchAsync("PnPSet1", "Set description");
                    await context.ExecuteAsync();

                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        var pnpSet1Direct = context2.TermStore.GetTermSetById(pnpSet1.Id, p => p.Description, p => p.Group);

                        Assert.IsTrue(pnpSet1Direct != null);
                        Assert.IsTrue(pnpSet1Direct.Id == pnpSet1.Id);
                        Assert.IsTrue(pnpSet1Direct.IsPropertyAvailable(p => p.Description));

                        await context2.TermStore.LoadAsync(p => p.Groups);
                        var group2 = context2.TermStore.Groups.AsRequested().FirstOrDefault(p => p.Name == newGroupName);

                        await group2.LoadAsync(p => p.Sets);

                        Assert.IsTrue(group2.Sets.Requested);
                        Assert.IsTrue(group2.Sets.Length == 1);

                        // Delete termsets and term group in batch
                        foreach (var set in group2.Sets.AsRequested())
                        {
                            await set.DeleteBatchAsync();
                        }
                        await context2.ExecuteAsync();

                        // Note: deleting termsets and group together in a single batch is not guaranteed to work as 
                        // the order in which graph batch requests are executed in not guaranteed
                        await group2.DeleteBatchAsync();
                        await context2.ExecuteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetTermSetsFromTermStoreWithoutKnowingTheGroupBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                if (group != null)
                {
                    // Add some term sets in batch
                    var pnpSet1 = await group.Sets.AddBatchAsync("PnPSet1", "Set description");
                    await context.ExecuteAsync();

                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        var pnpSet1Direct = context2.TermStore.GetTermSetByIdBatch(pnpSet1.Id, p => p.Description, p => p.Group);

                        context2.Execute();

                        Assert.IsTrue(pnpSet1Direct != null);
                        Assert.IsTrue(pnpSet1Direct.Id == pnpSet1.Id);
                        Assert.IsTrue(pnpSet1Direct.IsPropertyAvailable(p => p.Description));

                        await context2.TermStore.LoadAsync(p => p.Groups);
                        var group2 = context2.TermStore.Groups.AsRequested().FirstOrDefault(p => p.Name == newGroupName);

                        await group2.LoadAsync(p => p.Sets);

                        Assert.IsTrue(group2.Sets.Requested);
                        Assert.IsTrue(group2.Sets.Length == 1);

                        // Delete termsets and term group in batch
                        foreach (var set in group2.Sets.AsRequested())
                        {
                            await set.DeleteBatchAsync();
                        }
                        await context2.ExecuteAsync();

                        // Note: deleting termsets and group together in a single batch is not guaranteed to work as 
                        // the order in which graph batch requests are executed in not guaranteed
                        await group2.DeleteBatchAsync();
                        await context2.ExecuteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetTermFromTermStoreWithoutKnowingTheGroup()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                if (group != null)
                {
                    // Add some term sets in batch
                    var pnpSet1 = await group.Sets.AddBatchAsync("PnPSet1", "Set description");
                    await context.ExecuteAsync();

                    // Add some terms
                    var term1 = await pnpSet1.Terms.AddBatchAsync("One");
                    var term2 = await pnpSet1.Terms.AddBatchAsync("Two");
                    await context.ExecuteAsync();

                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        try
                        {
                            var pnpTerm1Direct = context2.TermStore.GetTermById(pnpSet1.Id, term1.Id, p => p.Descriptions, p => p.Set);

                            Assert.IsTrue(pnpTerm1Direct != null);
                            Assert.IsTrue(pnpTerm1Direct.Id == term1.Id);
                            Assert.IsTrue(pnpTerm1Direct.IsPropertyAvailable(p => p.Set));
                        }
                        finally
                        {
                            await context2.TermStore.LoadAsync(p => p.Groups);
                            var group2 = context2.TermStore.Groups.AsRequested().FirstOrDefault(p => p.Name == newGroupName);

                            await group2.LoadAsync(p => p.Sets);

                            Assert.IsTrue(group2.Sets.Requested);
                            Assert.IsTrue(group2.Sets.Length == 1);

                            // Delete termsets and term group in batch
                            foreach (var set in group2.Sets.AsRequested())
                            {
                                await set.DeleteBatchAsync();
                            }
                            await context2.ExecuteAsync();

                            // Note: deleting termsets and group together in a single batch is not guaranteed to work as 
                            // the order in which graph batch requests are executed in not guaranteed
                            await group2.DeleteBatchAsync();
                            await context2.ExecuteAsync();
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetTermFromTermStoreWithoutKnowingTheGroupBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                if (group != null)
                {
                    // Add some term sets in batch
                    var pnpSet1 = await group.Sets.AddBatchAsync("PnPSet1", "Set description");
                    await context.ExecuteAsync();

                    // Add some terms
                    var term1 = await pnpSet1.Terms.AddBatchAsync("One");
                    var term2 = await pnpSet1.Terms.AddBatchAsync("Two");
                    await context.ExecuteAsync();

                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        try
                        {
                            var pnpTerm1Direct = context2.TermStore.GetTermByIdBatch(pnpSet1.Id, term1.Id, p => p.Descriptions, p => p.Set);
                            var pnpTerm2Direct = context2.TermStore.GetTermByIdBatch(pnpSet1.Id, term2.Id, p => p.Descriptions, p => p.Set);

                            context2.Execute();

                            Assert.IsTrue(pnpTerm1Direct != null);
                            Assert.IsTrue(pnpTerm1Direct.Id == term1.Id);
                            Assert.IsTrue(pnpTerm1Direct.IsPropertyAvailable(p => p.Set));

                            Assert.IsTrue(pnpTerm2Direct != null);
                            Assert.IsTrue(pnpTerm2Direct.Id == term2.Id);
                            Assert.IsTrue(pnpTerm2Direct.IsPropertyAvailable(p => p.Set));
                        }
                        finally
                        {
                            await context2.TermStore.LoadAsync(p => p.Groups);
                            var group2 = context2.TermStore.Groups.AsRequested().FirstOrDefault(p => p.Name == newGroupName);

                            await group2.LoadAsync(p => p.Sets);

                            Assert.IsTrue(group2.Sets.Requested);
                            Assert.IsTrue(group2.Sets.Length == 1);

                            // Delete termsets and term group in batch
                            foreach (var set in group2.Sets.AsRequested())
                            {
                                await set.DeleteBatchAsync();
                            }
                            await context2.ExecuteAsync();

                            // Note: deleting termsets and group together in a single batch is not guaranteed to work as 
                            // the order in which graph batch requests are executed in not guaranteed
                            await group2.DeleteBatchAsync();
                            await context2.ExecuteAsync();
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddTermGroups()
        {
            //TestCommon.Instance.Mocking = false;            
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(newGroupName, "pnp group description");

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);
                Assert.IsTrue(newGroup.Description == "pnp group description");

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddSiteCollectionTermGroups()
        {
            //TestCommon.Instance.Mocking = false;            
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(newGroupName, "pnp group description", scope: TermGroupScope.SiteCollection);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);
                Assert.IsTrue(newGroup.Description == "pnp group description");

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetTermGroupsExtensionTests()
        {
            //TestCommon.Instance.Mocking = false;            
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(newGroupName, "pnp group description");

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);
                Assert.IsTrue(newGroup.Description == "pnp group description");

                var termStoreUpdated = await context.TermStore.GetAsync(p => p.Groups, o => o.Id);
                // Extensions

                var group = termStoreUpdated.Groups.GetByName(newGroupName);
                Assert.AreEqual(group.Id, newGroup.Id);

                var group2 = termStoreUpdated.Groups.GetByName(newGroupName);
                Assert.AreEqual(group2.Id, newGroup.Id);

                var group3 = termStoreUpdated.Groups.GetById(group2.Id);
                Assert.AreEqual(group3.Id, newGroup.Id);
                Assert.AreEqual(group3.Name, newGroup.Name);

                var group4 = termStoreUpdated.Groups.GetById(group2.Id);
                Assert.AreEqual(group4.Name, newGroup.Name);
                Assert.AreEqual(group3.Id, newGroup.Id);

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetTermGroupsExtensionExceptionsTests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(newGroupName, "pnp group description");

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);
                Assert.IsTrue(newGroup.Description == "pnp group description");

                var termStoreUpdated = await context.TermStore.GetAsync(p => p.Groups, o => o.Id);
                // Extensions

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    termStoreUpdated.Groups.GetByName(string.Empty);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    termStoreUpdated.Groups.GetByName(null);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    termStoreUpdated.Groups.GetById(string.Empty);
                });

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddTermGroupsInBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group                
                var newGroup = await termStore.Groups.AddBatchAsync(newGroupName, "pnp group description");
                await context.ExecuteAsync();
                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);
                Assert.IsTrue(newGroup.Description == "pnp group description");

                var newGroup2 = termStore.Groups.AddBatch($"{newGroupName}-2", "pnp group description");
                await context.ExecuteAsync();
                Assert.IsNotNull(newGroup2);
                Assert.IsTrue(newGroup2.Requested);
                Assert.IsTrue(newGroup2.Name == $"{newGroupName}-2");
                Assert.IsTrue(newGroup2.Description == "pnp group description");

                var newBatch = context.NewBatch();
                var newGroup3 = termStore.Groups.AddBatch(newBatch, $"{newGroupName}-3", "pnp group description");
                await context.ExecuteAsync(newBatch);
                Assert.IsNotNull(newGroup3);
                Assert.IsTrue(newGroup3.Requested);
                Assert.IsTrue(newGroup3.Name == $"{newGroupName}-3");
                Assert.IsTrue(newGroup3.Description == "pnp group description");

                // Delete the group again
                await newGroup.DeleteBatchAsync();
                await newGroup2.DeleteBatchAsync();
                await newGroup3.DeleteBatchAsync();
                await context.ExecuteAsync();
            }
        }

        [TestMethod]
        public async Task AddTermGroupsExceptionTests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    await termStore.Groups.AddAsync(string.Empty);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    await termStore.Groups.AddBatchAsync(string.Empty);
                    await context.ExecuteAsync();
                });

            }
        }

        [TestMethod]
        public async Task UpdateTermGroups()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(newGroupName);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);

                // Update the group name
                newGroup.Name = $"{newGroupName}2";
                await newGroup.UpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var termStore2 = await context2.TermStore.GetAsync(p => p.Groups);

                    var updatedGroup = termStore2.Groups.AsRequested().FirstOrDefault(p => p.Name == $"{newGroupName}2");
                    Assert.IsNotNull(updatedGroup);
                    Assert.IsTrue(updatedGroup.Id == newGroup.Id);

                    // Delete the updated group
                    await updatedGroup.DeleteAsync();
                }

            }
        }

        [TestMethod]
        public async Task GetTermGroupByLinq()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(newGroupName);

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var groupLoadedViaLinq = await context2.TermStore.Groups.GetByIdAsync(newGroup.Id, p => p.Name);
                    Assert.IsTrue(groupLoadedViaLinq.Requested);

                    var groupLoadedViaLinq2 = await context2.TermStore.Groups.GetByNameAsync(newGroupName);
                    Assert.IsTrue(groupLoadedViaLinq2.Requested);
                }

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetTermSets()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                if (group != null)
                {
                    // Add some term sets in batch
                    var pnpSet1 = await group.Sets.AddBatchAsync("PnPSet1", "Set description");
                    var pnpSet2 = await group.Sets.AddBatchAsync("PnPSet2", "Set description");
                    var pnpSet3 = await group.Sets.AddBatchAsync("PnPSet3", "Set description");
                    await context.ExecuteAsync();

                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        await context2.TermStore.LoadAsync(p => p.Groups);
                        var group2 = context2.TermStore.Groups.AsRequested().FirstOrDefault(p => p.Name == newGroupName);

                        await group2.LoadAsync(p => p.Sets);

                        Assert.IsTrue(group2.Sets.Requested);
                        Assert.IsTrue(group2.Sets.Length == 3);

                        // Delete termsets and term group in batch
                        foreach (var set in group2.Sets.AsRequested())
                        {
                            await set.DeleteBatchAsync();
                        }
                        await context2.ExecuteAsync();

                        // Note: deleting termsets and group together in a single batch is not guaranteed to work as 
                        // the order in which graph batch requests are executed in not guaranteed
                        await group2.DeleteBatchAsync();
                        await context2.ExecuteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetTermSetByLinq()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var newGroup = await context.TermStore.Groups.AddAsync(newGroupName);

                // Add term set
                var termSet = await newGroup.Sets.AddAsync("PnPSet1", "Set description");

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var groupLoadedViaLinq2 = await context2.TermStore.Groups.GetByNameAsync(newGroupName);
                    Assert.IsTrue(groupLoadedViaLinq2.Requested);

                    var termsetLoadedViaLinq2 = await groupLoadedViaLinq2.Sets.GetByIdAsync(termSet.Id);
                    Assert.IsTrue(termsetLoadedViaLinq2.Requested);
                    Assert.IsTrue(termsetLoadedViaLinq2.Id == termSet.Id);
                }

                // Delete the termset
                await termSet.DeleteAsync();

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetTermSetsAndTermsViaAsyncEnumeration()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                if (group != null)
                {
                    // Add some term sets in batch
                    var pnpSet1 = await group.Sets.AddBatchAsync("PnPSet1", "Set description");
                    var pnpSet2 = await group.Sets.AddBatchAsync("PnPSet2", "Set description");
                    var pnpSet3 = await group.Sets.AddBatchAsync("PnPSet3", "Set description");
                    await context.ExecuteAsync();

                    // Add some terms
                    await pnpSet1.Terms.AddBatchAsync("One");
                    await pnpSet1.Terms.AddBatchAsync("Two");
                    await pnpSet2.Terms.AddBatchAsync("Three");
                    await pnpSet3.Terms.AddBatchAsync("Four");
                    await pnpSet3.Terms.AddBatchAsync("Five");
                    await pnpSet3.Terms.AddBatchAsync("Six");
                    await context.ExecuteAsync();

                    bool termsetFound = false;
                    await foreach (var termset in group.Sets)
                    {
                        termsetFound = true;
                        bool termFound = false;
                        await foreach (var term in termset.Terms)
                        {
                            termFound = true;
                        }
                        Assert.IsTrue(termFound);
                    }
                    Assert.IsTrue(termsetFound);

                    await pnpSet1.DeleteAsync();
                    await pnpSet2.DeleteAsync();
                    await pnpSet3.DeleteAsync();
                    await group.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task AddAndUpdateTermSets()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(newGroupName);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);

                // Add term set
                var termSet = await newGroup.Sets.AddAsync("PnPSet1", "Set description");

                // Update termset 
                termSet.Description = "updated description";
                termSet.AddLocalizedName("Dutch name", "nl-NL");
                await termSet.UpdateAsync();

                // Delete term set 
                await termSet.DeleteAsync();

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddAndUpdateTermSetsBatchTests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(newGroupName);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);

                // Add term set
                var termSet = await newGroup.Sets.AddBatchAsync("PnPSet1", "Set description");
                await context.ExecuteAsync();

                var newBatch = context.NewBatch();
                var termSet2 = await newGroup.Sets.AddBatchAsync(newBatch, "PnPSet2", "Set description");
                await context.ExecuteAsync(newBatch);

                var termSet3 = newGroup.Sets.AddBatch("PnPSet3", "Set description");
                await context.ExecuteAsync();

                var newBatch2 = context.NewBatch();
                var termSet4 = await newGroup.Sets.AddBatchAsync(newBatch2, "PnPSet4", "Set description");
                await context.ExecuteAsync(newBatch2);

                // Delete term set 
                await termSet.DeleteAsync();
                await termSet2.DeleteAsync();
                await termSet3.DeleteAsync();
                await termSet4.DeleteAsync();

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddAndUpdateTermSetsExceptionsTests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(newGroupName);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);

                // Add term set
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    var termSet = await newGroup.Sets.AddBatchAsync(string.Empty);
                    await context.ExecuteAsync();
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    var termSet = await newGroup.Sets.AddAsync(string.Empty);
                    await context.ExecuteAsync();
                });

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddAndUpdateTermSetProperties()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var newGroup = await context.TermStore.Groups.AddAsync(newGroupName);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);

                // Add term set
                var termSet = await newGroup.Sets.AddAsync("PnPSet1", "Set description");

                // Update termset 
                termSet.AddProperty("property1", "value1");
                await termSet.UpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var groupLoadedViaLinq2 = await context2.TermStore.Groups.GetByNameAsync(newGroupName);
                    Assert.IsTrue(groupLoadedViaLinq2.Requested);

                    // read the termset again to test property loading
                    var termsetLoadedViaLinq2 = await groupLoadedViaLinq2.Sets.GetByIdAsync(termSet.Id, p => p.Properties);
                    Assert.IsTrue(termsetLoadedViaLinq2.Requested);
                    Assert.IsTrue(termsetLoadedViaLinq2.Id == termSet.Id);

                    Assert.IsTrue(termsetLoadedViaLinq2.IsPropertyAvailable(p => p.Properties));
                    Assert.IsTrue(termsetLoadedViaLinq2.Properties.FirstOrDefault(p => p.KeyField == "property1") != null);

                    // test property update
                    termsetLoadedViaLinq2.AddProperty("property1", "value1-updated");
                    await termsetLoadedViaLinq2.UpdateAsync();

                    // test property delete
                    (termsetLoadedViaLinq2.Properties as TermSetPropertyCollection).Clear();
                    await termsetLoadedViaLinq2.UpdateAsync();
                }

                // Delete term set 
                await termSet.DeleteAsync();

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetTermsAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                // Add term set
                var termSet = await group.Sets.AddAsync("PnPSet1", "Set description");

                // Add term
                var newTerm = await termSet.Terms.AddAsync("T1", "Description in English");

                // test term update
                newTerm.AddLabelAndDescription("T1 Dutch", "nl-NL", false, "Dutch label");
                await newTerm.UpdateAsync();

                // add child term
                var newChildTerm = await newTerm.Terms.AddAsync("T1.1", "English T1.1");

                // update child term
                newChildTerm.AddLabelAndDescription("T1.1 Dutch", "nl-NL", false, "Dutch label");
                await newChildTerm.UpdateAsync();

                // Retrieve the created terms
                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    // Use linq provider to get a group by name
                    var group2 = await context2.TermStore.Groups.Where(p => p.Name == newGroupName).FirstOrDefaultAsync();
                    if (group2 != null)
                    {
                        var groupWithSets = await group2.GetAsync(p => p.Sets);

                        var termSet2 = groupWithSets.Sets.AsRequested().FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "PnPSet1") != null);
                        if (termSet2 != null)
                        {
                            // Load terms and parent group
                            await termSet2.LoadAsync(p => p.Terms);
                            Assert.IsTrue(termSet2.Terms.Length > 0);

                            // Group is automatically assigned if the group was loaded before
                            Assert.IsTrue(termSet2.Group != null);

                            foreach (var term in termSet2.Terms.AsRequested())
                            {
                                // Term set is automatically assigned if the termset was loaded before
                                Assert.IsTrue(term.Set != null);

                                // Load the children of this term and set
                                await term.LoadAsync(p => p.Terms);

                                foreach (var child in term.Terms.AsRequested())
                                {
                                    Assert.IsTrue(child.Requested);
                                    Assert.IsTrue((child.Labels as TermLocalizedLabelCollection).Count > 0);

                                    Assert.IsTrue(child.Set != null);

                                }
                            }
                        }
                    }
                }

                // delete term
                await newChildTerm.DeleteAsync();

                // delete term 
                await newTerm.DeleteAsync();

                // Delete term set 
                await termSet.DeleteAsync();

                // Delete the group again
                await group.DeleteAsync();

            }
        }

        [TestMethod]
        public void GetTerms()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = context.TermStore.Groups.Add(newGroupName);

                // Add term set
                var termSet = group.Sets.Add("PnPSet1", "Set description");

                // Add term
                var newTerm = termSet.Terms.Add("T1", "Description in English");

                // test term update
                newTerm.AddLabelAndDescription("T1 Dutch", "nl-NL", false, "Dutch label");
                newTerm.Update();

                // add child term
                var newChildTerm = newTerm.Terms.Add("T1.1", "English T1.1");

                // update child term
                newChildTerm.AddLabelAndDescription("T1.1 Dutch", "nl-NL", false, "Dutch label");
                newChildTerm.Update();

                // Retrieve the created terms
                using (var context2 = TestCommon.Instance.GetContext(TestCommon.TestSite, 1))
                {
                    // Use linq provider to get a group by name
                    var group2 = context2.TermStore.Groups.Where(p => p.Name == newGroupName).FirstOrDefault();
                    if (group2 != null)
                    {
                        var groupWithSets = group2.Get(p => p.Sets);

                        var termSet2 = groupWithSets.Sets.AsRequested().FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "PnPSet1") != null);
                        if (termSet2 != null)
                        {
                            // Load terms and parent group
                            termSet2.Load(p => p.Terms);
                            Assert.IsTrue(termSet2.Terms.Length > 0);

                            // Group is automatically assigned if the group was loaded before
                            Assert.IsTrue(termSet2.Group != null);

                            foreach (var term in termSet2.Terms.AsRequested())
                            {
                                // Term set is automatically assigned if the termset was loaded before
                                Assert.IsTrue(term.Set != null);

                                // Load the children of this term and set
                                term.Load(p => p.Terms);

                                foreach (var child in term.Terms.AsRequested())
                                {
                                    Assert.IsTrue(child.Requested);
                                    Assert.IsTrue((child.Labels as TermLocalizedLabelCollection).Count > 0);

                                    Assert.IsTrue(child.Set != null);

                                }
                            }
                        }
                    }
                }

                // delete term
                newChildTerm.Delete();

                // delete term 
                newTerm.Delete();

                // Delete term set 
                termSet.Delete();

                // Delete the group again
                group.Delete();

            }
        }

        [TestMethod]
        public void GetNestedTerms()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = context.TermStore.Groups.Add(newGroupName);

                // Add term set
                var termSet = group.Sets.Add("PnPSet1", "Set description");

                // Add term
                var newTerm = termSet.Terms.Add("T1", "Description in English");

                // add child term
                var newChildTerm = newTerm.Terms.Add("T1.1", "English T1.1");

                // add child of child term
                var newChildChildTerm = newChildTerm.Terms.Add("T1.1.1", "English T1.1.1");

                // Retrieve the created terms
                using (var context2 = TestCommon.Instance.GetContext(TestCommon.TestSite, 1))
                {
                    // Use linq provider to get a group by name
                    var group2 = context2.TermStore.Groups.Where(p => p.Name == newGroupName).FirstOrDefault();
                    if (group2 != null)
                    {
                        var groupWithSets = group2.Get(p => p.Sets);

                        var termSet2 = groupWithSets.Sets.AsRequested().FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "PnPSet1") != null);
                        if (termSet2 != null)
                        {
                            // Load terms and parent group
                            termSet2.Load(p => p.Terms);
                            Assert.IsTrue(termSet2.Terms.Length > 0);

                            // Group is automatically assigned if the group was loaded before
                            Assert.IsTrue(termSet2.Group != null);

                            foreach (var term in termSet2.Terms.AsRequested())
                            {
                                // Term set is automatically assigned if the termset was loaded before
                                Assert.IsTrue(term.Set != null);

                                // Load the children of this term and set
                                term.Load(p => p.Terms);

                                foreach (var child in term.Terms.AsRequested())
                                {
                                    Assert.IsTrue(child.Requested);
                                    Assert.IsTrue((child.Labels as TermLocalizedLabelCollection).Count > 0);

                                    Assert.IsTrue(child.Set != null);

                                    child.Load(p => p.Terms);
                                    foreach(var childchild in child.Terms.AsRequested())
                                    {
                                        Assert.IsTrue(childchild.Requested);
                                        Assert.IsTrue((childchild.Labels as TermLocalizedLabelCollection).Count > 0);

                                        Assert.IsTrue(childchild.Set != null);
                                    }
                                }
                            }
                        }
                    }
                }

                // delete term
                newChildChildTerm.Delete();

                // delete term
                newChildTerm.Delete();

                // delete term 
                newTerm.Delete();

                // Delete term set 
                termSet.Delete();

                // Delete the group again
                group.Delete();

            }
        }

        [TestMethod]
        public async Task GetTermsBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var newBatch = context.NewBatch();

                // Add new group
                var group = context.TermStore.Groups.Add(newGroupName);

                // Add term set
                var termSet = group.Sets.Add("PnPSet1", "Set description");

                // Add term
                var newTerm = termSet.Terms.AddBatch(newBatch, "T1", "Description in English");
                await context.ExecuteAsync(newBatch); // Limited to terms only parents MUST exist

                // test term update
                newTerm.AddLabelAndDescription("T1 Dutch", "nl-NL", false, "Dutch label");
                newTerm.Update();

                var newTerm2 = termSet.Terms.AddBatch("T2", "Description in English");
                await context.ExecuteAsync();

                // add child term
                var newChildTerm = newTerm.Terms.Add("T1.1", "English T1.1");

                // update child term
                newChildTerm.AddLabelAndDescription("T1.1 Dutch", "nl-NL", false, "Dutch label");
                newChildTerm.Update();

                // Retrieve the created terms
                using (var context2 = TestCommon.Instance.GetContext(TestCommon.TestSite, 1))
                {
                    // Use linq provider to get a group by name
                    var group2 = context2.TermStore.Groups.Where(p => p.Name == newGroupName).FirstOrDefault();
                    if (group2 != null)
                    {
                        var groupWithSets = group2.Get(p => p.Sets);

                        var termSet2 = groupWithSets.Sets.AsRequested().FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "PnPSet1") != null);
                        if (termSet2 != null)
                        {
                            // Load terms and parent group
                            termSet2.Load(p => p.Terms, o => o.Id);
                            Assert.IsTrue(termSet2.Terms.Length > 0);

                            // Group is automatically assigned if the group was loaded before
                            Assert.IsTrue(termSet2.Group != null);

                            var termId = newChildTerm.Id;

                            var resultTerm = termSet2.Terms.GetById(termId);

                            Assert.IsNotNull(resultTerm);
                            Assert.AreEqual(resultTerm.Id, termId);
                        }
                    }
                }

                // delete term
                newChildTerm.Delete();

                // delete term 
                newTerm.Delete();

                // Delete term set 
                termSet.Delete();

                // Delete the group again
                group.Delete();

            }
        }

        [TestMethod]
        public async Task GetTermsExceptionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                var newBatch = context.NewBatch();

                // Add new group
                var group = context.TermStore.Groups.Add(newGroupName);

                // Add term set
                var termSet = group.Sets.Add("PnPSet1", "Set description");

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    await termSet.Terms.AddAsync(string.Empty);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    await termSet.Terms.AddBatchAsync(string.Empty);
                    await context.ExecuteAsync();
                });

                // Delete term set 
                termSet.Delete();

                // Delete the group again
                group.Delete();

            }
        }

        [TestMethod]
        public async Task AddUpdateDeleteTerms()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                // Add term set
                var termSet = await group.Sets.AddAsync("PnPSet1", "Set description");

                // Add term
                var newTerm = await termSet.Terms.AddAsync("T1", "Description in English");

                // test term update
                newTerm.AddLabelAndDescription("T1 Dutch", "nl-NL", false, "Dutch label");
                await newTerm.UpdateAsync();

                // add child term
                var newChildTerm = await newTerm.Terms.AddAsync("T1.1", "English T1.1");

                // update child term
                newChildTerm.AddLabelAndDescription("T1.1 Dutch", "nl-NL", false, "Dutch label");
                await newChildTerm.UpdateAsync();

                // delete term
                await newChildTerm.DeleteAsync();

                // delete term 
                await newTerm.DeleteAsync();

                // Delete term set 
                await termSet.DeleteAsync();

                // Delete the group again
                await group.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task AddUpdateDeleteTermProperties()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                // Add term set
                var termSet = await group.Sets.AddAsync("PnPSet1", "Set description");

                // Add term
                var newTerm = await termSet.Terms.AddAsync("T1", "Description in English");

                // test term update
                newTerm.AddProperty("property1", "value1");
                await newTerm.UpdateAsync();

                // add child term
                var newChildTerm = await newTerm.Terms.AddAsync("T1.1", "English T1.1");

                // update child term
                newChildTerm.AddProperty("property2", "value2");
                await newChildTerm.UpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var groupLoadedViaLinq2 = await context2.TermStore.Groups.GetByNameAsync(newGroupName);
                    Assert.IsTrue(groupLoadedViaLinq2.Requested);

                    // read the termset again to test property loading
                    var termsetLoadedViaLinq2 = await groupLoadedViaLinq2.Sets.GetByIdAsync(termSet.Id, p => p.Properties);
                    Assert.IsTrue(termsetLoadedViaLinq2.Requested);
                    Assert.IsTrue(termsetLoadedViaLinq2.Id == termSet.Id);

                    // load the term 
                    var termLoadedViaLinq2 = await termsetLoadedViaLinq2.Terms.GetByIdAsync(newTerm.Id);
                    Assert.IsTrue(termLoadedViaLinq2.Requested);
                    Assert.IsTrue(termLoadedViaLinq2.Id == newTerm.Id);

                    var childTermLoadedViaLinq2 = await termLoadedViaLinq2.Terms.GetByIdAsync(newChildTerm.Id);
                    Assert.IsTrue(childTermLoadedViaLinq2.Requested);
                    Assert.IsTrue(childTermLoadedViaLinq2.Id == newChildTerm.Id);

                    // check that properties were loaded
                    Assert.IsTrue(termLoadedViaLinq2.IsPropertyAvailable(p => p.Properties));
                    Assert.IsTrue(termLoadedViaLinq2.Properties.FirstOrDefault(p => p.KeyField == "property1") != null);

                    Assert.IsTrue(childTermLoadedViaLinq2.IsPropertyAvailable(p => p.Properties));
                    Assert.IsTrue(childTermLoadedViaLinq2.Properties.FirstOrDefault(p => p.KeyField == "property2") != null);

                    // update term properties
                    termLoadedViaLinq2.AddProperty("property1", "value1 - updated");
                    await termLoadedViaLinq2.UpdateBatchAsync();
                    childTermLoadedViaLinq2.AddProperty("property2", "value2 - updated");
                    await childTermLoadedViaLinq2.UpdateBatchAsync();
                    await context2.ExecuteAsync();

                    // delete term properties
                    (termLoadedViaLinq2.Properties as TermPropertyCollection).Clear();
                    await termLoadedViaLinq2.UpdateBatchAsync();
                    (childTermLoadedViaLinq2.Properties as TermPropertyCollection).Clear();
                    await childTermLoadedViaLinq2.UpdateBatchAsync();
                    await context2.ExecuteAsync();
                }

                // delete term
                await newChildTerm.DeleteAsync();

                // delete term 
                await newTerm.DeleteAsync();

                // Delete term set 
                await termSet.DeleteAsync();

                // Delete the group again
                await group.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task GetTermsByCustomProperty()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                // Add term set
                var termSet = await group.Sets.AddAsync("PnPSet1", "Set description");

                // Add term
                var newTerm = await termSet.Terms.AddAsync("T1", "Description in English");

                // test term update
                newTerm.AddProperty("property1", "value1");
                await newTerm.UpdateAsync();

                // add child term
                var newChildTerm = await newTerm.Terms.AddAsync("T1.1", "English T1.1");

                // update child term
                newChildTerm.AddProperty("property2", "value2");
                await newChildTerm.UpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var groupLoadedViaLinq2 = await context2.TermStore.Groups.GetByNameAsync(newGroupName);
                    Assert.IsTrue(groupLoadedViaLinq2.Requested);

                    // read the termset again to test property loading
                    var termsetLoadedViaLinq2 = await groupLoadedViaLinq2.Sets.GetByIdAsync(termSet.Id, p => p.Properties);
                    Assert.IsTrue(termsetLoadedViaLinq2.Requested);
                    Assert.IsTrue(termsetLoadedViaLinq2.Id == termSet.Id);

                    // load the terms by custom property
                    var terms = await termsetLoadedViaLinq2.GetTermsByCustomPropertyAsync("property2", "value2");
                    
                    Assert.AreEqual(terms.Count, 1);
                    
                    foreach (ITerm term in terms)
                    {
                        await term.DeleteAsync();
                    }
                    
                    terms = termsetLoadedViaLinq2.GetTermsByCustomProperty("property1", "value1");
                    Assert.AreEqual(terms.Count, 1);
                    
                    foreach (ITerm term in terms)
                    {
                        await term.DeleteAsync();
                    }

                    terms = await termsetLoadedViaLinq2.GetTermsByCustomPropertyAsync("property2", "value2");
                    Assert.AreEqual(terms.Count, 0);

                    terms = await termsetLoadedViaLinq2.GetTermsByCustomPropertyAsync("property1", "value1");
                    Assert.AreEqual(terms.Count, 0);
                }

                // Delete term set 
                await termSet.DeleteAsync();

                // Delete the group again
                await group.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task CreateAndGetTermRelations()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                // Add term sets
                var termSet1 = await group.Sets.AddBatchAsync("TermSet1", "");
                var termSet2 = await group.Sets.AddBatchAsync("TermSet2", "");
                await context.ExecuteAsync();

                // Add terms
                var termA = await termSet1.Terms.AddBatchAsync("TermA", "");
                var termAA = await termSet1.Terms.AddBatchAsync("TermAA", "");
                var termB = await termSet2.Terms.AddBatchAsync("TermB", "");
                await context.ExecuteAsync();

                // Pin TermA under TermB in TermSet2
                await termA.Relations.AddBatchAsync(TermRelationType.Pin, termSet2, termB);
                // Reuse TermAA under TermSet2
                await termAA.Relations.AddBatchAsync(TermRelationType.Reuse, termSet2);
                await context.ExecuteAsync();

                // Retrieve the created term relations
                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var groupLoadedViaLinq2 = await context2.TermStore.Groups.GetByNameAsync(newGroupName);
                    Assert.IsTrue(groupLoadedViaLinq2.Requested);

                    await groupLoadedViaLinq2.LoadAsync(p => p.Sets);

                    var termSet1Loaded = groupLoadedViaLinq2.Sets.AsRequested().FirstOrDefault(p => p.Id == termSet1.Id);
                    var termSet2Loaded = groupLoadedViaLinq2.Sets.AsRequested().FirstOrDefault(p => p.Id == termSet2.Id);

                    // Load relations via termset
                    await termSet1Loaded.LoadBatchAsync(p => p.Relations, p => p.Terms);
                    await termSet2Loaded.LoadBatchAsync(p => p.Relations, p => p.Terms);
                    await context2.ExecuteAsync();

                    // termset1 does not have any relations
                    Assert.IsTrue(termSet1Loaded.Requested);
                    Assert.IsTrue(termSet1Loaded.Relations.Requested);
                    Assert.IsTrue(termSet1Loaded.Relations.Length == 0);

                    // Since we reused termAA under termset2 there must be a relation
                    Assert.IsTrue(termSet2Loaded.Requested);
                    Assert.IsTrue(termSet2Loaded.Relations.Requested);
                    Assert.IsTrue(termSet2Loaded.Relations.Length > 0);
                    Assert.IsTrue(termSet2Loaded.Relations.AsRequested().First().Relationship == TermRelationType.Reuse);
                    Assert.IsTrue(termSet2Loaded.Relations.AsRequested().First().ToTerm.Id == termAA.Id);
                    Assert.IsTrue(termSet2Loaded.Relations.AsRequested().First().Set.Id == termSet2.Id);

                    var termALoaded = termSet1Loaded.Terms.AsRequested().FirstOrDefault(p => p.Id == termA.Id);
                    var termAALoaded = termSet1Loaded.Terms.AsRequested().FirstOrDefault(p => p.Id == termAA.Id);
                    var termBLoaded = termSet2Loaded.Terms.AsRequested().FirstOrDefault(p => p.Id == termB.Id);

                    // Load relations via term
                    await termALoaded.LoadBatchAsync(p => p.Relations);
                    await termAALoaded.LoadBatchAsync(p => p.Relations);
                    await termBLoaded.LoadBatchAsync(p => p.Relations);
                    await context2.ExecuteAsync();

                    // termA was pinned to termB
                    Assert.IsTrue(termALoaded.Requested);
                    Assert.IsTrue(termALoaded.Relations.Requested);
                    Assert.IsTrue(termALoaded.Relations.Length > 0);
                    Assert.IsTrue(termALoaded.Relations.AsRequested().First().Relationship == TermRelationType.Pin);
                    Assert.IsTrue(termALoaded.Relations.AsRequested().First().ToTerm.Id == termA.Id);
                    Assert.IsTrue(termALoaded.Relations.AsRequested().First().Set.Id == termSet2.Id);
                    Assert.IsTrue(termALoaded.Relations.AsRequested().First().FromTerm.Id == termB.Id);

                    // TermAA was reused to termset2
                    Assert.IsTrue(termAALoaded.Requested);
                    Assert.IsTrue(termAALoaded.Relations.Requested);
                    Assert.IsTrue(termAALoaded.Relations.Length > 0);
                    Assert.IsTrue(termAALoaded.Relations.AsRequested().First().Relationship == TermRelationType.Reuse);
                    Assert.IsTrue(termAALoaded.Relations.AsRequested().First().ToTerm.Id == termAA.Id);
                    Assert.IsTrue(termAALoaded.Relations.AsRequested().First().Set.Id == termSet2.Id);

                    // termB received termA as a pinned term
                    Assert.IsTrue(termBLoaded.Requested);
                    Assert.IsTrue(termBLoaded.Relations.Requested);
                    Assert.IsTrue(termBLoaded.Relations.Length > 0);
                    Assert.IsTrue(termBLoaded.Relations.AsRequested().First().Relationship == TermRelationType.Pin);
                    Assert.IsTrue(termBLoaded.Relations.AsRequested().First().ToTerm.Id == termA.Id);
                    Assert.IsTrue(termBLoaded.Relations.AsRequested().First().Set.Id == termSet2.Id);
                    Assert.IsTrue(termBLoaded.Relations.AsRequested().First().FromTerm.Id == termB.Id);
                }

                // Delete term sets, delete the term set reusing terms first to prevent deletion errors 
                await termSet2.DeleteAsync();

                await termSet1.DeleteAsync();

                // Delete the group again
                await group.DeleteAsync();

            }
        }

    }
}