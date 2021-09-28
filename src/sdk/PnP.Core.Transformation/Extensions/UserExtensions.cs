using System;
using System.Linq;
using System.Text;
using PnP.Core.Model.Security;
using PnP.Core.Transformation.Model;

namespace PnP.Core.Transformation
{
    /// <summary>
    /// Class holding extension methods for the PnP.Core.Model.Security.ISharePointUser class
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Determines if a string exists in another string regardless of casing
        /// </summary>
        /// <param name="user">original user</param>
        /// <returns>The user transformed into a User Entity</returns>
        public static UserEntity ToUserEntity(this ISharePointUser user)
        {
            var result = new UserEntity();

            result.Id = user.Id.ToString();
            result.Upn = user.UserPrincipalName;
            result.Name = user.Title;
            // result.Role = ???
            result.LoginName = user.LoginName;
            result.IsGroup = user.PrincipalType == PrincipalType.SharePointGroup || 
                user.PrincipalType == PrincipalType.SecurityGroup;

            return result;
        }
    }
}
