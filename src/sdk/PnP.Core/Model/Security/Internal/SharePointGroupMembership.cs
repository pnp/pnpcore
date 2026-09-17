using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Pure logic behind <see cref="ISharePointUser.GetTransitiveGroupsAsync"/>, kept free of requests so it can be tested offline
    /// </summary>
    internal static class SharePointGroupMembership
    {
        /// <summary>
        /// Microsoft Graph accepts at most 20 group ids per checkMemberGroups call
        /// </summary>
        internal const int CheckMemberGroupsBatchSize = 20;

        private const string M365GroupClaimPrefix = "c:0o.c|federateddirectoryclaimprovider|";
        private const string M365GroupOwnersClaimSuffix = "_o";

        internal enum GroupClaim
        {
            /// <summary>
            /// Not an Entra ID backed group claim, e.g. a regular user or "Everyone except external users"
            /// </summary>
            None,

            /// <summary>
            /// Entra ID security group or Microsoft 365 group members claim
            /// </summary>
            Members,

            /// <summary>
            /// Microsoft 365 group owners claim
            /// </summary>
            Owners
        }

        internal static GroupClaim Classify(PrincipalType principalType, string loginName, string aadObjectId)
        {
            // Special claims like "Everyone except external users" are security group principals too, but carry no Entra object id
            if (principalType != PrincipalType.SecurityGroup || !Guid.TryParse(aadObjectId, out _))
            {
                return GroupClaim.None;
            }

            // The owners claim reports the same object id as the members claim, only the login name tells them apart
            bool isOwnersClaim = loginName != null
                && loginName.StartsWith(M365GroupClaimPrefix, StringComparison.OrdinalIgnoreCase)
                && loginName.EndsWith(M365GroupOwnersClaimSuffix, StringComparison.OrdinalIgnoreCase);

            return isOwnersClaim ? GroupClaim.Owners : GroupClaim.Members;
        }

        internal static List<List<string>> ToBatches(IEnumerable<string> ids, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }

            return ids
                .Select((id, index) => (id, index))
                .GroupBy(item => item.index / batchSize, item => item.id)
                .Select(batch => batch.ToList())
                .ToList();
        }

        /// <summary>
        /// Whether a SharePoint group grants membership to the user, either directly or through one of its Entra ID group claims
        /// </summary>
        /// <param name="userId">SharePoint id of the user</param>
        /// <param name="members">Members of the SharePoint group</param>
        /// <param name="memberOfGroupIds">Entra ID group ids the user is a transitive member of</param>
        /// <param name="ownedGroupIds">Microsoft 365 group ids the user owns</param>
        internal static bool GrantsMembership(int userId, IEnumerable<(int Id, GroupClaim Claim, string AadObjectId)> members,
            ISet<string> memberOfGroupIds, ISet<string> ownedGroupIds)
        {
            return members.Any(member =>
                member.Id == userId
                || (member.Claim == GroupClaim.Members && memberOfGroupIds.Contains(member.AadObjectId))
                || (member.Claim == GroupClaim.Owners && ownedGroupIds.Contains(member.AadObjectId)));
        }
    }
}
