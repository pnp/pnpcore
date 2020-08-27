using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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

                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(4000);
                }

                // Delete the group again
                await newGroup.DeleteAsync();
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

                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(2000);
                }

                // Update termset 
                termSet.AddProperty("property1", "value1");
                await termSet.UpdateAsync();

                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(2000);
                }

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var groupLoadedViaLinq2 = await context2.TermStore.Groups.GetByNameAsync(newGroupName);
                    Assert.IsTrue(groupLoadedViaLinq2.Requested);

                    // read the termset again to test property loading
                    var termsetLoadedViaLinq2 = await groupLoadedViaLinq2.Sets.GetByIdAsync(termSet.Id, p => p.Properties);
                    Assert.IsTrue(termsetLoadedViaLinq2.Requested);
                    Assert.IsTrue(termsetLoadedViaLinq2.Id == termSet.Id);

                    Assert.IsTrue(termsetLoadedViaLinq2.IsPropertyAvailable(p => p.Properties));
                    Assert.IsTrue(termsetLoadedViaLinq2.Properties.FirstOrDefault(p => p.Key == "property1") != null);
                    
                    if (!TestCommon.Instance.Mocking)
                    {
                        Thread.Sleep(10000);
                    }
                    
                    // test property update
                    termsetLoadedViaLinq2.AddProperty("property1", "value1-updated");
                    await termsetLoadedViaLinq2.UpdateAsync();

                    if (!TestCommon.Instance.Mocking)
                    {
                        Thread.Sleep(10000);
                    }

                    // test property delete
                    termsetLoadedViaLinq2.Properties.Clear();
                    await termsetLoadedViaLinq2.UpdateAsync();

                }

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

                        var termSet2 = groupWithSets.Sets.FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "PnPSet1") != null);
                        if (termSet2 != null)
                        {
                            // Load terms and parent group
                            await termSet2.GetAsync(p => p.Terms);
                            Assert.IsTrue(termSet2.Terms.Length > 0);

                            // Group is automatically assigned if the group was loaded before
                            Assert.IsTrue(termSet2.Group != null);

                            foreach (var term in termSet2.Terms)
                            {
                                // Term set is automatically assigned if the termset was loaded before
                                Assert.IsTrue(term.Set != null);

                                // Load the children of this term and set
                                await term.GetAsync(p => p.Terms);

                                foreach (var child in term.Terms)
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
        public async Task AddUpdateDeleteTermProperties()
        {
            TestCommon.Instance.Mocking = false;
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

                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(2000);
                }

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
                    Assert.IsTrue(termLoadedViaLinq2.Properties.FirstOrDefault(p => p.Key == "property1") != null);

                    Assert.IsTrue(childTermLoadedViaLinq2.IsPropertyAvailable(p => p.Properties));
                    Assert.IsTrue(childTermLoadedViaLinq2.Properties.FirstOrDefault(p => p.Key == "property2") != null);

                    // update term properties
                    termLoadedViaLinq2.AddProperty("property1", "value1 - updated");
                    await termLoadedViaLinq2.UpdateBatchAsync();
                    childTermLoadedViaLinq2.AddProperty("property2", "value2 - updated");
                    await childTermLoadedViaLinq2.UpdateBatchAsync();
                    await context2.ExecuteAsync();

                    if (!TestCommon.Instance.Mocking)
                    {
                        Thread.Sleep(10000);
                    }

                    // delete term properties
                    termLoadedViaLinq2.Properties.Clear();
                    await termLoadedViaLinq2.UpdateBatchAsync();
                    childTermLoadedViaLinq2.Properties.Clear();
                    await childTermLoadedViaLinq2.UpdateBatchAsync();
                    await context2.ExecuteAsync();

                    if (!TestCommon.Instance.Mocking)
                    {
                        Thread.Sleep(10000);
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
                    Thread.Sleep(6000);
                }

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

                    await groupLoadedViaLinq2.GetAsync(p => p.Sets);

                    var termSet1Loaded = groupLoadedViaLinq2.Sets.FirstOrDefault(p => p.Id == termSet1.Id);
                    var termSet2Loaded = groupLoadedViaLinq2.Sets.FirstOrDefault(p => p.Id == termSet2.Id);

                    // Load relations via termset
                    await termSet1Loaded.GetBatchAsync(p => p.Relations, p => p.Terms);
                    await termSet2Loaded.GetBatchAsync(p => p.Relations, p => p.Terms);
                    await context2.ExecuteAsync();

                    // termset1 does not have any relations
                    Assert.IsTrue(termSet1Loaded.Requested);
                    Assert.IsTrue(termSet1Loaded.Relations.Requested);
                    Assert.IsTrue(termSet1Loaded.Relations.Length == 0);

                    // Since we reused termAA under termset2 there must be a relation
                    Assert.IsTrue(termSet2Loaded.Requested);
                    Assert.IsTrue(termSet2Loaded.Relations.Requested);
                    Assert.IsTrue(termSet2Loaded.Relations.Length > 0);
                    Assert.IsTrue(termSet2Loaded.Relations.First().Relationship == TermRelationType.Reuse);
                    Assert.IsTrue(termSet2Loaded.Relations.First().ToTerm.Id == termAA.Id);
                    Assert.IsTrue(termSet2Loaded.Relations.First().Set.Id == termSet2.Id);

                    var termALoaded = termSet1Loaded.Terms.FirstOrDefault(p => p.Id == termA.Id);
                    var termAALoaded = termSet1Loaded.Terms.FirstOrDefault(p => p.Id == termAA.Id);
                    var termBLoaded = termSet2Loaded.Terms.FirstOrDefault(p => p.Id == termB.Id);

                    // Load relations via term
                    await termALoaded.GetBatchAsync(p => p.Relations);
                    await termAALoaded.GetBatchAsync(p => p.Relations);
                    await termBLoaded.GetBatchAsync(p => p.Relations);
                    await context2.ExecuteAsync();

                    // termA was pinned to termB
                    Assert.IsTrue(termALoaded.Requested);
                    Assert.IsTrue(termALoaded.Relations.Requested);
                    Assert.IsTrue(termALoaded.Relations.Length > 0);
                    Assert.IsTrue(termALoaded.Relations.First().Relationship == TermRelationType.Pin);
                    Assert.IsTrue(termALoaded.Relations.First().ToTerm.Id == termA.Id);
                    Assert.IsTrue(termALoaded.Relations.First().Set.Id == termSet2.Id);
                    Assert.IsTrue(termALoaded.Relations.First().FromTerm.Id == termB.Id);

                    // TermAA was reused to termset2
                    Assert.IsTrue(termAALoaded.Requested);
                    Assert.IsTrue(termAALoaded.Relations.Requested);
                    Assert.IsTrue(termAALoaded.Relations.Length > 0);
                    Assert.IsTrue(termAALoaded.Relations.First().Relationship == TermRelationType.Reuse);
                    Assert.IsTrue(termAALoaded.Relations.First().ToTerm.Id == termAA.Id);
                    Assert.IsTrue(termAALoaded.Relations.First().Set.Id == termSet2.Id);

                    // termB received termA as a pinned term
                    Assert.IsTrue(termBLoaded.Requested);
                    Assert.IsTrue(termBLoaded.Relations.Requested);
                    Assert.IsTrue(termBLoaded.Relations.Length > 0);
                    Assert.IsTrue(termBLoaded.Relations.First().Relationship == TermRelationType.Pin);
                    Assert.IsTrue(termBLoaded.Relations.First().ToTerm.Id == termA.Id);
                    Assert.IsTrue(termBLoaded.Relations.First().Set.Id == termSet2.Id);
                    Assert.IsTrue(termBLoaded.Relations.First().FromTerm.Id == termB.Id);
                }

                // Delete term sets, delete the term set reusing terms first to prevent deletion errors 
                await termSet2.DeleteAsync();

                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(5000);
                }

                await termSet1.DeleteAsync();

                // Add a delay for live testing...seems that immediately deleting the group after the termsets are deleted does 
                // not always work (getting error about deleting non empty term group)
                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(10000);
                }

                // Delete the group again
                await group.DeleteAsync();

            }
        }

    }
}
