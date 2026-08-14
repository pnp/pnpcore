using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TermGroupModel = PnP.Core.Provisioning.Model.TermGroup;
using TermLabelModel = PnP.Core.Provisioning.Model.TermLabel;
using TermModel = PnP.Core.Provisioning.Model.Term;
using TermSetModel = PnP.Core.Provisioning.Model.TermSet;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectTermGroups</c>
    /// </summary>
    [TestClass]
    public class ObjectTermGroupsLiveTests : LiveTestBase
    {
        /// <summary>
        /// The names and ids one test uses, distinct from every other test's.
        /// </summary>
        private sealed class Fixture
        {
            internal Fixture(int index)
            {
                GroupName = $"{TestPrefix}Group{index}";
                TermSetName = $"{TestPrefix}Set{index}";

                GroupId = IdFor(index, 1);
                TermSetId = IdFor(index, 2);
                FirstTermId = IdFor(index, 3);
                SecondTermId = IdFor(index, 4);
                ChildTermId = IdFor(index, 5);
            }

            internal string GroupName { get; }

            internal string TermSetName { get; }

            internal Guid GroupId { get; }

            internal Guid TermSetId { get; }

            internal Guid FirstTermId { get; }

            internal Guid SecondTermId { get; }

            internal Guid ChildTermId { get; }

            private static Guid IdFor(int testIndex, int slot)
            {
                return new Guid($"4a1c7f10-{testIndex:D4}-4a5e-9f3a-6b2e5c8d{slot:D4}");
            }
        }

        #region Apply

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task TermGroups_CreatesTheGroupSetAndTermsWithTheIdsTheTemplateGave()
        {
            var fixture = new Fixture(1);

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    await context.GetProvisioningManager().ApplyTemplateAsync(BuildTemplate(fixture), Reporting())
                        .ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        ITermGroup group = await FindGroupAsync(fresh, fixture.GroupName).ConfigureAwait(false);

                        Assert.IsNotNull(group, "The term group was not created.");
                        Console.WriteLine($"Group '{group.Name}' -> {group.Id}");

                        Assert.AreEqual(fixture.GroupId.ToString(), group.Id, true,
                            "The group's id was not preserved - is the write going through Graph?");

                        await group.LoadAsync(g => g.Sets.QueryProperties(s => s.Id, s => s.LocalizedNames))
                            .ConfigureAwait(false);

                        ITermSet set = group.Sets.AsRequested().FirstOrDefault();
                        Assert.IsNotNull(set, "The term set was not created.");
                        Assert.AreEqual(fixture.TermSetId.ToString(), set.Id, true, "The term set's id was not preserved.");

                        await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id, t => t.Labels)).ConfigureAwait(false);

                        List<ITerm> terms = set.Terms.AsRequested().ToList();
                        Console.WriteLine($"Terms: {string.Join(", ", terms.Select(t => $"{NameOf(t)} ({t.Id})"))}");

                        Assert.AreEqual(2, terms.Count, "Expected exactly the two terms the template declares.");

                        Assert.IsTrue(terms.Any(t => string.Equals(t.Id, fixture.FirstTermId.ToString(), StringComparison.OrdinalIgnoreCase)),
                            "The first term's id was not preserved.");
                        Assert.IsTrue(terms.Any(t => string.Equals(t.Id, fixture.SecondTermId.ToString(), StringComparison.OrdinalIgnoreCase)),
                            "The second term's id was not preserved.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task TermGroups_CreatesAChildTerm()
        {
            var fixture = new Fixture(2);

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    await context.GetProvisioningManager().ApplyTemplateAsync(BuildTemplate(fixture), Reporting())
                        .ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        ITermGroup group = await FindGroupAsync(fresh, fixture.GroupName).ConfigureAwait(false);
                        Assert.IsNotNull(group, "The term group was not created.");

                        await group.LoadAsync(g => g.Sets.QueryProperties(s => s.Id)).ConfigureAwait(false);

                        ITermSet set = group.Sets.AsRequested().First();
                        await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id, t => t.Labels)).ConfigureAwait(false);

                        ITerm parent = set.Terms.AsRequested()
                            .FirstOrDefault(t => string.Equals(t.Id, fixture.FirstTermId.ToString(), StringComparison.OrdinalIgnoreCase));

                        Assert.IsNotNull(parent, "The parent term was not created.");

                        await parent.LoadAsync(t => t.Terms.QueryProperties(c => c.Id, c => c.Labels)).ConfigureAwait(false);

                        List<ITerm> children = parent.Terms.AsRequested().ToList();
                        Console.WriteLine($"Children of '{NameOf(parent)}': {string.Join(", ", children.Select(NameOf))}");

                        Assert.AreEqual(1, children.Count, "Expected exactly one child term.");
                        Assert.AreEqual(fixture.ChildTermId.ToString(), children[0].Id, true,
                            "The child term's id was not preserved.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task TermGroups_AppliedTwiceDoesNotDuplicate()
        {
            var fixture = new Fixture(3);

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    await manager.ApplyTemplateAsync(BuildTemplate(fixture), Reporting()).ConfigureAwait(false);
                    await manager.ApplyTemplateAsync(BuildTemplate(fixture), Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        await fresh.TermStore.LoadAsync(t => t.Groups.QueryProperties(g => g.Id, g => g.Name))
                            .ConfigureAwait(false);

                        List<ITermGroup> matches = fresh.TermStore.Groups.AsRequested()
                            .Where(g => g.Name == fixture.GroupName).ToList();

                        Assert.AreEqual(1, matches.Count, "The second apply created a second term group.");

                        await matches[0].LoadAsync(g => g.Sets.QueryProperties(s => s.Id)).ConfigureAwait(false);

                        ITermSet set = matches[0].Sets.AsRequested().Single();
                        await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id)).ConfigureAwait(false);

                        int termCount = set.Terms.AsRequested().Count();

                        Console.WriteLine($"After two applies: {matches.Count} group(s), {termCount} term(s)");

                        Assert.AreEqual(2, termCount, "The second apply duplicated the terms.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Extract

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task TermGroups_ExtractsTheGroupItJustCreated()
        {
            var fixture = new Fixture(4);

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                try
                {
                    await context.GetProvisioningManager().ApplyTemplateAsync(BuildTemplate(fixture), Reporting())
                        .ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        var configuration = new ExtractConfiguration();
                        configuration.Handlers.Add(ConfigurationHandler.Taxonomy);
                        configuration.Taxonomy.IncludeAllTermGroups = true;

                        ProvisioningTemplate extracted = await fresh.GetProvisioningManager()
                            .GetTemplateAsync(configuration).ConfigureAwait(false);

                        Console.WriteLine($"Extracted {extracted.TermGroups.Count} group(s)");

                        TermGroupModel group = extracted.TermGroups.FirstOrDefault(g => g.Name == fixture.GroupName);
                        Assert.IsNotNull(group, "The term group was not extracted.");
                        Assert.AreEqual(fixture.GroupId, group.Id, "The group's id was not read back.");

                        TermSetModel set = group.TermSets.FirstOrDefault();
                        Assert.IsNotNull(set, "The term set was not extracted.");
                        Assert.AreEqual(fixture.TermSetId, set.Id, "The term set's id was not read back.");
                        Assert.AreEqual(fixture.TermSetName, set.Name, "The term set's name was not read back.");

                        TermModel first = set.Terms.FirstOrDefault(t => t.Id == fixture.FirstTermId);
                        Assert.IsNotNull(first, "The first term was not extracted.");

                        Assert.AreEqual(1, first.Terms.Count, "The child term was not extracted under its parent.");
                        Assert.AreEqual(fixture.ChildTermId, first.Terms[0].Id, "The child term's id was not read back.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// A group with one term set, two terms, one child term, and a label on the first term.
        /// </summary>
        private static ProvisioningTemplate BuildTemplate(Fixture fixture)
        {
            var child = new TermModel
            {
                Id = fixture.ChildTermId,
                Name = $"Child_{fixture.GroupName}",
            };

            var first = new TermModel
            {
                Id = fixture.FirstTermId,
                Name = $"First_{fixture.GroupName}",
                Description = "First term",
                CustomSortOrder = 1,
            };
            first.Terms.Add(child);
            first.Labels.Add(new TermLabelModel
            {
                Value = $"Alias_{fixture.GroupName}",
                Language = CultureInfo.GetCultureInfo("en-US").LCID,
                IsDefaultForLanguage = false,
            });

            var second = new TermModel
            {
                Id = fixture.SecondTermId,
                Name = $"Second_{fixture.GroupName}",
                CustomSortOrder = 2,
            };

            var set = new TermSetModel
            {
                Id = fixture.TermSetId,
                Name = fixture.TermSetName,
                Description = "Created by ObjectTermGroupsLiveTests",
                IsOpenForTermCreation = false,
            };
            set.Terms.Add(first);
            set.Terms.Add(second);

            var group = new TermGroupModel
            {
                Id = fixture.GroupId,
                Name = fixture.GroupName,
                Description = "Created by ObjectTermGroupsLiveTests",
            };
            group.TermSets.Add(set);

            var template = new ProvisioningTemplate();
            template.TermGroups.Add(group);
            return template;
        }

        private static ApplyConfiguration Reporting()
        {
            return new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        Console.WriteLine($"[{type}] {message}");
                    }
                },
            };
        }

        private static string NameOf(ITerm term)
        {
            return term.Labels?.FirstOrDefault(l => l.IsDefault)?.Name ?? term.Labels?.FirstOrDefault()?.Name;
        }

        private static async Task<ITermGroup> FindGroupAsync(PnPContext context, string name)
        {
            await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(g => g.Id, g => g.Name, g => g.Description))
                .ConfigureAwait(false);

            return context.TermStore.Groups.AsRequested().FirstOrDefault(g => g.Name == name);
        }

        /// <summary>
        /// Deletes every term group this suite created, reporting what it could not remove.
        /// </summary>
        private static async Task SweepAsync()
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(g => g.Id, g => g.Name))
                        .ConfigureAwait(false);

                    foreach (ITermGroup group in context.TermStore.Groups.AsRequested()
                        .Where(g => g.Name != null && g.Name.StartsWith(TestPrefix, StringComparison.Ordinal))
                        .ToList())
                    {
                        string name = group.Name;

                        try
                        {
                            await DeleteGroupDeepAsync(group).ConfigureAwait(false);
                            Console.WriteLine($"Deleted term group '{name}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT DELETE term group '{name}': {Describe(ex)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT SWEEP term groups: {Describe(ex)}");
            }
        }

        private static async Task DeleteGroupDeepAsync(ITermGroup group)
        {
            await group.LoadAsync(g => g.Sets.QueryProperties(s => s.Id)).ConfigureAwait(false);

            foreach (ITermSet set in group.Sets.AsRequested().ToList())
            {
                await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id)).ConfigureAwait(false);

                foreach (ITerm term in set.Terms.AsRequested().ToList())
                {
                    await term.DeleteAsync().ConfigureAwait(false);
                }

                await set.DeleteAsync().ConfigureAwait(false);
            }

            await group.DeleteAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
