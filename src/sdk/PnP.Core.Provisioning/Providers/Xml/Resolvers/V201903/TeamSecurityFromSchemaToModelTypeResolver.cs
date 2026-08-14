using PnP.Core.Provisioning.Utilities;
using PnP.Core.Provisioning.Model.Teams;
using System;
using System.Collections.Generic;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Type resolver for Teams Security from Schema to Model
    /// </summary>
    internal class TeamSecurityFromSchemaToModelTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;
        public bool CustomCollectionResolver => false;


        public object Resolve(object source, Dictionary<string, IResolver> resolvers = null, bool recursive = false)
        {
            TeamSecurity result = null;

            var security = source.GetPublicInstancePropertyValue("Security");
            if (null != security)
            {
                result = new TeamSecurity();

                var clearExistingOwnersValue = (security?.GetPublicInstancePropertyValue("Owners")?.GetPublicInstancePropertyValue("ClearExistingItems"));
                result.ClearExistingOwners = clearExistingOwnersValue != null ? (Boolean)clearExistingOwnersValue : false;
                var clearExistingMembersValue = (security?.GetPublicInstancePropertyValue("Members")?.GetPublicInstancePropertyValue("ClearExistingItems"));
                result.ClearExistingMembers = clearExistingMembersValue != null ? (Boolean)clearExistingMembersValue : false;

                var usersResolver = new CollectionFromSchemaToModelTypeResolver(typeof(TeamSecurityUser));

                var owners = security.GetPublicInstancePropertyValue("Owners");
                if (null != owners)
                {
                    result.Owners.AddRange(
                        usersResolver.Resolve(owners.GetPublicInstancePropertyValue("User"))
                        as IEnumerable<TeamSecurityUser>);
                }

                var members = security.GetPublicInstancePropertyValue("Members");
                if (null != members)
                {
                    result.Members.AddRange(
                        usersResolver.Resolve(members.GetPublicInstancePropertyValue("User"))
                        as IEnumerable<TeamSecurityUser>);
                }
            }

            return (result);
        }
    }
}
