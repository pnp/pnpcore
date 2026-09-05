using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Taxonomy;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.Test.Offline.Csom
{
    /// <summary>
    /// Payload tests for the taxonomy CSOM requests - the ones spike <b>S1</b> forced into
    /// existence.
    /// </summary>
    [TestClass]
    public class CsomTaxonomyPayloadTests
    {
        private static readonly Guid SiteId = new Guid("11111111-1111-1111-1111-111111111111");
        private static readonly Guid WebId = new Guid("22222222-2222-2222-2222-222222222222");
        private static readonly Guid GroupId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid TermSetId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        private static readonly Guid TermId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");

        private static string Serialize<T>(IRequest<T> request)
        {
            var builder = new CSOMApiCallBuilder();
            builder.AddRequest(request);
            string payload = builder.SerializeCSOMRequests();

            XDocument.Parse(payload);

            return payload;
        }

        #region The GUID-preservation guarantee

        [TestMethod]
        [TestCategory("Offline")]
        public void CreateTermGroupRequest_SendsTheCallerSuppliedGuid()
        {
            string payload = Serialize(new CreateTermGroupRequest("My Group", GroupId, "A description"));

            StringAssert.Contains(payload, CsomTypeIds.TaxonomySession);
            StringAssert.Contains(payload, "Name=\"CreateGroup\"");
            StringAssert.Contains(payload, GroupId.ToString(),
                "The caller-supplied group id must reach the server - it is the entire reason this request exists.");
            StringAssert.Contains(payload, "My Group");
            StringAssert.Contains(payload, "A description");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void CreateTermSetRequest_SendsTheCallerSuppliedGuidAndLanguage()
        {
            string payload = Serialize(new CreateTermSetRequest(GroupId, "My Term Set", TermSetId, 1033));

            StringAssert.Contains(payload, "Name=\"GetGroup\"");
            StringAssert.Contains(payload, "Name=\"CreateTermSet\"");
            StringAssert.Contains(payload, TermSetId.ToString());
            StringAssert.Contains(payload, ">1033<");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void CreateTermRequest_SendsTheCallerSuppliedGuid()
        {
            string payload = Serialize(new CreateTermRequest(TermSetId, parentIsTermSet: true, "My Term", TermId, 1033));

            StringAssert.Contains(payload, "Name=\"GetTermSet\"");
            StringAssert.Contains(payload, "Name=\"CreateTerm\"");
            StringAssert.Contains(payload, TermId.ToString());
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void CreateTermRequest_UnderATermParentResolvesTheParentAsATerm()
        {
            string payload = Serialize(new CreateTermRequest(TermId, parentIsTermSet: false, "Child", Guid.NewGuid(), 1033));

            StringAssert.Contains(payload, "Name=\"GetTerm\"");
            Assert.IsFalse(payload.Contains("Name=\"GetTermSet\""),
                "A child term's parent is a term, so GetTermSet must not appear.");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void CreateRequests_RejectAnEmptyGuid()
        {
            Assert.ThrowsException<ArgumentException>(() => new CreateTermGroupRequest("x", Guid.Empty));
            Assert.ThrowsException<ArgumentException>(() => new CreateTermSetRequest(GroupId, "x", Guid.Empty, 1033));
            Assert.ThrowsException<ArgumentException>(() => new CreateTermRequest(TermSetId, true, "x", Guid.Empty, 1033));
        }

        #endregion

        #region The capabilities Graph cannot express

        [TestMethod]
        [TestCategory("Offline")]
        public void CreateTermSetRequest_SendsCustomSortOrder()
        {
            string payload = Serialize(new CreateTermSetRequest(GroupId, "Set", TermSetId, 1033, customSortOrder: "a:b:c"));

            StringAssert.Contains(payload, "Name=\"CustomSortOrder\"");
            StringAssert.Contains(payload, "a:b:c");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void DeprecateTermRequest_SendsTheDeprecationFlag()
        {
            StringAssert.Contains(Serialize(new DeprecateTermRequest(TermId, true)), "Name=\"Deprecate\"");
            StringAssert.Contains(Serialize(new DeprecateTermRequest(TermId, true)), ">true<");
            StringAssert.Contains(Serialize(new DeprecateTermRequest(TermId, false)), ">false<");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void SetTermCustomPropertyRequest_DistinguishesLocalFromSharedProperties()
        {
            string shared = Serialize(new SetTermCustomPropertyRequest(TermId, "Key", "Value", isLocal: false));
            StringAssert.Contains(shared, "Name=\"SetCustomProperty\"");

            string local = Serialize(new SetTermCustomPropertyRequest(TermId, "Key", "Value", isLocal: true));
            StringAssert.Contains(local, "Name=\"SetLocalCustomProperty\"");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ReuseTermRequest_UsesPinningOrBranchReuseAsAsked()
        {
            string branch = Serialize(new ReuseTermRequest(TermSetId, true, TermId, reuseBranch: true));
            StringAssert.Contains(branch, "Name=\"ReuseTerm\"");
            StringAssert.Contains(branch, ">true<");

            string pinned = Serialize(new ReuseTermRequest(TermSetId, true, TermId, pin: true));
            StringAssert.Contains(pinned, "Name=\"ReuseTermWithPinning\"");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void GetSiteCollectionTermGroupRequest_PassesTheSiteAndTheCreateFlag()
        {
            string payload = Serialize(new GetSiteCollectionTermGroupRequest(SiteId, WebId, createIfMissing: true));

            StringAssert.Contains(payload, "Name=\"GetSiteCollectionGroup\"");
            StringAssert.Contains(payload, $"site:{SiteId}:web:{WebId}:site:{SiteId}");
            StringAssert.Contains(payload, ">true<");
        }

        #endregion

        #region Committing

        [TestMethod]
        [TestCategory("Offline")]
        public void EveryWritingTaxonomyRequestCommitsTheTermStore()
        {
            var writes = new (string Name, string Payload)[]
            {
                ("CreateTermGroup", Serialize(new CreateTermGroupRequest("g", GroupId))),
                ("CreateTermSet", Serialize(new CreateTermSetRequest(GroupId, "s", TermSetId, 1033))),
                ("CreateTerm", Serialize(new CreateTermRequest(TermSetId, true, "t", TermId, 1033))),
                ("AddTermLabel", Serialize(new AddTermLabelRequest(TermId, "label", 1033, true))),
                ("SetTermCustomProperty", Serialize(new SetTermCustomPropertyRequest(TermId, "k", "v", false))),
                ("ReuseTerm", Serialize(new ReuseTermRequest(TermSetId, true, TermId))),
                ("DeprecateTerm", Serialize(new DeprecateTermRequest(TermId, true))),
            };

            foreach ((string name, string payload) in writes)
            {
                StringAssert.Contains(payload, "Name=\"CommitAll\"",
                    $"{name} writes to the term store but never commits it - the change would be discarded.");
            }
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ReadOnlyTaxonomyRequestsDoNotCommit()
        {
            string payload = Serialize(new GetSiteCollectionTermGroupRequest(SiteId, WebId, createIfMissing: false));

            Assert.IsFalse(payload.Contains("Name=\"CommitAll\""),
                "A read-only lookup must not commit the term store.");
        }

        #endregion

        #region Term store prologue

        [TestMethod]
        [TestCategory("Offline")]
        public void EveryTaxonomyRequestStartsFromTheDefaultSiteCollectionTermStore()
        {
            string payload = Serialize(new AddTermLabelRequest(TermId, "label", 1033, true));

            StringAssert.Contains(payload, "Name=\"GetTaxonomySession\"");
            StringAssert.Contains(payload, CsomTypeIds.TaxonomySession);
            StringAssert.Contains(payload, "Name=\"GetDefaultSiteCollectionTermStore\"");
        }

        #endregion
    }
}
