using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class TaxonomyTests
    {

        private static string GetGroupName(PnPContext context)
        {
            string newGroupName;
            if (!TestCommon.Instance.Mocking)
            {
                newGroupName = $"PnP-{Guid.NewGuid()}";
                Dictionary<string, string> properties = new Dictionary<string, string>
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
                var newGroup = await termStore.Groups.AddAsync(newGroupName);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == newGroupName);

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

                // Delete the group again
                await newGroup.DeleteBatchAsync();
                await context.ExecuteAsync();
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

                    var updatedGroup = termStore2.Groups.FirstOrDefault(p => p.Name == $"{newGroupName}2");
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

                    if (!TestCommon.Instance.Mocking)
                    {
                        Thread.Sleep(2000);
                    }

                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        await context2.TermStore.GetAsync(p => p.Groups);
                        var group2 = context2.TermStore.Groups.FirstOrDefault(p => p.Name == newGroupName);

                        await group2.GetAsync(p => p.Sets);

                        Assert.IsTrue(group2.Sets.Requested);
                        Assert.IsTrue(group2.Sets.Length == 3);                        

                        // Delete termsets and term group in batch
                        foreach(var set in group2.Sets)
                        {
                            await set.DeleteBatchAsync();
                        }
                        await context2.ExecuteAsync();

                        // Add a delay for live testing...seems that immediately deleting the group after the termsets are deleted does 
                        // not always work (getting error about deleting non empty term group)
                        if (!TestCommon.Instance.Mocking)
                        {
                            Thread.Sleep(4000);
                        }

                        // Note: deleting termsets and group together in a single batch is not guaranteed to work as 
                        // the order in which graph batch requests are executed in not guaranteed
                        await group2.DeleteBatchAsync();
                        await context2.ExecuteAsync();
                    }
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

                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(2000);
                }

                // Update termset 
                termSet.Description = "updated description";
                termSet.LocalizedNames.Add(new TermSetLocalizedName() { LanguageTag = "nl-NL", Name = "Dutch name" });
                await termSet.UpdateAsync();

                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(2000);
                }

                // Delete term set 
                await termSet.DeleteAsync();

                // Add a delay for live testing...seems that immediately deleting the group after the termsets are deleted does 
                // not always work (getting error about deleting non empty term group)
                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(4000);
                }

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetTerms()
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
                var newTerm = await termSet.Children.AddAsync("T1", "Description in English");

                // test term update
                newTerm.AddLabelAndDescription("T1 Dutch", "nl-NL", false, "Dutch label");
                await newTerm.UpdateAsync();

                // add child term
                var newChildTerm = await newTerm.Children.AddAsync("T1.1", "English T1.1");

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

                        var termSet2 = groupWithSets.Sets.FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "PnPSet1") != null);
                        if (termSet2 != null)
                        {
                            // Load terms and parent group
                            await termSet2.GetAsync(p => p.Children);
                            Assert.IsTrue(termSet2.Children.Length > 0);

                            // Group is automatically assigned if the group was loaded before
                            Assert.IsTrue(termSet2.Group != null);

                            foreach (var term in termSet2.Children)
                            {
                                // Term set is automatically assigned if the termset was loaded before
                                Assert.IsTrue(term.Set != null);

                                // Load the children of this term and set
                                await term.GetAsync(p => p.Children);

                                foreach (var child in term.Children)
                                {
                                    Assert.IsTrue(child.Requested);
                                    Assert.IsTrue(child.Labels.Count > 0);

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

                // Add a delay for live testing...seems that immediately deleting the group after the termsets are deleted does 
                // not always work (getting error about deleting non empty term group)
                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(4000);
                }

                // Delete the group again
                await group.DeleteAsync();

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
                var newTerm = await termSet.Children.AddAsync("T1", "Description in English");

                // test term update
                newTerm.AddLabelAndDescription("T1 Dutch", "nl-NL", false, "Dutch label");
                await newTerm.UpdateAsync();

                // add child term
                var newChildTerm = await newTerm.Children.AddAsync("T1.1", "English T1.1");

                // update child term
                newChildTerm.AddLabelAndDescription("T1.1 Dutch", "nl-NL", false, "Dutch label");
                await newChildTerm.UpdateAsync();

                // delete term
                await newChildTerm.DeleteAsync();

                // delete term 
                await newTerm.DeleteAsync();

                // Delete term set 
                await termSet.DeleteAsync();

                // Add a delay for live testing...seems that immediately deleting the group after the termsets are deleted does 
                // not always work (getting error about deleting non empty term group)
                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(4000);
                }

                // Delete the group again
                await group.DeleteAsync();

            }
        }

    }
}
