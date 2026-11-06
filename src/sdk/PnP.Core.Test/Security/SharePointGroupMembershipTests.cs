using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using Claim = PnP.Core.Model.Security.SharePointGroupMembership.GroupClaim;

namespace PnP.Core.Test.Security
{
    /// <summary>
    /// Offline tests for the matching logic behind ISharePointUser.GetTransitiveGroupsAsync. The requests themselves
    /// need a tenant with Entra ID groups, the decisions about what those responses mean are covered here.
    /// </summary>
    [TestClass]
    public class SharePointGroupMembershipTests
    {
        private const string GroupId = "f3330771-2e48-416f-affa-9926243fd683";
        private const int UserId = 6;

        private static readonly HashSet<string> None = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        [TestMethod]
        public void ClassifyIgnoresRegularUsers()
        {
            Assert.AreEqual(Claim.None, SharePointGroupMembership.Classify(PrincipalType.User, "i:0#.f|membership|joe@contoso.onmicrosoft.com", GroupId));
        }

        [TestMethod]
        public void ClassifyIgnoresSecurityGroupClaimsWithoutObjectId()
        {
            // "Everyone except external users" is a security group principal that returns no object id
            Assert.AreEqual(Claim.None, SharePointGroupMembership.Classify(PrincipalType.SecurityGroup, "c:0-.f|rolemanager|spo-grid-all-users/e57fc0a3-83fd-4a43-9bc9-63d13fe3791c", null));
            Assert.AreEqual(Claim.None, SharePointGroupMembership.Classify(PrincipalType.SecurityGroup, "c:0(.s|true", ""));
        }

        [TestMethod]
        public void ClassifyEntraSecurityGroupAsMembers()
        {
            Assert.AreEqual(Claim.Members, SharePointGroupMembership.Classify(PrincipalType.SecurityGroup, $"c:0t.c|tenant|{GroupId}", GroupId));
        }

        [TestMethod]
        public void ClassifyMicrosoft365GroupMembersClaimAsMembers()
        {
            Assert.AreEqual(Claim.Members, SharePointGroupMembership.Classify(PrincipalType.SecurityGroup, $"c:0o.c|federateddirectoryclaimprovider|{GroupId}", GroupId));
        }

        [TestMethod]
        public void ClassifyMicrosoft365GroupOwnersClaimAsOwners()
        {
            // Same object id as the members claim, only the _o suffix differs
            Assert.AreEqual(Claim.Owners, SharePointGroupMembership.Classify(PrincipalType.SecurityGroup, $"c:0o.c|federateddirectoryclaimprovider|{GroupId}_o", GroupId));
            Assert.AreEqual(Claim.Owners, SharePointGroupMembership.Classify(PrincipalType.SecurityGroup, $"C:0O.C|FEDERATEDDIRECTORYCLAIMPROVIDER|{GroupId}_O", GroupId));
        }

        [TestMethod]
        public void ToBatchesRespectsTheGraphLimit()
        {
            var ids = Enumerable.Range(0, 41).Select(i => i.ToString()).ToList();

            var batches = SharePointGroupMembership.ToBatches(ids, SharePointGroupMembership.CheckMemberGroupsBatchSize);

            CollectionAssert.AreEqual(new[] { 20, 20, 1 }, batches.Select(b => b.Count).ToArray());
            CollectionAssert.AreEqual(ids, batches.SelectMany(b => b).ToList());
        }

        [TestMethod]
        public void ToBatchesReturnsNothingForNoIds()
        {
            Assert.AreEqual(0, SharePointGroupMembership.ToBatches(new List<string>(), 20).Count);
        }

        [TestMethod]
        public void ToBatchesRejectsAnInvalidSize()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => SharePointGroupMembership.ToBatches(new[] { "a" }, 0));
        }

        [TestMethod]
        public void GrantsMembershipForADirectMember()
        {
            var members = new[] { (UserId, Claim.None, (string)null) };

            Assert.IsTrue(SharePointGroupMembership.GrantsMembership(UserId, members, None, None));
        }

        [TestMethod]
        public void GrantsMembershipThroughAGroupTheUserIsATransitiveMemberOf()
        {
            var members = new[] { (20, Claim.Members, GroupId) };
            var memberOf = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { GroupId.ToUpperInvariant() };

            Assert.IsTrue(SharePointGroupMembership.GrantsMembership(UserId, members, memberOf, None));
        }

        [TestMethod]
        public void DeniesMembershipThroughAGroupTheUserIsNotIn()
        {
            var members = new[] { (20, Claim.Members, GroupId), (7, Claim.None, (string)null) };

            Assert.IsFalse(SharePointGroupMembership.GrantsMembership(UserId, members, None, None));
        }

        [TestMethod]
        public void DeniesOwnersClaimToAMemberWhoIsNotAnOwner()
        {
            // The reason owners claims are classified separately: being a member of the Microsoft 365 group
            // must not match the owners claim, even though both carry the same object id
            var members = new[] { (26, Claim.Owners, GroupId) };
            var memberOf = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { GroupId };

            Assert.IsFalse(SharePointGroupMembership.GrantsMembership(UserId, members, memberOf, None));
        }

        [TestMethod]
        public void GrantsOwnersClaimToAnOwner()
        {
            var members = new[] { (26, Claim.Owners, GroupId) };
            var owned = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { GroupId };

            Assert.IsTrue(SharePointGroupMembership.GrantsMembership(UserId, members, None, owned));
        }

        [TestMethod]
        public void DeniesMembersClaimToAnOwnerWhoIsNotAMember()
        {
            var members = new[] { (22, Claim.Members, GroupId) };
            var owned = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { GroupId };

            Assert.IsFalse(SharePointGroupMembership.GrantsMembership(UserId, members, None, owned));
        }
    }
}
