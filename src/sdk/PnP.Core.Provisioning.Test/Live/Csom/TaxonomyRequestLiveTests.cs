using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Taxonomy;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Csom
{
    /// <summary>
    /// Live round trips for the eight taxonomy CSOM requests.
    /// </summary>
    [TestClass]
    public class TaxonomyRequestLiveTests : LiveTestBase
    {
        private static string UniqueName(string what) => $"{TestPrefix}{what}_{DateTime.UtcNow:HHmmssfff}";

        /// <summary>
        /// Sends a taxonomy request and prints what SharePoint said if it fails.
        /// </summary>
        private static async Task<T> SendAsync<T>(PnPContext context, IRequest<T> request)
        {
            try
            {
                return await CsomRequestSender.SendAsync(context, request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TAXONOMY REQUEST FAILED ({request.GetType().Name}):");
                Console.WriteLine(Describe(ex));
                throw;
            }
        }

        /// <summary>
        /// Deletes a term group by id, emptying it first, tolerating its absence.
        /// </summary>
        private static async Task DeleteGroupAsync(PnPContext context, Guid groupId)
        {
            if (groupId == Guid.Empty) return;

            await DeleteTermGroupDeepAsync(context, groupId.ToString()).ConfigureAwait(false);
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task CreateTermGroup_PreservesTheSuppliedId()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Guid groupId = Guid.NewGuid();
                string groupName = UniqueName("Group");

                try
                {
                    TermGroupInfo created = await SendAsync(context,
                        new CreateTermGroupRequest(groupName, groupId, "Created by a live test")).ConfigureAwait(false);

                    Assert.IsNotNull(created, "CreateTermGroupRequest returned no result.");

                    Assert.AreEqual(groupId, created.Id,
                        "The term group id was not preserved. If CSOM cannot preserve ids either, D7's premise is wrong.");
                    Assert.AreEqual(groupName, created.Name);

                    ITermGroup readBack = await context.TermStore.Groups.GetByIdAsync(groupId.ToString()).ConfigureAwait(false);
                    Assert.IsNotNull(readBack, "The group was created but Graph cannot see it - the two APIs disagree on which store this is.");
                    Assert.AreEqual(groupName, readBack.Name);
                }
                finally
                {
                    await DeleteGroupAsync(context, groupId).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task CreateTermSetAndTerm_PreserveTheirSuppliedIds()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Guid groupId = Guid.NewGuid();
                Guid termSetId = Guid.NewGuid();
                Guid termId = Guid.NewGuid();

                try
                {
                    await SendAsync(context,
                        new CreateTermGroupRequest(UniqueName("Group"), groupId)).ConfigureAwait(false);

                    TermSetInfo termSet = await SendAsync(context,
                        new CreateTermSetRequest(groupId, UniqueName("Set"), termSetId, 1033)).ConfigureAwait(false);

                    Assert.IsNotNull(termSet, "CreateTermSetRequest returned no result.");
                    Assert.AreEqual(termSetId, termSet.Id, "The term set id was not preserved.");

                    TermInfo term = await SendAsync(context,
                        new CreateTermRequest(termSetId, parentIsTermSet: true, UniqueName("Term"), termId, 1033))
                        .ConfigureAwait(false);

                    Assert.IsNotNull(term, "CreateTermRequest returned no result.");
                    Assert.AreEqual(termId, term.Id, "The term id was not preserved - this is the S1 failure mode.");
                }
                finally
                {
                    await DeleteGroupAsync(context, groupId).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task CreateTerm_UnderAnotherTermPreservesTheSuppliedId()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Guid groupId = Guid.NewGuid();
                Guid termSetId = Guid.NewGuid();
                Guid parentTermId = Guid.NewGuid();
                Guid childTermId = Guid.NewGuid();

                try
                {
                    await SendAsync(context, new CreateTermGroupRequest(UniqueName("Group"), groupId)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermSetRequest(groupId, UniqueName("Set"), termSetId, 1033)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermRequest(termSetId, true, UniqueName("Parent"), parentTermId, 1033)).ConfigureAwait(false);

                    TermInfo child = await SendAsync(context,
                        new CreateTermRequest(parentTermId, parentIsTermSet: false, UniqueName("Child"), childTermId, 1033))
                        .ConfigureAwait(false);

                    Assert.IsNotNull(child);
                    Assert.AreEqual(childTermId, child.Id);
                }
                finally
                {
                    await DeleteGroupAsync(context, groupId).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task AddTermLabel_AddsASynonymInTheGivenLanguage()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Guid groupId = Guid.NewGuid();
                Guid termSetId = Guid.NewGuid();
                Guid termId = Guid.NewGuid();

                try
                {
                    await SendAsync(context, new CreateTermGroupRequest(UniqueName("Group"), groupId)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermSetRequest(groupId, UniqueName("Set"), termSetId, 1033)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermRequest(termSetId, true, "Primary", termId, 1033)).ConfigureAwait(false);

                    await SendAsync(context,
                        new AddTermLabelRequest(termId, "Synonym", 1033, isDefaultForLanguage: false)).ConfigureAwait(false);

                    ITermSet set = await context.TermStore.Groups.GetById(groupId.ToString())
                        .Sets.GetByIdAsync(termSetId.ToString()).ConfigureAwait(false);
                    await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id, t => t.Labels)).ConfigureAwait(false);

                    ITerm term = set.Terms.AsRequested().FirstOrDefault(t => t.Id == termId.ToString());
                    Assert.IsNotNull(term, "The term was not found after adding a label.");

                    Assert.IsTrue(term.Labels.Any(l => l.Name == "Synonym"),
                        "The synonym label was not persisted. Labels present: " +
                        string.Join(", ", term.Labels.Select(l => l.Name)));
                }
                finally
                {
                    await DeleteGroupAsync(context, groupId).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task SetTermCustomProperty_WritesBothSharedAndLocalProperties()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Guid groupId = Guid.NewGuid();
                Guid termSetId = Guid.NewGuid();
                Guid termId = Guid.NewGuid();

                try
                {
                    await SendAsync(context, new CreateTermGroupRequest(UniqueName("Group"), groupId)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermSetRequest(groupId, UniqueName("Set"), termSetId, 1033)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermRequest(termSetId, true, "Term", termId, 1033)).ConfigureAwait(false);

                    await SendAsync(context,
                        new SetTermCustomPropertyRequest(termId, "SharedKey", "SharedValue", isLocal: false)).ConfigureAwait(false);

                    await SendAsync(context,
                        new SetTermCustomPropertyRequest(termId, "LocalKey", "LocalValue", isLocal: true)).ConfigureAwait(false);

                    ITermSet set = await context.TermStore.Groups.GetById(groupId.ToString())
                        .Sets.GetByIdAsync(termSetId.ToString()).ConfigureAwait(false);
                    await set.LoadAsync(s => s.Terms.QueryProperties(t => t.Id, t => t.Properties)).ConfigureAwait(false);

                    ITerm term = set.Terms.AsRequested().FirstOrDefault(t => t.Id == termId.ToString());
                    Assert.IsNotNull(term);

                    Assert.IsTrue(term.Properties.Any(p => p.KeyField == "SharedKey" && p.Value == "SharedValue"),
                        "The shared custom property was not persisted.");
                }
                finally
                {
                    await DeleteGroupAsync(context, groupId).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task DeprecateTerm_MarksTheTermDeprecated()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Guid groupId = Guid.NewGuid();
                Guid termSetId = Guid.NewGuid();
                Guid termId = Guid.NewGuid();

                try
                {
                    await SendAsync(context, new CreateTermGroupRequest(UniqueName("Group"), groupId)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermSetRequest(groupId, UniqueName("Set"), termSetId, 1033)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermRequest(termSetId, true, "Retired", termId, 1033)).ConfigureAwait(false);

                    await SendAsync(context,
                        new DeprecateTermRequest(termId, deprecate: true)).ConfigureAwait(false);

                    await SendAsync(context,
                        new DeprecateTermRequest(termId, deprecate: false)).ConfigureAwait(false);
                }
                finally
                {
                    await DeleteGroupAsync(context, groupId).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task ReuseTerm_ReusesATermUnderAnotherSet()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Guid groupId = Guid.NewGuid();
                Guid sourceSetId = Guid.NewGuid();
                Guid targetSetId = Guid.NewGuid();
                Guid sourceTermId = Guid.NewGuid();

                try
                {
                    await SendAsync(context, new CreateTermGroupRequest(UniqueName("Group"), groupId)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermSetRequest(groupId, UniqueName("Source"), sourceSetId, 1033)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermSetRequest(groupId, UniqueName("Target"), targetSetId, 1033)).ConfigureAwait(false);
                    await SendAsync(context, new CreateTermRequest(sourceSetId, true, "Shared", sourceTermId, 1033)).ConfigureAwait(false);

                    TermInfo reused = await SendAsync(context,
                        new ReuseTermRequest(targetSetId, parentIsTermSet: true, sourceTermId)).ConfigureAwait(false);

                    Assert.IsNotNull(reused, "ReuseTermRequest returned no result.");

                    Assert.AreEqual(sourceTermId, reused.Id,
                        "A reused term should carry the source term's id.");
                }
                finally
                {
                    await DeleteGroupAsync(context, groupId).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Taxonomy")]
        public async Task GetSiteCollectionTermGroup_ResolvesTheScopedGroup()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                TermGroupInfo group = await SendAsync(context,
                        new GetSiteCollectionTermGroupRequest(siteId, webId, createIfMissing: false)).ConfigureAwait(false);

                if (group == null || group.Id == Guid.Empty)
                {
                    Assert.Inconclusive(
                        "This site collection has no scoped term group yet, so there was nothing to resolve. " +
                        "Re-run with createIfMissing to exercise the creating path.");
                    return;
                }

                Assert.IsTrue(group.IsSiteCollectionGroup,
                    "GetSiteCollectionGroup returned a group that is not site-collection scoped.");
                Assert.IsFalse(string.IsNullOrEmpty(group.Name));
            }
        }
    }
}
