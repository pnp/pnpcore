using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class TaxonomyTests
    {

        private static readonly string TermGroup = "BertOnline metadata";
        private static readonly string NewGroup = "PnPCoreSDKTestGroup";

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
                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(NewGroup);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == NewGroup);
                
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
                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group                
                var newGroup = await termStore.Groups.AddBatchAsync(NewGroup, "pnp group description");
                await context.ExecuteAsync();


                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == NewGroup);
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
                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(NewGroup);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == NewGroup);

                // Update the group name
                newGroup.Name = $"{NewGroup}2";
                await newGroup.UpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var termStore2 = await context2.TermStore.GetAsync(p => p.Groups);

                    var updatedGroup = termStore2.Groups.FirstOrDefault(p => p.Name == $"{NewGroup}2");
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
                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(NewGroup);
                
                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var groupLoadedViaLinq = await context2.TermStore.Groups.GetByIdAsync(newGroup.Id, p => p.Name, p => p.Sets);
                    Assert.IsTrue(groupLoadedViaLinq.Requested);

                    var groupLoadedViaLinq2 = await context2.TermStore.Groups.GetByNameAsync(NewGroup);
                    Assert.IsTrue(groupLoadedViaLinq2.Requested);
                }

                // Delete the group again
                await newGroup.DeleteAsync();
            }
        }

        //[TestMethod]
        //public async Task GetTermSets()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var termStore = await context.TermStore.GetAsync(p => p.Groups);
        //        Assert.IsTrue(termStore.Requested);
        //        Assert.IsTrue(termStore.Groups.Length > 0);

        //        var group = termStore.Groups.Where(p => p.Name == TermGroup).FirstOrDefault();
        //        if (group != null)
        //        {
        //            var groupWithSets = await group.GetAsync(p => p.Sets);
        //        }
        //    }
        //}

        [TestMethod]
        public async Task AddAndUpdateTermSets()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var termStore = await context.TermStore.GetAsync(p => p.Groups);
                Assert.IsTrue(termStore.Requested);
                Assert.IsTrue(termStore.Groups.Length > 0);

                // Add new group
                var newGroup = await termStore.Groups.AddAsync(NewGroup);

                Assert.IsNotNull(newGroup);
                Assert.IsTrue(newGroup.Requested);
                Assert.IsTrue(newGroup.Name == NewGroup);

                // Add term set
                var termSet = await newGroup.Sets.AddAsync("PnPSet1", "Set description");

                // Update termset 
                termSet.Description = "updated description";
                termSet.LocalizedNames.Add(new TermSetLocalizedName() { LanguageTag = "nl-NL", Name = "Dutch name" });
                await termSet.UpdateAsync();

                // Delete term set - DOES NOT WORK YET!
                //await termSet.DeleteAsync();

                // Delete the group again
                //await newGroup.DeleteAsync();
            }
        }

        //[TestMethod]
        //public async Task TempDeleteTermSet()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var termStore = await context.TermStore.GetAsync(p => p.Groups);

        //        var group = termStore.Groups.FirstOrDefault(p => p.Name == NewGroup);
        //        if (group != null)
        //        {
        //            await group.GetAsync(p => p.Sets);
        //            var termSet = group.Sets.FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "PnPSet1") != null);
        //            if (termSet != null)
        //            {
        //                await termSet.DeleteAsync();

        //            }
        //        }
        //    }
        //}

        //[TestMethod]
        //public async Task TempUpdateTermSets()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var termStore = await context.TermStore.GetAsync(p => p.Groups);

        //        var group = termStore.Groups.FirstOrDefault(p => p.Name == NewGroup);
        //        if (group != null)
        //        {
        //            await group.GetAsync(p => p.Sets);
        //            var termSet = group.Sets.FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "PnPSet1") != null);
        //            if (termSet != null)
        //            {
        //                termSet.Description = "updated description 2";
        //                termSet.LocalizedNames.Add(new TermSetLocalizedName() { LanguageTag = "nl-NL", Name = "Dutch name" });
        //                termSet.LocalizedNames.FirstOrDefault(p => p.LanguageTag == "en-US").Name = "new name";
        //                await termSet.UpdateAsync();
        //            }
        //        }
        //    }
        //}


        //[TestMethod]
        //public async Task GetTerms()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var termStore = await context.TermStore.GetAsync(p => p.Groups);
        //        Assert.IsTrue(termStore.Requested);
        //        Assert.IsTrue(termStore.Groups.Length > 0);

        //        var group = termStore.Groups.Where(p => p.Name == TermGroup).FirstOrDefault();
        //        if (group != null)
        //        {
        //            var groupWithSets = await group.GetAsync(p => p.Sets);

        //            var termSet = groupWithSets.Sets.FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "General Business Taxonomy (v1.0)") != null);
        //            if (termSet != null)
        //            {
        //                // Load terms and parent group
        //                await termSet.GetAsync(p => p.Children);
        //                Assert.IsTrue(termSet.Children.Length > 0);

        //                // Group is automatically assigned if the group was loaded before
        //                Assert.IsTrue(termSet.Group != null);

        //                foreach(var term in termSet.Children)
        //                {
        //                    // Term set is automatically assigned if the termset was loaded before
        //                    Assert.IsTrue(term.Set != null);

        //                    // Load the children of this term and set
        //                    await term.GetAsync(p => p.Children);

        //                    foreach(var child in term.Children)
        //                    {
        //                        Assert.IsTrue(child.Requested);
        //                        Assert.IsTrue(child.Labels.Count > 0);

        //                        Assert.IsTrue(child.Set != null);

        //                    }
        //                }
        //            }

        //        }

        //    }
        //}


        //[TestMethod]
        //public async Task TempAddUpdateTerms()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var termStore = await context.TermStore.GetAsync(p => p.Groups);

        //        var group = termStore.Groups.FirstOrDefault(p => p.Name == NewGroup);
        //        if (group != null)
        //        {
        //            await group.GetAsync(p => p.Sets);
        //            var termSet = group.Sets.FirstOrDefault(p => p.LocalizedNames.FirstOrDefault(p => p.Name == "PnPSet1") != null);
        //            if (termSet != null)
        //            {
        //                var newTerm = await termSet.Children.AddAsync("T1", "Description in English");

        //                // test term update
        //                newTerm.Labels.Add(new TermLocalizedLabel() { LanguageTag = "nl-NL", Name = "T1Dutch", IsDefault = false });
        //                newTerm.Descriptions.Add(new TermLocalizedDescription() { LanguageTag = "nl-NL", Description = "Dutch label" });
        //                await newTerm.UpdateAsync();

        //                // add child term
        //                var newChildTerm = await newTerm.Children.AddAsync("T1.1", "English T1.1");

        //                // update child term
        //                newChildTerm.Labels.Add(new TermLocalizedLabel() { LanguageTag = "nl-NL", Name = "T1Dutch", IsDefault = false });
        //                newChildTerm.Descriptions.Add(new TermLocalizedDescription() { LanguageTag = "nl-NL", Description = "Dutch label" });
        //                await newChildTerm.UpdateAsync();

        //                // delete term
        //                await newChildTerm.DeleteAsync();


        //                // delete term 
        //                await newTerm.DeleteAsync();

        //            }
        //        }
        //    }
        //}

    }
}
