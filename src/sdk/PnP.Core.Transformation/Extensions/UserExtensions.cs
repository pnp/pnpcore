using PnP.Core.Model.Security;
using PnP.Core.Transformation.Model;

namespace PnP.Core.Transformation
{
    /// <summary>
    /// Class holding extension methods for the PnP.Core.Model.Security.ISharePointUser class
    /// </summary>
    internal static class UserExtensions
    {
        /// <summary>
        /// Determines if a string exists in another string regardless of casing
        /// </summary>
        /// <param name="user">original user</param>
        /// <returns>The user transformed into a User Entity</returns>
        internal static UserEntity ToUserEntity(this ISharePointUser user)
        {
            var result = new UserEntity
            {
                Id = user.Id.ToString(),
                Upn = user.UserPrincipalName,
                Name = user.Title,
                // result.Role = ???
                LoginName = user.LoginName,
                IsGroup = user.PrincipalType == PrincipalType.SharePointGroup ||
                user.PrincipalType == PrincipalType.SecurityGroup
            };

            return result;
        }
    }
}
